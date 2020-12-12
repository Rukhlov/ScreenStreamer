﻿
Texture2D image				 : register(t0);
SamplerState textureSampler  : register(s0);

cbuffer dataBuffer : register(b0)
{
	float2 base_dimension_i;
	//...
};


//uniform float4x4 ViewProj;

//static  float2 base_dimension;
//static  float2 base_dimension_i;
static float undistort_factor = 1.0;


struct VertData {
	float4 pos : SV_Position;
	float2 uv  : TEXCOORD0;
};

//struct VertOut {
//	float2 uv  : TEXCOORD0;
//	float4 pos : SV_Position;
//};

struct FragData {

	float4 pos : SV_Position;
	float2 uv : TexCoord0;
};

//VertOut VSDefault(VertData v_in)
//{
//	VertOut vert_out;
//	vert_out.uv = v_in.uv * base_dimension;
//	vert_out.pos = mul(float4(v_in.pos.xyz, 1.0), ViewProj);
//	return vert_out;
//}

float4 weight4(float x)
{
	/* Sharper version.  May look better in some cases. B=0, C=0.75 */
	return float4(
		((-0.75 * x + 1.5) * x - 0.75) * x,
		(1.25 * x - 2.25) * x * x + 1.0,
		((-1.25 * x + 1.5) * x + 0.75) * x,
		(0.75 * x - 0.75) * x * x);
}

float AspectUndistortX(float x, float a)
{
	// The higher the power, the longer the linear part will be.
	return (1.0 - a) * (x * x * x * x * x) + a * x;
}

float AspectUndistortU(float u)
{
	// Normalize texture coord to -1.0 to 1.0 range, and back.
	//return AspectUndistortX((u - 0.5) * 2.0, 1) * 0.5 + 0.5;

	return AspectUndistortX((u - 0.5) * 2.0, undistort_factor) * 0.5 + 0.5;
}

float2 undistort_coord(float xpos, float ypos)
{
	return float2(AspectUndistortU(xpos), ypos);
}

float4 undistort_pixel(float xpos, float ypos)
{
	return image.Sample(textureSampler, undistort_coord(xpos, ypos));
}

float4 undistort_line(float4 xpos, float ypos, float4 rowtaps)
{
	return undistort_pixel(xpos.x, ypos) * rowtaps.x +
		undistort_pixel(xpos.y, ypos) * rowtaps.y +
		undistort_pixel(xpos.z, ypos) * rowtaps.z +
		undistort_pixel(xpos.w, ypos) * rowtaps.w;
}

float4 DrawBicubic(FragData f_in, bool undistort)
{
	float2 base_dimension = float2(1280.0, 720.0);
	//float2 base_dimension_i = float2(1.0 / 2560.0, 1.0 / 1440.0);

	float2 pos = f_in.uv;
	float2 pos1 = floor(pos - 0.5) + 0.5;
	float2 f = pos - pos1;

	float4 rowtaps = weight4(f.x);
	float4 coltaps = weight4(f.y);

	float2 uv1 = pos1 * base_dimension_i;
	float2 uv0 = uv1 - base_dimension_i;
	float2 uv2 = uv1 + base_dimension_i;
	float2 uv3 = uv2 + base_dimension_i;

	if (undistort) {
		float4 xpos = float4(uv0.x, uv1.x, uv2.x, uv3.x);

		return undistort_line(xpos, uv0.y, rowtaps) * coltaps.x +
			undistort_line(xpos, uv1.y, rowtaps) * coltaps.y +
			undistort_line(xpos, uv2.y, rowtaps) * coltaps.z +
			undistort_line(xpos, uv3.y, rowtaps) * coltaps.w;
	}

	float u_weight_sum = rowtaps.y + rowtaps.z;
	float u_middle_offset = rowtaps.z * base_dimension_i.x / u_weight_sum;
	float u_middle = uv1.x + u_middle_offset;

	float v_weight_sum = coltaps.y + coltaps.z;
	float v_middle_offset = coltaps.z * base_dimension_i.y / v_weight_sum;
	float v_middle = uv1.y + v_middle_offset;

	int2 coord_top_left = int2(max(uv0 * base_dimension, 0.5));
	int2 coord_bottom_right = int2(min(uv3 * base_dimension, base_dimension - 0.5));

	float4 top = image.Load(int3(coord_top_left, 0)) * rowtaps.x;
	top += image.Sample(textureSampler, float2(u_middle, uv0.y)) * u_weight_sum;
	top += image.Load(int3(coord_bottom_right.x, coord_top_left.y, 0)) * rowtaps.w;
	float4 total = top * coltaps.x;

	float4 middle = image.Sample(textureSampler, float2(uv0.x, v_middle)) * rowtaps.x;
	middle += image.Sample(textureSampler, float2(u_middle, v_middle)) * u_weight_sum;
	middle += image.Sample(textureSampler, float2(uv3.x, v_middle)) * rowtaps.w;
	total += middle * v_weight_sum;

	float4 bottom = image.Load(int3(coord_top_left.x, coord_bottom_right.y, 0)) * rowtaps.x;
	bottom += image.Sample(textureSampler, float2(u_middle, uv3.y)) * u_weight_sum;
	bottom += image.Load(int3(coord_bottom_right, 0)) * rowtaps.w;
	total += bottom * coltaps.w;

	return total;
}
float4 PSDrawBicubicRGBA(FragData f_in) : SV_Target
{
	return DrawBicubic(f_in, true);
}

//float4 PSDrawBicubicRGBA(FragData f_in, bool undistort) : SV_Target
//{
//	return DrawBicubic(f_in, undistort);
//}

float4 PSDrawBicubicRGBADivide(FragData f_in) : SV_Target
{
	float4 rgba = DrawBicubic(f_in, false);
	float alpha = rgba.a;
	float multiplier = (alpha > 0.0) ? (1.0 / alpha) : 0.0;
	return float4(rgba.rgb * multiplier, alpha);
}



///*
// * bicubic sharper (better for downscaling)
// * note - this shader is adapted from the GPL bsnes shader, very good stuff
// * there.
// */
//
//uniform float4x4 ViewProj;
//uniform texture2d image;
//uniform float2 base_dimension;
//uniform float2 base_dimension_i;
//uniform float undistort_factor = 1.0;
//
//sampler_state textureSampler {
//	Filter    = Linear;
//	AddressU  = Clamp;
//	AddressV  = Clamp;
//};
//
//struct VertData {
//	float4 pos : POSITION;
//	float2 uv  : TEXCOORD0;
//};
//
//struct VertOut {
//	float2 uv  : TEXCOORD0;
//	float4 pos : POSITION;
//};
//
//struct FragData {
//	float2 uv : TEXCOORD0;
//};
//
//VertOut VSDefault(VertData v_in)
//{
//	VertOut vert_out;
//	vert_out.uv = v_in.uv * base_dimension;
//	vert_out.pos = mul(float4(v_in.pos.xyz, 1.0), ViewProj);
//	return vert_out;
//}
//
//float4 weight4(float x)
//{
//	/* Sharper version.  May look better in some cases. B=0, C=0.75 */
//	return float4(
//		((-0.75 * x + 1.5) * x - 0.75) * x,
//		(1.25 * x - 2.25) * x * x + 1.0,
//		((-1.25 * x + 1.5) * x + 0.75) * x,
//		(0.75 * x - 0.75) * x * x);
//}
//
//float AspectUndistortX(float x, float a)
//{
//	// The higher the power, the longer the linear part will be.
//	return (1.0 - a) * (x * x * x * x * x) + a * x;
//}
//
//float AspectUndistortU(float u)
//{
//	// Normalize texture coord to -1.0 to 1.0 range, and back.
//	return AspectUndistortX((u - 0.5) * 2.0, undistort_factor) * 0.5 + 0.5;
//}
//
//float2 undistort_coord(float xpos, float ypos)
//{
//	return float2(AspectUndistortU(xpos), ypos);
//}
//
//float4 undistort_pixel(float xpos, float ypos)
//{
//	return image.Sample(textureSampler, undistort_coord(xpos, ypos));
//}
//
//float4 undistort_line(float4 xpos, float ypos, float4 rowtaps)
//{
//	return undistort_pixel(xpos.x, ypos) * rowtaps.x +
//	       undistort_pixel(xpos.y, ypos) * rowtaps.y +
//	       undistort_pixel(xpos.z, ypos) * rowtaps.z +
//	       undistort_pixel(xpos.w, ypos) * rowtaps.w;
//}
//
//float4 DrawBicubic(FragData f_in, bool undistort)
//{
//	float2 pos = f_in.uv;
//	float2 pos1 = floor(pos - 0.5) + 0.5;
//	float2 f = pos - pos1;
//
//	float4 rowtaps = weight4(f.x);
//	float4 coltaps = weight4(f.y);
//
//	float2 uv1 = pos1 * base_dimension_i;
//	float2 uv0 = uv1 - base_dimension_i;
//	float2 uv2 = uv1 + base_dimension_i;
//	float2 uv3 = uv2 + base_dimension_i;
//
//	if (undistort) {
//		float4 xpos = float4(uv0.x, uv1.x, uv2.x, uv3.x);
//		return undistort_line(xpos, uv0.y, rowtaps) * coltaps.x +
//		       undistort_line(xpos, uv1.y, rowtaps) * coltaps.y +
//		       undistort_line(xpos, uv2.y, rowtaps) * coltaps.z +
//		       undistort_line(xpos, uv3.y, rowtaps) * coltaps.w;
//	}
//
//	float u_weight_sum = rowtaps.y + rowtaps.z;
//	float u_middle_offset = rowtaps.z * base_dimension_i.x / u_weight_sum;
//	float u_middle = uv1.x + u_middle_offset;
//
//	float v_weight_sum = coltaps.y + coltaps.z;
//	float v_middle_offset = coltaps.z * base_dimension_i.y / v_weight_sum;
//	float v_middle = uv1.y + v_middle_offset;
//
//	int2 coord_top_left = int2(max(uv0 * base_dimension, 0.5));
//	int2 coord_bottom_right = int2(min(uv3 * base_dimension, base_dimension - 0.5));
//
//	float4 top = image.Load(int3(coord_top_left, 0)) * rowtaps.x;
//	top += image.Sample(textureSampler, float2(u_middle, uv0.y)) * u_weight_sum;
//	top += image.Load(int3(coord_bottom_right.x, coord_top_left.y, 0)) * rowtaps.w;
//	float4 total = top * coltaps.x;
//
//	float4 middle = image.Sample(textureSampler, float2(uv0.x, v_middle)) * rowtaps.x;
//	middle += image.Sample(textureSampler, float2(u_middle, v_middle)) * u_weight_sum;
//	middle += image.Sample(textureSampler, float2(uv3.x, v_middle)) * rowtaps.w;
//	total += middle * v_weight_sum;
//
//	float4 bottom = image.Load(int3(coord_top_left.x, coord_bottom_right.y, 0)) * rowtaps.x;
//	bottom += image.Sample(textureSampler, float2(u_middle, uv3.y)) * u_weight_sum;
//	bottom += image.Load(int3(coord_bottom_right, 0)) * rowtaps.w;
//	total += bottom * coltaps.w;
//
//	return total;
//}
//
//float4 PSDrawBicubicRGBA(FragData f_in, bool undistort) : TARGET
//{
//	return DrawBicubic(f_in, undistort);
//}
//
//float4 PSDrawBicubicRGBADivide(FragData f_in) : TARGET
//{
//	float4 rgba = DrawBicubic(f_in, false);
//	float alpha = rgba.a;
//	float multiplier = (alpha > 0.0) ? (1.0 / alpha) : 0.0;
//	return float4(rgba.rgb * multiplier, alpha);
//}
//
//technique Draw
//{
//	pass
//	{
//		vertex_shader = VSDefault(v_in);
//		pixel_shader = PSDrawBicubicRGBA(f_in, false);
//	}
//}
//
//technique DrawAlphaDivide
//{
//	pass
//	{
//		vertex_shader = VSDefault(v_in);
//		pixel_shader = PSDrawBicubicRGBADivide(f_in);
//	}
//}
//
//technique DrawUndistort
//{
//	pass
//	{
//		vertex_shader = VSDefault(v_in);
//		pixel_shader = PSDrawBicubicRGBA(f_in, true);
//	}
//}

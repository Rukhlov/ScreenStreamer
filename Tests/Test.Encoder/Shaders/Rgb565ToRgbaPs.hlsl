﻿
Texture2D<float>  rgb16Texture : t0;
SamplerState      defaultSampler : s0;

struct VertData
{
	float4 pos      : SV_Position;
	float2 texCoord : TexCoord0;
};

float3 Rgb565ToRgb888(float rgb565) {
	/* rgb565 -> rgb888
		WORD red_mask = 0xF800;
		WORD green_mask = 0x7E0;
		WORD blue_mask = 0x1F;

		BYTE red_value = (pixel & red_mask) >> 11;
		BYTE green_value = (pixel & green_mask) >> 5;
		BYTE blue_value = (pixel & blue_mask);

		// Expand to 8-bit values.
		BYTE red   = red_value << 3;
		BYTE green = green_value << 2;
		BYTE blue  = blue_value << 3;
	*/

	//uint i = asuint(rgb565);
	uint i = rgb565 * 0xFFFF;
	uint r = (i & 0xF800) >> 11; //(16-5)
	uint g = (i & 0x7E0) >> 5; //(16-5-6)
	uint b = (i & 0x1F);// (16 - 5 - 6 - 5)
	return float3(r / 31.0, g / 63.0, b / 31.0);

	//uint r = ((i & 0xF800) >> 11) << 3; //8 - 5
	//uint g = ((i & 0x7E0) >> 5) << 2; //8 - 6
	//uint b = ((i & 0x1F) << 3); ////8 - 5
	//return float3(r / 255.0, g / 255.0, b / 255.0);

}

float3 Rgb555ToRgb888(float rgb555) {
	/* rgb555 -> rgb888
		WORD red_mask = 0x7C00;
		WORD green_mask = 0x3E0;
		WORD blue_mask = 0x1F;

		BYTE red_value = (pixel & red_mask) >> 10; //(16 - 1 - 5)
		BYTE green_value = (pixel & green_mask) >> 5;//(16 - 1 - 5 - 5)
		BYTE blue_value = (pixel & blue_mask); //(16 - 1 - 5 - 5 - 5)

		// Expand to 8-bit values:
		BYTE red   = red_value << 3; //8 - 5
		BYTE green = green_value << 3; //8 - 5
		BYTE blue  = blue_value << 3; //8 - 5
	*/

	uint i = rgb555 * 0xFFFF;
	uint r = (i & 0x7C00) >> 10;
	uint g = (i & 0x3E0) >> 5;
	uint b = (i & 0x1F);

	return float3(r / 31.0, g / 31.0, b / 31.0);

}

float4 PS(VertData input) : SV_Target
{
	float rgb16 = rgb16Texture.Sample(defaultSampler, input.texCoord);

	return float4(saturate(Rgb565ToRgb888(rgb16)), 1.0);

}

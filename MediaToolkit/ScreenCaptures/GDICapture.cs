﻿using MediaToolkit.Core;
using MediaToolkit.Utils;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

using MediaToolkit.Logging;
using System.Runtime.InteropServices;
using MediaToolkit.SharedTypes;
using System.ComponentModel;

namespace MediaToolkit.ScreenCaptures
{


    /// <summary>
    /// Быстрее всего работает с выключенной композитной отрисовкой
    /// и с PixelFormat.Format32bppArgb 
    /// </summary>
    public class GDICapture : ScreenCapture, ITexture2DSource
    {
        private Device device = null;
        private Texture2D gdiTexture = null;

        public Texture2D SharedTexture { get; private set; }
        public long AdapterId { get; private set; }
        public bool UseHwContext { get; set; } = false;

        public override void Init(Rectangle srcRect, Size destSize = default(Size))
        {
            base.Init(srcRect, destSize);

            if (UseHwContext)
            {
                InitDx();
            }

        }

        private void InitDx()
        {
            logger.Debug("GDICapture::InitDx()");

            SharpDX.DXGI.Factory1 dxgiFactory = null;
            SharpDX.DXGI.Adapter1 adapter = null;
            SharpDX.DXGI.Output output = null;
            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();

                logger.Info(MediaFoundation.DxTool.LogDxAdapters(dxgiFactory.Adapters1));

                //var hMonitor = NativeAPIs.User32.GetMonitorFromRect(this.srcRect);
                //if (hMonitor != IntPtr.Zero)
                //{
                //    foreach (var _adapter in dxgiFactory.Adapters1)
                //    {
                //        foreach (var _output in _adapter.Outputs)
                //        {
                //            var descr = _output.Description;
                //            if (descr.MonitorHandle == hMonitor)
                //            {
                //                adapter = _adapter;
                //                output = _output;

                //                break;
                //            }
                //        }
                //    }
                //}

                if (adapter == null)
                {// первым идет адаптер с которому подключен primary монитор
                    adapter = dxgiFactory.GetAdapter1(0);
                }

                AdapterId = adapter.Description.Luid;

                //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                var deviceCreationFlags =
                    //DeviceCreationFlags.Debug |
                    DeviceCreationFlags.VideoSupport |
                    DeviceCreationFlags.BgraSupport;

                device = new Device(adapter, deviceCreationFlags);
                using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                {
                    multiThread.SetMultithreadProtected(true);
                }

                SharedTexture = new Texture2D(device,
                 new Texture2DDescription
                 {
                     CpuAccessFlags = CpuAccessFlags.None,
                     BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                     Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                     Width = DestSize.Width,
                     Height = DestSize.Height,

                     MipLevels = 1,
                     ArraySize = 1,
                     SampleDescription = { Count = 1, Quality = 0 },
                     Usage = ResourceUsage.Default,
                     OptionFlags = ResourceOptionFlags.Shared,

                 });

                gdiTexture = new Texture2D(device,
                 new Texture2DDescription
                 {
                     CpuAccessFlags = CpuAccessFlags.None,
                     BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                     Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                     Width = DestSize.Width,
                     Height = DestSize.Height,

                     MipLevels = 1,
                     ArraySize = 1,
                     SampleDescription = { Count = 1, Quality = 0 },
                     Usage = ResourceUsage.Default,
                     OptionFlags = ResourceOptionFlags.GdiCompatible,

                 });

            }
            finally
            {
                if (adapter != null)
                {
                    adapter.Dispose();
                    adapter = null;
                }

                if (output != null)
                {
                    output.Dispose();
                    output = null;
                }

                if (dxgiFactory != null)
                {
                    dxgiFactory.Dispose();
                    dxgiFactory = null;
                }
            }
        }



        public override ErrorCode UpdateBuffer(int timeout = 10)
        {
            ErrorCode result = ErrorCode.Ok;
            if (UseHwContext)
            {
                result = CopyScreenToSurface();
            }
            else
            {
                result = TryGetScreen(base.SrcRect, ref base.videoBuffer, this.CaptureMouse, timeout);
            }

            return result;

            //return TryGetScreen(base.SrcRect, ref base.videoBuffer, this.CaptureMouse, timeout);
        }



        private ErrorCode CopyScreenToSurface()
        {

            ErrorCode errorCode = ErrorCode.Unexpected;

            using (var surf = gdiTexture.QueryInterface<SharpDX.DXGI.Surface1>())
            {

                int nXSrc = SrcRect.Left;
                int nYSrc = SrcRect.Top;

                int nWidthSrc = SrcRect.Width;
                int nHeightSrc = SrcRect.Height;

                var descr = surf.Description;

                int nXDest = 0;
                int nYDest = 0;
                int nWidth = descr.Width;
                int nHeight = descr.Height;


                try
                {
                    var hdcDest = surf.GetDC(true);
                    IntPtr hdcSrc = IntPtr.Zero;
                    try
                    {
                        hdcSrc = User32.GetDC(IntPtr.Zero);

                        var dwRop = TernaryRasterOperations.CAPTUREBLT | TernaryRasterOperations.SRCCOPY;
                        //var dwRop = TernaryRasterOperations.SRCCOPY;

                        bool success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
                        if (!success)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                        if (CaptureMouse)
                        {
                            User32.DrawCursorEx(hdcDest, nXSrc, nYSrc);
                        }

                        errorCode = ErrorCode.Ok;

                    }
                    finally
                    {
                        Gdi32.DeleteDC(hdcSrc);
                    }
                }
                catch (Win32Exception ex)
                {
                    logger.Warn(ex);

                    if (ex.NativeErrorCode == Win32ErrorCodes.ERROR_ACCESS_DENIED)
                    {
                        errorCode = ErrorCode.AccessDenied;
                    }

                }
                finally
                {
                    surf.ReleaseDC();
                }

				if(errorCode == ErrorCode.Ok)
				{
					device.ImmediateContext.CopyResource(gdiTexture, SharedTexture);
					device.ImmediateContext.Flush();
				}


			}

            return errorCode;
        }

        public override void Close()
        {
            base.Close();

            CloseDx();
        }

        private void CloseDx()
        {
            logger.Debug("GDICapture::CloseDx()");

            if (SharedTexture != null)
            {
                SharedTexture.Dispose();
                SharedTexture = null;

            }

            if (gdiTexture != null)
            {
                gdiTexture.Dispose();
                gdiTexture = null;

            }

            if (device != null)
            {
                device.Dispose();
                device = null;

            }

        }

        public static ErrorCode TryGetScreen(Rectangle srcRect, ref VideoBuffer videoBuffer, bool captureMouse = false, int timeout = 10)
        {
            bool success = false;
            ErrorCode errorCode = ErrorCode.Unexpected;

            var syncRoot = videoBuffer.syncRoot;
            bool lockTaken = false;

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcDest = IntPtr.Zero;
            Graphics graphDest = null;
            try
            {
                Monitor.TryEnter(syncRoot, timeout, ref lockTaken);

                if (lockTaken)
                {
                    var bmp = videoBuffer.bitmap;
                    graphDest = System.Drawing.Graphics.FromImage(bmp);
                    hdcDest = graphDest.GetHdc();
                    Size destSize = bmp.Size;

                    int nXDest = 0;
                    int nYDest = 0;
                    int nWidth = destSize.Width;
                    int nHeight = destSize.Height;

                    hdcSrc = User32.GetDC(IntPtr.Zero);

                    int nXSrc = srcRect.Left;
                    int nYSrc = srcRect.Top;

                    int nWidthSrc = srcRect.Width;
                    int nHeightSrc = srcRect.Height;

                    // в этом режиме мигает курсор, но захватывается все содержимое рабочего стола (ContextMenu, ToolTip-ы PopUp-ы ...)
                    // https://docs.microsoft.com/en-us/previous-versions/technet-magazine/dd392008(v=msdn.10)
                    var dwRop = TernaryRasterOperations.CAPTUREBLT | TernaryRasterOperations.SRCCOPY;
                    //var dwRop = TernaryRasterOperations.SRCCOPY;


                    if (destSize.Width == srcRect.Width && destSize.Height == srcRect.Height)
                    {
                        //IntPtr hOldBmp = Gdi32.SelectObject(hMemoryDC, hBitmap);
                        //success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
                        //hBitmap = Gdi32.SelectObject(hMemoryDC, hOldBmp);
                        //videoBuffer.bitmap = Bitmap.FromHbitmap(hBitmap);

                        success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
                        if (!success)
                        {
                            int code = Marshal.GetLastWin32Error();
                            throw new Win32Exception(code);
                        }

                        if (captureMouse)
                        {
                            User32.DrawCursorEx(hdcDest, nXSrc, nYSrc);
                        }

                    }
                    else
                    {// Лучше не использовать масштабирование StretchBlt !!!

                        //самый быстрый и самый кривой режим масштабирования
                        Gdi32.SetStretchBltMode(hdcDest, StretchingMode.COLORONCOLOR);

                        //самый качественный но все равно выглядит хуже чем масштабирование sws_scale
                        //Gdi32.SetStretchBltMode(hdcDest, StretchingMode.HALFTONE);

                        success = Gdi32.StretchBlt(hdcDest, nXDest, nYDest, nWidth, nHeight,
                            hdcSrc, nXSrc, nYSrc, nWidthSrc, nHeightSrc,
                            dwRop);

                        if (!success)
                        {
                            int code = Marshal.GetLastWin32Error();
                            throw new Win32Exception(code);
                        }
                    }

                }

            }
            catch (Win32Exception ex)
            {
                logger.Warn(ex);

                if (ex.ErrorCode == (int)HResult.E_ACCESSDENIED)
                {
                    errorCode = ErrorCode.AccessDenied;
                }
            }
            finally
            {
                Gdi32.DeleteDC(hdcSrc);

                graphDest?.ReleaseHdc(hdcDest);
                graphDest?.Dispose();
                graphDest = null;

                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }

                // videoBuffer.bitmap.Save("d:\\__test123.bmp", ImageFormat.Bmp);
            }



            return errorCode;
        }



        public static Bitmap GetScreen(Rectangle rect)
        {
            return GetWindow(IntPtr.Zero, rect);
        }
        public static Bitmap GetWindow(IntPtr handle, Rectangle rect)
        {
            Bitmap bmp;

            Graphics g = null;
            IntPtr dest = IntPtr.Zero;
            IntPtr src = IntPtr.Zero;
            try
            {
                bmp = new Bitmap(rect.Width, rect.Height);
                g = System.Drawing.Graphics.FromImage(bmp);

                dest = g.GetHdc();
                src = User32.GetDC(handle);

                Gdi32.BitBlt(dest, rect.Left, rect.Top, rect.Width, rect.Height, src, 0, 0,
                    TernaryRasterOperations.CAPTUREBLT | TernaryRasterOperations.SRCCOPY);

            }
            finally
            {

                g.ReleaseHdc(dest);
                //g.ReleaseHdc(dc2);

                g.Dispose();
            }
            return bmp;

        }

    }

}

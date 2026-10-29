using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VisionImageTool
{
    /// <summary>
    /// Giải mã ảnh bằng WIC (Windows Imaging Component - có sẵn trong Windows) qua COM interop, KHÔNG
    /// cần reference PresentationCore / WindowsBase.
    ///
    /// Lý do: tạo thumbnail bằng GDI+ phải giải mã TOÀN BỘ ảnh 5 MP rồi mới thu nhỏ (~60-100 ms/ảnh).
    /// Bộ scaler của WIC dùng IWICBitmapSourceTransform của decoder JPEG để giải mã thẳng ở 1/2, 1/4,
    /// 1/8 độ phân giải (thu nhỏ trong miền DCT) - nhanh hơn nhiều lần khi ảnh đích chỉ ~80×60 px.
    /// </summary>
    internal static class WicImaging
    {
        #region COM

        private static readonly Guid ClsidWicImagingFactory = new Guid("cacaf262-9370-4615-a13b-9f5539da4c0a");
        private static readonly Guid PixelFormat24bppBgr = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90c");

        private const uint GenericRead = 0x80000000;
        private const int DecodeMetadataCacheOnDemand = 0;
        private const int InterpolationFant = 3;

        [ComImport, Guid("ec5ec8a9-c395-4314-9c77-54d7a935ff70"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IWICImagingFactory
        {
            IWICBitmapDecoder CreateDecoderFromFilename([MarshalAs(UnmanagedType.LPWStr)] string filename,
                IntPtr vendor, uint desiredAccess, int metadataOptions);
            void CreateDecoderFromStream();
            void CreateDecoderFromFileHandle();
            void CreateComponentInfo();
            void CreateDecoder();
            void CreateEncoder();
            void CreatePalette();
            IWICFormatConverter CreateFormatConverter();
            IWICBitmapScaler CreateBitmapScaler();
        }

        [ComImport, Guid("9EDDE9E7-8DEE-47ea-99DF-E6FAF2ED44BF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IWICBitmapDecoder
        {
            void QueryCapability();
            void Initialize();
            void GetContainerFormat();
            void GetDecoderInfo();
            void CopyPalette();
            void GetMetadataQueryReader();
            void GetPreview();
            void GetColorContexts();
            void GetThumbnail();
            uint GetFrameCount();
            IWICBitmapSource GetFrame(uint index);
        }

        [ComImport, Guid("00000120-a8f2-4877-ba0a-fd2b6645fb94"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IWICBitmapSource
        {
            void GetSize(out uint width, out uint height);
            Guid GetPixelFormat();
            void GetResolution(out double dpiX, out double dpiY);
            void CopyPalette(IntPtr palette);
            void CopyPixels(IntPtr rect, uint stride, uint bufferSize, IntPtr buffer);
        }

        [ComImport, Guid("00000302-a8f2-4877-ba0a-fd2b6645fb94"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IWICBitmapScaler
        {
            void GetSize(out uint width, out uint height);
            Guid GetPixelFormat();
            void GetResolution(out double dpiX, out double dpiY);
            void CopyPalette(IntPtr palette);
            void CopyPixels(IntPtr rect, uint stride, uint bufferSize, IntPtr buffer);
            void Initialize(IWICBitmapSource source, uint width, uint height, int mode);
        }

        [ComImport, Guid("00000301-a8f2-4877-ba0a-fd2b6645fb94"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IWICFormatConverter
        {
            void GetSize(out uint width, out uint height);
            Guid GetPixelFormat();
            void GetResolution(out double dpiX, out double dpiY);
            void CopyPalette(IntPtr palette);
            void CopyPixels(IntPtr rect, uint stride, uint bufferSize, IntPtr buffer);
            void Initialize(IWICBitmapSource source, [In] ref Guid destinationFormat,
                int dither, IntPtr palette, double alphaThreshold, int paletteType);
        }

        #endregion

        /// <summary>Mỗi luồng một factory (xuất Excel chạy song song nhiều luồng).</summary>
        [ThreadStatic]
        private static IWICImagingFactory _factory;

        private static IWICImagingFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = (IWICImagingFactory)Activator.CreateInstance(Type.GetTypeFromCLSID(ClsidWicImagingFactory));
                }

                return _factory;
            }
        }

        /// <summary>
        /// Đọc ảnh và thu nhỏ thành bitmap 24 bit. <paramref name="fit"/> nhận kích thước ảnh gốc, trả
        /// về kích thước đích. Lỗi (định dạng lạ, file hỏng...) thì ném exception - bên gọi tự lùi về GDI+.
        /// </summary>
        public static Bitmap LoadScaled(string path, Func<Size, Size> fit)
        {
            IWICBitmapDecoder decoder = null;
            IWICBitmapSource frame = null;
            IWICBitmapScaler scaler = null;
            IWICFormatConverter converter = null;

            try
            {
                decoder = Factory.CreateDecoderFromFilename(path, IntPtr.Zero, GenericRead, DecodeMetadataCacheOnDemand);
                frame = decoder.GetFrame(0);

                uint width, height;
                frame.GetSize(out width, out height);
                Size target = fit(new Size((int)width, (int)height));

                scaler = Factory.CreateBitmapScaler();
                scaler.Initialize(frame, (uint)target.Width, (uint)target.Height, InterpolationFant);

                converter = Factory.CreateFormatConverter();
                Guid format = PixelFormat24bppBgr;
                converter.Initialize((IWICBitmapSource)scaler, ref format, 0, IntPtr.Zero, 0, 0);

                // GDI+ Format24bppRgb lưu byte theo thứ tự B,G,R - trùng với 24bppBGR của WIC.
                var bitmap = new Bitmap(target.Width, target.Height, PixelFormat.Format24bppRgb);
                var data = bitmap.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                try
                {
                    converter.CopyPixels(IntPtr.Zero, (uint)data.Stride, (uint)(data.Stride * target.Height), data.Scan0);
                }
                catch
                {
                    bitmap.UnlockBits(data);
                    bitmap.Dispose();
                    throw;
                }

                bitmap.UnlockBits(data);
                return bitmap;
            }
            finally
            {
                Release(converter);
                Release(scaler);
                Release(frame);
                Release(decoder);
            }
        }

        private static void Release(object com)
        {
            if (com != null)
            {
                Marshal.ReleaseComObject(com);
            }
        }
    }
}

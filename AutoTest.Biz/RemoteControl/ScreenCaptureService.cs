using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AutoTest.Biz.RemoteControl
{
    /// <summary>
    /// 屏幕指定区域截图服务（单例）
    /// </summary>
    public sealed class ScreenCaptureService
    {
        private static readonly ScreenCaptureService _instance = new ScreenCaptureService();
        public static ScreenCaptureService Instance => _instance;

        private ScreenCaptureService() { }

        /// <summary>
        /// 当前捕获区域（屏幕坐标）
        /// </summary>
        public Rectangle CaptureRegion { get; private set; } = new Rectangle(0, 0, 1280, 720);

        /// <summary>
        /// JPEG压缩质量 1-100
        /// </summary>
        public long JpegQuality { get; set; } = 70;

        /// <summary>
        /// 设定截图区域
        /// </summary>
        public void SetRegion(int x, int y, int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("宽度和高度必须大于0");
            CaptureRegion = new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// 截取指定区域并返回JPEG字节
        /// </summary>
        public byte[] CaptureJpegBytes()
        {
            var region = CaptureRegion;
            using (var bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(region.Location, Point.Empty, region.Size, CopyPixelOperation.SourceCopy);

                var encoder = GetJpegEncoder();
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, JpegQuality);

                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, encoder, encoderParams);
                    return ms.ToArray();
                }
            }
        }

        private static ImageCodecInfo GetJpegEncoder()
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == "image/jpeg")
                    return codec;
            }
            throw new InvalidOperationException("未找到JPEG编码器");
        }
    }
}

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;

namespace Screenshot
{
    public static class ScreenshotHelper
    {
        public static BitmapSource CopyScreen()
        {
            using (var screenBmp = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;
                    bmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    bmpGraphics.CompositingQuality = CompositingQuality.HighQuality;
                    bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size, CopyPixelOperation.SourceCopy);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public static BitmapSource CopyScreen(int left, int top, int width, int height)
        {
            using (var screenBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;
                    bmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    bmpGraphics.CompositingQuality = CompositingQuality.HighQuality;
                    bmpGraphics.CopyFromScreen(left, top, 0, 0, screenBmp.Size, CopyPixelOperation.SourceCopy);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

    }
}
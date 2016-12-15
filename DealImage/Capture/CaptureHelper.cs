using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace DealImage.Capture
{
    public static class CaptureHelper
    {
        public static BitmapSource CopyScreen()
        {
            using (
                var screenBmp = new Bitmap((int) SystemParameters.PrimaryScreenWidth,
                    (int) SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb))
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

        public static BitmapSource ScreenCropper(ViewBox viewBox)
        {
            try
            {
                var imageSource = CopyScreen(
                (int)Math.Round(viewBox.ScreenshotBox.Left, 0) - 7,
                (int)Math.Round(viewBox.ScreenshotBox.Top, 0) - 7,
                (int)Math.Round(viewBox.ScreenshotBox.Width, 0),
                (int)Math.Round(viewBox.ScreenshotBox.Height, 0));
                if (!viewBox.IsRotated)
                {
                    return imageSource;
                }
                else
                {
                    //旋转变换
                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        var brush = new ImageBrush(imageSource)
                        {
                            Stretch = Stretch.None,
                            RelativeTransform = viewBox.RenderTransform,

                        };
                        context.DrawRectangle(brush, null, new Rect(0, 0, viewBox.RotateShowbox.Width, viewBox.RotateShowbox.Height));

                    }
                    var renderBitmap = new RenderTargetBitmap((int)viewBox.RotateShowbox.Width, (int)viewBox.RotateShowbox.Height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);

                    //裁剪
                    drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        var brush = new ImageBrush(renderBitmap)
                        {
                            ViewboxUnits = BrushMappingMode.Absolute,
                            Viewbox = viewBox.CropViewbox,
                        };
                        context.DrawRectangle(brush, null, new Rect(0, 0, viewBox.CropViewbox.Width, viewBox.CropViewbox.Height));
                    }
                    renderBitmap = new RenderTargetBitmap((int)viewBox.CropViewbox.Width, (int)viewBox.CropViewbox.Height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);
                    return renderBitmap;


                }
            }
            catch
            {
                return null;
            }
        }


    }
}

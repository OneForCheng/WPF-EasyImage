using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Screenshot
{
    public static class ScreenCropper
    {
        public static BitmapSource CopyScreen()
        {
            return CropScreen(0, 0, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
        }

        public static BitmapSource CropScreen(int left, int top, int width, int height)
        {
            using (var screenBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;
                    bmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    bmpGraphics.CompositingQuality = CompositingQuality.GammaCorrected;
                    bmpGraphics.CopyFromScreen(left, top, 0, 0, screenBmp.Size, CopyPixelOperation.SourceCopy);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public static BitmapSource CropScreen(Rect rect)
        {
            return CropScreen((int)Math.Round(rect.Left, 0), (int)Math.Round(rect.Top, 0), (int)Math.Round(rect.Width, 0), (int)Math.Round(rect.Height, 0));
        }

        public static BitmapSource CropScreen(double left, double top, double width, double height)
        {
            return CropScreen((int)Math.Round(left, 0), (int)Math.Round(top, 0), (int)Math.Round(width, 0), (int)Math.Round(height, 0));
        }

        public static BitmapSource CropScreen(CropViewBox viewBox)
        {
            return CropScreen(CopyScreen(), viewBox);
        }

        public static BitmapSource CropScreen(BitmapSource fullScreenBtimap, CropViewBox viewBox)
        {
            try
            {
                var cropScreenbox  = new Int32Rect(
                    (int) Math.Round(viewBox.CropScreenbox.Left, 0),
                    (int) Math.Round(viewBox.CropScreenbox.Top, 0), 
                    (int) Math.Round(viewBox.CropScreenbox.Width, 0),
                    (int) Math.Round(viewBox.CropScreenbox.Height, 0));
                var extendBitmap = ExtendFullScreenBitmap(fullScreenBtimap, ref cropScreenbox);
                var croppedBitmap = new CroppedBitmap(extendBitmap, cropScreenbox);
                if (viewBox.IsTransform)
                {
                    //Ðý×ª¡¢·­×ª±ä»»
                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        var brush = new ImageBrush(croppedBitmap)
                        {
                            Stretch = Stretch.None,
                            RelativeTransform = viewBox.RenderTransform,

                        };
                        context.DrawRectangle(brush, null, new Rect(0, 0, viewBox.TransformViewbox.Width, viewBox.TransformViewbox.Height));
                    }
                    var renderBitmap = new RenderTargetBitmap((int)viewBox.TransformViewbox.Width, (int)viewBox.TransformViewbox.Height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);
                    croppedBitmap = new CroppedBitmap(renderBitmap, new Int32Rect((int)Math.Round(viewBox.CropViewbox.X, 0), (int)Math.Round(viewBox.CropViewbox.Y, 0), (int)Math.Round(viewBox.CropViewbox.Width, 0), (int)Math.Round(viewBox.CropViewbox.Height, 0)));
                }
                return croppedBitmap;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return null;
            }
        }

        private static BitmapSource ExtendFullScreenBitmap(BitmapSource fullScreenBtimap, ref Int32Rect rect)
        {
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var screenHeight = SystemParameters.VirtualScreenHeight;
            var rectRight = rect.X + rect.Width;
            var rectBottom = rect.Y + rect.Height;
            if (rect.X >= 0 && rect.Y >= 0 && rectRight <= screenWidth && rectBottom <= screenHeight)
            {
                return fullScreenBtimap;
            }
            double left = 0.0, top = 0.0;
            if (rect.X < 0)
            {
                left = rect.X;
                rect.X = 0;
            }
            if (rect.Y < 0)
            {
                top = rect.Y;
                rect.Y = 0;
            }
            var right = (rectRight > screenWidth) ? rectRight : screenWidth;
            var bottom = (rectBottom > screenHeight) ? rectBottom : screenHeight;
            var viewbox = new Rect(left, top, right - left, bottom - top);

            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                var brush = new ImageBrush(fullScreenBtimap)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = viewbox,
                    Stretch = Stretch.None,
                };
                context.DrawRectangle(brush, null, new Rect(0, 0, viewbox.Width, viewbox.Height));
            }
            var renderBitmap = new RenderTargetBitmap((int)viewbox.Width, (int)viewbox.Height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);

            return renderBitmap;
        }
    }
}

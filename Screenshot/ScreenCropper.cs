using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
            return CropScreen(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public static BitmapSource CropScreen(double left, double top, double width, double height)
        {
            using (var screenBmp = new Bitmap((int)Math.Round(width, 0), (int)Math.Round(height, 0), PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;
                    bmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    bmpGraphics.CompositingQuality = CompositingQuality.GammaCorrected;
                    bmpGraphics.CopyFromScreen((int)Math.Round(left, 0), (int)Math.Round(top, 0), 0, 0, screenBmp.Size, CopyPixelOperation.SourceCopy);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public static BitmapSource CropScreen(ViewBox viewBox)
        {
            try
            {
                var imageSource = CropScreen(viewBox.CropScreenbox.Left, viewBox.CropScreenbox.Top, viewBox.CropScreenbox.Width, viewBox.CropScreenbox.Height);
                if (!viewBox.IsTransform)
                {
                    return imageSource;
                }
                else
                {
                    //旋转、翻转变换
                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        var brush = new ImageBrush(imageSource)
                        {
                            Stretch = Stretch.None,
                            RelativeTransform = viewBox.RenderTransform,

                        };
                        context.DrawRectangle(brush, null, new Rect(0, 0, viewBox.TransformViewbox.Width, viewBox.TransformViewbox.Height));
                    }
                    var renderBitmap = new RenderTargetBitmap((int)viewBox.TransformViewbox.Width, (int)viewBox.TransformViewbox.Height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);
                    return new CroppedBitmap(renderBitmap, new Int32Rect((int)Math.Round(viewBox.CropViewbox.X, 0), (int)Math.Round(viewBox.CropViewbox.Y, 0), (int)Math.Round(viewBox.CropViewbox.Width, 0), (int)Math.Round(viewBox.CropViewbox.Height, 0)));
                    
                }
            }
            catch
            {
                return null;
            }
        }

        public static BitmapSource CropScreen(BitmapViewBox viewBox, CropStyle cropStyle)
        {
            try
            {
                var imageSource = CropScreen(viewBox.CropScreenbox.Left, viewBox.CropScreenbox.Top, viewBox.CropScreenbox.Width, viewBox.CropScreenbox.Height);
                if (!viewBox.IsTransform)
                {
                    var bitmap = imageSource.GetBitmap();
                    switch (cropStyle)
                    {
                        case CropStyle.Normal:
                            break;
                        case CropStyle.Shadow:
                            bitmap = bitmap.ShadowCropBitmap(viewBox.Bitmap);
                            break;
                        case CropStyle.Transparent:
                            bitmap = bitmap.TransparentCropBitmap(viewBox.Bitmap);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(cropStyle), cropStyle, null);
                    }
                    return bitmap.GetBitmapSource();
                }
                else
                {
                    //旋转、翻转变换
                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        var brush = new ImageBrush(imageSource)
                        {
                            Stretch = Stretch.None,
                            RelativeTransform = viewBox.RenderTransform,

                        };
                        context.DrawRectangle(brush, null, new Rect(0, 0, viewBox.TransformViewbox.Width, viewBox.TransformViewbox.Height));
                    }
                    var renderBitmap = new RenderTargetBitmap((int)viewBox.TransformViewbox.Width, (int)viewBox.TransformViewbox.Height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);

                    var cropped = new CroppedBitmap(renderBitmap, new Int32Rect((int)Math.Round(viewBox.CropViewbox.X, 0), (int)Math.Round(viewBox.CropViewbox.Y, 0), (int)Math.Round(viewBox.CropViewbox.Width, 0), (int)Math.Round(viewBox.CropViewbox.Height, 0)));
                    var bitmap = cropped.GetBitmap();
                    switch (cropStyle)
                    {
                        case CropStyle.Normal:
                            break;
                        case CropStyle.Shadow:
                            bitmap = bitmap.ShadowCropBitmap(viewBox.Bitmap);
                            break;
                        case CropStyle.Transparent:
                            bitmap = bitmap.TransparentCropBitmap(viewBox.Bitmap);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(cropStyle), cropStyle, null);
                    }
                    return bitmap.GetBitmapSource();
                }
            }
            catch
            {
                return null;
            }
        }

    }
}

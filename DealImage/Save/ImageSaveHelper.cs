using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DealImage.Save
{
    public static class ImageSaveHelper
    {
        public static void SaveControlToImage(this FrameworkElement element, String outputPath)
        {
            var type = ImageFileType.Png;
            switch (outputPath.Split('.').Last().ToUpper())
            {
                case "BMP":
                    type = ImageFileType.Bmp;
                    break;
                case "GIF":
                    type = ImageFileType.Gif;
                    break;
                case "JPG":
                    type = ImageFileType.Jpeg;
                    break;
                case "PNG":
                   type = ImageFileType.Png;
                    break;
                case "TIF":
                    type = ImageFileType.Tiff;
                    break;
            }
            SaveControlToImage(element, type, outputPath);
        }

        public static void SaveControlToImage(this FrameworkElement element, ImageFileType type, String outputPath)
        {
            // Get the size of the Visual and its descendants.
            var rect = VisualTreeHelper.GetDescendantBounds(element);

            // Make a DrawingVisual to make a screen
            // representation of the control.
            var drawingVisual = new DrawingVisual();

            // Fill a rectangle the same size as the control
            // with a brush containing images of the control.
            using (var context = drawingVisual.RenderOpen())
            {
                var brush = new VisualBrush(element);
                context.DrawRectangle(brush, null, new Rect(rect.Size));
            }

            // Make a bitmap and draw on it.
            var bitmapRender = new RenderTargetBitmap(
                (int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmapRender.Render(drawingVisual);

            BitmapEncoder encoder;

            //选取编码器
            switch (type)
            {
                case ImageFileType.Bmp:
                    encoder = new BmpBitmapEncoder();
                    break;
                case ImageFileType.Gif:
                    encoder = new GifBitmapEncoder();
                    break;
                case ImageFileType.Jpeg:
                    encoder = new JpegBitmapEncoder();
                    break;
                case ImageFileType.Png:
                    encoder = new PngBitmapEncoder();
                    break;
                case ImageFileType.Tiff:
                    encoder = new TiffBitmapEncoder();
                    break;
                default:
                    throw new InvalidOperationException("不支持此图片类型.");
            }

            //对于一般的图片，只有一帧，动态图片是有多帧的。
            encoder.Frames.Add(BitmapFrame.Create(bitmapRender));
            var folder = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(folder))
            {
                if (folder != null) Directory.CreateDirectory(folder);
            }
            using (var file = File.Create(outputPath))
            {
                encoder.Save(file);
            }
        }

    }
}

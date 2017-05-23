using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DealImage.Save
{
    public static class ImageSaver
    {
        public static async void SaveToFile(this BitmapImage bitmapImage, string outputPath)
        {
            BitmapSource bitmapSource = bitmapImage;
            var encoder = GetEncoderByFileExt(outputPath.Split('.').Last());
            if (encoder is GifBitmapEncoder && ImageHelper.GetImageExtension(bitmapImage.StreamSource) == ImageExtension.Gif)
            {
                try
                {
                    var folder = Path.GetDirectoryName(outputPath);
                    if (!Directory.Exists(folder))
                    {
                        if (folder != null) Directory.CreateDirectory(folder);
                    }
                    using (var file = File.Create(outputPath))
                    {
                        bitmapImage.StreamSource.Position = 0;
                        await bitmapImage.StreamSource.CopyToAsync(file);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
            else
            {
                if (encoder is JpegBitmapEncoder || encoder is BmpBitmapEncoder)
                {
                    //若存在透明色，则设置白色的
                    var width = (int)bitmapImage.Width;
                    var height = (int)bitmapImage.Height;
                    var rect = new Rect(0, 0, width, height);
                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        context.DrawRectangle(Brushes.White, null, rect);
                        context.DrawImage(bitmapImage, rect);
                    }
                    var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);
                    bitmapSource = renderBitmap;
                }
                try
                {
                    var folder = Path.GetDirectoryName(outputPath);
                    if (!Directory.Exists(folder))
                    {
                        if (folder != null) Directory.CreateDirectory(folder);
                    }
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (var file = File.Create(outputPath))
                    {
                        encoder.Save(file);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
            
        }

        private static BitmapEncoder GetEncoderByFileExt(string fileExt)
        {
            BitmapEncoder encoder;
            switch (fileExt.ToUpper())
            {
                case "GIF":
                    encoder = new GifBitmapEncoder();
                    break;
                case "JPG":
                case "JPEG":
                case "JPE":
                case "JFIF":
                    encoder = new JpegBitmapEncoder();
                    break;
                case "PNG":
                    encoder = new PngBitmapEncoder();
                    break;
                case "TIF":
                case "TIFF":
                    encoder = new TiffBitmapEncoder();
                    break;
                case "BMP":
                case "DIB":
                case "RLE":
                    encoder = new BmpBitmapEncoder();
                    break;
                case "WDP":
                    encoder = new WmpBitmapEncoder();
                    break;
                default:
                    encoder = new PngBitmapEncoder();
                    break;
            }
            return encoder;
        }

    }
}

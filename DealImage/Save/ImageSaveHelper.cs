using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DealImage.Save
{
    public static class ImageSaveHelper
    {
        //public static void SaveControlToImage(this FrameworkElement element, String outputPath)
        //{
        //    element.SaveControlToImage(GetImageFileType(outputPath.Split('.').Last()), outputPath);
        //}

        //public static void SaveControlToImage(this FrameworkElement element, ImageFileType type, String outputPath)
        //{
        //    var rect = element.GetMinContainerBounds();
        //    var drawingVisual = new DrawingVisual();
        //    using (var context = drawingVisual.RenderOpen())
        //    {
        //        var brush = new VisualBrush(element);
        //        context.DrawRectangle(brush, null, rect);
        //    }

        //    var renderBitmap = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Pbgra32);
        //    renderBitmap.Render(drawingVisual);


        //    SaveBitmapToFile(renderBitmap, type, outputPath);
        //}

        //public static void SaveChildControlToImage(this FrameworkElement element, FrameworkElement childElement, String outputPath)
        //{
        //    element.SaveChildControlToImage(childElement, GetImageFileType(outputPath.Split('.').Last()), outputPath);
        //}

        //public static void SaveChildControlToImage(this FrameworkElement element, FrameworkElement  childElement, ImageFileType type, String outputPath)
        //{
        //    var rect = childElement.GetMinContainerBounds();

        //    var drawingVisual = new DrawingVisual();
        //    using (var context = drawingVisual.RenderOpen())
        //    {
        //        var viewbox = element.GetChildViewbox(childElement);
        //        var brush = new VisualBrush(element)
        //        {
        //            ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
        //            Viewbox = viewbox,
        //        };
        //        context.DrawRectangle(brush, null, rect);
        //    }

        //    var renderBitmap = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Pbgra32);
        //    renderBitmap.Render(drawingVisual);

        //    SaveBitmapToFile(renderBitmap, type, outputPath);
        //}

        public static void SaveChildElementsToImage(this IDictionary<FrameworkElement, FrameworkElement> dictionary, String outputPath)
        {
            SaveChildElementsToImage(dictionary, GetEncoderByFileExt(outputPath.Split('.').Last()), outputPath);
        }

        public static void SaveChildElementsToImage(this IDictionary<FrameworkElement, FrameworkElement> dictionary, BitmapEncoder encoder, String outputPath)
        {
            if (encoder is JpegBitmapEncoder || encoder is BmpBitmapEncoder)
            {
                SaveBitmapToFile(dictionary.GetRenderTargetBitmap(System.Windows.Media.Brushes.White), encoder, outputPath);
            }
            else
            {
                SaveBitmapToFile(dictionary.GetRenderTargetBitmap(), encoder, outputPath);
            }
        }

        private static void SaveBitmapToFile(BitmapSource bitmap, BitmapEncoder encoder, String outputPath)
        {
            try
            {
                var folder = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(folder))
                {
                    if (folder != null) Directory.CreateDirectory(folder);
                }
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
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

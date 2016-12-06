using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            SaveChildElementsToImage(dictionary, GetImageFileType(outputPath.Split('.').Last()), outputPath);
        }

        public static void SaveChildElementsToImage(this IDictionary<FrameworkElement, FrameworkElement> dictionary, ImageFileType type, String outputPath)
        {
            SaveBitmapToFile(dictionary.GetRenderTargetBitmap(), type, outputPath);
        }

        private static void SaveBitmapToFile(BitmapSource bitmap, ImageFileType type, String outputPath)
        {
            try
            {
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

                encoder.Frames.Add(BitmapFrame.Create(bitmap));
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
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private static ImageFileType GetImageFileType(string fileExt)
        {
            ImageFileType type;
            switch (fileExt.ToUpper())
            {
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
                case "BMP":
                    type = ImageFileType.Bmp;
                    break;
                default:
                    throw new InvalidOperationException("不支持此图像格式.");
            }
            return type;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DealImage.Save
{
    public static class ImageSaver
    {
        public static void SaveChildElementsToImage(this IDictionary<FrameworkElement, FrameworkElement> dictionary, String outputPath)
        {
            SaveChildElementsToImage(dictionary, GetEncoderByFileExt(outputPath.Split('.').Last()), outputPath);
        }

        public static void SaveChildElementsToImage(this IDictionary<FrameworkElement, FrameworkElement> dictionary, BitmapEncoder encoder, String outputPath)
        {
            if (encoder is JpegBitmapEncoder || encoder is BmpBitmapEncoder)
            {
                SaveBitmapToFile(dictionary.GetMinContainBitmap(System.Windows.Media.Brushes.White), encoder, outputPath);
            }
            else
            {
                SaveBitmapToFile(dictionary.GetMinContainBitmap(), encoder, outputPath);
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

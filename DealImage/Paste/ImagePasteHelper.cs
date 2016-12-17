using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DealImage.Paste
{
    public static class ImagePasteHelper
    {
        public static bool CanInternalPasteFromClipboard()
        {
            var dataObject = Clipboard.GetDataObject();
            return dataObject != null && dataObject.GetFormats().Contains(ImageDataFormats.Internal);
        }

        public static object GetInternalPasteDataFromClipboard()
        {
            var bytes = Clipboard.GetData(ImageDataFormats.Internal) as byte[];
            if (bytes == null) return null;
            object result = null;
            try
            {
                var stream = new MemoryStream(bytes) {Position = 0};
                result = new BinaryFormatter().Deserialize(stream);
                stream.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            return result;

        }

        public static bool CanPasteImageFromClipboard()
        {
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null) return false;
            var dataFormats = dataObject.GetFormats();
            if (ImageDataFormats.GetFormats().Any(item => dataFormats.Contains(item)))
            {
                return true;
            }
            if (Clipboard.ContainsFileDropList())
            {
                return
                    Clipboard.GetFileDropList()
                        .Cast<string>()
                        .Any(item => IsValidFileExtension(ImageHelper.GetFileExtension(item)));
            }
            return false;
        }

        public static bool CanExchangeImageFromClip()
        {
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null) return false;
            var dataFormats = dataObject.GetFormats();
            if (ImageDataFormats.GetFormats().Any(item => dataFormats.Contains(item)))
            {
                return true;
            }

            if (Clipboard.ContainsFileDropList())
            {
                var filePaths = Clipboard.GetFileDropList();
                if (filePaths.Count != 1) return false;
                if (IsValidFileExtension(ImageHelper.GetFileExtension(filePaths[0])))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<ImageSource> GetPasteImagesFromClipboard()
        {
            var imageSources = new List<ImageSource>();
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null) return imageSources;

            var dataFormats = dataObject.GetFormats();
            foreach (var item in ImageDataFormats.GetFormats())
            {
                if (!dataFormats.Contains(item)) continue;
                var data = dataObject.GetData(item);
                var imageSource = new BitmapImage();
                var stream = data as MemoryStream;
                if (stream != null)
                {
                    try
                    {
                        stream.Position = 0;
                        imageSource.BeginInit();
                        imageSource.StreamSource = stream;
                        imageSource.EndInit();
                        imageSources.Add(imageSource);
                        return imageSources;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }

                var bitmapSource = data as BitmapSource;
                if (bitmapSource != null)
                {
                    try
                    {
                        var ms = new MemoryStream();
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(ms);
                        imageSource.BeginInit();
                        imageSource.StreamSource = ms;
                        imageSource.EndInit();
                        imageSources.Add(imageSource);
                        return imageSources;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                   
                }

                var bitmap = data as Bitmap;
                if (bitmap != null)
                {
                    try
                    {
                        var ms = new MemoryStream();
                        bitmap.Save(ms, ImageFormat.Png);
                        imageSource.BeginInit();
                        imageSource.StreamSource = ms;
                        imageSource.EndInit();
                        imageSources.Add(imageSource);
                        return imageSources;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
            }
            
            if (Clipboard.ContainsFileDropList())
            {
                foreach (var filePath in Clipboard.GetFileDropList())
                {
                    if (!IsValidFileExtension(ImageHelper.GetFileExtension(filePath))) continue;
                    try
                    {
                        var stream = new MemoryStream();
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(stream);
                        }
                        stream.Position = 0;
                        var imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = stream;
                        imageSource.EndInit();
                        imageSources.Add(imageSource);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
            }

            //兼容
            if (System.Windows.Forms.Clipboard.ContainsImage())
            {
                var image = System.Windows.Forms.Clipboard.GetImage();
                if (image != null)
                {
                    try
                    {
                        var imageSource = new BitmapImage();
                        var ms = new MemoryStream();
                        image.Save(ms, ImageFormat.Png);
                        imageSource.BeginInit();
                        imageSource.StreamSource = ms;
                        imageSource.EndInit();
                        imageSources.Add(imageSource);
                        return imageSources;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
            }

            return imageSources;
        }

        public static bool IsValidFileExtension(FileExtension extension)
        {
            switch (extension)
            {
                case FileExtension.Ico:
                case FileExtension.Jpg:
                case FileExtension.Gif:
                case FileExtension.Bmp:
                case FileExtension.Png:
                case FileExtension.Tif:
                    //case FileExtension.Wmf:
                    //case FileExtension.Emf:
                    return true;
            }
            return false;
        }

    }

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

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
                using (var stream = new MemoryStream(bytes) {Position = 0})
                {
                    result = new BinaryFormatter().Deserialize(stream);
                }
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
            if (ImageDataFormats.GetPasteFormats().Any(item => dataFormats.Contains(item)))
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
            if (ImageDataFormats.GetPasteFormats().Any(item => dataFormats.Contains(item)))
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

            if (dataFormats.Contains(ImageDataFormats.Png))
            {
                var stream = dataObject.GetData(ImageDataFormats.Png) as MemoryStream;
                if (stream != null)
                {
                    try
                    {
                        stream.Position = 0;
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        imageSources.Add(bitmapImage);
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
                var isSuccess = false;
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
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        imageSources.Add(bitmapImage);
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
                if (isSuccess)
                {
                    return imageSources;
                }
            }

            if (dataFormats.Contains(ImageDataFormats.Html))
            {
                var html = dataObject.GetData(ImageDataFormats.Html)?.ToString();
                if (html != null)
                {
                    var reg = new Regex(@"(http|ftp|https)://(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,4})*(/[\w- \./?%&~=]*)\.(png|jpg|gif)", RegexOptions.IgnoreCase);//正则表达式的类实例化
                    var mc = reg.Matches(html);
                    if (mc.Count > 0)
                    {
                        try
                        {
                            var tempFilePath = Path.GetTempFileName();
                            using (var webClient = new WebClient())
                            {
                                webClient.DownloadFile(mc[0].Value, tempFilePath);
                            }
                            var stream = new MemoryStream();
                            using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                            {
                                fileStream.CopyTo(stream);
                            }
                            stream.Position = 0;
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = stream;
                            bitmapImage.EndInit();
                            imageSources.Add(bitmapImage);
                            return imageSources;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.ToString());
                        }
                    }
                }
            }

            if (dataFormats.Contains(ImageDataFormats.Bitmap))
            {
                var bitmapSource = dataObject.GetData(ImageDataFormats.Bitmap) as BitmapSource;
                if (bitmapSource != null)
                {
                    try
                    {
                        var encoder = new BmpBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        var ms = new MemoryStream();
                        encoder.Save(ms);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        imageSources.Add(bitmapImage);
                        return imageSources;
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
                        var ms = new MemoryStream();
                        image.Save(ms, ImageFormat.Png);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        imageSources.Add(bitmapImage);
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

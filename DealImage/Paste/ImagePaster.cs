using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DealImage.Paste
{
    public static class ImagePaster
    {

        private const string RegexImgLable = @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>";

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
            if (dataFormats.Contains(ImageDataFormats.FileDrop))
            {
                var filePaths = dataObject.GetData(ImageDataFormats.FileDrop) as string[];
                if (filePaths != null)
                {
                    return filePaths.Any(item => ImageHelper.IsValidFileExtension(ImageHelper.GetImageExtension(item)));
                }
            }
            if (dataFormats.Contains(ImageDataFormats.Html))
            {
                var html = dataObject.GetData(ImageDataFormats.Html)?.ToString();
                if (html != null)
                {
                    html = HttpUtility.HtmlDecode(html);
                    var reg = new Regex(RegexImgLable, RegexOptions.IgnoreCase);//正则表达式的类实例化
                    var mc = reg.Matches(html);
                    if (mc.Count > 0)
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        public static List<ImageSource> GetPasteImagesFromClipboard()
        {

            var imageSources = GetImageFromIDataObject(Clipboard.GetDataObject());
            if (imageSources.Count > 0) return imageSources;

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

        public static List<ImageSource> GetImageFromIDataObject(IDataObject dataObject)
        {
            var imageSources = new List<ImageSource>();

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

            if (dataFormats.Contains(ImageDataFormats.FileDrop))
            {
                var isSuccess = false;
                var filePaths = dataObject.GetData(ImageDataFormats.FileDrop) as string[];
                if (filePaths != null)
                {
                    foreach (var filePath in filePaths)
                    {
                        if (!ImageHelper.IsValidFileExtension(ImageHelper.GetImageExtension(filePath))) continue;
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
                    
                    html = HttpUtility.HtmlDecode(html);
                    var reg = new Regex(RegexImgLable, RegexOptions.IgnoreCase);//正则表达式的类实例化
                    var mc = reg.Matches(html);
                    if (mc.Count > 0)
                    {
                        try
                        {
                            MemoryStream stream;
                            var source = mc[0].Groups["imgUrl"].Value;
                            if (source.StartsWith("data:image"))
                            {
                                var imageBytes = Convert.FromBase64String(source.Substring(source.IndexOf(',') + 1));
                                stream = new MemoryStream(imageBytes);
                            }
                            else
                            {
                                var request = (HttpWebRequest)WebRequest.Create(source);
                                //request.AllowAutoRedirect = false;
                                request.Method = "GET";
                                request.Timeout = 1000 * 5;
                                request.ReadWriteTimeout = 1000 * 5;
                                request.ContentType = "application/x-www-form-urlencoded";
                                using (var response = (HttpWebResponse)request.GetResponse())
                                {
                                    using (var reader = response.GetResponseStream())
                                    {
                                        stream = new MemoryStream();
                                        reader?.CopyTo(stream);
                                    }
                                }
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

            return imageSources;
        }

    }

}

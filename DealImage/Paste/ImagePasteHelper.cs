using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace DealImage.Paste
{
    public static class ImagePasteHelper
    {
        public static bool CanInnerPasteFromClipboard()
        {
            return Clipboard.ContainsData("[EASYIMAGE]");
        }

        public static bool CanPasteImageFromClipboard()
        {
            if (Clipboard.ContainsData("PNG"))
            {
                return true;
            }
            if (System.Windows.Forms.Clipboard.ContainsImage())
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
            if (Clipboard.ContainsData("PNG"))
            {
                return true;
            }
            if (System.Windows.Forms.Clipboard.ContainsImage())
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

        public static ImageSource GetExchangeImageFromClip()
        {
            if (Clipboard.ContainsData("PNG"))
            {
                var stream = Clipboard.GetData("PNG") as MemoryStream;
                if (stream != null)
                {
                    try
                    {
                        var imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = stream;
                        imageSource.EndInit();
                        return imageSource;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }

                }
            }
            if (System.Windows.Forms.Clipboard.ContainsImage())
            {
                var image = System.Windows.Forms.Clipboard.GetImage();
                if (image != null)
                {
                    try
                    {
                        var stream = new MemoryStream();
                        image.Save(stream, ImageFormat.Bmp);
                        var imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = stream;
                        imageSource.EndInit();
                        return imageSource;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }

                }
            }
            if (Clipboard.ContainsFileDropList())
            {
                var filePaths = Clipboard.GetFileDropList();
                if (filePaths.Count != 1) return null;
                if (IsValidFileExtension(ImageHelper.GetFileExtension(filePaths[0])))
                {
                    try
                    {
                        return new BitmapImage(new Uri(filePaths[0]));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }

                }
            }
            return null;
        }

        public static object GetInnerPasteDataFromClipboard()
        {
            var bytes = Clipboard.GetData("[EASYIMAGE]") as byte[];
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

        public static IEnumerable<ImageSource> GetPasteImagesFromClipboard()
        {
            var imageSourceList = new List<ImageSource>();
            if (Clipboard.ContainsData("PNG"))
            {
                var stream = Clipboard.GetData("PNG") as MemoryStream;
                if (stream != null)
                {
                    try
                    {
                        var imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = stream;
                        imageSource.EndInit();
                        imageSourceList.Add(imageSource);
                        return imageSourceList;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
            }
            if (System.Windows.Forms.Clipboard.ContainsImage())
            {
                var image = System.Windows.Forms.Clipboard.GetImage();
                if (image != null)
                {
                    try
                    {
                        var stream = new MemoryStream();
                        image.Save(stream, ImageFormat.Bmp);
                        var imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = stream;
                        imageSource.EndInit();
                        imageSourceList.Add(imageSource);
                        return imageSourceList;
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
                    try
                    {
                        if (!IsValidFileExtension(ImageHelper.GetFileExtension(filePath))) continue;
                        var stream = new MemoryStream();
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(stream);
                        }
                        var imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = stream;
                        imageSource.EndInit();
                        imageSourceList.Add(imageSource);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }

                }
            }
            return imageSourceList;
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DealImage.Paste
{
    public static class ImagePasteHelper
    {
        public static bool CanPasteImageFromClipboard()
        {
            if (Clipboard.ContainsData("PNG"))
            {
                if (Clipboard.GetData("PNG") is MemoryStream)
                {
                    return true;
                }
            }
            if (Clipboard.ContainsImage())
            {
                return true;
            }
            if (Clipboard.ContainsFileDropList())
            {
                foreach (var item in Clipboard.GetFileDropList())
                {
                    switch (GetFileExtension(item))
                    {
                        case FileExtension.Ico:
                        case FileExtension.Jpg:
                        case FileExtension.Gif:
                        case FileExtension.Bmp:
                        case FileExtension.Png:
                            return true;
                    }
                }
            }
            return false;
        }

        public static IEnumerable<ImageSource> PasteImageFromClipboard()
        {
            var imageSourceList = new List<ImageSource>();
            if (Clipboard.ContainsData("PNG"))
            {
                var stream = Clipboard.GetData("PNG") as MemoryStream;
                if (stream != null)
                {
                    var imageSource = new BitmapImage();
                    imageSource.BeginInit();
                    imageSource.StreamSource = stream;
                    imageSource.EndInit();
                    imageSourceList.Add(imageSource);
                    return imageSourceList;
                }
            }
            if (System.Windows.Forms.Clipboard.ContainsImage())
            {
                var image = System.Windows.Forms.Clipboard.GetImage();
                if (image != null)
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
               
            }
            if (Clipboard.ContainsFileDropList())
            {
                foreach (var filePath in Clipboard.GetFileDropList())
                {
                    switch (GetFileExtension(filePath))
                    {
                        case FileExtension.Ico:
                        case FileExtension.Jpg:
                        case FileExtension.Gif:
                        case FileExtension.Bmp:
                        case FileExtension.Png:
                            imageSourceList.Add(new BitmapImage(new Uri(filePath)));
                            break;
                    }
                }
            }
            return imageSourceList;
        }

        public static FileExtension GetFileExtension(string fileName)
        {
            var extension = FileExtension.Unknow;
            if (!File.Exists(fileName)) return extension;
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fs);
            var fileType = string.Empty;
            try
            {
                var data = br.ReadByte();
                fileType += data.ToString();
                data = br.ReadByte();
                fileType += data.ToString();

                try
                {
                    extension = (FileExtension)Enum.Parse(typeof(FileExtension), fileType);
                }
                catch
                {
                    extension = FileExtension.Unknow;
                }
            }
            catch
            {
                //Trace.WriteLine(ex.Message);
            }
            finally
            {
                fs.Close();
                br.Close();
            }
            return extension;
        }

    }
}

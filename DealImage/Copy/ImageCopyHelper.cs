using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DealImage.Copy
{

    public static class ImageCopyHelper
    {
        public static void CopyChildElementsToClipBoard(this IDictionary<FrameworkElement, FrameworkElement> dictionary, object attach = null)
        {
            using (var stream = new MemoryStream())
            {
                var dataObject = new DataObject();
                //內部图片格式
                if (attach != null)
                {
                    new BinaryFormatter().Serialize(stream, attach);
                    stream.Position = 0;
                    dataObject.SetData(ImageDataFormats.Internal, stream.ToArray(), true);
                }
                
                //普通图片格式
                dataObject.SetData(ImageDataFormats.Bitmap, dictionary.GetMinContainBitmap(System.Windows.Media.Brushes.White), true);

                //兼容PNG透明格式图片
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(dictionary.GetMinContainBitmap()));
                stream.Position = 0;
                encoder.Save(stream);
                dataObject.SetData(ImageDataFormats.Png, stream, true);

                //兼容QQ
                var tempFilePath = Path.GetTempFileName();
                using (var fs = File.OpenWrite(tempFilePath))
                {
                    var data = stream.ToArray();
                    fs.Write(data, 0, data.Length);
                }
                var byteData = Encoding.UTF8.GetBytes("<QQRichEditFormat><Info version=\"1001\"></Info><EditElement type=\"1\" filepath=\"" + tempFilePath + "\" shortcut=\"\"></EditElement><EditElement type=\"0\"><![CDATA[]]></EditElement></QQRichEditFormat>");
                dataObject.SetData(ImageDataFormats.QqUnicodeRichEditFormat, new MemoryStream(byteData), true);
                dataObject.SetData(ImageDataFormats.QqRichEditFormat, new MemoryStream(byteData), true);
                dataObject.SetData(ImageDataFormats.FileDrop, new[] { tempFilePath }, true);
                dataObject.SetData(ImageDataFormats.FileNameW, new[] { tempFilePath }, true);
                dataObject.SetData(ImageDataFormats.FileName, new[] { tempFilePath }, true);

                
                
                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject, true);
            }
        }
    }
}

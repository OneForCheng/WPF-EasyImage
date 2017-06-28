using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DealImage.Copy
{

    public static class ImageCopyer
    {
       
        public static async void CopyToClipBoard(this BitmapImage bitmapImage)
        {
            using (var stream = new MemoryStream())
            {
                bitmapImage.StreamSource.Position = 0;
                await bitmapImage.StreamSource.CopyToAsync(stream);

                var dataObject = new DataObject();

                //普通图片格式
                var width = (int)bitmapImage.Width;
                var height = (int)bitmapImage.Height;
                var rect = new Rect(0, 0, width, height);
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    context.DrawRectangle(Brushes.White, null, rect);
                    context.DrawImage(bitmapImage,rect);
                }
                var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawingVisual);
                dataObject.SetData(ImageDataFormats.Bitmap, renderBitmap, true);

                //兼容PNG透明格式图片
                dataObject.SetData(ImageDataFormats.Png, stream, true);

                //兼容QQ
                var tempFilePath = Path.GetTempFileName();
                var fileExt = ImageHelper.GetImageExtension(stream);
                var ext = ".png";
                switch (fileExt)
                {
                    case ImageExtension.Gif:
                        ext = ".gif";
                        break;
                    case ImageExtension.Jpg:
                        ext = ".jpg";
                        break;
                    case ImageExtension.Tif:
                        ext = ".tif";
                        break;
                    case ImageExtension.Bmp:
                        ext = ".bmp";
                        break;
                }
               
                tempFilePath = tempFilePath.Substring(0, tempFilePath.Length - 4) + ext;
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

        public static async void CopyToClipBoard(this BitmapImage bitmapImage, object attach)
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

                stream.Position = 0;
                bitmapImage.StreamSource.Position = 0;
                await bitmapImage.StreamSource.CopyToAsync(stream);

                //普通图片格式
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
                dataObject.SetData(ImageDataFormats.Bitmap, renderBitmap, true);

                //兼容PNG透明格式图片
                dataObject.SetData(ImageDataFormats.Png, stream, true);

                //兼容QQ
                var tempFilePath = Path.GetTempFileName();
                var fileExt = ImageHelper.GetImageExtension(stream);
                var ext = ".png";
                switch (fileExt)
                {
                    case ImageExtension.Gif:
                        ext = ".gif";
                        break;
                    case ImageExtension.Jpg:
                        ext = ".jpg";
                        break;
                    case ImageExtension.Tif:
                        ext = ".tif";
                        break;
                    case ImageExtension.Bmp:
                        ext = ".bmp";
                        break;
                }
                tempFilePath = tempFilePath.Substring(0, tempFilePath.Length - 4) + ext;
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

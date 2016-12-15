using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
                if (attach != null)
                {
                    new BinaryFormatter().Serialize(stream, attach);
                    stream.Position = 0;
                    dataObject.SetData(ImageDataFormats.Internal, stream.ToArray(), true);
                }
                dataObject.SetData(DataFormats.Bitmap, dictionary.GetRenderTargetBitmap(System.Windows.Media.Brushes.White), true);
                
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(dictionary.GetRenderTargetBitmap()));
                stream.Position = 0;
                encoder.Save(stream);
                dataObject.SetData(ImageDataFormats.Png, stream, true);
                
                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject, true);
            }
        }
    }
}

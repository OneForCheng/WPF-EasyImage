using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

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
                    dataObject.SetData("[EASYIMAGE]", stream.ToArray(), true);
                }
                dataObject.SetData(DataFormats.Bitmap, dictionary.GetRenderTargetBitmap(System.Windows.Media.Brushes.White), true);
                
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(dictionary.GetRenderTargetBitmap()));
                stream.Position = 0;
                encoder.Save(stream);
                dataObject.SetData("PNG", stream, true);
                
                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject, true);
            }
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DealImage.Copy
{
    public static class ImageCopyHelper
    {
        public static void CopyChildElementsToClipBoard(this IDictionary<FrameworkElement, FrameworkElement> dictionary)
        {
            using (var stream = new MemoryStream())
            {
                var dataObject = new DataObject();

                dataObject.SetData(DataFormats.Bitmap, dictionary.GetRenderTargetBitmap(System.Windows.Media.Brushes.White), true);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(dictionary.GetRenderTargetBitmap()));
                encoder.Save(stream);
                dataObject.SetData("PNG", stream, true);

                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject, true);
            }
        }
        
    }
}

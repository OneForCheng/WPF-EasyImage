using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace IconMaker
{
    public enum IconSize
    {
        Bmp16,
        Bmp24,
        Bmp32,
        Bmp48,
        Bmp64,
        Bmp72,
        Bmp96,
        Bmp128,
        Bmp256,
    }

    public static class IconSaveHelper
    {
        public static void SaveIcon(this BitmapSource source, Stream stream, IconSize iconSize = IconSize.Bmp32)
        {
            var encoder = new IconBitmapEncoder();
           
            switch (iconSize)
            {
                case IconSize.Bmp16:
                    var bmp16 = IconBitmapEncoder.GetResized(source, 16);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp16)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp16)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp16)));
                    break;
                case IconSize.Bmp24:
                    var bmp24 = IconBitmapEncoder.GetResized(source, 24);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp24)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp24)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp24)));
                    break;
                case IconSize.Bmp32:
                    var bmp32 = IconBitmapEncoder.GetResized(source, 32);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp32)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp32)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp32)));
                    break;
                case IconSize.Bmp48:
                    var bmp48 = IconBitmapEncoder.GetResized(source, 48);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp48)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp48)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp48)));
                    break;
                case IconSize.Bmp64:
                    var bmp64 = IconBitmapEncoder.GetResized(source, 64);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp64)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp64)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp64)));
                    break;
                case IconSize.Bmp72:
                    var bmp72 = IconBitmapEncoder.GetResized(source, 72);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp72)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp72)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp72)));
                    break;
                case IconSize.Bmp96:
                    var bmp96 = IconBitmapEncoder.GetResized(source, 96);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp96)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp96)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp96)));
                    break;
                case IconSize.Bmp128:
                    var bmp128 = IconBitmapEncoder.GetResized(source, 128);
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24Plus8BitImage(bmp128)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get4BitImage(bmp128)));
                    encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp128)));
                    break;
                case IconSize.Bmp256:
                    var bmp256 = IconBitmapEncoder.GetResized(source, 256);
                    encoder.Frames.Add(BitmapFrame.Create(bmp256));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(iconSize), iconSize, null);
            }
            encoder.Save(stream);
        }

        public static void SaveIcon(this BitmapSource source, string fileName, IconSize iconSize = IconSize.Bmp32)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                SaveIcon(source, fs, iconSize);
            }
        }

    }
}

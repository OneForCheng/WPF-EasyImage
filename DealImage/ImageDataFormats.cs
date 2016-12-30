﻿namespace DealImage
{
    public static class ImageDataFormats
    {
        public static readonly string Internal = "EasyImage Internal Theme";
        public static readonly string Png = "PNG";
        public static readonly string Dib = "DeviceIndependentBitmap";
        public static readonly string Bitmap = "Bitmap";
        public static readonly string Html = "HTML Format";
        public static readonly string QqUnicodeRichEditFormat = "QQ_Unicode_RichEdit_Format";
        public static readonly string QqRichEditFormat = "QQ_RichEdit_Format";
        public static readonly string FileDrop = "FileDrop";
        public static readonly string FileNameW = "FileNameW";
        public static readonly string FileName = "FileName";

        public static string[] GetPasteFormats()
        {
            return new[] {Png, Bitmap, Dib};
        }

    }
}
namespace DealImage
{
    public static class ImageDataFormats
    {
        public static readonly string Internal = "EasyImage Internal Theme";
        public static readonly string Png = "PNG";
        public static readonly string Jfif = "JFIF";
        public static readonly string Jif = "JIF";
        public static readonly string Bitmap = "Bitmap";
        public static readonly string Dib = "DeviceIndependentBitmap";
        
        public static string[] GetFormats()
        {
            return new[] { Png, Jif, Jfif, Bitmap, Dib};
        }

    }
}
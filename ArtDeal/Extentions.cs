using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ArtDeal
{
    public static class Extentions
    {
        public static Bitmap ResizeBitmap(this Bitmap bitmap, int width, int height)
        {
            var resizeBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var bmpGraphics = Graphics.FromImage(resizeBitmap))
            {
                bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;
                bmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                bmpGraphics.CompositingQuality = CompositingQuality.GammaCorrected;
                bmpGraphics.DrawImage(bitmap, 0, 0, width, height);
            }
            return resizeBitmap;
        }

    }
}

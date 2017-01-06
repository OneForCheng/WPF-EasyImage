using System.Drawing;
using System.Drawing.Imaging;

namespace DealImage.DealColor
{
    public static class ImageColorHelper
    {
        public static Bitmap ReplaceColor(this Bitmap bitmap, Color sourceColor, Color targetColor)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var outputBitmap = new Bitmap(width, height);

            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var outData = outputBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            const int pixelSize = 4;
            unsafe
            {
                var output = (byte*)outData.Scan0;
                var offset = outData.Stride - width * 4;
                for (var y = 0; y < height; y++)
                {
                    //每一行内存中的位置  
                    var row = (byte*)data.Scan0 + (y * data.Stride);
                    for (var x = 0; x < width; x++)
                    {
                        if (row[x * pixelSize + 3] == sourceColor.A && row[x * pixelSize] == sourceColor.R && row[x * pixelSize + 1] == sourceColor.G && row[x * pixelSize + 2] == sourceColor.B)
                        {
                            output[0] = targetColor.R;
                            output[1] = targetColor.G;
                            output[2] = targetColor.B;
                            output[3] = targetColor.A;
                        }
                        output += 4;
                    }
                    output += offset;
                }
            }
            bitmap.UnlockBits(data);
            outputBitmap.UnlockBits(outData);

            return outputBitmap;
        }

    }
}

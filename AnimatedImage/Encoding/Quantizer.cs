using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AnimatedImage.Encoding
{
    public abstract class Quantizer
    {
        private readonly bool _singlePass;

        protected Quantizer(bool singlePass)
        {
            _singlePass = singlePass;
        }

        protected virtual unsafe void FirstPass(BitmapData sourceData, int width, int height)
        {
            var numPtr = (byte*)sourceData.Scan0.ToPointer();
            for (var i = 0; i < height; i++)
            {
                var numPtr2 = (int*)numPtr;
                var num2 = 0;
                while (num2 < width)
                {
                    InitialQuantizePixel((Color32*)numPtr2);
                    num2++;
                    numPtr2++;
                }
                numPtr += sourceData.Stride;
            }
        }

        protected abstract ColorPalette GetPalette(ColorPalette original);
        protected virtual unsafe void InitialQuantizePixel(Color32* pixel)
        {
        }

        public Bitmap Quantize(Image source)
        {
            var height = source.Height;
            var width = source.Width;
            var rect = new Rectangle(0, 0, width, height);
            var image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PageUnit = GraphicsUnit.Pixel;
                graphics.DrawImageUnscaled(source, rect);
            }
            BitmapData sourceData = null;
            try
            {
                sourceData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                if (!_singlePass)
                {
                    FirstPass(sourceData, width, height);
                }
                output.Palette = GetPalette(output.Palette);
                SecondPass(sourceData, output, width, height, rect);
            }
            finally
            {
                image.UnlockBits(sourceData);
            }
            return output;
        }

     
        protected abstract unsafe byte QuantizePixel(Color32* pixel);
        protected virtual unsafe void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
        {
            BitmapData bitmapdata = null;
            try
            {
                bitmapdata = output.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                var numPtr = (byte*)sourceData.Scan0.ToPointer();
                var numPtr2 = (int*)numPtr;
                var numPtr3 = numPtr2;
                var numPtr4 = (byte*)bitmapdata.Scan0.ToPointer();
                var numPtr5 = numPtr4;
                var num = QuantizePixel((Color32*)numPtr2);
                numPtr5[0] = num;
                for (var i = 0; i < height; i++)
                {
                    numPtr2 = (int*)numPtr;
                    numPtr5 = numPtr4;
                    var num3 = 0;
                    while (num3 < width)
                    {
                        if (numPtr3[0] != numPtr2[0])
                        {
                            num = QuantizePixel((Color32*)numPtr2);
                            numPtr3 = numPtr2;
                        }
                        numPtr5[0] = num;
                        num3++;
                        numPtr2++;
                        numPtr5++;
                    }
                    numPtr += sourceData.Stride;
                    numPtr4 += bitmapdata.Stride;
                }
            }
            finally
            {
                output.UnlockBits(bitmapdata);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Color32
        {
            [FieldOffset(3)]
            public byte Alpha;
            [FieldOffset(0)]
            public int ARGB;
            [FieldOffset(0)]
            public byte Blue;
            [FieldOffset(1)]
            public byte Green;
            [FieldOffset(2)]
            public byte Red;

            public Color Color => Color.FromArgb(Alpha, Red, Green, Blue);
        }
    }
}

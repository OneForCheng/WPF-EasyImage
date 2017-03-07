using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace AnimatedImage.Encoding
{
    public class PaletteQuantizer : Quantizer
    {
        private readonly Hashtable _colorMap;
        private readonly Color[] _colors;

        public PaletteQuantizer(ArrayList palette) : base(true)
        {
            _colorMap = new Hashtable();
            _colors = new Color[palette.Count];
            palette.CopyTo(_colors);
        }

        protected override ColorPalette GetPalette(ColorPalette palette)
        {
            for (var i = 0; i < _colors.Length; i++)
            {
                palette.Entries[i] = _colors[i];
            }
            return palette;
        }

        protected override unsafe byte QuantizePixel(Color32* pixel)
        {
            int num3;
            byte num = 0;
            var aRgb = pixel->ARGB;
            if (_colorMap.ContainsKey(aRgb))
            {
                return (byte)_colorMap[aRgb];
            }
            if (0 == pixel->Alpha)
            {
                for (num3 = 0; num3 < _colors.Length; num3++)
                {
                    if (0 == _colors[num3].A)
                    {
                        num = (byte)num3;
                        break;
                    }
                }
            }
            else
            {
                var num4 = 0x7fffffff;
                int red = pixel->Red;
                int green = pixel->Green;
                int blue = pixel->Blue;
                for (num3 = 0; num3 < _colors.Length; num3++)
                {
                    var color = _colors[num3];
                    var num8 = color.R - red;
                    var num9 = color.G - green;
                    var num10 = color.B - blue;
                    var num11 = ((num8 * num8) + (num9 * num9)) + (num10 * num10);
                    if (num11 < num4)
                    {
                        num = (byte)num3;
                        num4 = num11;
                        if (0 == num11)
                        {
                            break;
                        }
                    }
                }
            }
            _colorMap.Add(aRgb, num);
            return num;
        }
    }
}

using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace InvertColor
{
    public class PluginInvert : IHandle
    {
        public string GetPluginName()
        {
            return "反色处理";
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public void InitPlugin(string appStartupPath)
        {
            
        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var byColorInfo = new byte[bitmap.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
            for (int x = 0, width = bitmap.Width; x < width; x++)
            {
                for (int y = 0, height = bitmap.Height; y < height; y++)
                {
                    var byB = byColorInfo[y * bmpData.Stride + x * 3];
                    var byG = byColorInfo[y * bmpData.Stride + x * 3 + 1];
                    var byR = byColorInfo[y * bmpData.Stride + x * 3 + 2];
                    byColorInfo[y * bmpData.Stride + x * 3] = (byte)(255 - byB);
                    byColorInfo[y * bmpData.Stride + x * 3 + 1] = (byte)(255 - byG);
                    byColorInfo[y * bmpData.Stride + x * 3 + 2] = (byte)(255 - byR);
                }
            }
            Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);
            bitmap.UnlockBits(bmpData);
            return new HandleResult(bitmap, true);
        }
    }
}

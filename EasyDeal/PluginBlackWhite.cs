using System;
using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace EasyDeal
{
    public class PluginBlackWhite : IHandle
    {
        public string GetPluginName()
        {
            return "黑白处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.GrayIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            try
            {
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var byColorInfo = new byte[bitmap.Height * bmpData.Stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
                for (int x = 0, width = bitmap.Width; x < width; x++)
                {
                    for (int y = 0, height = bitmap.Height; y < height; y++)
                    {
                        var index = y * bmpData.Stride + x * 4;
                        var byB = byColorInfo[index];
                        var byG = byColorInfo[index + 1];
                        var byR = byColorInfo[index + 2];
                        byColorInfo[index] =
                        byColorInfo[index + 1] =
                        byColorInfo[index + 2] = (byte)((byB + byG + byR) / 3);
                    }
                }
                Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);
                bitmap.UnlockBits(bmpData);
                return new HandleResult(bitmap, true);
            }
            catch (Exception e)
            {
                return  new HandleResult(e);
            }
            
        }
    }
}

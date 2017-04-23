using System;
using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;


namespace EasyDeal
{
    public class PluginBlackWhite : IFilter
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
                var width = bitmap.Width;
                var height = bitmap.Height;
                const int pixelSize = 4;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                #region Safe

                //var byColorInfo = new byte[bitmap.Height * bmpData.Stride];
                //Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
                //for (var x = 0; x < width; x++)
                //{
                //    for (var y = 0; y < height; y++)
                //    {
                //        var index = y * bmpData.Stride + x * pixelSize;
                //        var byB = byColorInfo[index];
                //        var byG = byColorInfo[index + 1];
                //        var byR = byColorInfo[index + 2];
                //        byColorInfo[index] =
                //        byColorInfo[index + 1] =
                //        byColorInfo[index + 2] = (byte)((byB + byG + byR) / 3);
                //    }
                //}
                //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);


                #endregion

                #region Unsafe

                unsafe
                {
                    var ptr = (byte*)(bmpData.Scan0);
                    var n = height*width;
                    for (var i = 0; i < n; i++)
                    {
                        ptr[0] = ptr[1] = ptr[2] = (byte)((ptr[0] + ptr[1] + ptr[2]) / 3);
                        ptr += pixelSize;
                    }
                }

                #endregion

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

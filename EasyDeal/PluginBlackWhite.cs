using System;
using System.Collections.Generic;
using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;


namespace EasyDeal
{
    public class PluginBlackWhite : IMultiFilter
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

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            try
            {
                var resultBitmaps = new List<Bitmap>();
                foreach (var bitmap in bitmaps)
                {
                    var width = bitmap.Width;
                    var height = bitmap.Height;
                    const int pixelSize = 4;
                    var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    #region Unsafe

                    unsafe
                    {
                        var ptr = (byte*)(bmpData.Scan0);
                        var n = height * width;
                        for (var i = 0; i < n; i++)
                        {
                            ptr[0] = ptr[1] = ptr[2] = (byte)((ptr[0] + ptr[1] + ptr[2]) / 3);
                            ptr += pixelSize;
                        }
                    }

                    #endregion

                    bitmap.UnlockBits(bmpData);
                    resultBitmaps.Add((Bitmap)bitmap.Clone());
                }
                
                return new HandleResult(resultBitmaps, true);
            }
            catch (Exception e)
            {
                return  new HandleResult(e);
            }
            
        }
    }
}

using System;
using System.Collections.Generic;
using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;

namespace EasyDeal
{
    public class PluginInvertColor : IMultiFilter
    {

        public string GetPluginName()
        {
            return "反色处理[GIF]";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.InvertIcon;
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
                    //        byColorInfo[index] = (byte)(255 - byB);
                    //        byColorInfo[index + 1] = (byte)(255 - byG);
                    //        byColorInfo[index + 2] = (byte)(255 - byR);
                    //    }
                    //}
                    //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);


                    #endregion

                    #region Unsafe

                    unsafe
                    {
                        var ptr = (byte*)(bmpData.Scan0);
                        //for (var y = 0; y < height; y++)
                        //{
                        //    for (var x = 0; x < width; x++)
                        //    {
                        //        ptr[0] = (byte)(255 - ptr[0]);
                        //        ptr[1] = (byte)(255 - ptr[1]);
                        //        ptr[2] = (byte)(255 - ptr[2]);
                        //        ptr += pixelSize;
                        //    }
                        //}
                        var n = height * width;
                        for (var i = 0; i < n; i++)
                        {
                            ptr[0] = (byte)(255 - ptr[0]);
                            ptr[1] = (byte)(255 - ptr[1]);
                            ptr[2] = (byte)(255 - ptr[2]);
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
                return new HandleResult(e);
            }
            
        }
    }
}

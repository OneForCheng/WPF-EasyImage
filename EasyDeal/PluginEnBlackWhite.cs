using System;
using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;

namespace EasyDeal
{
    public class PluginEnBlackWhite : IHandle
    {
        public string GetPluginName()
        {
            return "黑白(增强)处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.EngrayIcon;
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
                //        //增强对比度
                //        var byB = (Math.Abs(byColorInfo[index] + byColorInfo[index] - byColorInfo[index + 1] + byColorInfo[index + 2]) * byColorInfo[index + 1]) >> 8;
                //        var byG = (Math.Abs(byColorInfo[index] + byColorInfo[index] - byColorInfo[index + 1] + byColorInfo[index + 2]) * byColorInfo[index + 2]) >> 8;
                //        var byR = (Math.Abs(byColorInfo[index + 1] + byColorInfo[index + 1] - byColorInfo[index] + byColorInfo[index + 2]) * byColorInfo[index + 2]) >> 8;
                //        if(byB < 0)
                //        {
                //            byB = 0;
                //        }
                //        else if(byB > 255)
                //        {
                //            byB = 255;
                //        }
                //        if (byG < 0)
                //        {
                //            byG = 0;
                //        }
                //        else if (byG > 255)
                //        {
                //            byG = 255;
                //        }
                //        if (byR < 0)
                //        {
                //            byR = 0;
                //        }
                //        else if (byR > 255)
                //        {
                //            byR = 255;
                //        }
                //        var gray = (byB + byG + byR) / 3;//计算灰度
                //        byR = gray + 10;
                //        if(byR > 255)
                //        {
                //            byR = 255;
                //        }
                //        byColorInfo[index] = (byte)gray;
                //        byColorInfo[index + 1] =
                //        byColorInfo[index + 2] = (byte)byR;
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
                    //        var byB = (Math.Abs(ptr[0] + ptr[0] - ptr[1] + ptr[2]) * ptr[1]) >> 8;
                    //        var byG = (Math.Abs(ptr[0] + ptr[0] - ptr[1] + ptr[2]) * ptr[2]) >> 8;
                    //        var byR = (Math.Abs(ptr[1] + ptr[1] - ptr[0] + ptr[2]) * ptr[2]) >> 8;
                    //        if (byB < 0)
                    //        {
                    //            byB = 0;
                    //        }
                    //        else if (byB > 255)
                    //        {
                    //            byB = 255;
                    //        }
                    //        if (byG < 0)
                    //        {
                    //            byG = 0;
                    //        }
                    //        else if (byG > 255)
                    //        {
                    //            byG = 255;
                    //        }
                    //        if (byR < 0)
                    //        {
                    //            byR = 0;
                    //        }
                    //        else if (byR > 255)
                    //        {
                    //            byR = 255;
                    //        }
                    //        var gray = (byB + byG + byR) / 3;//计算灰度
                    //        byR = gray + 10;
                    //        if (byR > 255)
                    //        {
                    //            byR = 255;
                    //        }
                    //        ptr[0] = (byte)gray;
                    //        ptr[1] =
                    //        ptr[2] = (byte)byR;
                    //        ptr += pixelSize;
                    //    }
                    //}

                    var n = height * width;
                    for (var i = 0; i < n; i++)
                    {
                        var byB = (Math.Abs(ptr[0] + ptr[0] - ptr[1] + ptr[2]) * ptr[1]) >> 8;
                        var byG = (Math.Abs(ptr[0] + ptr[0] - ptr[1] + ptr[2]) * ptr[2]) >> 8;
                        var byR = (Math.Abs(ptr[1] + ptr[1] - ptr[0] + ptr[2]) * ptr[2]) >> 8;
                        if (byB < 0)
                        {
                            byB = 0;
                        }
                        else if (byB > 255)
                        {
                            byB = 255;
                        }
                        if (byG < 0)
                        {
                            byG = 0;
                        }
                        else if (byG > 255)
                        {
                            byG = 255;
                        }
                        if (byR < 0)
                        {
                            byR = 0;
                        }
                        else if (byR > 255)
                        {
                            byR = 255;
                        }
                        var gray = (byB + byG + byR) / 3;//计算灰度
                        byR = gray + 10;
                        if (byR > 255)
                        {
                            byR = 255;
                        }
                        ptr[0] = (byte)gray;
                        ptr[1] =
                        ptr[2] = (byte)byR;
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

using System;
using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace EasyDeal
{
    public class PluginSobelEdge : IFilter
    {
        public string GetPluginName()
        {
            return "边缘检测";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.SobeledgeIcon;
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
                var stride = bmpData.Stride;
                var byColorInfo = new byte[height * stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);


                #region Unsafe

                unsafe
                {
                    fixed (byte* source = byColorInfo)
                    {
                        #region 灰度化
                        var clone = source;
                        var n = height * width;
                        for (var i = 0; i < n; i++)
                        {
                            clone[0] = clone[1] = clone[2] = (byte)((clone[0] + clone[1] + clone[2]) / 3);
                            clone += pixelSize;
                        }
                        #endregion

                        clone = source;
                        var ptr = (byte*)(bmpData.Scan0);
                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < width; x++)
                            {

                                //边缘检测
                                int byB, byG, byR;
                                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                                {
                                    byB = byG = byR = 0;
                                }
                                else
                                {
                                    byB = Math.Abs(
                                            -clone[- stride - pixelSize]
                                            + clone[- stride + pixelSize]
                                            - clone[- pixelSize]
                                            - clone[- pixelSize]
                                            + clone[pixelSize]
                                            + clone[pixelSize]
                                            - clone[stride - pixelSize]
                                            + clone[stride + pixelSize]) +
                                            Math.Abs(
                                            clone[- stride - pixelSize]
                                            + clone[- stride]
                                            + clone[- stride]
                                            + clone[- stride + pixelSize]
                                            - clone[stride - pixelSize]
                                            - clone[stride]
                                            - clone[stride]
                                            - clone[stride + pixelSize]);
                                    byG = Math.Abs(
                                            -clone[- stride - pixelSize + 1]
                                            + clone[- stride + pixelSize + 1]
                                            - clone[- pixelSize + 1]
                                            - clone[- pixelSize + 1]
                                            + clone[pixelSize + 1]
                                            + clone[pixelSize + 1]
                                            - clone[stride - pixelSize + 1]
                                            + clone[stride + pixelSize + 1]) +
                                            Math.Abs(
                                            clone[- stride - pixelSize + 1]
                                            + clone[- stride + 1]
                                            + clone[- stride + 1]
                                            + clone[- stride + pixelSize + 1]
                                            - clone[stride - pixelSize + 1]
                                            - clone[stride + 1]
                                            - clone[stride + 1]
                                            - clone[stride + pixelSize + 1]);
                                    byR = Math.Abs(
                                            -clone[- stride - pixelSize + 2]
                                            + clone[- stride + pixelSize + 2]
                                            - clone[- pixelSize + 2]
                                            - clone[- pixelSize + 2]
                                            + clone[pixelSize + 2]
                                            + clone[pixelSize + 2]
                                            - clone[stride - pixelSize + 2]
                                            + clone[stride + pixelSize + 2]) +
                                            Math.Abs(
                                            clone[- stride - pixelSize + 2]
                                            + clone[- stride + 2]
                                            + clone[- stride + 2]
                                            + clone[- stride + pixelSize + 2]
                                            - clone[stride - pixelSize + 2]
                                            - clone[stride + 2]
                                            - clone[stride + 2]
                                            - clone[stride + pixelSize + 2]);
                                }

                                //处理超过边界的值
                                if (byB > 255) byB = 255;
                                else if (byB < 0) byB = 0;
                                if (byG > 255) byG = 255;
                                else if (byG < 0) byG = 0;
                                if (byR > 255) byR = 255;
                                else if (byR < 0) byR = 0;

                                //反色处理
                                ptr[0] = (byte)(255 - byB);
                                ptr[1] = (byte)(255 - byG);
                                ptr[2] = (byte)(255 - byR);

                                ptr += pixelSize;
                                clone += pixelSize;
                            }
                        }
                    }

                }

                #endregion

                bitmap.UnlockBits(bmpData);
                return new HandleResult(bitmap, true);
            }
            catch (Exception e)
            {
                return new HandleResult(e);
            }

        }
    }
}

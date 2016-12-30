using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Screenshot
{
    public static class ImageCropper
    {
        public static BitmapSource CropBitmapSource(BitmapSource source, BitmapSource cropBitmap, CropStyle cropStyle)
        {
            switch (cropStyle)
            {
                case CropStyle.Shadow:
                    return source.GetBitmap().ShadowCropBitmap(cropBitmap.GetBitmap()).GetBitmapSource();
                case CropStyle.Transparent:
                    return source.GetBitmap().TransparentCropBitmap(cropBitmap.GetBitmap()).GetBitmapSource();
                default:
                    return source;
            }
        }

        public static Bitmap ShadowCropBitmap(this Bitmap bitmap, Bitmap transparentBitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var width1 = transparentBitmap.Width;
            var height1 = transparentBitmap.Height;
            var outputBitmap = new Bitmap(width, height);

            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var compareData = transparentBitmap.LockBits(new Rectangle(0, 0, width1, height1), ImageLockMode.ReadOnly,transparentBitmap.PixelFormat);
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
                    var row1 = (byte*)compareData.Scan0 + (y * compareData.Stride);
                    for (var x = 0; x < width; x++)
                    {
                        //判断坐标点是否在指定范围内   
                        if (x < width1 && y < height1)
                        {
                            if (row1[x * pixelSize + 3] == 0)
                            {
                                output[3] = 0;//设置输出图这部分为透明   
                            }
                            else
                            {
                                output[0] = row[x * pixelSize];
                                output[1] = row[x * pixelSize + 1];
                                output[2] = row[x * pixelSize + 2];
                                output[3] = row[x * pixelSize + 3];
                            }
                        }
                        else 
                        {
                            output[3] = 0;//设置输出图这部分为透明  
                        }
                        output += 4;
                    }
                    output += offset;
                }
            }
            bitmap.UnlockBits(data);
            transparentBitmap.UnlockBits(compareData);
            outputBitmap.UnlockBits(outData);

            return outputBitmap;
        }

        public static Bitmap TransparentCropBitmap(this Bitmap bitmap, Bitmap transparentBitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var width1 = transparentBitmap.Width;
            var height1 = transparentBitmap.Height;
            var outputBitmap = new Bitmap(width, height);

            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var compareData = transparentBitmap.LockBits(new Rectangle(0, 0, width1, height1), ImageLockMode.ReadOnly, transparentBitmap.PixelFormat);
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
                    var row1 = (byte*)compareData.Scan0 + (y * compareData.Stride);
                    for (var x = 0; x < width; x++)
                    {
                        //判断坐标点是否在指定范围内  
                        if (x < width1 && y < height1)
                        {
                            if (row1[x * pixelSize + 3] == 0)
                            {
                                output[0] = row[x * pixelSize];
                                output[1] = row[x * pixelSize + 1];
                                output[2] = row[x * pixelSize + 2];
                                output[3] = row[x * pixelSize + 3];
                            }
                            else
                            {
                                output[3] = 0;//设置输出图这部分为透明   
                            }
                        }
                        else
                        { 
                            output[0] = row[x * pixelSize];
                            output[1] = row[x * pixelSize + 1];
                            output[2] = row[x * pixelSize + 2];
                            output[3] = row[x * pixelSize + 3];
                        }
                        output += 4;
                    }
                    output += offset;
                }
            }
            bitmap.UnlockBits(data);
            transparentBitmap.UnlockBits(compareData);
            outputBitmap.UnlockBits(outData);

            return outputBitmap;
        }

        public static Bitmap SolidColorCropBitmap(this Bitmap bitmap,  Color color)
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
                        if (row[x * pixelSize + 3] == color.A && row[x * pixelSize] == color.A && row[x * pixelSize + 1] == color.G && row[x * pixelSize + 2] == color.B)
                        {
                            output[0] = color.R;
                            output[1] = color.G;
                            output[2] = color.B;
                            output[3] = color.A;
                        }
                        else
                        {
                            output[3] = 0;//设置输出图这部分为透明
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

        public static Bitmap GetBitmap(this BitmapSource source)
        {
            var bmp = new Bitmap
            (
              source.PixelWidth,
              source.PixelHeight,
              PixelFormat.Format32bppPArgb
            );

            var data = bmp.LockBits
             (
                 new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                 ImageLockMode.WriteOnly,
                 PixelFormat.Format32bppPArgb
             );

            source.CopyPixels
            (
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride
            );

            bmp.UnlockBits(data);

            return bmp;
        }

        public static BitmapSource GetBitmapSource(this Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
        }

    }
}

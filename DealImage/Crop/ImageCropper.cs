using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace DealImage.Crop
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
            const int pixelSize = 4;

            var width1 = transparentBitmap.Width;
            var height1 = transparentBitmap.Height;
            var outputBitmap = new Bitmap(width, height);

            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var compareData = transparentBitmap.LockBits(new Rectangle(0, 0, width1, height1), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var outData = outputBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            
            unsafe
            {
                var output = (byte*)outData.Scan0;
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
                                output[0] = output[1] = output[2] = 255;
                                output[3] = 0;
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
                            output[0] = output[1] = output[2] = 255;
                            output[3] = 0;
                        }
                        output += pixelSize;
                    }
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
            const int pixelSize = 4;

            var width1 = transparentBitmap.Width;
            var height1 = transparentBitmap.Height;
            var outputBitmap = new Bitmap(width, height);

            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var compareData = transparentBitmap.LockBits(new Rectangle(0, 0, width1, height1), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var outData = outputBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
           
            unsafe
            {
                var output = (byte*)outData.Scan0;
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
                                output[0] = output[1] = output[2] = 255;
                                output[3] = 0;
                            }
                        }
                        else
                        {
                            output[0] = row[x * pixelSize];
                            output[1] = row[x * pixelSize + 1];
                            output[2] = row[x * pixelSize + 2];
                            output[3] = row[x * pixelSize + 3];
                        }
                        output += pixelSize;
                    }
                }
            }
            bitmap.UnlockBits(data);
            transparentBitmap.UnlockBits(compareData);
            outputBitmap.UnlockBits(outData);

            return outputBitmap;
        }

        public static Bitmap ShadowSwapTransparent(this Bitmap bitmap, Color color)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            const int pixelSize = 4;

            var outputBitmap = new Bitmap(width, height);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var outData = outputBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            
            unsafe
            {
                var output = (byte*)outData.Scan0;
                for (var y = 0; y < height; y++)
                {
                    //每一行内存中的位置  
                    var row = (byte*)data.Scan0 + (y * data.Stride);
                    for (var x = 0; x < width; x++)
                    {
                        if (row[x * pixelSize + 3] == 0)
                        {
                            output[0] = color.B;
                            output[1] = color.G;
                            output[2] = color.R;
                            output[3] = color.A;
                        }
                        else
                        {
                            output[0] = output[1] = output[2] = 255;
                            output[3] = 0;
                        }
                        output += pixelSize;
                    }
                }
            }
            bitmap.UnlockBits(data);
            outputBitmap.UnlockBits(outData);

            return outputBitmap;
        }

    }
}

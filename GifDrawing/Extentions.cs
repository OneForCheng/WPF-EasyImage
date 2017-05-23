using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace GifDrawing
{
    public static class Extentions
    {

        public static T GetTransform<T>(this UIElement element) where T : Transform, new()
        {
            var transform = element.RenderTransform;
            var targetTransform = transform as T;
            if (targetTransform != null)
            {
                return targetTransform;
            }
            else
            {
                var group = transform as TransformGroup;
                if (group != null)
                {
                    var count = group.Children.Count;
                    for (var i = count - 1; i >= 0; i--)
                    {
                        targetTransform = group.Children[i] as T;
                        if (targetTransform != null)
                        {
                            break;
                        }
                    }
                    if (targetTransform != null) return targetTransform;
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    return targetTransform;
                }
                else
                {
                    group = new TransformGroup();
                    if (transform != null)
                    {
                        group.Children.Add(transform);
                    }
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    element.RenderTransform = group;

                    return targetTransform;
                }
            }
        }

        public static Bitmap GetBitmap(this BitmapSource source)
        {
            var bmp = new Bitmap
            (
              source.PixelWidth,
              source.PixelHeight,
              PixelFormat.Format32bppArgb
            );

            var data = bmp.LockBits
             (
                 new Rectangle(Point.Empty, bmp.Size),
                 ImageLockMode.WriteOnly,
                 PixelFormat.Format32bppArgb
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

        public static Bitmap ResizeBitmap(this Bitmap bitmap, int width, int height)
        {
            if (bitmap.Width == width && bitmap.Height == height)
            {
                return (Bitmap)bitmap.Clone();
            }
            var resizeBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var bmpGraphics = Graphics.FromImage(resizeBitmap))
            {
                bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;
                bmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                bmpGraphics.CompositingQuality = CompositingQuality.GammaCorrected;
                bmpGraphics.DrawImage(bitmap, 0, 0, width, height);
            }
            return resizeBitmap;
        }

        public static async Task CopyImageToClipboard(this BitmapImage bitmapImage)
        {
            using (var stream = new MemoryStream())
            {
                bitmapImage.StreamSource.Position = 0;
                await bitmapImage.StreamSource.CopyToAsync(stream);

                var dataObject = new DataObject();

                //普通图片格式
                var width = (int)bitmapImage.Width;
                var height = (int)bitmapImage.Height;
                var rect = new Rect(0, 0, width, height);
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    context.DrawRectangle(Brushes.White, null, rect);
                    context.DrawImage(bitmapImage, rect);
                }
                var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawingVisual);
                dataObject.SetData(ImageDataFormats.Bitmap, renderBitmap, true);

                //兼容PNG透明格式图片
                dataObject.SetData(ImageDataFormats.Png, stream, true);

                //兼容QQ
                var tempFilePath = Path.GetTempFileName();
                using (var fs = File.OpenWrite(tempFilePath))
                {
                    var data = stream.ToArray();
                    fs.Write(data, 0, data.Length);
                }
                var byteData = Encoding.UTF8.GetBytes("<QQRichEditFormat><Info version=\"1001\"></Info><EditElement type=\"1\" filepath=\"" + tempFilePath + "\" shortcut=\"\"></EditElement><EditElement type=\"0\"><![CDATA[]]></EditElement></QQRichEditFormat>");
                dataObject.SetData(ImageDataFormats.QqUnicodeRichEditFormat, new MemoryStream(byteData), true);
                dataObject.SetData(ImageDataFormats.QqRichEditFormat, new MemoryStream(byteData), true);
                dataObject.SetData(ImageDataFormats.FileDrop, new[] { tempFilePath }, true);
                dataObject.SetData(ImageDataFormats.FileNameW, new[] { tempFilePath }, true);
                dataObject.SetData(ImageDataFormats.FileName, new[] { tempFilePath }, true);

                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject, true);
            }
        }

        public static Bitmap ToMosaic(this Bitmap bitmap, int factor)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (factor <= 1) return bmp;

            try
            {
                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;

                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                #region Unsafe

                unsafe
                {
                    var ptr = (byte*)bmpData.Scan0;
                    for (var x = 0; x < width; x += factor)
                    {
                        for (var y = 0; y < height; y += factor)
                        {
                            int r = 0, g = 0, b = 0;
                            var count = 0;
                            for (int tempx = x, lentx = x + factor <= width ? x + factor : width; tempx < lentx; tempx++)
                            {
                                for (int tempy = y, lenty = y + factor <= height ? y + factor : height; tempy < lenty; tempy++)
                                {
                                    var index = tempy * bmpData.Stride + tempx * pixelSize;

                                    b += ptr[index];
                                    g += ptr[index + 1];
                                    r += ptr[index + 2];
                                    count++;
                                }
                            }
                            for (int tempx = x, lentx = x + factor <= width ? x + factor : width; tempx < lentx; tempx++)
                            {
                                for (int tempy = y, lenty = y + factor <= height ? y + factor : height; tempy < lenty; tempy++)
                                {
                                    var index = tempy * bmpData.Stride + tempx * pixelSize;
                                    ptr[index] = (byte)(b / count);
                                    ptr[index + 1] = (byte)(g / count);
                                    ptr[index + 2] = (byte)(r / count);
                                }
                            }
                        }
                    }
                }

                #endregion

                bmp.UnlockBits(bmpData);
                return bmp;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return bmp;
            }
        }

        public static Bitmap ZoomBitmap(this Bitmap bitmap , double scale)
        {
            if (scale <= 0)
            {
                throw new ArgumentException("scale must be more than zero");
            }
            var bmp = new Bitmap((int)Math.Ceiling(bitmap.Width * scale), (int)Math.Ceiling(bitmap.Height * scale));
            var height = bmp.Height;
            var width = bmp.Width;
            const int pixelSize = 4;
            var bmpDataNew = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var bmpDataOld = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            #region Safe
            var byColorNew = new byte[bmpDataNew.Height * bmpDataNew.Stride];
            var byColorOld = new byte[bmpDataOld.Height * bmpDataOld.Stride];
            Marshal.Copy(bmpDataOld.Scan0, byColorOld, 0, byColorOld.Length);
            for (var x = 0; x < width; x++)
            {
                var offsetX = (int)(x / scale) * pixelSize;
                for (var y = 0; y < height; y++)
                {
                    var offsetOld = (int)(y / scale) * bmpDataOld.Stride + offsetX;
                    var offsetNew = y * bmpDataNew.Stride + x * pixelSize;
                    byColorNew[offsetNew] = byColorOld[offsetOld];
                    byColorNew[offsetNew + 1] = byColorOld[offsetOld + 1];
                    byColorNew[offsetNew + 2] = byColorOld[offsetOld + 2];
                    byColorNew[offsetNew + 3] = byColorOld[offsetOld + 3];
                }
            }
            Marshal.Copy(byColorNew, 0, bmpDataNew.Scan0, byColorNew.Length);

            #endregion

            #region Unsafe

            unsafe
            {
                var ptrNew = (byte*)bmpDataNew.Scan0;
                var ptrOld = (byte*)bmpDataOld.Scan0;
                for  (var y = 0; y < height; y++)
                {
                    var offsetY = (int)(y / scale) * bmpDataOld.Stride;
                    for (var x = 0; x < width; x++)
                    {
                        var offsetOld = (int)(x / scale) * pixelSize + offsetY;
                        ptrNew[0] = ptrOld[offsetOld];
                        ptrNew[1] = ptrOld[offsetOld + 1];
                        ptrNew[2] = ptrOld[offsetOld + 2];
                        ptrNew[3] = ptrOld[offsetOld + 3];
                        ptrNew += pixelSize;
                    }
                }
            }

            #endregion


            bitmap.UnlockBits(bmpDataOld);
            bmp.UnlockBits(bmpDataNew);

            return bmp;
        }

        public static void FillFloodColor(this Bitmap bitmap, Point location, Color fillColor, double threshold)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            const int pixelSize = 4;
            //修正
            if (location.X < 0)
            {
                location.X = 0;
            }
            else if (location.X > width - 1)
            {
                location.X = width - 1;
            }
            if (location.Y < 0)
            {
                location.Y = 0;
            }
            else if (location.Y > height - 1)
            {
                location.Y = height - 1;
            }
            var fillPoints = new Stack<Point>(width * height);
            fillPoints.Push(new Point(location.X, location.Y));
            var  mask = new bool[width, height];

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            #region Safe

            //var byColorInfo = new byte[height * bmpData.Stride];
            //Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
            
            //var index = location.X * pixelSize + location.Y * bmpData.Stride;
            //var backColor = Color.FromArgb(byColorInfo[index + 3], byColorInfo[index + 2],  byColorInfo[index + 1],  byColorInfo[index]);
            //var gray = (backColor.R + backColor.G + backColor.B) / 3.0;
            
            //while (fillPoints.Count > 0)
            //{
            //    var p = fillPoints.Pop();
            //    mask[p.X, p.Y] = true;
            //    byColorInfo[pixelSize * p.X + p.Y * bmpData.Stride] = fillColor.B;
            //    byColorInfo[pixelSize * p.X + 1 + p.Y * bmpData.Stride] = fillColor.G;
            //    byColorInfo[pixelSize * p.X + 2 + p.Y * bmpData.Stride] = fillColor.R;
            //    byColorInfo[pixelSize * p.X + 3 + p.Y * bmpData.Stride] = fillColor.A;

            //    if (p.X > 0 && (Math.Abs(gray - ((byColorInfo[pixelSize * (p.X - 1) + p.Y * bmpData.Stride] + byColorInfo[pixelSize * (p.X - 1) + 1 + p.Y * bmpData.Stride] + byColorInfo[pixelSize * (p.X - 1) + 2 + p.Y * bmpData.Stride]) / 3.0)) <= threshold) && !mask[p.X - 1, p.Y])
            //    {
            //        byColorInfo[pixelSize * (p.X - 1) + p.Y * bmpData.Stride] = fillColor.B;
            //        byColorInfo[pixelSize * (p.X - 1) + 1 + p.Y * bmpData.Stride] = fillColor.G;
            //        byColorInfo[pixelSize * (p.X - 1) + 2 + p.Y * bmpData.Stride] = fillColor.R;
            //        byColorInfo[pixelSize * (p.X - 1) + 3 + p.Y * bmpData.Stride] = fillColor.A;
            //        fillPoints.Push(new Point(p.X - 1, p.Y));
            //        mask[p.X - 1, p.Y] = true;
            //    }
            //    if (p.X < width - 1 && (Math.Abs(gray - ((byColorInfo[pixelSize * (p.X + 1) + p.Y * bmpData.Stride] + byColorInfo[pixelSize * (p.X + 1) + 1 + p.Y * bmpData.Stride] + byColorInfo[pixelSize * (p.X + 1) + 2 + p.Y * bmpData.Stride]) / 3.0)) <= threshold) && !mask[p.X + 1, p.Y])
            //    {
            //        byColorInfo[pixelSize * (p.X + 1) + p.Y * bmpData.Stride] = fillColor.B;
            //        byColorInfo[pixelSize * (p.X + 1) + 1 + p.Y * bmpData.Stride] = fillColor.G;
            //        byColorInfo[pixelSize * (p.X + 1) + 2 + p.Y * bmpData.Stride] = fillColor.R;
            //        byColorInfo[pixelSize * (p.X + 1) + 3 + p.Y * bmpData.Stride] = fillColor.A;
            //        fillPoints.Push(new Point(p.X + 1, p.Y));
            //        mask[p.X + 1, p.Y] = true;
            //    }
            //    if (p.Y > 0 && (Math.Abs(gray - ((byColorInfo[pixelSize * p.X + (p.Y - 1) * bmpData.Stride] + byColorInfo[pixelSize * p.X + 1 + (p.Y - 1) * bmpData.Stride] + byColorInfo[pixelSize * p.X + 2 + (p.Y - 1) * bmpData.Stride]) / 3.0)) <= threshold) && !mask[p.X, p.Y - 1])
            //    {
            //        byColorInfo[pixelSize * p.X + (p.Y - 1) * bmpData.Stride] = fillColor.B;
            //        byColorInfo[pixelSize * p.X + 1 + (p.Y - 1) * bmpData.Stride] = fillColor.G;
            //        byColorInfo[pixelSize * p.X + 2 + (p.Y - 1) * bmpData.Stride] = fillColor.R;
            //        byColorInfo[pixelSize * p.X + 3 + (p.Y - 1) * bmpData.Stride] = fillColor.A;
            //        fillPoints.Push(new Point(p.X, p.Y - 1));
            //        mask[p.X, p.Y - 1] = true;
            //    }
            //    if (p.Y < height - 1 && (Math.Abs(gray - ((byColorInfo[pixelSize * p.X + (p.Y + 1) * bmpData.Stride] + byColorInfo[pixelSize * p.X + 1 + (p.Y + 1) * bmpData.Stride] + byColorInfo[pixelSize * p.X + 2 + (p.Y + 1) * bmpData.Stride]) / 3.0)) <= threshold) && !mask[p.X, p.Y + 1])
            //    {
            //        byColorInfo[pixelSize * p.X + (p.Y + 1) * bmpData.Stride] = fillColor.B;
            //        byColorInfo[pixelSize * p.X + 1 + (p.Y + 1) * bmpData.Stride] = fillColor.G;
            //        byColorInfo[pixelSize * p.X + 2 + (p.Y + 1) * bmpData.Stride] = fillColor.R;
            //        byColorInfo[pixelSize * p.X + 3 + (p.Y + 1) * bmpData.Stride] = fillColor.A;
            //        fillPoints.Push(new Point(p.X, p.Y + 1));
            //        mask[p.X, p.Y + 1] = true;
            //    }
            //}
            //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);

            #endregion

            #region Unsafe

            unsafe
            {
                var ptr = (byte*)bmpData.Scan0;
                var index = location.X * pixelSize + location.Y * bmpData.Stride;
                var backColor = Color.FromArgb(ptr[index + 3], ptr[index + 2], ptr[index + 1], ptr[index]);
                var gray = (backColor.R + backColor.G + backColor.B) / 3.0;
                while (fillPoints.Count > 0)
                {
                    var p = fillPoints.Pop();

                    mask[p.X, p.Y] = true;
                    ptr[pixelSize * p.X + p.Y * bmpData.Stride] = fillColor.B;
                    ptr[pixelSize * p.X + 1 + p.Y * bmpData.Stride] = fillColor.G;
                    ptr[pixelSize * p.X + 2 + p.Y * bmpData.Stride] = fillColor.R;
                    ptr[pixelSize * p.X + 3 + p.Y * bmpData.Stride] = fillColor.A;

                    if (p.X > 0 && 
                        Math.Abs(gray - (ptr[pixelSize * (p.X - 1) + p.Y * bmpData.Stride] + ptr[pixelSize * (p.X - 1) + 1 + p.Y * bmpData.Stride] + ptr[pixelSize * (p.X - 1) + 2 + p.Y * bmpData.Stride]) / 3.0) <= threshold && 
                        Math.Abs(ptr[pixelSize * (p.X - 1) + 3 + p.Y * bmpData.Stride] - backColor.A) <= threshold && 
                        !mask[p.X - 1, p.Y])
                    {
                        ptr[pixelSize * (p.X - 1) + p.Y * bmpData.Stride] = fillColor.B;
                        ptr[pixelSize * (p.X - 1) + 1 + p.Y * bmpData.Stride] = fillColor.G;
                        ptr[pixelSize * (p.X - 1) + 2 + p.Y * bmpData.Stride] = fillColor.R;
                        ptr[pixelSize * (p.X - 1) + 3 + p.Y * bmpData.Stride] = fillColor.A;
                        fillPoints.Push(new Point(p.X - 1, p.Y));
                        mask[p.X - 1, p.Y] = true;
                    }
                    if (p.X < width - 1 &&
                        Math.Abs(gray - (ptr[pixelSize * (p.X + 1) + p.Y * bmpData.Stride] + ptr[pixelSize * (p.X + 1) + 1 + p.Y * bmpData.Stride] + ptr[pixelSize * (p.X + 1) + 2 + p.Y * bmpData.Stride]) / 3.0) <= threshold &&
                        Math.Abs(ptr[pixelSize * (p.X + 1) + 3 + p.Y * bmpData.Stride] - backColor.A) <= threshold &&
                        !mask[p.X + 1, p.Y])
                    {
                        ptr[pixelSize * (p.X + 1) + p.Y * bmpData.Stride] = fillColor.B;
                        ptr[pixelSize * (p.X + 1) + 1 + p.Y * bmpData.Stride] = fillColor.G;
                        ptr[pixelSize * (p.X + 1) + 2 + p.Y * bmpData.Stride] = fillColor.R;
                        ptr[pixelSize * (p.X + 1) + 3 + p.Y * bmpData.Stride] = fillColor.A;
                        fillPoints.Push(new Point(p.X + 1, p.Y));
                        mask[p.X + 1, p.Y] = true;
                    }
                    if (p.Y > 0 &&
                        Math.Abs(gray - (ptr[pixelSize * p.X + (p.Y - 1) * bmpData.Stride] + ptr[pixelSize * p.X + 1 + (p.Y - 1) * bmpData.Stride] + ptr[pixelSize * p.X + 2 + (p.Y - 1) * bmpData.Stride]) / 3.0) <= threshold &&
                        Math.Abs(ptr[pixelSize * p.X + 3 + (p.Y - 1) * bmpData.Stride]- backColor.A) <= threshold &&
                        !mask[p.X, p.Y - 1])
                    {
                        ptr[pixelSize * p.X + (p.Y - 1) * bmpData.Stride] = fillColor.B;
                        ptr[pixelSize * p.X + 1 + (p.Y - 1) * bmpData.Stride] = fillColor.G;
                        ptr[pixelSize * p.X + 2 + (p.Y - 1) * bmpData.Stride] = fillColor.R;
                        ptr[pixelSize * p.X + 3 + (p.Y - 1) * bmpData.Stride] = fillColor.A;
                        fillPoints.Push(new Point(p.X, p.Y - 1));
                        mask[p.X, p.Y - 1] = true;
                    }
                    if (p.Y < height - 1 &&
                        Math.Abs(gray - (ptr[pixelSize * p.X + (p.Y + 1) * bmpData.Stride] + ptr[pixelSize * p.X + 1 + (p.Y + 1) * bmpData.Stride] + ptr[pixelSize * p.X + 2 + (p.Y + 1) * bmpData.Stride]) / 3.0) <= threshold &&
                        Math.Abs(ptr[pixelSize * p.X + 3 + (p.Y + 1) * bmpData.Stride] - backColor.A) <= threshold &&
                        !mask[p.X, p.Y + 1])
                    {
                        ptr[pixelSize * p.X + (p.Y + 1) * bmpData.Stride] = fillColor.B;
                        ptr[pixelSize * p.X + 1 + (p.Y + 1) * bmpData.Stride] = fillColor.G;
                        ptr[pixelSize * p.X + 2 + (p.Y + 1) * bmpData.Stride] = fillColor.R;
                        ptr[pixelSize * p.X + 3 + (p.Y + 1) * bmpData.Stride] = fillColor.A;
                        fillPoints.Push(new Point(p.X, p.Y + 1));
                        mask[p.X, p.Y + 1] = true;
                    }
                }

            }

            #endregion

            bitmap.UnlockBits(bmpData);

        }

    }
}

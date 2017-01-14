using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using IPlugins;

namespace ThresholdDeal
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SoftenWindow
    {
        private readonly Bitmap _cacheBitmap;

        private Bitmap _resultBitmap;

        public HandleResult HandleResult { get; private set; }


        public SoftenWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _cacheBitmap = bitmap;

            var screenHeight = SystemParameters.VirtualScreenHeight;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var height = bitmap.Height + 125.0;
            var width = bitmap.Width + 20.0;
            if (height < 300)
            {
                height = 300;
            }
            else if (height > screenHeight)
            {
                height = screenHeight;
            }
            if (width < 300)
            {
                width = 300;
            }
            else if (width > screenWidth)
            {
                width = screenWidth;
            }
            Height = height;
            Width = width;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            TitleLbl.Content = "柔化处理: 1";
            _resultBitmap = GetHandledImage(_cacheBitmap, 1);
            TargetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                if (Slider.Value >= 1)
                {
                    Slider.Value--;
                }
            }
            else if (e.Key == Key.Right)
            {
                if (Slider.Value <= 99)
                {
                    Slider.Value++;
                }
            }
        }

        private void TitleLbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(null, false);
            Close();
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(_resultBitmap, true);
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(null, false);
            Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_cacheBitmap == null) return;
            _resultBitmap?.Dispose();
            var newValue = (int)e.NewValue;
            TitleLbl.Content = $"柔化处理: {newValue}";
            _resultBitmap = GetHandledImage(_cacheBitmap, newValue);
            TargetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private Bitmap GetHandledImage(Bitmap bitmp, int radius)
        {
            var bmp = new Bitmap(bitmp);
            if (radius <= 0) return bmp;
            try
            {
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var byColorInfo = new byte[bmp.Height * bmpData.Stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
                var clone = (byte[])byColorInfo.Clone();
                var width = bmp.Width;
                var height = bmp.Height;

                var redColSum = new int[width + 2 * radius];
                var greenColSum = new int[width + 2 * radius];
                var blueColSum = new int[width + 2 * radius];
                var colCount = new int[width + 2 * radius];
                int count = 0, sumRed = 0, sumGreen = 0, sumBlue = 0;
                for (var y = 0; y <= radius && y < height; y++)
                {
                    for (var x = radius; x < width + radius; x++)
                    {
                        colCount[x]++;
                        blueColSum[x] += clone[y * bmpData.Stride + (x - radius) * 4];
                        greenColSum[x] += clone[y * bmpData.Stride + (x - radius) * 4 + 1];
                        redColSum[x] += clone[y * bmpData.Stride + (x - radius) * 4 + 2];
                    }
                }
                for (var x = radius; x <= radius * 2; x++)
                {
                    count += colCount[x];
                    sumRed += redColSum[x];
                    sumGreen += greenColSum[x];
                    sumBlue += blueColSum[x];
                }

                var index = 0;
                byColorInfo[index] = (byte)(sumBlue / count);
                byColorInfo[index + 1] = (byte)(sumGreen / count);
                byColorInfo[index + 2] = (byte)(sumRed / count);
                index += 4;

                int lastIndex;
                int nextIndex;
                for (var x = radius + 1; x < width + radius; x++)
                {
                    lastIndex = x - radius - 1;
                    nextIndex = x + radius;
                    count += -colCount[lastIndex] + colCount[nextIndex];
                    sumRed += -redColSum[lastIndex] + redColSum[nextIndex];
                    sumGreen += -greenColSum[lastIndex] + greenColSum[nextIndex];
                    sumBlue += -blueColSum[lastIndex] + blueColSum[nextIndex];
                    byColorInfo[index] = (byte)(sumBlue / count);
                    byColorInfo[index + 1] = (byte)(sumGreen / count);
                    byColorInfo[index + 2] = (byte)(sumRed / count);
                    index += 4;
                }
                for (var y = radius + 1; y < height + radius; y++)
                {
                    count = sumRed = sumGreen = sumBlue = 0;
                    index = (y - radius) * bmpData.Stride;
                    for (var x = radius; x < width + radius; x++)
                    {
                        var yIndex = y - radius - 1;
                        if (yIndex >= radius && yIndex < height + radius)
                        {
                            colCount[x]--;
                            blueColSum[x] -= clone[(yIndex - radius) * bmpData.Stride + (x - radius) * 4];
                            greenColSum[x] -= clone[(yIndex - radius) * bmpData.Stride + (x - radius) * 4 + 1];
                            redColSum[x] -= clone[(yIndex - radius) * bmpData.Stride + (x - radius) * 4 + 2];
                        }
                        yIndex = y + radius;
                        if (yIndex >= radius && yIndex < height + radius)
                        {
                            colCount[x]++;
                            blueColSum[x] += clone[(yIndex - radius) * bmpData.Stride + (x - radius) * 4];
                            greenColSum[x] += clone[(yIndex - radius) * bmpData.Stride + (x - radius) * 4 + 1];
                            redColSum[x] += clone[(yIndex - radius) * bmpData.Stride + (x - radius) * 4 + 2];
                        }
                    }

                    for (var x = radius; x <= radius * 2; x++)
                    {
                        count += colCount[x];
                        sumRed += redColSum[x];
                        sumGreen += greenColSum[x];
                        sumBlue += blueColSum[x];
                    }

                    byColorInfo[index] = (byte)(sumBlue / count);
                    byColorInfo[index + 1] = (byte)(sumGreen / count);
                    byColorInfo[index + 2] = (byte)(sumRed / count);
                    index += 4;

                    for (var x = radius + 1; x < width + radius; x++)
                    {
                        lastIndex = x - radius - 1;
                        nextIndex = x + radius;
                        count += -colCount[lastIndex] + colCount[nextIndex];
                        sumRed += -redColSum[lastIndex] + redColSum[nextIndex];
                        sumGreen += -greenColSum[lastIndex] + greenColSum[nextIndex];
                        sumBlue += -blueColSum[lastIndex] + blueColSum[nextIndex];
                        byColorInfo[index] = (byte)(sumBlue / count);
                        byColorInfo[index + 1] = (byte)(sumGreen / count);
                        byColorInfo[index + 2] = (byte)(sumRed / count);
                        index += 4;
                    }
                }

                Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);
                bmp.UnlockBits(bmpData);
                return bmp;
            }
            catch (Exception e)
            {
                HandleResult = new HandleResult(e);
                Close();
                return bmp;
            }
            
        }

        ///// <summary>
        ///// 分层算法
        ///// </summary>
        ///// <param name="bitmp"></param>
        ///// <param name="radius"></param>
        ///// <returns></returns>
        //private Bitmap GetHandledImage(Bitmap bitmp, int radius)
        //{
        //    var bmp = new Bitmap(bitmp);
        //    if (radius <= 0) return bmp;
        //    var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //    var byColorInfo = new byte[bmp.Height * bmpData.Stride];
        //    Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
        //    var width = bmp.Width;
        //    var height = bmp.Height;
        //    var redColSum = new int[width + 2 * radius];
        //    var greenColSum = new int[width + 2 * radius];
        //    var blueColSum = new int[width + 2 * radius];
        //    var colCount = new int[width + 2 * radius];
        //    int count = 0, sumRed = 0, sumGreen = 0, sumBlue = 0;
        //    for (var y = 0; y <= radius; y++)
        //    {
        //        for (var x = radius; x < width + radius; x++)
        //        {
        //            colCount[x] += 1;
        //            blueColSum[x] += byColorInfo[y * bmpData.Stride + (x - radius) * 3];
        //            greenColSum[x] += byColorInfo[y * bmpData.Stride + (x - radius) * 3 + 1];
        //            redColSum[x] += byColorInfo[y * bmpData.Stride + (x - radius) * 3 + 2];
        //        }
        //    }
        //    for (var x = radius; x <= radius * 2; x++)
        //    {
        //        count += colCount[x];
        //        sumRed += redColSum[x];
        //        sumGreen += greenColSum[x];
        //        sumBlue += blueColSum[x];
        //    }
        //    var index = 0;
        //    byColorInfo[index] = (byte)(sumBlue / count);
        //    byColorInfo[index + 1] = (byte)(sumGreen / count);
        //    byColorInfo[index + 2] = (byte)(sumRed / count);
        //    index += 3;
        //    int lastIndex;
        //    int nextIndex;
        //    for (var x = radius + 1; x < width + radius; x++)
        //    {
        //        lastIndex = x - radius - 1;
        //        nextIndex = x + radius;
        //        count += -colCount[lastIndex] + colCount[nextIndex];
        //        sumRed += -redColSum[lastIndex] + redColSum[nextIndex];
        //        sumGreen += -greenColSum[lastIndex] + greenColSum[nextIndex];
        //        sumBlue += -blueColSum[lastIndex] + blueColSum[nextIndex];
        //        byColorInfo[index] = (byte)(sumBlue / count);
        //        byColorInfo[index + 1] = (byte)(sumGreen / count);
        //        byColorInfo[index + 2] = (byte)(sumRed / count);
        //        index += 3;
        //    }
        //    for (var y = radius + 1; y < height + radius; y++)
        //    {
        //        count = sumRed = sumGreen = sumBlue = 0;
        //        index = (y - radius) * bmpData.Stride;
        //        for (var x = radius; x < width + radius; x++)
        //        {
        //            var yIndex = y - radius - 1;
        //            if (yIndex >= radius && yIndex < height + radius)
        //            {
        //                colCount[x]--;
        //                blueColSum[x] -= byColorInfo[(yIndex - radius) * bmpData.Stride + (x - radius) * 3];
        //                greenColSum[x] -= byColorInfo[(yIndex - radius) * bmpData.Stride + (x - radius) * 3 + 1];
        //                redColSum[x] -= byColorInfo[(yIndex - radius) * bmpData.Stride + (x - radius) * 3 + 2];
        //            }
        //            yIndex = y + radius;
        //            if (yIndex >= radius && yIndex < height + radius)
        //            {
        //                colCount[x]++;
        //                blueColSum[x] -= byColorInfo[(yIndex - radius) * bmpData.Stride + (x - radius) * 3];
        //                greenColSum[x] -= byColorInfo[(yIndex - radius) * bmpData.Stride + (x - radius) * 3 + 1];
        //                redColSum[x] -= byColorInfo[(yIndex - radius) * bmpData.Stride + (x - radius) * 3 + 2];
        //            }
        //        }
        //        for (var x = radius; x <= radius * 2; x++)
        //        {
        //            count += colCount[x];
        //            sumRed += redColSum[x];
        //            sumGreen += greenColSum[x];
        //            sumBlue += blueColSum[x];
        //        }
        //        byColorInfo[index] = (byte)(sumBlue / count);
        //        byColorInfo[index + 1] = (byte)(sumGreen / count);
        //        byColorInfo[index + 2] = (byte)(sumRed / count);
        //        index += 3;
        //        for (var x = radius + 1; x < width + radius; x++)
        //        {
        //            lastIndex = x - radius - 1;
        //            nextIndex = x + radius;
        //            count += -colCount[lastIndex] + colCount[nextIndex];
        //            sumRed += -redColSum[lastIndex] + redColSum[nextIndex];
        //            sumGreen += -greenColSum[lastIndex] + greenColSum[nextIndex];
        //            sumBlue += -blueColSum[lastIndex] + blueColSum[nextIndex];
        //            byColorInfo[index] = (byte)(sumBlue / count);
        //            byColorInfo[index + 1] = (byte)(sumGreen / count);
        //            byColorInfo[index + 2] = (byte)(sumRed / count);
        //            index += 3;
        //        }
        //    }
        //    Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);
        //    bmp.UnlockBits(bmpData);
        //    return bmp;
        //}
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IPlugins;
using Brushes = System.Windows.Media.Brushes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Beauty
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SoftenWindow : IDisposable
    {
        private readonly Bitmap _cacheBitmap;
        private Bitmap _resultBitmap;
        private WriteableBitmap _writeableBitmap;
        private byte[] _bitmapBuffer;
        private int _radius;
        //private double _sigma;
        //private bool _lockRadiusSlider;

        public HandleResult HandleResult { get; private set; }


        public SoftenWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _cacheBitmap = bitmap;

            var screenHeight = SystemParameters.VirtualScreenHeight;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var height = bitmap.Height + 125.0;
            var width = bitmap.Width + 40.0;
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
            _radius = 1;
            //_sigma = 1.0;
            //_lockRadiusSlider = true;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            TitleLbl.Content = $"柔化处理: {_radius}";
            _resultBitmap = GetHandledImage(_cacheBitmap, _radius);
            _writeableBitmap = new WriteableBitmap(Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            TargetImage.Source = _writeableBitmap;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            var value = (int)Slider.Value;
            if (e.Key == Key.Left)
            {
                if (value >= 1)
                {
                    Slider.Value = value - 1;
                }
            }
            else if (e.Key == Key.Right)
            {
                if (value <= 99)
                {
                    Slider.Value = value + 1;
                }
            }
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
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
            HandleResult = new HandleResult((Bitmap)_resultBitmap.Clone(), true);
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(null, false);
            Close();
        }

        private void ExchangeBgCbx_Click(object sender, RoutedEventArgs e)
        {
            if (ExchangeBgCbx.IsChecked == true)
            {
                ImageViewGrid.Background = Brushes.White;
                ImageBorder.BorderThickness = new Thickness(1);
            }
            else
            {
                ImageViewGrid.Background = Brushes.Transparent;
                ImageBorder.BorderThickness = new Thickness(0);
            }
        }

        private void ExchangeBgCbx_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new ColorDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var color = dialog.Color;
            ImageViewGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_cacheBitmap == null) return;
            //_lockRadiusSlider = true;
            _resultBitmap?.Dispose();
            _radius = (int)e.NewValue;
            TitleLbl.Content = $"柔化处理: {_radius}";
            _resultBitmap = GetHandledImage(_cacheBitmap, _radius);
            UpdateImage(_resultBitmap);
        }

        #region 快速均值模糊算法

        ///// <summary>
        ///// 快速均值模糊算法
        ///// </summary>
        ///// <param name="bitmap">原图像</param>
        ///// <param name="radius">柔化半径</param>
        ///// <returns></returns>
        //private Bitmap GetHandledImage(Bitmap bitmap, int radius)
        //{
        //    var bmp = (Bitmap)bitmap.Clone();
        //    if (radius <= 0) return bmp;
        //    try
        //    {
        //        var width = bmp.Width;
        //        var height = bmp.Height;
        //        const int pixelSize = 4;

        //        var redColSum = new int[width + 2 * radius];
        //        var greenColSum = new int[width + 2 * radius];
        //        var blueColSum = new int[width + 2 * radius];
        //        var colCount = new int[width + 2 * radius];
        //        int count = 0, sumRed = 0, sumGreen = 0, sumBlue = 0;

        //        var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //        var byColorInfo = new byte[height * bmpData.Stride];
        //        Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);

        //        #region Safe

        //        //var clone = (byte[])byColorInfo.Clone();
        //        //for (var y = 0; y <= radius && y < height; y++)
        //        //{
        //        //    for (var x = radius; x < width + radius; x++)
        //        //    {
        //        //        colCount[x]++;
        //        //        blueColSum[x] += clone[y * bmpData.Stride + (x - radius) * pixelSize];
        //        //        greenColSum[x] += clone[y * bmpData.Stride + (x - radius) * pixelSize + 1];
        //        //        redColSum[x] += clone[y * bmpData.Stride + (x - radius) * pixelSize + 2];
        //        //    }
        //        //}
        //        //for (var x = radius; x <= radius * 2; x++)
        //        //{
        //        //    count += colCount[x];
        //        //    sumRed += redColSum[x];
        //        //    sumGreen += greenColSum[x];
        //        //    sumBlue += blueColSum[x];
        //        //}

        //        //var index = 0;
        //        //byColorInfo[index] = (byte)(sumBlue / count);
        //        //byColorInfo[index + 1] = (byte)(sumGreen / count);
        //        //byColorInfo[index + 2] = (byte)(sumRed / count);
        //        //index += pixelSize;

        //        //int lastIndex;
        //        //int nextIndex;
        //        //for (var x = radius + 1; x < width + radius; x++)
        //        //{
        //        //    lastIndex = x - radius - 1;
        //        //    nextIndex = x + radius;
        //        //    count += -colCount[lastIndex] + colCount[nextIndex];
        //        //    sumRed += -redColSum[lastIndex] + redColSum[nextIndex];
        //        //    sumGreen += -greenColSum[lastIndex] + greenColSum[nextIndex];
        //        //    sumBlue += -blueColSum[lastIndex] + blueColSum[nextIndex];
        //        //    byColorInfo[index] = (byte)(sumBlue / count);
        //        //    byColorInfo[index + 1] = (byte)(sumGreen / count);
        //        //    byColorInfo[index + 2] = (byte)(sumRed / count);
        //        //    index += pixelSize;
        //        //}
        //        //for (var y = radius + 1; y < height + radius; y++)
        //        //{
        //        //    count = sumRed = sumGreen = sumBlue = 0;
        //        //    index = (y - radius) * bmpData.Stride;
        //        //    for (var x = radius; x < width + radius; x++)
        //        //    {
        //        //        var yIndex = y - radius - 1;
        //        //        if (yIndex >= radius && yIndex < height + radius)
        //        //        {
        //        //            colCount[x]--;
        //        //            blueColSum[x] -= clone[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize];
        //        //            greenColSum[x] -= clone[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 1];
        //        //            redColSum[x] -= clone[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 2];
        //        //        }
        //        //        yIndex = y + radius;
        //        //        if (yIndex >= radius && yIndex < height + radius)
        //        //        {
        //        //            colCount[x]++;
        //        //            blueColSum[x] += clone[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize];
        //        //            greenColSum[x] += clone[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 1];
        //        //            redColSum[x] += clone[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 2];
        //        //        }
        //        //    }

        //        //    for (var x = radius; x <= radius * 2; x++)
        //        //    {
        //        //        count += colCount[x];
        //        //        sumRed += redColSum[x];
        //        //        sumGreen += greenColSum[x];
        //        //        sumBlue += blueColSum[x];
        //        //    }

        //        //    byColorInfo[index] = (byte)(sumBlue / count);
        //        //    byColorInfo[index + 1] = (byte)(sumGreen / count);
        //        //    byColorInfo[index + 2] = (byte)(sumRed / count);
        //        //    index += pixelSize;

        //        //    for (var x = radius + 1; x < width + radius; x++)
        //        //    {
        //        //        lastIndex = x - radius - 1;
        //        //        nextIndex = x + radius;
        //        //        count += -colCount[lastIndex] + colCount[nextIndex];
        //        //        sumRed += -redColSum[lastIndex] + redColSum[nextIndex];
        //        //        sumGreen += -greenColSum[lastIndex] + greenColSum[nextIndex];
        //        //        sumBlue += -blueColSum[lastIndex] + blueColSum[nextIndex];
        //        //        byColorInfo[index] = (byte)(sumBlue / count);
        //        //        byColorInfo[index + 1] = (byte)(sumGreen / count);
        //        //        byColorInfo[index + 2] = (byte)(sumRed / count);
        //        //        index += pixelSize;
        //        //    }
        //        //}
        //        //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);


        //        #endregion

        //        #region Unsafe

        //        unsafe
        //        {
        //            fixed (byte* source = byColorInfo)
        //            {
        //                fixed (int* pColCount = colCount, pBlueColSum = blueColSum, pGreenColSum = greenColSum, pRedColSum = redColSum)
        //                {
        //                    var ptr = (byte*)(bmpData.Scan0);
        //                    for (var y = 0; y <= radius && y < height; y++)
        //                    {
        //                        for (var x = radius; x < width + radius; x++)
        //                        {
        //                            pColCount[x]++;
        //                            pBlueColSum[x] += source[y * bmpData.Stride + (x - radius) * pixelSize];
        //                            pGreenColSum[x] += source[y * bmpData.Stride + (x - radius) * pixelSize + 1];
        //                            pRedColSum[x] += source[y * bmpData.Stride + (x - radius) * pixelSize + 2];
        //                        }
        //                    }
        //                    for (var x = radius; x <= radius * 2; x++)
        //                    {
        //                        count += pColCount[x];
        //                        sumRed += pRedColSum[x];
        //                        sumGreen += pGreenColSum[x];
        //                        sumBlue += pBlueColSum[x];
        //                    }

        //                    var index = 0;
        //                    ptr[index] = (byte)(sumBlue / count);
        //                    ptr[index + 1] = (byte)(sumGreen / count);
        //                    ptr[index + 2] = (byte)(sumRed / count);
        //                    index += pixelSize;

        //                    int lastIndex;
        //                    int nextIndex;
        //                    for (var x = radius + 1; x < width + radius; x++)
        //                    {
        //                        lastIndex = x - radius - 1;
        //                        nextIndex = x + radius;
        //                        count += -pColCount[lastIndex] + pColCount[nextIndex];
        //                        sumRed += -pRedColSum[lastIndex] + pRedColSum[nextIndex];
        //                        sumGreen += -pGreenColSum[lastIndex] + pGreenColSum[nextIndex];
        //                        sumBlue += -pBlueColSum[lastIndex] + pBlueColSum[nextIndex];
        //                        ptr[index] = (byte)(sumBlue / count);
        //                        ptr[index + 1] = (byte)(sumGreen / count);
        //                        ptr[index + 2] = (byte)(sumRed / count);
        //                        index += pixelSize;
        //                    }
        //                    for (var y = radius + 1; y < height + radius; y++)
        //                    {
        //                        count = sumRed = sumGreen = sumBlue = 0;
        //                        index = (y - radius) * bmpData.Stride;
        //                        for (var x = radius; x < width + radius; x++)
        //                        {
        //                            var yIndex = y - radius - 1;
        //                            if (yIndex >= radius && yIndex < height + radius)
        //                            {
        //                                pColCount[x]--;
        //                                pBlueColSum[x] -= source[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize];
        //                                pGreenColSum[x] -= source[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 1];
        //                                pRedColSum[x] -= source[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 2];
        //                            }
        //                            yIndex = y + radius;
        //                            if (yIndex >= radius && yIndex < height + radius)
        //                            {
        //                                pColCount[x]++;
        //                                pBlueColSum[x] += source[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize];
        //                                pGreenColSum[x] += source[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 1];
        //                                pRedColSum[x] += source[(yIndex - radius) * bmpData.Stride + (x - radius) * pixelSize + 2];
        //                            }
        //                        }

        //                        for (var x = radius; x <= radius * 2; x++)
        //                        {
        //                            count += pColCount[x];
        //                            sumRed += pRedColSum[x];
        //                            sumGreen += pGreenColSum[x];
        //                            sumBlue += pBlueColSum[x];
        //                        }

        //                        ptr[index] = (byte)(sumBlue / count);
        //                        ptr[index + 1] = (byte)(sumGreen / count);
        //                        ptr[index + 2] = (byte)(sumRed / count);
        //                        index += pixelSize;

        //                        for (var x = radius + 1; x < width + radius; x++)
        //                        {
        //                            lastIndex = x - radius - 1;
        //                            nextIndex = x + radius;
        //                            count += -pColCount[lastIndex] + pColCount[nextIndex];
        //                            sumRed += -pRedColSum[lastIndex] + pRedColSum[nextIndex];
        //                            sumGreen += -pGreenColSum[lastIndex] + pGreenColSum[nextIndex];
        //                            sumBlue += -pBlueColSum[lastIndex] + pBlueColSum[nextIndex];
        //                            ptr[index] = (byte)(sumBlue / count);
        //                            ptr[index + 1] = (byte)(sumGreen / count);
        //                            ptr[index + 2] = (byte)(sumRed / count);
        //                            index += pixelSize;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        #endregion

        //        bmp.UnlockBits(bmpData);
        //        return bmp;
        //    }
        //    catch (Exception e)
        //    {
        //        HandleResult = new HandleResult(e);
        //        Close();
        //        return (Bitmap)bitmap.Clone();
        //    }

        //}

        #endregion

        #region 三次均值(方框)模糊算法(拟合高斯模糊算法)

        private Bitmap GetHandledImage(Bitmap bitmap, int radius)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (radius <= 0) return bmp;
            try
            {
                var radiuses = BoxesForGauss((radius + 1) / 3.0, 3);
                foreach (var r in radiuses)
                {
                    BoxBlur(bmp, (r - 1) / 2);
                }
                return bmp;
            }
            catch (Exception e)
            {
                HandleResult = new HandleResult(e);
                Close();
                return (Bitmap)bitmap.Clone();
            }

        }

        /// <summary>
        /// 根据高斯函数获取方框大小
        /// </summary>
        /// <param name="sigma">标准差</param>
        /// <param name="n">方框个数</param>
        /// <returns></returns>
        private int[] BoxesForGauss(double sigma, int n)
        {
            var avg1 = (int)Math.Sqrt(12 * sigma * sigma / n + 1);
            if (avg1%2 == 0)
            {
                avg1--;
            }
            var avg2 = avg1 + 2;

            var m = (int)Math.Round((12 * sigma * sigma - n * avg1 * avg1 - 4 * n * avg1 - 3 * n) / (-4 * avg1 - 4));
            var sizes = new int[n];
            for (var i = 0; i < n; i++)
            {
                sizes[i] = (i < m) ? avg1 : avg2;
            }
            return sizes;
        }

        /// <summary>
        /// 方框模糊
        /// </summary>
        /// <param name="bmp">目标图像</param>
        /// <param name="radius">模糊半径</param>
        private void BoxBlur(Bitmap bmp, int radius)
        {
            if (radius <= 0) return;
            try
            {
                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;

                //修正最大的模糊半径
                var min = Math.Min(width, height);
                if (radius > min) radius = min;

                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var stride = bmpData.Stride;

                var byColorInfo = new byte[height * stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);


                #region Unsafe

                unsafe
                {
                    fixed (byte* clone = byColorInfo)
                    {
                        var ptr = (byte*)(bmpData.Scan0);
                        int byB, byG, byR;
                        int index, leftIndex, rightIndex;
                        byte firstValB, lastValB;
                        byte firstValG, lastValG;
                        byte firstValR, lastValR;
                        double n = radius + radius + 1;

                        for (var y = 0; y < height; y++)
                        {
                            index = leftIndex = y * stride;
                            rightIndex = index + radius * pixelSize;

                            firstValB = ptr[index];
                            firstValG = ptr[index + 1];
                            firstValR = ptr[index + 2];

                            lastValB = ptr[index + (width - 1)*pixelSize];
                            lastValG = ptr[index + (width - 1)*pixelSize + 1];
                            lastValR = ptr[index + (width - 1)*pixelSize + 2];

                            byB = (radius + 1)*firstValB;
                            byG = (radius + 1)*firstValG;
                            byR = (radius + 1)*firstValR;

                            for (var x = 0; x < radius; x ++)
                            {
                                byB += ptr[index + x*pixelSize];
                                byG += ptr[index + x*pixelSize + 1];
                                byR += ptr[index + x*pixelSize + 2];
                            }
                            for (var x = 0; x <= radius; x++)
                            {
                                byB += ptr[rightIndex] - firstValB;
                                byG += ptr[rightIndex + 1] - firstValG;
                                byR += ptr[rightIndex + 2] - firstValR;

                                clone[index] =  (byte)Math.Round(byB/n);
                                clone[index + 1] =  (byte)Math.Round(byG/n);
                                clone[index + 2] =  (byte)Math.Round(byR/n);

                                index += pixelSize;
                                rightIndex += pixelSize;
                            }
                            for (var x = radius + 1; x <width - radius; x++)
                            {
                                byB += ptr[rightIndex] - ptr[leftIndex];
                                byG += ptr[rightIndex + 1] - ptr[leftIndex + 1];
                                byR += ptr[rightIndex + 2] - ptr[leftIndex + 2];

                                clone[index] = (byte)Math.Round(byB / n);
                                clone[index + 1] = (byte)Math.Round(byG / n);
                                clone[index + 2] = (byte)Math.Round(byR / n);

                                index += pixelSize;
                                leftIndex += pixelSize;
                                rightIndex += pixelSize;

                            }
                            for (var x = width - radius; x < width; x++)
                            {
                                byB += lastValB - ptr[leftIndex];
                                byG += lastValG - ptr[leftIndex + 1];
                                byR += lastValR - ptr[leftIndex + 2];

                                clone[index] = (byte)Math.Round(byB / n);
                                clone[index + 1] = (byte)Math.Round(byG / n);
                                clone[index + 2] = (byte)Math.Round(byR / n);

                                index += pixelSize;
                                leftIndex += pixelSize;
                            }
                        }

                        for (var x = 0; x < width; x++)
                        {
                            index = leftIndex = x * pixelSize;
                            rightIndex = index + radius * stride;

                            firstValB = clone[index];
                            firstValG = clone[index + 1];
                            firstValR = clone[index + 2];

                            lastValB = clone[index + (height - 1) * stride];
                            lastValG = clone[index + (height - 1) * stride + 1];
                            lastValR = clone[index + (height - 1) * stride + 2];

                            byB = (radius + 1) * firstValB;
                            byG = (radius + 1) * firstValG;
                            byR = (radius + 1) * firstValR;

                            for (var y = 0; y < radius; y++)
                            {
                                byB += clone[index + y * stride];
                                byG += clone[index + y * stride + 1];
                                byR += clone[index + y * stride + 2];
                            }
                            for (var y = 0; y <= radius; y++)
                            {
                                byB += clone[rightIndex] - firstValB;
                                byG += clone[rightIndex + 1] - firstValG;
                                byR += clone[rightIndex + 2] - firstValR;

                                ptr[index] = (byte)Math.Round(byB / n);
                                ptr[index + 1] = (byte)Math.Round(byG / n);
                                ptr[index + 2] = (byte)Math.Round(byR / n);

                                index += stride;
                                rightIndex += stride;
                            }
                            for (var y = radius + 1; y < height - radius; y++)
                            {
                                byB += clone[rightIndex] - clone[leftIndex];
                                byG += clone[rightIndex + 1] - clone[leftIndex + 1];
                                byR += clone[rightIndex + 2] - clone[leftIndex + 2];

                                ptr[index] = (byte)Math.Round(byB / n);
                                ptr[index + 1] = (byte)Math.Round(byG / n);
                                ptr[index + 2] = (byte)Math.Round(byR / n);

                                index += stride;
                                leftIndex += stride;
                                rightIndex += stride;
                            }
                            for (var y = height - radius; y < height; y++)
                            {
                                byB += lastValB - clone[leftIndex];
                                byG += lastValG - clone[leftIndex + 1];
                                byR += lastValR - clone[leftIndex + 2];

                                ptr[index] = (byte)Math.Round(byB / n);
                                ptr[index + 1] = (byte)Math.Round(byG / n);
                                ptr[index + 2] = (byte)Math.Round(byR / n);

                                index += stride;
                                leftIndex += stride;

                            }
                        }

                    }
                }

                #endregion

                bmp.UnlockBits(bmpData);

            }
            catch (Exception e)
            {
                HandleResult = new HandleResult(e);
                Close();
            }
        }

        #endregion

        #region 标准高斯模糊算法
        ///// <summary>
        ///// 高斯模糊算法
        ///// </summary>
        ///// <param name="bitmap">原图像</param>
        ///// <param name="radius">高斯半径（1-100）</param>
        ///// <returns></returns>
        //private Bitmap GetHandledImage(Bitmap bitmap, int radius)
        //{
        //    var bmp = (Bitmap)bitmap.Clone();
        //    if (radius <= 0) return bmp;
        //    try
        //    {
        //        var width = bmp.Width;
        //        var height = bmp.Height;
        //        const int pixelSize = 4;

        //        var kernel = GaussKernel(radius, radius / 6.0);
        //        //var kernel = GaussKernel(radius, sigma);

        //        var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //        var byColorInfo = new byte[height * bmpData.Stride];
        //        Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);

        //        #region Safe
        //        //double byB, byG, byR;
        //        //int index;
        //        //double k;
        //        //var clone = (byte[])byColorInfo.Clone();
        //        //for (var y = 0; y < height; y++)
        //        //{
        //        //    for (var x = 0; x < width; x++)
        //        //    {
        //        //        byB = byG = byR = 0.0;
        //        //        for (var i = -radius; i <= radius; i++)
        //        //        {
        //        //            index = (Math.Abs(x + i) % width) * pixelSize + y*bmpData.Stride;
        //        //            k = kernel[i + radius];
        //        //            byB += byColorInfo[index] * k;
        //        //            byG += byColorInfo[index + 1] * k;
        //        //            byR += byColorInfo[index + 2] * k;
        //        //        }
        //        //        index = x* pixelSize + y*bmpData.Stride;
        //        //        clone[index] = (byte) byB;
        //        //        clone[index + 1] = (byte) byG;
        //        //        clone[index + 2] = (byte) byR;
        //        //    }
        //        //}

        //        //for  (var x = 0; x < width; x++)
        //        //{
        //        //    for (var y = 0; y < height; y++)
        //        //    {
        //        //        byB = byG = byR = 0.0;
        //        //        for (var i = -radius; i <= radius; i++)
        //        //        {
        //        //            index = x * pixelSize + (Math.Abs(y + i) % height) * bmpData.Stride;
        //        //            k = kernel[i + radius];
        //        //            byB += clone[index] * k;
        //        //            byG += clone[index + 1] * k;
        //        //            byR += clone[index + 2] * k;
        //        //        }
        //        //        index = x * pixelSize + y * bmpData.Stride;
        //        //        byColorInfo[index] = (byte)byB;
        //        //        byColorInfo[index + 1] = (byte)byG;
        //        //        byColorInfo[index + 2] = (byte)byR;

        //        //    }
        //        //}

        //        //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);

        //        #endregion

        //        #region Unsafe

        //        unsafe
        //        {
        //            fixed (byte* clone = byColorInfo)
        //            {
        //                var ptr = (byte*)(bmpData.Scan0);
        //                double byB, byG, byR;
        //                int index;
        //                double k;

        //                for (var y = 0; y < height; y++)
        //                {
        //                    for (var x = 0; x < width; x++)
        //                    {
        //                        byB = byG = byR = 0.0;
        //                        for (var i = -radius; i <= radius; i++)
        //                        {
        //                            index = (Math.Abs(x + i) % width) * pixelSize + y * bmpData.Stride;
        //                            k = kernel[i + radius];
        //                            byB += ptr[index] * k;
        //                            byG += ptr[index + 1] * k;
        //                            byR += ptr[index + 2] * k;
        //                        }
        //                        index = x * pixelSize + y * bmpData.Stride;
        //                        clone[index] = (byte)byB;
        //                        clone[index + 1] = (byte)byG;
        //                        clone[index + 2] = (byte)byR;
        //                    }
        //                }

        //                for (var x = 0; x < width; x++)
        //                {
        //                    for (var y = 0; y < height; y++)
        //                    {
        //                        byB = byG = byR = 0.0;
        //                        for (var i = -radius; i <= radius; i++)
        //                        {
        //                            index = x * pixelSize + (Math.Abs(y + i) % height) * bmpData.Stride;
        //                            k = kernel[i + radius];
        //                            byB += clone[index] * k;
        //                            byG += clone[index + 1] * k;
        //                            byR += clone[index + 2] * k;
        //                        }
        //                        index = x * pixelSize + y * bmpData.Stride;
        //                        ptr[index] = (byte)byB;
        //                        ptr[index + 1] = (byte)byG;
        //                        ptr[index + 2] = (byte)byR;

        //                    }
        //                }

        //            }
        //        }

        //        #endregion

        //        bmp.UnlockBits(bmpData);
        //        return bmp;
        //    }
        //    catch (Exception e)
        //    {
        //        HandleResult = new HandleResult(e);
        //        Close();
        //        return (Bitmap)bitmap.Clone();
        //    }

        //}

        ///// <summary>
        ///// 高斯模板
        ///// </summary>
        ///// <param name="radius">高斯半径</param>
        ///// <param name="sigma">方差</param>
        ///// <returns></returns>
        //private double[] GaussKernel(int radius, double sigma)
        //{
        //    var length = 2 * radius + 1;
        //    var kernel = new double[length];
        //    var sum = 0.0;
        //    for (var i = 0; i < length; i++)
        //    {
        //        kernel[i] = Math.Exp(-(i - radius) * (i - radius) / (2.0 * sigma * sigma));
        //        sum += kernel[i];
        //    }
        //    for (var i = 0; i < length; i++)
        //    {
        //        kernel[i] = kernel[i] / sum;
        //    }
        //    return kernel;
        //}
        #endregion
        
        private void UpdateImage(Bitmap bitmap)
        {
            _writeableBitmap.Lock();
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            if (_bitmapBuffer == null)
            {
                _bitmapBuffer = new byte[bitmap.Height * bmpData.Stride];
            }
            Marshal.Copy(bmpData.Scan0, _bitmapBuffer, 0, _bitmapBuffer.Length);
            Marshal.Copy(_bitmapBuffer, 0, _writeableBitmap.BackBuffer, _bitmapBuffer.Length);
            bitmap.UnlockBits(bmpData);
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _writeableBitmap.PixelWidth, _writeableBitmap.PixelHeight));
            _writeableBitmap.Unlock();
        }

        public void Dispose()
        {
            _cacheBitmap?.Dispose();
            _resultBitmap?.Dispose();
        }
    }
}

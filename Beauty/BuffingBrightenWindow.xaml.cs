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
    /// BuffingBrightenWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BuffingBrightenWindow : IDisposable
    {
        private readonly Bitmap _cacheBitmap;
        private Bitmap _highPassBitmap;
        private Bitmap _resultBitmap;
        private WriteableBitmap _writeableBitmap;
        private byte[] _bitmapBuffer;
        private double[] _cumtrapz;
        private double[] _productIntegral;
        private readonly byte[] _rgbColorTable;
        private int _denoiseLevel;
        private int _brightLevel;
        private bool _selectFirst;
        private bool _persistTexture;

        public HandleResult HandleResult { get; private set; }

        public BuffingBrightenWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _cacheBitmap = bitmap;
            
            var screenHeight = SystemParameters.VirtualScreenHeight;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var height = bitmap.Height + 145.0;
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
            _denoiseLevel = 3;
            _brightLevel = 3;
            _selectFirst = true;
            _persistTexture = true;
            _rgbColorTable = new byte[256];
            
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            TitleLbl.Content = $"磨皮美白处理: [{_denoiseLevel},{_brightLevel}]";
            SetRgbColorTable(_brightLevel);
            _resultBitmap = GetHandledImage(_cacheBitmap, _denoiseLevel, _brightLevel);
            _writeableBitmap = new WriteableBitmap(Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            TargetImage.Source = _writeableBitmap;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                if (_selectFirst)
                {

                    if ((int)FirstSlider.Value >= 1)
                    {
                        FirstSlider.Value = (int)FirstSlider.Value - 1;
                    }
                }
                else
                {
                    if ((int)SecondSlider.Value >= -9)
                    {
                        SecondSlider.Value = (int)SecondSlider.Value - 1;
                    }
                }
                
            }
            else if (e.Key == Key.Right)
            {
                if (_selectFirst)
                {
                    if ((int)FirstSlider.Value <= 10)
                    {
                        FirstSlider.Value = (int)FirstSlider.Value + 1;
                    }
                }
                else
                {
                    if ((int)SecondSlider.Value <= 10)
                    {
                        SecondSlider.Value = (int)SecondSlider.Value + 1;
                    }
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

        private void TextureCbx_Click(object sender, RoutedEventArgs e)
        {
            _persistTexture = TextureCbx.IsChecked.GetValueOrDefault();
            _resultBitmap = GetHandledImage(_cacheBitmap, _denoiseLevel, _brightLevel);
            UpdateImage(_resultBitmap);
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

        private void FirstSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_cacheBitmap == null) return;
            _selectFirst = true;
            _resultBitmap?.Dispose();
            _denoiseLevel = (int)e.NewValue;
            TitleLbl.Content = $"磨皮美白处理: [{_denoiseLevel},{_brightLevel}]";
            _resultBitmap = GetHandledImage(_cacheBitmap, _denoiseLevel, _brightLevel);
            UpdateImage(_resultBitmap);
        }

        private void SecondSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_cacheBitmap == null) return;
            _selectFirst = false; 
            _resultBitmap?.Dispose();
            _brightLevel = (int)e.NewValue;
            SetRgbColorTable(_brightLevel);
            TitleLbl.Content = $"磨皮美白处理: [{_denoiseLevel},{_brightLevel}]";
            _resultBitmap = GetHandledImage(_cacheBitmap, _denoiseLevel, _brightLevel);
            UpdateImage(_resultBitmap);
        }

        private void SetRgbColorTable(int brightLevel)
        {
            if (brightLevel > 0)
            {
                brightLevel++;
                for (var i = 0; i < 256; i++)
                {
                    _rgbColorTable[i] = (byte)Math.Round(Math.Log(i / 255.0 * (brightLevel - 1) + 1, brightLevel) * 255);
                }
            }
            else if (brightLevel < 0)
            {
                brightLevel = -brightLevel + 1;
                for (var i = 0; i < 256; i++)
                {
                    _rgbColorTable[i] = (byte)Math.Round((Math.Pow(brightLevel, i / 255.0) - 1)/(brightLevel - 1) * 255);
                }
            }
            
        }

        private Bitmap GetHandledImage(Bitmap bitmap, int denoiseLevel, int brightLevel)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (denoiseLevel <= 0 && brightLevel == 0) return bmp;
            try
            {
                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;

                if (_highPassBitmap == null)
                {
                    _highPassBitmap = HighPass(bmp, 3);
                }
                var highPassBmpData = _highPassBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                #region Unsafe

                unsafe
                {
                    var index = 0;
                    if (_cumtrapz == null)
                    {
                        _cumtrapz = new double[height * bmpData.Stride];
                        _productIntegral = new double[height * bmpData.Stride];
                        
                        var source = (byte*)(bmpData.Scan0);

                        double sumB, sumG, sumR;
                        double sumB2, sumG2, sumR2;
                        for (var y = 0; y < height; y++)
                        {
                            
                            sumB = sumG = sumR = sumB2 = sumG2 = sumR2 = 0.0;
                            var offsetY = y == 0 ? 0 : bmpData.Stride;
                            for (var x = 0; x < width; x++)
                            {

                                sumB += source[0];
                                sumG += source[1];
                                sumR += source[2];

                                sumB2 += source[0]*source[0];
                                sumG2 += source[1]*source[1];
                                sumR2 += source[2]*source[2];

                                _cumtrapz[index] = _cumtrapz[index - offsetY] + sumB;
                                _cumtrapz[index + 1] = _cumtrapz[index + 1 - offsetY] + sumG;
                                _cumtrapz[index + 2] = _cumtrapz[index + 2 - offsetY] + sumR;

                                _productIntegral[index] = _productIntegral[index - offsetY] + sumB2;
                                _productIntegral[index + 1] = _productIntegral[index + 1 - offsetY] + sumG2;
                                _productIntegral[index + 2] = _productIntegral[index + 2 - offsetY] + sumR2;

                                source += pixelSize;
                                index += pixelSize;
                            }
                        }
                    }

                    var ptr = (byte*)bmpData.Scan0;
                    var highPassPtr = (byte*) highPassBmpData.Scan0;
                    var radius = (int)Math.Ceiling(Math.Max(width, height) * 0.02);

                    var sigma = 10 + denoiseLevel * denoiseLevel * 5;
                    double topLeftB, topRightB, bottomLeftB, bottomRightB;
                    double topLeftG, topRightG, bottomLeftG, bottomRightG;
                    double topLeftR, topRightR, bottomLeftR, bottomRightR;

                    double topLeftB2, topRightB2, bottomLeftB2, bottomRightB2;
                    double topLeftG2, topRightG2, bottomLeftG2, bottomRightG2;
                    double topLeftR2, topRightR2, bottomLeftR2, bottomRightR2;

                    //磨皮美白
                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {

                            //if (denoiseLevel != 0 && ptr[0] > 20 && ptr[1] > 40 && ptr[2] > 95)//肤色检查
                            if (denoiseLevel != 0)
                            {

                                #region 局部均值与方差
                                if (y <= radius || x <= radius)
                                {
                                    topLeftB = topLeftG = topLeftR = 0.0;
                                    topLeftB2 = topLeftG2 = topLeftR2 = 0.0;
                                }
                                else
                                {
                                    index = (y - radius - 1) * bmpData.Stride + (x - radius - 1) * pixelSize;
                                    topLeftB = _cumtrapz[index];
                                    topLeftG = _cumtrapz[index + 1];
                                    topLeftR = _cumtrapz[index + 2];

                                    topLeftB2 = _productIntegral[index];
                                    topLeftG2 = _productIntegral[index + 1];
                                    topLeftR2 = _productIntegral[index + 2];
                                }

                                if (y <= radius)
                                {
                                    topRightB = topRightG = topRightR = 0.0;
                                    topRightB2 = topRightG2 = topRightR2 = 0.0;
                                }
                                else if (x + radius >= width)
                                {
                                    index = (y - radius - 1) * bmpData.Stride + (width - 1) * pixelSize;
                                    topRightB = _cumtrapz[index];
                                    topRightG = _cumtrapz[index + 1];
                                    topRightR = _cumtrapz[index + 2];

                                    topRightB2 = _productIntegral[index];
                                    topRightG2 = _productIntegral[index + 1];
                                    topRightR2 = _productIntegral[index + 2];
                                }
                                else
                                {
                                    index = (y - radius - 1) * bmpData.Stride + (x + radius) * pixelSize;
                                    topRightB = _cumtrapz[index];
                                    topRightG = _cumtrapz[index + 1];
                                    topRightR = _cumtrapz[index + 2];

                                    topRightB2 = _productIntegral[index];
                                    topRightG2 = _productIntegral[index + 1];
                                    topRightR2 = _productIntegral[index + 2];
                                }

                                if (x <= radius)
                                {
                                    bottomLeftB = bottomLeftG = bottomLeftR = 0.0;
                                    bottomLeftB2 = bottomLeftG2 = bottomLeftR2 = 0.0;
                                }
                                else if (y + radius >= height)
                                {
                                    index = (height - 1) * bmpData.Stride + (x - radius - 1) * pixelSize;
                                    bottomLeftB = _cumtrapz[index];
                                    bottomLeftG = _cumtrapz[index + 1];
                                    bottomLeftR = _cumtrapz[index + 2];

                                    bottomLeftB2 = _productIntegral[index];
                                    bottomLeftG2 = _productIntegral[index + 1];
                                    bottomLeftR2 = _productIntegral[index + 2];
                                }
                                else
                                {
                                    index = (y + radius) * bmpData.Stride + (x - radius - 1) * pixelSize;
                                    bottomLeftB = _cumtrapz[index];
                                    bottomLeftG = _cumtrapz[index + 1];
                                    bottomLeftR = _cumtrapz[index + 2];

                                    bottomLeftB2 = _productIntegral[index];
                                    bottomLeftG2 = _productIntegral[index + 1];
                                    bottomLeftR2 = _productIntegral[index + 2];
                                }

                                if (x + radius < width && y + radius < height)
                                {
                                    index = (y + radius) * bmpData.Stride + (x + radius) * pixelSize;
                                }
                                else if (x + radius >= width && y + radius < height)
                                {
                                    index = (y + radius) * bmpData.Stride + (width - 1) * pixelSize;
                                }
                                else if (x + radius < width && y + radius >= height)
                                {
                                    index = (height - 1) * bmpData.Stride + (x + radius) * pixelSize;
                                }
                                else
                                {
                                    index = (height - 1) * bmpData.Stride + (width - 1) * pixelSize;
                                }

                                bottomRightB = _cumtrapz[index];
                                bottomRightG = _cumtrapz[index + 1];
                                bottomRightR = _cumtrapz[index + 2];

                                bottomRightB2 = _productIntegral[index];
                                bottomRightG2 = _productIntegral[index + 1];
                                bottomRightR2 = _productIntegral[index + 2];



                                #endregion

                                #region 局部窗口内像素个数
                                var row = 2 * radius + 1;
                                var col = 2 * radius + 1;
                                if (x < radius)
                                {
                                    col -= radius - x;
                                }
                                if (x + radius > width - 1)
                                {
                                    col -= x + radius - width + 1;
                                }
                                if (y < radius)
                                {
                                    row -= radius - y;
                                }
                                if (y + radius > height - 1)
                                {
                                    row -= y + radius - height + 1;
                                }
                                var n = row * col;
                                #endregion

                                #region 磨皮
                                var mB = (bottomRightB - topRightB - bottomLeftB + topLeftB) / n;
                                var mG = (bottomRightG - topRightG - bottomLeftG + topLeftG) / n;
                                var mR = (bottomRightR - topRightR - bottomLeftR + topLeftR) / n;

                                var vB = (bottomRightB2 - topRightB2 - bottomLeftB2 + topLeftB2) / n - mB * mB;
                                var vG = (bottomRightG2 - topRightG2 - bottomLeftG2 + topLeftG2) / n - mG * mG;
                                var vR = (bottomRightR2 - topRightR2 - bottomLeftR2 + topLeftR2) / n - mR * mR;

                                var kB = vB / (vB + sigma);
                                var kG = vG / (vG + sigma);
                                var kR = vR / (vR + sigma);

                                ptr[0] = (byte)((1 - kB) * mB + kB * ptr[0]);
                                ptr[1] = (byte)((1 - kG) * mG + kG * ptr[1]);
                                ptr[2] = (byte)((1 - kR) * mR + kR * ptr[2]);

                                #endregion

                                if (_persistTexture)
                                {
                                    #region 线性图层混合+透明度50%
                                    var byB = ptr[0] + highPassPtr[0] - 128;
                                    var byG = ptr[1] + highPassPtr[1] - 128;
                                    var byR = ptr[2] + highPassPtr[2] - 128;

                                    if (byB < 0) byB = 0;
                                    else if (byB > 255) byB = 255;
                                    if (byG < 0) byG = 0;
                                    else if (byG > 255) byG = 255;
                                    if (byR < 0) byR = 0;
                                    else if (byR > 255) byR = 255;

                                    ptr[0] = (byte)byB;
                                    ptr[1] = (byte)byG;
                                    ptr[2] = (byte)byR;

                                    highPassPtr += pixelSize;
                                    #endregion
                                }

                            }

                            //美白
                            if (brightLevel != 0)
                            {
                                ptr[0] = _rgbColorTable[ptr[0]];
                                ptr[1] = _rgbColorTable[ptr[1]];
                                ptr[2] = _rgbColorTable[ptr[2]];
                            }

                            ptr += pixelSize;
                           
                        }
                    }

                }

                #endregion

                _highPassBitmap.UnlockBits(highPassBmpData);
                bmp.UnlockBits(bmpData);
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
        /// 高反差保留(利用高斯模糊)
        /// </summary>
        /// <param name="bitmap">原图像</param>
        /// <param name="radius">高斯半径（1-100）</param>
        /// <returns></returns>
        private Bitmap HighPass(Bitmap bitmap, int radius)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (radius <= 0) return bmp;
            try
            {
                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;

                var kernel = GaussKernel(radius, radius / 3.0);

                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var byColorInfo = new byte[height * bmpData.Stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);

                #region Unsafe

                unsafe
                {
                    fixed (byte* clone = byColorInfo)
                    {
                        var ptr = (byte*)(bmpData.Scan0);
                        double byB, byG, byR;
                        int index;
                        double k;

                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < width; x++)
                            {
                                byB = byG = byR = 0.0;
                                for (var i = -radius; i <= radius; i++)
                                {
                                    index = (Math.Abs(x + i) % width) * pixelSize + y * bmpData.Stride;
                                    k = kernel[i + radius];
                                    byB += ptr[index] * k;
                                    byG += ptr[index + 1] * k;
                                    byR += ptr[index + 2] * k;
                                }
                                index = x * pixelSize + y * bmpData.Stride;
                                clone[index] = (byte)byB;
                                clone[index + 1] = (byte)byG;
                                clone[index + 2] = (byte)byR;
                            }
                        }

                        for (var x = 0; x < width; x++)
                        {
                            for (var y = 0; y < height; y++)
                            {
                                byB = byG = byR = 0.0;
                                for (var i = -radius; i <= radius; i++)
                                {
                                    index = x * pixelSize + (Math.Abs(y + i) % height) * bmpData.Stride;
                                    k = kernel[i + radius];
                                    byB += clone[index] * k;
                                    byG += clone[index + 1] * k;
                                    byR += clone[index + 2] * k;
                                }
                                index = x * pixelSize + y * bmpData.Stride;

                                //高反差
                                byB = ptr[index] - byB + 128;
                                byG = ptr[index + 1] - byG + 128;
                                byR = ptr[index + 2] - byR + 128;

                                if (byB < 0) byB = 0;
                                else if (byB > 255) byB = 255;
                                if (byG < 0) byG = 0;
                                else if (byG > 255) byG = 255;
                                if (byR < 0) byR = 0;
                                else if (byR > 255) byR = 255;

                                ptr[index] = (byte)byB;
                                ptr[index + 1] = (byte)byG;
                                ptr[index + 2] = (byte)byR;

                            }
                        }

                    }
                }

                #endregion

                bmp.UnlockBits(bmpData);
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
        /// 高斯模板
        /// </summary>
        /// <param name="radius">高斯半径</param>
        /// <param name="sigma">方差</param>
        /// <returns></returns>
        private double[] GaussKernel(int radius, double sigma)
        {
            var length = 2 * radius + 1;
            var kernel = new double[length];
            var sum = 0.0;
            for (var i = 0; i < length; i++)
            {
                kernel[i] = Math.Exp(-(i - radius) * (i - radius) / (2.0 * sigma * sigma));
                sum += kernel[i];
            }
            for (var i = 0; i < length; i++)
            {
                kernel[i] = kernel[i] / sum;
            }
            return kernel;
        }

        #region 表面模糊算法
        ///// <summary>
        ///// 表面模糊（速度太慢,需改进）
        ///// </summary>
        ///// <param name="bitmap"></param>
        ///// <param name="radius"></param>
        ///// <param name="threshold"></param>
        ///// <returns></returns>
        //private Bitmap GetHandledImage(Bitmap bitmap, int radius, int threshold)
        //{
        //    var bmp = (Bitmap)bitmap.Clone();
        //    if (radius <= 0 || threshold <= 0) return bmp;
        //    try
        //    {
        //        var width = bmp.Width;
        //        var height = bmp.Height;
        //        const int pixelSize = 4;

        //        var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //        var byColorInfo = new byte[height * bmpData.Stride];
        //        Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);

        //        #region Unsafe

        //        double bWightSum, gWightSum, rWightSum;
        //        double byB, byG, byR;

        //        unsafe
        //        {
        //            fixed (byte* clone = byColorInfo)
        //            {
        //                var t = threshold * 2.5;
        //                var ptr = (byte*)(bmpData.Scan0);
        //                for (var y = 0; y < height; y++)
        //                {
        //                    for (var x = 0; x < width; x++)
        //                    {
        //                        byB = byG = byR = bWightSum = gWightSum = rWightSum = 0.0;
        //                        for (var j = -radius; j <= radius; j++)
        //                        {
        //                            for (var i = -radius; i <= radius; i++)
        //                            {
        //                                var index = (Math.Abs(x + i) % width) * pixelSize + (Math.Abs(y + j) % height) * bmpData.Stride;
        //                                var bWeight = 1 - Math.Abs(ptr[0] - clone[index]) / t;
        //                                var gWeight = 1 - Math.Abs(ptr[1] - clone[index + 1]) / t;
        //                                var rWeight = 1 - Math.Abs(ptr[2] - clone[index + 2]) / t;

        //                                if (bWeight > 0)
        //                                {
        //                                    byB += clone[index] * bWeight;
        //                                    bWightSum += bWeight;
        //                                }
        //                                if (gWeight > 0)
        //                                {
        //                                    byG += clone[index + 1] * gWeight;
        //                                    gWightSum += gWeight;
        //                                }
        //                                if (rWeight > 0)
        //                                {
        //                                    byR += clone[index + 2] * rWeight;
        //                                    rWightSum += rWeight;
        //                                }
        //                            }
        //                        }
        //                        ptr[0] = (byte)(byB / bWightSum);
        //                        ptr[1] = (byte)(byG / gWightSum);
        //                        ptr[2] = (byte)(byR / rWightSum);

        //                        ptr += pixelSize;
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
            _highPassBitmap?.Dispose();
        }


       
    }
}

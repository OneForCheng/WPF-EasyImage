using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IPlugins;
using Brushes = System.Windows.Media.Brushes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Property
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PropertiesWindow : IDisposable
    {
        private readonly Bitmap _cacheFirstBitmap;
        private readonly IEnumerable<Bitmap> _cacheBitmaps;
        private Bitmap _resultBitmap;
        private Bitmap _gaussBlurBitmap;
        private WriteableBitmap _writeableBitmap;
        private byte[] _bitmapBuffer;
        private readonly int[] _sliderValues;
        private Slider _selectSlider;
        private bool _enableChange;
        private readonly byte[] _brightRgbMapTable;
        private readonly byte[] _averageBrights;
        private readonly byte[][] _contrastRgbMapTable;
        private readonly byte[][] _hueRgbMapTable;

        public HandleResult HandleResult { get; private set; }

        public PropertiesWindow(IEnumerable<Bitmap> bitmaps)
        {
            InitializeComponent();
            _cacheBitmaps = bitmaps;
            _cacheFirstBitmap = _cacheBitmaps.First();
            
            var screenHeight = SystemParameters.VirtualScreenHeight;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var height = _cacheFirstBitmap.Height + 165.0;
            var width = _cacheFirstBitmap.Width + 40.0;
            if (height < 350)
            {
                height = 350;
            }
            else if (height > screenHeight)
            {
                height = screenHeight;
            }
            if (width < 400)
            {
                width = 400;
            }
            else if (width > screenWidth)
            {
                width = screenWidth;
            }
            Height = height;
            Width = width;
            _sliderValues = new int[6];
            
            _brightRgbMapTable = new byte[256];

            _averageBrights = new byte[3];
            _contrastRgbMapTable = new byte[3][];
            _contrastRgbMapTable[0] = new byte[256];
            _contrastRgbMapTable[1] = new byte[256];
            _contrastRgbMapTable[2] = new byte[256];

            _hueRgbMapTable = new byte[3][];
            _hueRgbMapTable[0] = new byte[256];
            _hueRgbMapTable[1] = new byte[256];
            _hueRgbMapTable[2] = new byte[256];

            

            _selectSlider = FirstSlider;
            InitAverageBrights(_cacheFirstBitmap);
            _enableChange = true;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            _resultBitmap = (Bitmap)_cacheFirstBitmap.Clone();
            _writeableBitmap = new WriteableBitmap(Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            TargetImage.Source = _writeableBitmap;
            _gaussBlurBitmap = GaussBlur(_resultBitmap, 10);
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            var value = (int) _selectSlider.Value;
            if (e.Key == Key.Left)
            {
                var sliderFlag = (SliderFlag)_selectSlider.Tag;
                if (sliderFlag == SliderFlag.Opacity)
                {
                    if (value >= 1)
                    {
                        _selectSlider.Value = value - 1;
                    }
                }
                else
                {
                    if (value >= -99)
                    {
                        _selectSlider.Value = value - 1;
                    }
                }
                
            }
            else if (e.Key == Key.Right)
            {
                if (value <= 99)
                {
                    _selectSlider.Value = value + 1;
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
            var resultBitmaps = new List<Bitmap>()
            {
                (Bitmap)_resultBitmap.Clone()
            };
            for (var i = 1; i < _cacheBitmaps.Count(); i++)
            {
                _gaussBlurBitmap?.Dispose();
                InitAverageBrights(_cacheBitmaps.ElementAt(i));
                _gaussBlurBitmap = GaussBlur(_cacheBitmaps.ElementAt(i), 10);
                resultBitmaps.Add(GetHandledImage(_cacheBitmaps.ElementAt(i), _sliderValues[0], _sliderValues[1], _sliderValues[2], _sliderValues[3], _sliderValues[4], _sliderValues[5]));
            }
            HandleResult = new HandleResult(resultBitmaps, true);
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(null, false);
            Close();
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            _enableChange = false;
            for (var i = 0; i < _sliderValues.Length; i++)
            {
                _sliderValues[i] = 0;
            }
            FirstSliderValue.Content = 0;
            SecondSliderValue.Content = 0;
            ThirdSliderValue.Content = 0;
            ForthSliderValue.Content = 0;
            FifthSliderValue.Content = 0;
            SixthSliderValue.Content = 0;
            FirstSlider.Value = 0;
            SecondSlider.Value = 0;
            ThirdSlider.Value = 0;
            ForthSlider.Value = 0;
            FifthSlider.Value = 0;
            SixthSlider.Value = 0;
            _resultBitmap?.Dispose();
            _resultBitmap = (Bitmap) _cacheFirstBitmap.Clone();
            UpdateImage(_resultBitmap);
            _enableChange = true;
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
            if (!_enableChange) return;
            _selectSlider = sender as Slider;
            if (_selectSlider == null) return;
            _resultBitmap?.Dispose();
            var sliderFlag = (SliderFlag)_selectSlider.Tag;
            var newValue = (int)e.NewValue;
            switch (sliderFlag)
            {
                case SliderFlag.Brightness:
                    FirstSliderValue.Content = newValue;
                    SetBrightRgbMapTable(newValue);
                    break;
                case SliderFlag.Saturation:
                    SecondSliderValue.Content = newValue;
                    break;
                case SliderFlag.Warmth:
                    ThirdSliderValue.Content = newValue;
                    SetHueRgbMapTable(newValue);
                    break;
                case SliderFlag.Sharpness:
                    ForthSliderValue.Content = newValue;
                    break;
                case SliderFlag.Contrast:
                    FifthSliderValue.Content = newValue;
                    SetContrastRgbMapTable(newValue);
                    break;
                case SliderFlag.Opacity:
                    SixthSliderValue.Content = newValue;
                    break;
            }
            _sliderValues[(int)sliderFlag] = newValue;
            _resultBitmap = GetHandledImage(_cacheFirstBitmap, _sliderValues[0], _sliderValues[1], _sliderValues[2], _sliderValues[3], _sliderValues[4], _sliderValues[5]);
            UpdateImage(_resultBitmap);
        }

        private Bitmap GetHandledImage(Bitmap bitmap, int brightLevel,int saturationLevel, int warmthLevel, int sharpLevel, int contrastLevel, int opacityLevel)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (brightLevel == 0 && saturationLevel == 0 && warmthLevel == 0 && contrastLevel == 0 && sharpLevel == 0 &&  opacityLevel == 0) return bmp;
            try
            {
                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;

                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var gaussBmpData = _gaussBlurBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                #region Unsafe

                unsafe
                {
                    var sharpFactor = sharpLevel / 100.0 + 1;
                    var opacityFactor = (100 - opacityLevel) / 100.0;
                    var saturationFactor = saturationLevel/100.0;

                    var gaussBlurPtr = (byte*)gaussBmpData.Scan0;
                    var ptr = (byte*)bmpData.Scan0;
                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            //改变清晰度
                            if (sharpLevel != 0)
                            {
                               var byB = ptr[0] * sharpFactor + gaussBlurPtr[0] * (1 - sharpFactor);
                               var byG = ptr[1] * sharpFactor + gaussBlurPtr[1] * (1 - sharpFactor);
                               var byR = ptr[2] * sharpFactor + gaussBlurPtr[2] * (1 - sharpFactor);

                                if (byB < 0) byB = 0;
                                else if (byB > 255) byB = 255;
                                if (byG < 0) byG = 0;
                                else if (byG > 255) byG = 255;
                                if (byR < 0) byR = 0;
                                else if (byR > 255) byR = 255;

                                ptr[0] = (byte) byB;
                                ptr[1] = (byte) byG;
                                ptr[2] = (byte) byR;

                                gaussBlurPtr += pixelSize;
                            }

                            //改变明暗度
                            if (brightLevel != 0)
                            {
                                ptr[0] = _brightRgbMapTable[ptr[0]];
                                ptr[1] = _brightRgbMapTable[ptr[1]];
                                ptr[2] = _brightRgbMapTable[ptr[2]];
                            }

                            //改变饱和度
                            if (saturationLevel != 0)
                            {
                                var min = Math.Min(Math.Min(ptr[0], ptr[1]), ptr[2]);
                                var max = Math.Max(Math.Max(ptr[0], ptr[1]), ptr[2]);
                                if (max != min)
                                {
                                    var delta = (max - min) / 255.0;
                                    var value = (max + min) / 255.0;
                                    var l = value/2;
                                    var s = (l < 0.5)?delta / value : delta/ (2 - value);
                                    if (saturationFactor >= 0)
                                    {
                                        var alpha = (saturationFactor + s >= 1) ? s : 1 - saturationFactor;
                                        alpha = 1/alpha - 1;
                                        ptr[0] = (byte)(ptr[0] + (ptr[0] - l * 255) * alpha);
                                        ptr[1] = (byte)(ptr[1] + (ptr[1] - l * 255) * alpha);
                                        ptr[2] = (byte)(ptr[2] + (ptr[2] - l * 255) * alpha);
                                    }
                                    else
                                    {
                                        var alpha = 1 + saturationFactor;
                                        ptr[0] = (byte)(l * 255 + (ptr[0] - l * 255) * alpha);
                                        ptr[1] = (byte)(l * 255 + (ptr[1] - l * 255) * alpha);
                                        ptr[2] = (byte)(l * 255 + (ptr[2] - l * 255) * alpha);
                                    }
                                }
                            }
                            
                            //改变冷暖度
                            if (warmthLevel != 0)
                            {
                                ptr[0] = _hueRgbMapTable[0][ptr[0]];
                                ptr[1] = _hueRgbMapTable[1][ptr[1]];
                                ptr[2] = _hueRgbMapTable[2][ptr[2]];
                            }

                            //改变对比度
                            if (contrastLevel != 0)
                            {
                                ptr[0] = _contrastRgbMapTable[0][ptr[0]];
                                ptr[1] = _contrastRgbMapTable[1][ptr[1]];
                                ptr[2] = _contrastRgbMapTable[2][ptr[2]];
                            }

                            //改变透明度
                            if (opacityLevel != 0)
                            {
                                ptr[3] = (byte)(ptr[3] * opacityFactor);
                            }

                            ptr += pixelSize;
                        }
                    }
                }

                #endregion

                _gaussBlurBitmap.UnlockBits(gaussBmpData);
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

        private void SetBrightRgbMapTable(int brightLevel)
        {
            if (brightLevel > 0)
            {
                brightLevel++;
                for (var i = 0; i < 256; i++)
                {
                    _brightRgbMapTable[i] = (byte)Math.Round(Math.Log(i / 255.0 * (brightLevel - 1) + 1, brightLevel) * 255);
                }
            }
            else if (brightLevel < 0)
            {
                brightLevel = -brightLevel + 1;
                for (var i = 0; i < 256; i++)
                {
                    _brightRgbMapTable[i] = (byte)Math.Round((Math.Pow(brightLevel, i / 255.0) - 1) / (brightLevel - 1) * 255);
                }
            }

        }

        private void SetHueRgbMapTable(int hueLevel)
        {
            if (hueLevel > 0)
            {
                hueLevel++;
                for (var i = 0; i < 256; i++)
                {

                    _hueRgbMapTable[0][i] = 
                    _hueRgbMapTable[1][i] = (byte)Math.Round((Math.Pow(hueLevel, i / 255.0) - 1) / (hueLevel - 1) * 255);
                    _hueRgbMapTable[2][i] = (byte)Math.Round(Math.Log(i / 255.0 * (hueLevel - 1) + 1, hueLevel) * 255);
                }
            }
            else if (hueLevel < 0)
            {
                hueLevel = -hueLevel + 1;
                for (var i = 0; i < 256; i++)
                {
                    _hueRgbMapTable[0][i] = (byte)Math.Round(Math.Log(i / 255.0 * (hueLevel - 1) + 1, hueLevel) * 255);
                    _hueRgbMapTable[1][i] = 
                    _hueRgbMapTable[2][i] = (byte)Math.Round((Math.Pow(hueLevel, i / 255.0) - 1) / (hueLevel - 1) * 255);
                }
            }
        }

        private void SetContrastRgbMapTable(int contrastLevel)
        {
            if (contrastLevel == 0) return;
            var delta = contrastLevel / 100.0 + 1;
            for (var i = 0; i < 256; i++)
            {
                var byB = _averageBrights[0] + (i - _averageBrights[0]) * delta;
                var byG = _averageBrights[1] + (i - _averageBrights[1]) * delta;
                var byR = _averageBrights[2] + (i - _averageBrights[2]) * delta;
                if (byB > 255) byB = 255;
                else if (byB < 0) byB = 0;
                if (byG > 255) byG = 255;
                else if (byG < 0) byG = 0;
                if (byR > 255) byR = 255;
                else if (byR < 0) byR = 0;
                _contrastRgbMapTable[0][i] = (byte)byB;
                _contrastRgbMapTable[1][i] = (byte)byG;
                _contrastRgbMapTable[2][i] = (byte)byR;
            }
        }

        private void InitAverageBrights(Bitmap bitmap)
        {
           
            try
            {
                var width = bitmap.Width;
                var height = bitmap.Height;
                var n = width * height;

                const int pixelSize = 4;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                #region Unsafe

                unsafe
                {
                    int byB = 0, byG = 0, byR = 0;
                    var ptr = (byte*)bmpData.Scan0;
                    for (var i = 0; i < n; i++)
                    {
                        byB += ptr[0];
                        byG += ptr[1];
                        byR += ptr[2];
                        ptr += pixelSize;
                    }
                    _averageBrights[0] = (byte)(byB / n);
                    _averageBrights[0] = (byte)(byG / n);
                    _averageBrights[0] = (byte)(byR / n);
                }

                #endregion

                bitmap.UnlockBits(bmpData);
            }
            catch
            {
                _averageBrights[0] = _averageBrights[1] = _averageBrights[2] = 128;
            }
           
        }

        #region 高斯模糊

        /// <summary>
        /// 高斯模糊
        /// </summary>
        /// <param name="bitmap">原图像</param>
        /// <param name="radius">高斯半径（1-100）</param>
        /// <returns></returns>
        private Bitmap GaussBlur(Bitmap bitmap, int radius)
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

        #endregion

        private void UpdateImage(Bitmap bitmap)
        {
            _writeableBitmap.Lock();
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            if (_bitmapBuffer == null)
            {
                _bitmapBuffer = new byte[bitmap.Height*bmpData.Stride];
            }
            Marshal.Copy(bmpData.Scan0, _bitmapBuffer, 0, _bitmapBuffer.Length);
            Marshal.Copy(_bitmapBuffer, 0, _writeableBitmap.BackBuffer, _bitmapBuffer.Length);
            bitmap.UnlockBits(bmpData);
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _writeableBitmap.PixelWidth, _writeableBitmap.PixelHeight));
            _writeableBitmap.Unlock();
        }

        public void Dispose()
        {
            _resultBitmap?.Dispose();
            _gaussBlurBitmap?.Dispose();
        }

        
    }
}

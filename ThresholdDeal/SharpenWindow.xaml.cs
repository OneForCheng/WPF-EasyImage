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
    public partial class SharpenWindow
    {
        private readonly Bitmap _cacheBitmap;

        private Bitmap _resultBitmap;

        public HandleResult HandleResult { get; private set; }

        public SharpenWindow(Bitmap bitmap)
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
            TitleLbl.Content = "锐化处理: 50";
            _resultBitmap = GetHandledImage(_cacheBitmap, 50 / 100.0);
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
                if (Slider.Value <= 999)
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
            TitleLbl.Content = $"锐化处理: {newValue}";
            _resultBitmap = GetHandledImage(_cacheBitmap, newValue / 100.0);
            TargetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public Bitmap GetHandledImage(Bitmap bitmp, double byValue)
        {
            var bmp = new Bitmap(bitmp);
            try
            { 
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var byColorInfo = new byte[bmp.Height * bmpData.Stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
                var clone = (byte[])byColorInfo.Clone();
                var width = bmp.Width;
                var height = bmp.Height;
                for (var x = 1; x < width - 1; x++)
                {
                    for (var y = 1; y < height - 1; y++)
                    {
                        var index = y * bmpData.Stride + x * 4;
                        var byB = clone[index] + (clone[index] - (clone[index - bmpData.Stride - 4] + clone[index - bmpData.Stride] + clone[index - bmpData.Stride + 4] + clone[index - 4] + clone[index + 4] + clone[index + bmpData.Stride - 4] + clone[index + bmpData.Stride] + clone[index + bmpData.Stride + 4]) / 8.0) * byValue;
                        var byG = clone[index + 1] + (clone[index + 1] - (clone[index - bmpData.Stride - 4 + 1] + clone[index - bmpData.Stride + 1] + clone[index - bmpData.Stride + 4 + 1] + clone[index - 4 + 1] + clone[index + 4 + 1] + clone[index + bmpData.Stride - 4 + 1] + clone[index + bmpData.Stride + 1] + clone[index + bmpData.Stride + 4 + 1]) / 8.0) * byValue;
                        var byR = clone[index + 2] + (clone[index + 2] - (clone[index - bmpData.Stride - 4 + 2] + clone[index - bmpData.Stride + 2] + clone[index - bmpData.Stride + 4 + 2] + clone[index - 4 + 2] + clone[index + 4 + 2] + clone[index + bmpData.Stride - 4 + 2] + clone[index + bmpData.Stride + 2] + clone[index + bmpData.Stride + 4 + 2]) / 8.0) * byValue;
                        if (byB > 255) byB = 255;
                        else if (byB < 0) byB = 0;
                        if (byG > 255) byG = 255;
                        else if (byG < 0) byG = 0;
                        if (byR > 255) byR = 255;
                        else if (byR < 0) byR = 0;
                        byColorInfo[index] = (byte)byB;
                        byColorInfo[index + 1] = (byte)byG;
                        byColorInfo[index + 2] = (byte)byR;
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

       
    }
}

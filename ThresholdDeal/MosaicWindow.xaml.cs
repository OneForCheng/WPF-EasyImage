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
    public partial class MosaicWindow
    {
        private readonly Bitmap _cacheBitmap;

        private Bitmap _resultBitmap;

        public HandleResult HandleResult { get; private set; }

        public MosaicWindow(Bitmap bitmap)
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
            TitleLbl.Content = "马赛克处理: 5";
            _resultBitmap = GetHandledImage(_cacheBitmap, 5);
            TargetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                if (Slider.Value >= 2)
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
            TitleLbl.Content = $"马赛克处理: {newValue}";
            _resultBitmap = GetHandledImage(_cacheBitmap, newValue);
            TargetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public Bitmap GetHandledImage(Bitmap bitmp, int byValue)
        {
            var bmp = new Bitmap(bitmp);
            if (byValue <= 1) return bmp;
            try
            {
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var byColorInfo = new byte[bmp.Height * bmpData.Stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);

                var width = bmp.Width;
                var height = bmp.Height;

                var byB = byColorInfo[0];
                var byG = byColorInfo[1];
                var byR = byColorInfo[2];
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var index = y * bmpData.Stride + x * 4;
                        if (y % byValue == 0)
                        {
                            if (x % byValue == 0)
                            {
                                byB = byColorInfo[index];
                                byG = byColorInfo[index + 1];
                                byR = byColorInfo[index + 2];
                            }
                            else
                            {
                                byColorInfo[index] = byB;
                                byColorInfo[index + 1] = byG;
                                byColorInfo[index + 2] = byR;
                            }
                        }
                        else
                        {
                            byColorInfo[index] = byColorInfo[index - bmpData.Stride];
                            byColorInfo[index + 1] = byColorInfo[index - bmpData.Stride + 1];
                            byColorInfo[index + 2] = byColorInfo[index - bmpData.Stride + 2];
                        }
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

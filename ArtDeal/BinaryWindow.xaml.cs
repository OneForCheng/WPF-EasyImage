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

namespace ArtDeal
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BinaryWindow : IDisposable
    {
        private readonly Bitmap _cacheBitmap;
        private Bitmap _resultBitmap;
        private WriteableBitmap _writeableBitmap;
        private byte[] _bitmapBuffer;

        public HandleResult HandleResult { get; private set; }


        public BinaryWindow(Bitmap bitmap)
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
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            TitleLbl.Content = "二值化处理: 127";
            _resultBitmap = GetHandledImage(_cacheBitmap, 127);
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
                if (value <= 254)
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
            HandleResult = new HandleResult(new [] { (Bitmap)_resultBitmap.Clone() } , true);
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
            _resultBitmap?.Dispose();
            var newValue = (byte)e.NewValue;
            TitleLbl.Content = $"二值化处理: {newValue}";
            _resultBitmap = GetHandledImage(_cacheBitmap, newValue);
            UpdateImage(_resultBitmap);
        }

        private Bitmap GetHandledImage(Bitmap bitmap, byte factor)
        {
            var bmp = (Bitmap)bitmap.Clone();
            try
            {

                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;
                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                #region Safe

                //var byColorInfo = new byte[height * bmpData.Stride];
                //Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
                //for (var x = 0; x < width; x++)
                //{
                //    for (var y = 0; y < height; y++)
                //    {
                //        var index = y * bmpData.Stride + x * pixelSize;
                //        var byB = byColorInfo[index];
                //        var byG = byColorInfo[index + 1];
                //        var byR = byColorInfo[index + 2];
                //        var byV = (byte)((byR + byG + byB) / 3);
                //        byColorInfo[index] =
                //            byColorInfo[index + 1] =
                //            byColorInfo[index + 2] = (byte)(byV > factor ? 255 : 0);
                //    }
                //}
                //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);


                #endregion

                #region Unsafe

                unsafe
                {
                    var ptr = (byte*)(bmpData.Scan0);
                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var avg = (byte) ((ptr[0] + ptr[1] + ptr[2]) / 3);
                            ptr[0] = ptr[1] = ptr[2] = (byte)(avg > factor ? 255 : 0);
                            ptr += pixelSize;
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

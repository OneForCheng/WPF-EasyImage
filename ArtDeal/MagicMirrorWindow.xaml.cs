using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using Point = System.Windows.Point;

namespace ArtDeal
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MagicMirrorWindow : IDisposable
    {
        private readonly Bitmap _cacheBitmap;
        private Point _originPoint;
        private int _factor;
        private Bitmap _resultBitmap;
        private WriteableBitmap _writeableBitmap;
        private byte[] _bitmapBuffer;

        public HandleResult HandleResult { get; private set; }

        public MagicMirrorWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _cacheBitmap = ResizeBitmap(bitmap);

            var screenHeight = SystemParameters.VirtualScreenHeight;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var height = _cacheBitmap.Height + 125.0;
            var width = _cacheBitmap.Width + 40.0;
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
            _factor = 100;
            _originPoint = new Point(_cacheBitmap.Width / 2.0, _cacheBitmap.Height / 2.0);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            TitleLbl.Content = $"哈哈镜处理: {_factor}";
            _resultBitmap = GetHandledImage(_cacheBitmap, _originPoint, _factor);
            _writeableBitmap = new WriteableBitmap(Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            TargetImage.Source = _writeableBitmap;
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

        private void TargetImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _cacheBitmap == null) return;
            _originPoint = e.GetPosition(TargetImage);
            _resultBitmap?.Dispose();
            _resultBitmap = GetHandledImage(_cacheBitmap, _originPoint, _factor);
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

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_cacheBitmap == null) return;
            _resultBitmap?.Dispose();
            _factor = (int)e.NewValue;
            TitleLbl.Content = $"哈哈镜处理: {_factor}";
            _resultBitmap = GetHandledImage(_cacheBitmap, _originPoint, _factor);
            UpdateImage(_resultBitmap);
        }

        private Bitmap GetHandledImage(Bitmap bitmap, Point originPoint, int factor)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (factor == 0) return bmp;
            try
            {
                var originX = (int)originPoint.X;
                var originY = (int)originPoint.Y;
                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;
                
                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var byColorInfo = new byte[height * bmpData.Stride];
                Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);

                #region Safe

                //var clone = (byte[])byColorInfo.Clone();
                //for (var x = 0; x < width; x++)
                //{
                //    for (var y = 0; y < height; y++)
                //    {
                //        var dx = x - originX;
                //        var dy = y - originY;
                //        var theta = Math.Atan2(dy, dx);
                //        var mapR = Math.Sqrt(Math.Sqrt(dx * dx + dy * dy) * factor);
                //        dx = originX + (int)(mapR * Math.Cos(theta));
                //        dy = originY + (int)(mapR * Math.Sin(theta));
                //        if (dx < 0 || dx >= width || dy < 0 || dy >= height)
                //        {
                //            byColorInfo[y * bmpData.Stride + x * pixelSize + 3] = 0;
                //        }
                //        else
                //        {
                //            byColorInfo[y * bmpData.Stride + x * pixelSize] = clone[dy * bmpData.Stride + dx * 4];
                //            byColorInfo[y * bmpData.Stride + x * pixelSize + 1] = clone[dy * bmpData.Stride + dx * 4 + 1];
                //            byColorInfo[y * bmpData.Stride + x * pixelSize + 2] = clone[dy * bmpData.Stride + dx * 4 + 2];
                //        }
                //    }
                //}
                //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);


                #endregion

                #region Unsafe

                unsafe
                {
                    fixed (byte* source = byColorInfo)
                    {
                        var ptr = (byte*)(bmpData.Scan0);
                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < width; x++)
                            {
                                var dx = x - originX;
                                var dy = y - originY;
                                var theta = Math.Atan2(dy, dx);
                                var mapR = Math.Sqrt(Math.Sqrt(dx * dx + dy * dy) * factor);
                                dx = originX + (int)(mapR * Math.Cos(theta));
                                dy = originY + (int)(mapR * Math.Sin(theta));
                                if (dx < 0 || dx >= width || dy < 0 || dy >= height)
                                {
                                    ptr[3] = 0;
                                }
                                else
                                {
                                    ptr[0] = source[dy * bmpData.Stride + dx * pixelSize];
                                    ptr[1] = source[dy * bmpData.Stride + dx * pixelSize + 1];
                                    ptr[2] = source[dy * bmpData.Stride + dx * pixelSize + 2];
                                }
                                ptr += pixelSize;
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
                return bmp;
            }
            
        }

        private Bitmap ResizeBitmap(Bitmap bitmap)
        {
            try
            {
                var width = bitmap.Width;
                var height = bitmap.Height;
                var screenHeight = (int)SystemParameters.VirtualScreenHeight;
                var screenWidth = (int)SystemParameters.VirtualScreenWidth;
                var resize = false;

                if (width < 260 && height < 175)
                {
                    if (width > height)
                    {
                        height = 260 * height / width;
                        width = 260;
                        resize = true;
                    }
                    else
                    {
                        width = 175 * width / height;
                        height = 175;
                        resize = true;
                    }
                }
               
                if (width > screenWidth - 40)
                {
                    height = (screenWidth - 40) * height / width;
                    width = screenWidth - 40;
                    resize = true;
                }

                if (height > screenHeight - 125)
                {
                    width = (screenHeight - 125) * width / height;
                    height = screenHeight - 125;
                    resize = true;
                }

                if (!resize)
                {
                    return bitmap;
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
            catch
            {
                return bitmap;
            }
        }

        public void UpdateImage(Bitmap bitmap)
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

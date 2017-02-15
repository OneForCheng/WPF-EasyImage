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
using Point = System.Windows.Point;

namespace ArtDeal
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SpherizeWindow : IDisposable
    {
        private bool _resize;
        private int _oldWidth;
        private int _oldHeight;

        private readonly Bitmap _cacheBitmap;
        private Point _originPoint;
        private int _radius;
        private bool _raised;
        private Bitmap _resultBitmap;
        private WriteableBitmap _writeableBitmap;
        private byte[] _bitmapBuffer;

        public HandleResult HandleResult { get; private set; }

        public SpherizeWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _cacheBitmap = ResizeBitmap(bitmap);

            var screenHeight = SystemParameters.VirtualScreenHeight;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var height = _cacheBitmap.Height + 145.0;
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
            TargetImage.Height = _cacheBitmap.Height;
            TargetImage.Width = _cacheBitmap.Width;

            _raised = true;
            _radius = 100;
            _originPoint = new Point(_cacheBitmap.Width / 2.0, _cacheBitmap.Height / 2.0);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            _resultBitmap = GetHandledImage(_cacheBitmap, _originPoint, _radius, _raised);
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

        private void LeftRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (_cacheBitmap == null) return;
            _raised = true;
            UpdateImage();
        }

        private void LeftRadioBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_cacheBitmap == null) return;
            _raised = false;
            UpdateImage();
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(null, false);
            Close();
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(_resize? _resultBitmap.ResizeBitmap(_oldWidth, _oldHeight) : (Bitmap)_resultBitmap.Clone(), true);
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
            UpdateImage();
        }

        private void TargetImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(TargetImage);
            var x = (int)Math.Ceiling(position.X);
            var y = (int)Math.Ceiling(position.Y);
            if (x > 0) x--;
            if (y > 0) y--;
            TitleLbl.Content = $"球面化处理: ({x},{y})";
        }

        private void TargetImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TitleLbl.Content = "球面化处理";
        }

        private void ExchangeBgCbx_Click(object sender, RoutedEventArgs e)
        {
            if (ExchangeBgCbx.IsChecked == true)
            {
                ImageViewGrid.Background = Brushes.White;
                ImageBorder.BorderThickness = new Thickness(0.1);
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
            _radius = (int)e.NewValue;
            CenterLbl.Content = _radius;
            UpdateImage();
        }

        /// <summary>
        /// 图像滤镜---球面（凹凸）化
        /// </summary>
        /// <param name="bitmap">待处理图片</param>
        /// <param name="originPoint">球面化的处理原点</param>
        /// <param name="radius">球面化半径</param>
        /// <param name="raised">是否凸面化</param>
        /// <returns></returns>
        private Bitmap GetHandledImage(Bitmap bitmap, Point originPoint, int radius, bool raised)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (radius <= 0) return bmp;
            try
            {
                var originX = (int)Math.Ceiling(originPoint.X) - 1;
                var originY = (int)Math.Ceiling(originPoint.Y) - 1;
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
                //        var distance = Math.Sqrt(dx * dx + dy * dy);
                //        if (distance <= radius)
                //        {
                //            var theta = Math.Atan2(dy, dx);
                //            var mapR = raised ? (2 * radius / Math.PI) * Math.Asin(distance / radius) : Math.Sin(Math.PI * distance / (2 * radius)) * radius;

                //            dx = originX + (int)(mapR * Math.Cos(theta));
                //            dy = originY + (int)(mapR * Math.Sin(theta));

                //            if (dx < 0 || dx >= width || dy < 0 || dy >= height)
                //            {
                //                byColorInfo[y * bmpData.Stride + x * pixelSize] =
                //                    byColorInfo[y * bmpData.Stride + x * pixelSize + 1] =
                //                        byColorInfo[y * bmpData.Stride + x * pixelSize + 2] =
                //                            byColorInfo[y * bmpData.Stride + x * pixelSize + 3] = 0;
                //            }
                //            else
                //            {
                //                byColorInfo[y * bmpData.Stride + x * pixelSize] = clone[dy * bmpData.Stride + dx * 4];
                //                byColorInfo[y * bmpData.Stride + x * pixelSize + 1] = clone[dy * bmpData.Stride + dx * 4 + 1];
                //                byColorInfo[y * bmpData.Stride + x * pixelSize + 2] = clone[dy * bmpData.Stride + dx * 4 + 2];
                //                byColorInfo[y * bmpData.Stride + x * pixelSize + 3] = clone[dy * bmpData.Stride + dx * 4 + 3];
                //            }
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
                                var distance = Math.Sqrt(dx * dx + dy * dy);
                                if (distance <= radius)
                                {
                                    var theta = Math.Atan2(dy, dx);
                                    var mapR = raised ? (2 * radius / Math.PI) * Math.Asin(distance / radius): Math.Sin(Math.PI * distance / (2 * radius)) * radius;

                                    dx = originX + (int)(mapR * Math.Cos(theta));
                                    dy = originY + (int)(mapR * Math.Sin(theta));

                                    if (dx < 0 || dx >= width || dy < 0 || dy >= height)
                                    {
                                        ptr[0] = ptr[1] = ptr[2] = ptr[3] = 0;
                                    }
                                    else 
                                    {
                                        ptr[0] = source[dy * bmpData.Stride + dx * pixelSize];
                                        ptr[1] = source[dy * bmpData.Stride + dx * pixelSize + 1];
                                        ptr[2] = source[dy * bmpData.Stride + dx * pixelSize + 2];
                                        ptr[3] = source[dy * bmpData.Stride + dx * pixelSize + 3];

                                    }
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
                return (Bitmap)bitmap.Clone();
            }
            
        }

        private Bitmap ResizeBitmap(Bitmap bitmap)
        {
            try
            {
                var width = _oldWidth = bitmap.Width;
                var height = _oldHeight = bitmap.Height;
                var screenHeight = (int)SystemParameters.VirtualScreenHeight;
                var screenWidth = (int)SystemParameters.VirtualScreenWidth;
                _resize = false;

                if (width < 260 && height < 155)
                {
                    if (width > height)
                    {
                        height = 260 * height / width;
                        width = 260;
                        _resize = true;
                    }
                    else
                    {
                        width = 155 * width / height;
                        height = 155;
                        _resize = true;
                    }
                }
               
                if (width > screenWidth - 40)
                {
                    height = (screenWidth - 40) * height / width;
                    width = screenWidth - 40;
                    _resize = true;
                }

                if (height > screenHeight - 145)
                {
                    width = (screenHeight - 145) * width / height;
                    height = screenHeight - 145;
                    _resize = true;
                }

                if (!_resize)
                {
                    return bitmap;
                }
                var resizeBitmap = bitmap.ResizeBitmap(width, height);
                bitmap.Dispose();
                return resizeBitmap;
            }
            catch
            {
                return bitmap;
            }
        }


        private void UpdateImage()
        {
            _resultBitmap?.Dispose();
            _resultBitmap = GetHandledImage(_cacheBitmap, _originPoint, _radius, _raised);
            UpdateImage(_resultBitmap);
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

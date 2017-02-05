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
    public partial class MosaicWindow : IDisposable
    {
        private readonly Bitmap _cacheBitmap;
        private Bitmap _resultBitmap;
        private WriteableBitmap _writeableBitmap;
        private byte[] _bitmapBuffer;

        public HandleResult HandleResult { get; private set; }

        public MosaicWindow(Bitmap bitmap)
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
            TitleLbl.Content = "马赛克处理: 5";
            _resultBitmap = GetHandledImage(_cacheBitmap, 5);
            _writeableBitmap = new WriteableBitmap(Imaging.CreateBitmapSourceFromHBitmap(_resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            TargetImage.Source = _writeableBitmap;
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
            _resultBitmap?.Dispose();
            var newValue = (int)e.NewValue;
            TitleLbl.Content = $"马赛克处理: {newValue}";
            _resultBitmap = GetHandledImage(_cacheBitmap, newValue);
            UpdateImage(_resultBitmap);
        }

        ////左上角为基准的马赛克算法
        //private Bitmap GetHandledImage(Bitmap bitmap, int factor)
        //{
        //    var bmp = (Bitmap)bitmap.Clone();
        //    if (factor <= 1) return bmp;
        //    try
        //    {
        //        var width = bmp.Width;
        //        var height = bmp.Height;
        //        const int pixelSize = 4;
        //        var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //        #region Safe
        //        //var byColorInfo = new byte[height * bmpData.Stride];
        //        //Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
        //        //var byB = byColorInfo[0];
        //        //var byG = byColorInfo[1];
        //        //var byR = byColorInfo[2];
        //        //for (var y = 0; y < height; y++)
        //        //{
        //        //    for (var x = 0; x < width; x++)
        //        //    {
        //        //        var index = y * bmpData.Stride + x * pixelSize;
        //        //        if (y % factor == 0)
        //        //        {
        //        //            if (x % factor == 0)
        //        //            {
        //        //                byB = byColorInfo[index];
        //        //                byG = byColorInfo[index + 1];
        //        //                byR = byColorInfo[index + 2];
        //        //            }
        //        //            else
        //        //            {
        //        //                byColorInfo[index] = byB;
        //        //                byColorInfo[index + 1] = byG;
        //        //                byColorInfo[index + 2] = byR;
        //        //            }
        //        //        }
        //        //        else
        //        //        {
        //        //            byColorInfo[index] = byColorInfo[index - bmpData.Stride];
        //        //            byColorInfo[index + 1] = byColorInfo[index - bmpData.Stride + 1];
        //        //            byColorInfo[index + 2] = byColorInfo[index - bmpData.Stride + 2];
        //        //        }
        //        //    }
        //        //}
        //        //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);
        //        #endregion
        //        #region Unsafe
        //        unsafe
        //        {
        //            var ptr = (byte*)(bmpData.Scan0);
        //            var byB = ptr[0];
        //            var byG = ptr[1];
        //            var byR = ptr[2];
        //            for (var y = 0; y < height; y++)
        //            {
        //                for (var x = 0; x < width; x++)
        //                {               
        //                    if (y % factor == 0)
        //                    {
        //                        if (x % factor == 0)
        //                        {
        //                            byB = ptr[0];
        //                            byG = ptr[1];
        //                            byR = ptr[2];
        //                        }
        //                        else
        //                        {
        //                            ptr[0] = byB;
        //                            ptr[1] = byG;
        //                            ptr[2] = byR;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ptr[0] = (ptr - bmpData.Stride)[0];
        //                        ptr[1] = (ptr - bmpData.Stride)[1];
        //                        ptr[2] = (ptr - bmpData.Stride)[2];
        //                    }
        //                    ptr += pixelSize;
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
        //        return bmp;
        //    }
        //}

        private Bitmap GetHandledImage(Bitmap bitmap, int factor)
        {
            var bmp = (Bitmap)bitmap.Clone();
            if (factor <= 1) return bmp;

            try
            {
                var width = bmp.Width;
                var height = bmp.Height;
                const int pixelSize = 4;

                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                #region Safe

                //var byColorInfo = new byte[bmpData.Height * bmpData.Stride];
                //Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);
                //for (var x = 0; x < width; x += factor)
                //{
                //    for (var y = 0; y < height; y += factor)
                //    {
                //        int r = 0, g = 0, b = 0;
                //        var count = 0;
                //        for (int tempx = x, lentx = x + factor <= width ? x + factor : width; tempx < lentx; tempx++)
                //        {
                //            for (int tempy = y, lenty = y + factor <= height ? y + factor : height; tempy < lenty; tempy++)
                //            {
                //                var index = tempy * bmpData.Stride + tempx * pixelSize;

                //                b += byColorInfo[index];
                //                g += byColorInfo[index + 1];
                //                r += byColorInfo[index + 2];
                //                count++;
                //            }
                //        }
                //        for (int tempx = x, lentx = x + factor <= width ? x + factor : width; tempx < lentx; tempx++)
                //        {
                //            for (int tempy = y, lenty = y + factor <= height ? y + factor : height; tempy < lenty; tempy++)
                //            {
                //                var index = tempy * bmpData.Stride + tempx * pixelSize;
                //                byColorInfo[index] = (byte)(b / (count));
                //                byColorInfo[index + 1] = (byte)(g / (count));
                //                byColorInfo[index + 2] = (byte)(r / (count));
                //            }
                //        }
                //    }
                //}
                //Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);

                #endregion

                #region Unsafe

                unsafe
                {
                    var ptr = (byte*)(bmpData.Scan0);
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
                                    ptr[index] = (byte)(b / (count));
                                    ptr[index + 1] = (byte)(g / (count));
                                    ptr[index + 2] = (byte)(r / (count));
                                }
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

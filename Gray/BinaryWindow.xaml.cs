using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Gray
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BinaryWindow
    {
        private readonly Bitmap _cacheBitmap;

        public Bitmap ResultBitmap { get; private set; }

        public BinaryWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _cacheBitmap = bitmap;
        }

        private void BinaryWin_Loaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            TitleLbl.Content = "二值化处理: 127";
            ResultBitmap = GetBinaryImage(_cacheBitmap, 127);
            TargetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(ResultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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
            DialogResult = false;
            Close();
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(_cacheBitmap == null) return;
            ResultBitmap?.Dispose();
            var newValue = (byte)e.NewValue;
            TitleLbl.Content = $"二值化处理: {newValue}";
            ResultBitmap = GetBinaryImage(_cacheBitmap, newValue);
            TargetImage.Source = Imaging.CreateBitmapSourceFromHBitmap(ResultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public Bitmap GetBinaryImage(Bitmap bitmap, byte byValue)
        {
            var bmp = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format24bppRgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            var byColorInfo = new byte[bmp.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, byColorInfo, 0, byColorInfo.Length);

            for (int x = 0, width = bmp.Width; x < width; x++)
            {
                for (int y = 0, height = bmp.Height; y < height; y++)
                {
                    var byB = byColorInfo[y * bmpData.Stride + x * 3];
                    var byG = byColorInfo[y * bmpData.Stride + x * 3 + 1];
                    var byR = byColorInfo[y * bmpData.Stride + x * 3 + 2];
                    var byV = (byte)((byR + byG + byB) / 3);
                    byColorInfo[y * bmpData.Stride + x * 3] =
                        byColorInfo[y * bmpData.Stride + x * 3 + 1] =
                        byColorInfo[y * bmpData.Stride + x * 3 + 2] = (byte)(byV > byValue ? 255 : 0);
                }
            }
            Marshal.Copy(byColorInfo, 0, bmpData.Scan0, byColorInfo.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

    }
}

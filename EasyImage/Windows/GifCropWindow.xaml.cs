using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnimatedImage.Encoding;
using DealImage;
using UnmanagedToolkit;
using Brushes = System.Windows.Media.Brushes;

namespace EasyImage.Windows
{
    /// <summary>
    /// ImageCropWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GifCropWindow
    {
        private bool _isSave;
        private bool _isMousePressed;
        private System.Drawing.Point _startPoint;

        private readonly AnimatedImage.AnimatedImage _animatedImage;
        private readonly int _imageViewWidth;
        private readonly int _imageViewHeight;
        private readonly int _pixelWidth;
        private readonly int _pixelHeight;

        public AnimatedImage.AnimatedImage NewAnimatedImage { get; private set; }

        public GifCropWindow(AnimatedImage.AnimatedImage animatedImage)
        {
            InitializeComponent();
            var screenHeight = (int)SystemParameters.VirtualScreenHeight;
            var screenWidth = (int)SystemParameters.VirtualScreenWidth;

            _animatedImage = animatedImage;
            _imageViewWidth = _pixelWidth = _animatedImage.BitmapFrames.First().PixelWidth;
            _imageViewHeight = _pixelHeight = _animatedImage.BitmapFrames.First().PixelHeight;
           
            if (_imageViewWidth > screenWidth - 40)
            {
                _imageViewHeight = (screenWidth - 40) * _imageViewHeight / _imageViewWidth;
                _imageViewWidth = screenWidth - 40;
            }

            if (_imageViewHeight > screenHeight - 105)
            {
                _imageViewWidth = (screenHeight - 105) * _imageViewWidth / _imageViewHeight;
                _imageViewHeight = screenHeight - 105;
            }
            var height = _imageViewHeight + 105.0;
            var width = _imageViewWidth + 40.0;

            if (height < 300)
            {
                height = 300;
            }
            if (width < 300)
            {
                width = 300;
            }
            Height = height;
            Width = width;
            TargetImage.Source = _animatedImage.Source;
            ImageVisulGrid.Height = _imageViewHeight;
            ImageVisulGrid.Width = _imageViewWidth;

            _isSave = false;
            _isMousePressed = false;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            var isChanged = CropRect.Width > 1 && CropRect.Height > 1 &&
                (CropRect.Width < _imageViewWidth || CropRect.Height < _imageViewHeight);
            if (_isSave && isChanged)
            {
                var left = (int)(CropRect.Margin.Left/_imageViewWidth * _pixelWidth);
                var top = (int)(CropRect.Margin.Top/ _imageViewHeight * _pixelHeight);
                var width = (int)(CropRect.Width / _imageViewWidth * _pixelWidth);
                var height = (int)(CropRect.Height / _imageViewHeight * _pixelHeight);
                if (width == 0 || height == 0)
                {
                    return;
                }
                var croppedBox = new Int32Rect(left, top, width, height);
                var bitmapFrames = _animatedImage.BitmapFrames;
                var stream = new MemoryStream();
                using (var encoder = new GifEncoder(stream, width, height, _animatedImage.RepeatCount))
                {
                    var delays = _animatedImage.Delays;
                    for (var i = 0; i < bitmapFrames.Count; i++)
                    {
                        using (var bitmap = new CroppedBitmap(bitmapFrames[i], croppedBox).GetBitmap())
                        {
                            encoder.AppendFrame(bitmap, (int)delays[i].TotalMilliseconds);
                        }
                    }
                }
                stream.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();

                NewAnimatedImage = new AnimatedImage.AnimatedImage() { Source = bitmapImage, Stretch = Stretch.Fill };
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
            _isSave = false;
            Close();
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            _isSave = true;
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            _isSave = false;
            Close();
        }

        private void TargetImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var position = e.GetPosition(TargetImage);
            var x = (int)position.X;
            var y = (int)position.Y;
            _startPoint.X = x;
            _startPoint.Y = y;
            CropRect.Width = 0;
            CropRect.Height = 0;
            TargetImage.CaptureMouse();
            _isMousePressed = true;
        }

        private void TargetImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMousePressed = false;
            TargetImage.ReleaseMouseCapture();
        }

        private void TargetImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(TargetImage);
            var x = (int)position.X;
            var y = (int)position.Y;
            if (x < 0)
            {
                x = 0;
            }
            else if (x > _imageViewWidth)
            {
                x = _imageViewWidth;
            }
            if (y < 0)
            {
                y = 0;
            }
            else if (y > _imageViewHeight)
            {
                y = _imageViewHeight;
            }
           
            TitleLbl.Content = $"动态图裁剪: ({x},{y})";
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_isMousePressed)
            {
                int minX, minY, maxX, maxY;
                if (x > _startPoint.X)
                {
                    minX = _startPoint.X;
                    maxX = x;
                }
                else
                {
                    minX = x;
                    maxX = _startPoint.X;
                }
                if (y > _startPoint.Y)
                {
                    minY = _startPoint.Y;
                    maxY = y;
                }
                else
                {
                    minY = y;
                    maxY = _startPoint.Y;
                }
                CropRect.Margin = new Thickness(minX, minY, 0, 0);
                CropRect.Width = maxX - minX;
                CropRect.Height = maxY - minY;
            }
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
            ImageViewGrid.Background = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        
    }
}

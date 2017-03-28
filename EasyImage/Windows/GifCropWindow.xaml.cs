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
        private readonly int _imageWidth;
        private readonly int _imageHeight;
        private readonly int _pixelWidth;
        private readonly int _pixelHeight;

        public AnimatedImage.AnimatedImage NewAnimatedImage { get; private set; }

        public GifCropWindow(AnimatedImage.AnimatedImage animatedImage, int width, int height)
        {
            InitializeComponent();
            var screenHeight = (int)SystemParameters.VirtualScreenHeight;
            var screenWidth = (int)SystemParameters.VirtualScreenWidth;

            _animatedImage = animatedImage;
            _imageViewWidth = _imageWidth = width;
            _imageViewHeight = _imageHeight = height;
            _pixelWidth = _animatedImage.BitmapFrames.First().PixelWidth;
            _pixelHeight = _animatedImage.BitmapFrames.First().PixelHeight;

            if (_imageViewWidth > screenWidth - 40)
            {
                _imageViewHeight = (screenWidth - 40) * _imageViewHeight / _imageViewWidth;
                _imageViewWidth = screenWidth - 40;
            }

            if (_imageViewHeight > screenHeight - 125)
            {
                _imageViewWidth = (screenHeight - 125) * _imageViewWidth / _imageViewHeight;
                _imageViewHeight = screenHeight - 125;
            }
            var winHeight = _imageViewHeight + 125.0;
            var winWidth = _imageViewWidth + 40.0;

            if (winHeight < 300)
            {
                winHeight = 300;
            }
            if (winWidth < 300)
            {
                winWidth = 300;
            }
            Height = winHeight;
            Width = winWidth;
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
            var isChanged = CropRect.Width >= 1 && CropRect.Height >= 1 &&
                (CropRect.Width < _imageViewWidth || CropRect.Height < _imageViewHeight);
            if (_isSave && isChanged)
            {
                var left = (int)(CropRect.Margin.Left/_imageViewWidth * _pixelWidth);
                var top = (int)(CropRect.Margin.Top/ _imageViewHeight * _pixelHeight);
                var width = (int)(CropRect.Width / _imageViewWidth * _pixelWidth);
                var height = (int)(CropRect.Height / _imageViewHeight * _pixelHeight);
                var realWidth = (int)(CropRect.Width / _imageViewWidth * _imageWidth);
                var realHeight = (int)(CropRect.Height / _imageViewHeight * _imageHeight);
                if (width == 0 || height == 0 || realWidth == 0 || realHeight == 0)
                {
                    return;
                }
                var croppedBox = new Int32Rect(left, top, width, height);
                
                BitmapSource bitmapSource;
                if (_animatedImage.Animatable)
                {
                    var bitmapFrames = _animatedImage.BitmapFrames;
                    var stream = new MemoryStream();
                    using (var encoder = new GifEncoder(stream, realWidth, realHeight, _animatedImage.RepeatCount))
                    {
                        var delays = _animatedImage.Delays;
                        for (var i = 0; i < bitmapFrames.Count; i++)
                        {
                            using (var bitmap = new CroppedBitmap(bitmapFrames[i], croppedBox).GetResizeBitmap(realWidth, realHeight).GetBitmap())
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
                    bitmapSource = bitmapImage;
                }
                else
                {
                    bitmapSource = _animatedImage.Source as BitmapSource;
                    if(bitmapSource == null)return;
                    bitmapSource = new CroppedBitmap(bitmapSource, croppedBox).GetResizeBitmap(realWidth, realHeight).GetBitmapImage();
                }
               
                NewAnimatedImage = new AnimatedImage.AnimatedImage() { Source = bitmapSource, Stretch = Stretch.Fill };
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

            TopValueLbl.Content = 0;
            LeftValueLbl.Content = 0;
            WidthValueLbl.Content = 0;
            HeightValueLbl.Content = 0;

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

                TopValueLbl.Content = minX;
                LeftValueLbl.Content = minY;
                WidthValueLbl.Content = CropRect.Width;
                HeightValueLbl.Content = CropRect.Height;
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

using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnimatedImage;

namespace EasyImage.Controls
{
    [Serializable]
    public class ImageControlBaseInfo
    {
        private readonly bool _isLockAspect;
        private readonly double _width;
        private readonly double _height;
        private readonly string _imageSource;
        private readonly string _renderTransform;

        public ImageControlBaseInfo(ImageControl imageControl)
        {
            _isLockAspect = imageControl.IsLockAspect;
            _width = imageControl.Width;
            _height = imageControl.Height;
            
            var bitmapImage = (imageControl.Content as AnimatedGif)?.Source as BitmapImage;
            if (bitmapImage == null)
            {
                var bitmapSource = (imageControl.Content as AnimatedGif)?.Source as BitmapSource;
                if (bitmapSource != null)
                {
                    bitmapImage = bitmapSource.GetBitmapImage();
                }
            }
            var stream = bitmapImage?.StreamSource as MemoryStream;
            if (stream != null)
            {
                stream.Position = 0;
                _imageSource = Convert.ToBase64String(stream.ToArray());
            }
            _renderTransform = XamlWriter.Save(imageControl.RenderTransform);

        }

        public bool IsLockAspect => _isLockAspect;

        public double Width => _width;

        public double Height => _height;

        public Transform RenderTransform => XamlReader.Parse(_renderTransform) as Transform;

        public ImageSource ImageSource
        {
            get
            {
                var bytes = Convert.FromBase64String(_imageSource);
                var stream = new MemoryStream(bytes);
                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.EndInit();
                return imageSource;
            }
        }
    }

    public class ImageControl : UserControl, ICloneable
    {

        #region Private Fields


        #endregion

        #region Public Properties

        /// <summary>
        /// 保持纵横比
        /// </summary>
        public bool IsLockAspect { get; set; }

        /// <summary>
        /// 控件管理器
        /// </summary>
        public ControlManager ControlManager { get; }

        #endregion

        #region Constructors

        public ImageControl(ControlManager controlManager)
        {
            ControlManager = controlManager;
            IsLockAspect = true;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <returns>返回一个副本</returns>
        public object Clone()
        {
            var animatedImage = new AnimatedGif
            {
                Source = (Content as AnimatedGif)?.Source,
                Stretch = Stretch.Fill
            };
            
            var imageControl = new ImageControl(ControlManager)
            {
                IsLockAspect = IsLockAspect,
                Width = Width,
                Height = Height,
                Content = animatedImage,
                Template = Template,
                RenderTransform = RenderTransform.Clone(),
            };
            return imageControl;
        }

        #endregion

    }

}

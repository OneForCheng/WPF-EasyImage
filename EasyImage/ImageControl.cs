using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EasyImage
{
    [Serializable]
    public class ImageControlBaseInfo
    {
        private readonly double _width;
        private readonly double _height;
        private readonly string _imageSource;
        private readonly string _renderTransform;

        public ImageControlBaseInfo(UserControl imageControl)
        {
            _width = imageControl.Width;
            _height = imageControl.Height;
            var stream = ((imageControl.Content as AnimatedImage.AnimatedImage)?.Source as BitmapImage)?.StreamSource as MemoryStream;
            if (stream != null)
            {
                stream.Position = 0;
                _imageSource = Convert.ToBase64String(stream.ToArray());
            }
            _renderTransform = XamlWriter.Save(imageControl.RenderTransform);
           
        }

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

    public class ImageControl : UserControl
    {
        public ControlManager<ImageControl> ControlManager { get;}

        public ImageControl(ControlManager<ImageControl> controlManager)
        {
            ControlManager = controlManager;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Security.RightsManagement;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EasyImage.Controls
{
    [Serializable]
    public class ImageControlBaseInfo
    {
        private readonly string _id;
        private readonly bool _freeResize;
        private readonly double _width;
        private readonly double _height;
        private readonly string _imageSource;
        private readonly string _renderTransform;

        public ImageControlBaseInfo(ImageControl imageControl)
        {
            _id = imageControl.Id;
            _freeResize = imageControl.IsLockAspect;
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

        public string Id => _id;

        public bool FreeResize => _freeResize;

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
        public string Id { get; }

        public bool IsLockAspect { get; set; }

        public ControlManager ControlManager { get; }

        public ImageControl(ControlManager controlManager)
        {
            ControlManager = controlManager;
            Id = Guid.NewGuid().ToString("N");
            IsLockAspect = true;
        }

        public ImageControl(ControlManager controlManager, Guid guid)
        {
            ControlManager = controlManager;
            Id = guid.ToString("N");
            IsLockAspect = true;
        }

        public object Clone()
        {
            var animatedImage = new AnimatedImage.AnimatedImage
            {
                Source = (Content as AnimatedImage.AnimatedImage)?.Source,
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
    }

    public class CropImageControl : ImageControl
    {
        public CropImageControl(ControlManager controlManager) : base(controlManager)
        {

        }
    }

}

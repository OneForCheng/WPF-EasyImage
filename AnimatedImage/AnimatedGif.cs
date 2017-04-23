using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace AnimatedImage
{

    /// <summary>
    /// 动态的图像
    /// </summary>
    public class AnimatedGif : Image
    {
        #region Constructor
        /// <summary>
        /// 构造方法
        /// </summary>
        public AnimatedGif()
        {
            BitmapFrames = new List<BitmapSource>();
            Delays = new List<TimeSpan>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 是否为动态图像
        /// </summary>
        public bool Animatable { get; private set; }

        /// <summary>
        /// 获取所有位图帧的集合
        /// </summary>
        public List<BitmapSource> BitmapFrames { get; }

        /// <summary>
        /// 获取动态图的重复次数
        /// </summary>
        public int RepeatCount { get; private set; }

        /// <summary>
        /// 获取每一帧图像的间隔延迟时间的集合
        /// </summary>
        public List<TimeSpan> Delays { get; }


        #endregion
       
        #region Dependency Properties

        /// <summary>
        /// 获取或设置图像的 ImageSource
        /// </summary>
        public new ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Source 的依赖属性
        /// </summary>
        public new static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
                "Source",
                typeof(ImageSource),
                typeof(AnimatedGif),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnSourceChanged));

        /// <summary>
        /// 图像的 ImageSource 发生改变时，执行相应操作
        /// </summary>
        /// <param name="obj">依赖对象</param>
        /// <param name="args">依赖属性的改变参数</param>
        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as AnimatedGif)?.OnSourceChanged(args);
        }

        /// <summary>
        /// 图像的 ImageSource 发生改变时，执行相应操作
        /// </summary>
        /// <param name="args">依赖属性的改变参数</param>
        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            var newValue = args.NewValue as ImageSource;
            if (newValue == null)return;
            var image = this as Image;
            image.BeginAnimation(Image.SourceProperty, null);
            image.Source = newValue;
            image.DoWhenLoaded(InitAnimationOrImage);
        }

        #endregion

        #region Private Methods

        private void InitAnimationOrImage(Image image)
        {
            Animatable = false;
            RepeatCount = -1;
            BitmapFrames.Clear();
            Delays.Clear();

            var source = image.Source as BitmapSource;
            if (source == null) return;
            var decoder = GetDecoder(source) as GifBitmapDecoder;
            if (decoder != null && decoder.Frames.Count > 1)
            {
                var fullSize = GetFullSize(decoder);

                var animation = new ObjectAnimationUsingKeyFrames();
                var totalDuration = TimeSpan.Zero;
                BitmapSource prevFrame = null;
                foreach (var rawFrame in decoder.Frames)
                {
                    var info = GetFrameInfo(rawFrame);

                    var frame = MakeFrame(
                        fullSize,
                        rawFrame, info,
                        prevFrame);

                    BitmapFrames.Add(frame);
                    Delays.Add(info.Delay);

                    var keyFrame = new DiscreteObjectKeyFrame(frame, totalDuration);
                    animation.KeyFrames.Add(keyFrame);
                    totalDuration += info.Delay;

                    if (info.DisposalMethod == FrameDisposalMethod.Replace || info.DisposalMethod == FrameDisposalMethod.Combine)
                    {
                        prevFrame = frame;
                    }
                    else if (info.DisposalMethod == FrameDisposalMethod.RestoreBackground)
                    {
                        prevFrame = IsFullFrame(info, fullSize) ? null : ClearArea(frame, info);
                    }
                }

                animation.Duration = totalDuration;

                RepeatCount = GetRepeatCount(decoder);
                animation.RepeatBehavior = (RepeatCount == 0) ? RepeatBehavior.Forever : new RepeatBehavior(RepeatCount);

                if (animation.KeyFrames.Count > 0)
                    image.Source = (ImageSource)animation.KeyFrames[0].Value;
                else
                    image.Source = decoder.Frames[0];
                image.BeginAnimation(Image.SourceProperty, animation);
                Animatable = true;
            }
            else
            {
                BitmapFrames.Add(source);
                Delays.Add(TimeSpan.Zero);
            }
        }


        #region  Static Methods

        private static bool IsFullFrame(FrameInfo info, Int32Size fullSize)
        {
            return info.Left.Equals(0)
                   && info.Top.Equals(0)
                   && info.Width.Equals(fullSize.Width)
                   && info.Height.Equals(fullSize.Height);
        }

        private static BitmapSource ClearArea(BitmapSource frame, FrameInfo info)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var fullRect = new Rect(0, 0, frame.PixelWidth, frame.PixelHeight);
                var clearRect = new Rect(info.Left, info.Top, info.Width, info.Height);
                var clip = Geometry.Combine(
                    new RectangleGeometry(fullRect),
                    new RectangleGeometry(clearRect),
                    GeometryCombineMode.Exclude,
                    null);
                context.PushClip(clip);
                context.DrawImage(frame, fullRect);
            }

            var bitmap = new RenderTargetBitmap(
                    frame.PixelWidth, frame.PixelHeight,
                    frame.DpiX, frame.DpiY,
                    PixelFormats.Pbgra32);
            bitmap.Render(visual);

            if (bitmap.CanFreeze && !bitmap.IsFrozen)
                bitmap.Freeze();
            return bitmap;
        }

        private static BitmapDecoder GetDecoder(BitmapSource image)
        {
            BitmapDecoder decoder = null;
            Stream stream = null;
            Uri uri = null;
            var createOptions = BitmapCreateOptions.None;

            var bmp = image as BitmapImage;
            if (bmp != null)
            {
                createOptions = bmp.CreateOptions;
                if (bmp.StreamSource != null)
                {
                    stream = bmp.StreamSource;
                }
                else if (bmp.UriSource != null)
                {
                    uri = bmp.UriSource;
                    if (bmp.BaseUri != null && !uri.IsAbsoluteUri)
                        uri = new Uri(bmp.BaseUri, uri);
                }
            }
            else
            {
                var frame = image as BitmapFrame;
                if (frame != null)
                {
                    decoder = frame.Decoder;
                    Uri.TryCreate(frame.BaseUri, frame.ToString(), out uri);
                }
            }

            if (decoder != null) return decoder;
            if (stream != null)
            {
                stream.Position = 0;
                decoder = BitmapDecoder.Create(stream, createOptions, BitmapCacheOption.OnLoad);
            }
            else if (uri != null && uri.IsAbsoluteUri)
            {
                decoder = BitmapDecoder.Create(uri, createOptions, BitmapCacheOption.OnLoad);
            }
            return decoder;
        }

        private static int GetRepeatCount(BitmapDecoder decoder)
        {
            var ext = GetApplicationExtension(decoder, "NETSCAPE2.0");
            var bytes = ext?.GetQueryOrNull<byte[]>("/Data");
            if (bytes != null && bytes.Length >= 4)
                return BitConverter.ToUInt16(bytes, 2);
            return 1;
        }

        private static BitmapMetadata GetApplicationExtension(BitmapDecoder decoder, string application)
        {
            var count = 0;
            var query = "/appext";
            var extension = decoder.Metadata.GetQueryOrNull<BitmapMetadata>(query);
            while (extension != null)
            {
                var bytes = extension.GetQueryOrNull<byte[]>("/Application");
                if (bytes != null)
                {
                    var extApplication = System.Text.Encoding.ASCII.GetString(bytes);
                    if (extApplication == application)
                        return extension;
                }
                query = $"/[{++count}]appext";
                extension = decoder.Metadata.GetQueryOrNull<BitmapMetadata>(query);
            }
            return null;
        }

        private static BitmapSource MakeFrame(Int32Size fullSize, BitmapSource rawFrame, FrameInfo frameInfo, BitmapSource previousFrame)
        {

            //if (previousFrame == null && IsFullFrame(frameInfo, fullSize))
            //{
            //    return rawFrame;
            //}

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                if (previousFrame != null)
                {
                    var fullRect = new Rect(0, 0, fullSize.Width, fullSize.Height);
                    context.DrawImage(previousFrame, fullRect);
                }

                context.DrawImage(rawFrame, frameInfo.Rect);
            }
            var bitmap = new RenderTargetBitmap(
                fullSize.Width, fullSize.Height,
                96, 96,
                PixelFormats.Pbgra32);
            bitmap.Render(visual);

            if (bitmap.CanFreeze && !bitmap.IsFrozen)
                bitmap.Freeze();

            return bitmap;
        }

        private static Int32Size GetFullSize(BitmapDecoder decoder)
        {
            var width = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Width", 0);
            var height = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Height", 0);
            return new Int32Size(width, height);
        }

        private static FrameInfo GetFrameInfo(BitmapFrame frame)
        {
            var metadata = (BitmapMetadata)frame.Metadata;
            var delay = TimeSpan.FromMilliseconds(100);
            var metadataDelay = metadata.GetQueryOrDefault("/grctlext/Delay", 10);
            if (metadataDelay != 0)
                delay = TimeSpan.FromMilliseconds(metadataDelay * 10);
            var disposalMethod = (FrameDisposalMethod)metadata.GetQueryOrDefault("/grctlext/Disposal", 0);
            var frameMetadata = new FrameInfo
            {
                Left = metadata.GetQueryOrDefault("/imgdesc/Left", 0),
                Top = metadata.GetQueryOrDefault("/imgdesc/Top", 0),
                Width = metadata.GetQueryOrDefault("/imgdesc/Width", frame.PixelWidth),
                Height = metadata.GetQueryOrDefault("/imgdesc/Height", frame.PixelHeight),
                Delay = delay,
                DisposalMethod = disposalMethod
            };
            return frameMetadata;
        }


        #endregion

        #endregion

        #region Private Types

        private struct Int32Size
        {
            public Int32Size(int width, int height) : this()
            {
                Width = width;
                Height = height;
            }

            public int Width { get; }
            public int Height { get; }
        }

        private class FrameInfo
        {
            public TimeSpan Delay { get; set; }
            public FrameDisposalMethod DisposalMethod { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }

            public Rect Rect => new Rect(Left, Top, Width, Height);
        }

        private enum FrameDisposalMethod
        {
            Replace = 0,
            Combine = 1,
            RestoreBackground = 2,
            //RestorePrevious = 3
        }
        #endregion


    }
}

using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace AnimatedImage
{
    public static class ImageBehavior
    {
        #region AnimatedSource

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static ImageSource GetAnimatedSource(Image obj)
        {
            return (ImageSource)obj.GetValue(AnimatedSourceProperty);
        }

        public static void SetAnimatedSource(Image obj, ImageSource value)
        {
            obj.SetValue(AnimatedSourceProperty, value);
        }

        public static readonly DependencyProperty AnimatedSourceProperty = DependencyProperty.RegisterAttached(
              "AnimatedSource",
              typeof(ImageSource),
              typeof(ImageBehavior),
              new UIPropertyMetadata(null,AnimatedSourceChanged));

        private static void AnimatedSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var imageControl = o as Image;
            if (imageControl == null)return;

            var oldValue = e.OldValue as ImageSource;
            var newValue = e.NewValue as ImageSource;
            if (oldValue != null)
            {
                imageControl.BeginAnimation(Image.SourceProperty, null);
            }
            if (newValue != null)
            {
                imageControl.DoWhenLoaded(InitAnimationOrImage);
            }
        }

        private static void InitAnimationOrImage(Image imageControl)
        {
            var source = GetAnimatedSource(imageControl) as BitmapSource;
            if(source == null)return;
            var decoder = GetDecoder(source) as GifBitmapDecoder;
            if (decoder != null && decoder.Frames.Count > 1)
            {
                var animation = new ObjectAnimationUsingKeyFrames();
                var totalDuration = TimeSpan.Zero;
                BitmapSource prevFrame = null;
                foreach (var rawFrame in decoder.Frames)
                {
                    var info = GetFrameInfo(rawFrame);

                    var frame = MakeFrame(
                        source,
                        rawFrame, info,
                        prevFrame);

                    var keyFrame = new DiscreteObjectKeyFrame(frame, totalDuration);
                    animation.KeyFrames.Add(keyFrame);
                    totalDuration += info.Delay;

                    if (info.DisposalMethod == FrameDisposalMethod.Replace || info.DisposalMethod == FrameDisposalMethod.Combine)
                    {
                        prevFrame = frame;
                    }
                    else if (info.DisposalMethod == FrameDisposalMethod.RestoreBackground)
                    {
                        prevFrame = IsFullFrame(info, source) ? null : ClearArea(frame, info);
                    }
                }

                animation.Duration = totalDuration;

                var repeatCount = GetRepeatCount(decoder);
                animation.RepeatBehavior = (repeatCount == 0) ? RepeatBehavior.Forever : new RepeatBehavior(repeatCount);

                if (animation.KeyFrames.Count > 0)
                    imageControl.Source = (ImageSource)animation.KeyFrames[0].Value;
                else
                    imageControl.Source = decoder.Frames[0];
                imageControl.BeginAnimation(Image.SourceProperty, animation);
                return;
            }
            imageControl.Source = source;
        }

        private static bool IsFullFrame(FrameInfo info, BitmapSource source)
        {
            return info.Left.Equals(0)
                   && info.Top.Equals(0)
                   && info.Width.Equals(source.Width)
                   && info.Height.Equals(source.Height);
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
                    var extApplication = Encoding.ASCII.GetString(bytes);
                    if (extApplication == application)
                        return extension;
                }
                query = $"/[{++count}]appext";
                extension = decoder.Metadata.GetQueryOrNull<BitmapMetadata>(query);
            }
            return null;
        }

        private static BitmapSource MakeFrame(BitmapSource fullImage,BitmapSource rawFrame, FrameInfo frameInfo, BitmapSource previousFrame)
        {
            if (previousFrame == null && IsFullFrame(frameInfo, fullImage))
            {
                return rawFrame;
            }

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                if (previousFrame != null)
                {
                    var fullRect = new Rect(0, 0, fullImage.PixelWidth, fullImage.PixelHeight);
                    context.DrawImage(previousFrame, fullRect);
                }

                context.DrawImage(rawFrame, frameInfo.Rect);
            }
            var bitmap = new RenderTargetBitmap(
                fullImage.PixelWidth, fullImage.PixelHeight,
                //fullImage.DpiX, fullImage.DpiY,
                96, 96,
                PixelFormats.Pbgra32);
            bitmap.Render(visual);

            if (bitmap.CanFreeze && !bitmap.IsFrozen)
                bitmap.Freeze();

            return bitmap;
        }

        private class FrameInfo
        {
            public TimeSpan Delay { get; set; }
            public FrameDisposalMethod DisposalMethod { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }

            public Rect Rect => new Rect(Left, Top, Width, Height);
        }

        private enum FrameDisposalMethod
        {
            Replace = 0,
            Combine = 1,
            RestoreBackground = 2,
            RestorePrevious = 3
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
    }

}

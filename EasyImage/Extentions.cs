using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AnimatedImage;
using AnimatedImage.Encoding;
using DealImage;
using WindowTemplate;
using Point = System.Windows.Point;

namespace EasyImage
{
    public static class Extentions
    {
        /// <summary>
        /// 从指定UI元素中获取指定位置转换信息
        /// </summary>
        /// <typeparam name="T">位置转换信息</typeparam>
        /// <param name="element">UI元素</param>
        /// <returns></returns>
        public static T GetTransform<T>(this UIElement element) where T : Transform, new()
        {
            var transform = element.RenderTransform;
            var targetTransform = transform as T;
            if (targetTransform != null)
            {
                return targetTransform;
            }
            else
            {
                var group = transform as TransformGroup;
                if (group != null)
                {
                    var count = group.Children.Count;
                    for (var i = count - 1; i >= 0; i--)
                    {
                        targetTransform = group.Children[i] as T;
                        if (targetTransform != null)
                        {
                            break;
                        }
                    }
                    if (targetTransform != null) return targetTransform;
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    return targetTransform;
                }
                else
                {
                    group = new TransformGroup();
                    if (transform != null)
                    {
                        group.Children.Add(transform);
                    }
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    element.RenderTransform = group;

                    return targetTransform;
                }
            }
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content)
        {
            var msgWin = new MessageWindow(new Message(content, MessageBoxMode.SingleMode))
            {
                //Owner = Owner,
                MiddleBtnContent = "确定(Y)",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            msgWin.SetOpacityAnimation(new DoubleAnimation(1, 0.1, new Duration(TimeSpan.FromSeconds(10))), msgWin.Close);
            msgWin.ShowDialog();
        }

        public static BitmapImage GetBitmapImage(this BitmapSource bitmapSource)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            var stream = new MemoryStream();
            encoder.Save(stream);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        /// <summary>
        /// 获取合并后的图片
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static BitmapImage GetCombinedBitmap(this IDictionary<FrameworkElement, FrameworkElement> dictionary, Rect rect)
        {
            var animatedGifs = dictionary.Values.Select((item, index) => new { item, index }).Where(m => ((AnimatedGif)m.item).Animatable).ToArray();
            if (animatedGifs.Length == 1)
            {
                var imageWidth = (int)rect.Width;
                var imageHeight = (int)rect.Height;
                var relationPoint = new Point(rect.X, rect.Y);

                //动态图信息
                var animatedGif = (AnimatedGif)animatedGifs[0].item;
                var index = animatedGifs[0].index;
                var element = dictionary.Keys.ElementAt(index);
                var width = (int)Math.Round(element.Width);
                var height = (int)Math.Round(element.Height);
                var angle = element.GetTransform<RotateTransform>().Angle;
                var scaleTransform = element.GetTransform<ScaleTransform>();
                var scaleX = scaleTransform.ScaleX;
                var scaleY = scaleTransform.ScaleY;
                var gifRect = animatedGif.GetRelationRect(relationPoint);
                var visualWidth = (int)Math.Round(gifRect.Width);
                var visualHeight = (int)Math.Round(gifRect.Height);

                var bitmapFrames = animatedGif.BitmapFrames;
                var stream = new MemoryStream();
                using (var encoder = new GifEncoder(stream, imageWidth, imageHeight, animatedGif.RepeatCount))
                {
                    var delays = animatedGif.Delays;
                    for (var i = 0; i < bitmapFrames.Count; i++)
                    {
                        var drawingVisual = new DrawingVisual();
                        using (var context = drawingVisual.RenderOpen())
                        {
                            var j = 0;
                            foreach (var item in dictionary)
                            {
                                if (j == index)
                                {
                                    //绘制动态图的每一帧
                                    var brush = new ImageBrush(bitmapFrames[i].GetMinContainBitmap(width, height, angle, scaleX, scaleY, visualWidth, visualHeight));
                                    context.DrawRectangle(brush, null, gifRect);
                                }
                                else
                                {
                                    var viewbox = item.Key.GetChildViewbox(item.Value);
                                    var brush = new VisualBrush(item.Key)
                                    {
                                        ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                                        Viewbox = viewbox,
                                    };
                                    context.DrawRectangle(brush, null, item.Value.GetRelationRect(relationPoint));
                                }
                                j++;
                            }
                        }
                        var renderBitmap = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Pbgra32);
                        renderBitmap.Render(drawingVisual);
                        using (var frame = renderBitmap.GetBitmap())
                        {
                            encoder.AppendFrame(frame, (int)delays[i].TotalMilliseconds);
                        }
                    }
                }
                stream.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            else
            {
                return dictionary.GetMinContainBitmap().GetBitmapImage();
            }
           
        }

        public static async Task<BitmapImage> GetBitmapImage(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            var stream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(stream);
            }
            stream.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static BitmapImage GetBitmapFormFileIcon(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            var stream = new MemoryStream();
            using (var icon = Icon.ExtractAssociatedIcon(filePath))
            {
                if (icon != null)
                {
                    var bitmap = icon.ToBitmap();
                    bitmap.Save(stream, ImageFormat.Png);
                }
            }
            stream.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        #region 元素是否相交
        public static bool IsOverlapped(this FrameworkElement element, FrameworkElement otherElement)
        {
            var rect = VisualTreeHelper.GetDescendantBounds(element);
            var rect1 = VisualTreeHelper.GetDescendantBounds(otherElement);
            var width = rect.Width;
            var height = rect.Height;
            var width1 = rect1.Width;
            var height1 = rect1.Height;

            var topLeft = element.PointToScreen(new Point(0, 0));
            var topRight = element.PointToScreen(new Point(rect.Width, 0));
            var bottomRight = element.PointToScreen(new Point(rect.Width, rect.Height));
            var bottomLeft = element.PointToScreen(new Point(0, rect.Height));

            var topLeft1 = otherElement.PointToScreen(new Point(0, 0));
            var topRight1 = otherElement.PointToScreen(new Point(rect1.Width, 0));
            var bottomRight1 = otherElement.PointToScreen(new Point(rect1.Width, rect1.Height));
            var bottomLeft1 = otherElement.PointToScreen(new Point(0, rect1.Height));

            //端点在矩形内
            if (element.IsContainPoint(topLeft1, width, height) ||
                element.IsContainPoint(topRight1, width, height) ||
                element.IsContainPoint(bottomRight1, width, height) ||
                element.IsContainPoint(bottomLeft1, width, height) ||
                otherElement.IsContainPoint(topLeft, width1, height1) ||
                otherElement.IsContainPoint(topRight, width1, height1) ||
                otherElement.IsContainPoint(bottomRight, width1, height1) ||
                otherElement.IsContainPoint(bottomLeft, width1, height1))
            {
                return true;
            }

            //边相交
            return IsLineIntersect(topLeft, topRight, topLeft1, topRight1) || IsLineIntersect(topLeft, topRight, topRight1, bottomRight1) || IsLineIntersect(topLeft, topRight, bottomRight1, bottomLeft1) || IsLineIntersect(topLeft, topRight, bottomLeft1, topLeft1) || IsLineIntersect(topRight, bottomRight, topLeft1, topRight1) || IsLineIntersect(topRight, bottomRight, topRight1, bottomRight1) || IsLineIntersect(topRight, bottomRight, bottomRight1, bottomLeft1) || IsLineIntersect(topRight, bottomRight, bottomLeft1, topLeft1) || IsLineIntersect(bottomRight, bottomLeft, topLeft1, topRight1) || IsLineIntersect(bottomRight, bottomLeft, topRight1, bottomRight1) || IsLineIntersect(bottomRight, bottomLeft, bottomRight1, bottomLeft1) || IsLineIntersect(bottomRight, bottomLeft, bottomLeft1, topLeft1) || IsLineIntersect(bottomLeft, topLeft, topLeft1, topRight1) || IsLineIntersect(bottomLeft, topLeft, topRight1, bottomRight1) || IsLineIntersect(bottomLeft, topLeft, bottomRight1, bottomLeft1) || IsLineIntersect(bottomLeft, topLeft, bottomLeft1, topLeft1);
        }

        private static bool IsContainPoint(this Visual element, Point point, double width, double height)
        {
   
            var translatedPoint = element.PointFromScreen(point);
            return (translatedPoint.X > 0) && (translatedPoint.X < width) && (translatedPoint.Y > 0) && (translatedPoint.Y < height);
        }

        private static bool IsLineIntersect(Point start1, Point end1, Point start2, Point end2)
        {
            var x1 = end1.X - start1.X;
            var y1 = end1.Y - start1.Y;
            var x2 = end2.X - start2.X;
            var y2 = end2.Y - start2.Y;

            var denom = x1 * y2 - x2 * y1;

            //parallel
            if (denom.Equals(0)) return false;

            var s = (-y1 * (start1.X - start2.X) + x1 * (start1.Y - start2.Y)) / denom;
            var t = (x2 * (start1.Y - start2.Y) - y2 * (start1.X - start2.X)) / denom;

            return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
        }
        
        #endregion

    }
}

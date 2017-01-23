using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WindowTemplate;

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
                MiddleBtnContent = "确定",
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

        public static BitmapImage GetBitmapImage(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            var stream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(stream);
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
            using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
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

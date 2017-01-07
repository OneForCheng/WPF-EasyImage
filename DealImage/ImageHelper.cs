using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace DealImage
{
    public static class ImageHelper
    {
        public  static BitmapSource GetResizeBitmap(this BitmapSource bitmapSource, int width, int height)
        {
            var transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = bitmapSource;
            var scaleX = width / (double)bitmapSource.PixelWidth;
            var scaleY = height / (double)bitmapSource.PixelHeight;
            var scaleTransform = new ScaleTransform(scaleX, scaleY, bitmapSource.Width / 2, bitmapSource.Height / 2);
            transformedBitmap.Transform = scaleTransform;
            transformedBitmap.EndInit();
            return transformedBitmap;
        }

        public static Bitmap GetBitmap(this BitmapSource source)
        {
            var bmp = new Bitmap
            (
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            var data = bmp.LockBits
             (
                 new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                 ImageLockMode.WriteOnly,
                 System.Drawing.Imaging.PixelFormat.Format32bppPArgb
             );

            source.CopyPixels
            (
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride
            );

            bmp.UnlockBits(data);

            return bmp;
        }

        public static BitmapSource GetBitmapSource(this Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
        }

        public static ImageExtension GetImageExtension(string filePath)
        {
            if (!File.Exists(filePath)) return ImageExtension.Unknow;

            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileExt = GetImageExtension(fs);
            fs.Close();
            return fileExt;
        }

        public static ImageExtension GetImageExtension(Stream stream)
        {
            ImageExtension extension;
            try
            {
                stream.Position = 0;
                var reader = new BinaryReader(stream);
                var data = reader.ReadByte();
                var fileType = string.Empty;
                fileType += data.ToString();
                data = reader.ReadByte();
                fileType += data.ToString();
                extension = (ImageExtension)Enum.Parse(typeof(ImageExtension), fileType);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                extension = ImageExtension.Unknow;
            }
            return extension;
        }

        public static BitmapSource GetMinContainBitmap(this IDictionary<FrameworkElement, FrameworkElement> dictionary, SolidColorBrush backgrand = null)
        {
            var rect = dictionary.Values.GetMinContainRect();
            var relationPoint = new Point(rect.X, rect.Y);
            rect.X = rect.Y = 0;
            
            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                foreach (var item in dictionary)
                {
                    var viewbox = item.Key.GetChildViewbox(item.Value);
                    var brush = new VisualBrush(item.Key)
                    {
                        ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                        Viewbox = viewbox,
                    };
                    context.DrawRectangle(brush, null, item.Value.GetRelationRect(relationPoint));
                }
            }

            var renderBitmap = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Pbgra32);
            var rectangle = new Rectangle
            {
                Width = (int) rect.Width,
                Height = (int) rect.Height,
                Fill = backgrand??Brushes.Transparent,
            };
            rectangle.Measure(rect.Size);
            rectangle.Arrange(new Rect(rect.Size));

            renderBitmap.Render(rectangle);
            renderBitmap.Render(drawingVisual);

            return renderBitmap;
        }

        public static Rect GetMinContainRect(this FrameworkElement element)
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            var rect = VisualTreeHelper.GetDescendantBounds(element);
            var topLeft = element.PointToScreen(new Point(0, 0));
            var topRight = element.PointToScreen(new Point(rect.Width, 0));
            var bottomRight = element.PointToScreen(new Point(rect.Width, rect.Height));
            var bottomLeft = element.PointToScreen(new Point(0, rect.Height));
            minX = Math.Min(minX, Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)));
            maxX = Math.Max(maxX, Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)));
            minY = Math.Min(minY, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)));
            maxY = Math.Max(maxY, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)));

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public static Rect GetMinContainRect(this IEnumerable<FrameworkElement> elements)
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            foreach (var element in elements)
            {
                var rect = VisualTreeHelper.GetDescendantBounds(element);
                var topLeft = element.PointToScreen(new Point(0, 0));
                var topRight = element.PointToScreen(new Point(rect.Width, 0));
                var bottomRight = element.PointToScreen(new Point(rect.Width, rect.Height));
                var bottomLeft = element.PointToScreen(new Point(0, rect.Height));
                minX = Math.Min(minX, Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X)));
                maxX = Math.Max(maxX, Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)));
                minY = Math.Min(minY, Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y)));
                maxY = Math.Max(maxY, Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)));
            }
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private static Rect GetRelationRect(this FrameworkElement element, Point relationPoint)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            var rect = VisualTreeHelper.GetDescendantBounds(element);

            var topLeftPoint = element.PointToScreen(new Point(0, 0));
            var topRightPoint = element.PointToScreen(new Point(rect.Width, 0));
            var bottomRightPoint = element.PointToScreen(new Point(rect.Width, rect.Height));
            var bottomLeftPoint = element.PointToScreen(new Point(0, rect.Height));
            var width = Math.Max(Math.Abs(topLeftPoint.X - bottomRightPoint.X), Math.Abs(topRightPoint.X - bottomLeftPoint.X));
            var height = Math.Max(Math.Abs(topLeftPoint.Y - bottomRightPoint.Y), Math.Abs(topRightPoint.Y - bottomLeftPoint.Y));
            minX = Math.Min(minX, Math.Min(Math.Min(topLeftPoint.X, topRightPoint.X), Math.Min(bottomLeftPoint.X, bottomRightPoint.X)));
            minY = Math.Min(minY, Math.Min(Math.Min(topLeftPoint.Y, topRightPoint.Y), Math.Min(bottomLeftPoint.Y, bottomRightPoint.Y)));

            return new Rect(minX - relationPoint.X, minY - relationPoint.Y, width, height);
        }

        private static Rect GetChildViewbox(this FrameworkElement element, FrameworkElement childElement)
        {
            var rect = VisualTreeHelper.GetDescendantBounds(element);
            var childRect = VisualTreeHelper.GetDescendantBounds(childElement);

            var topLeftPoint1 = element.PointToScreen(rect.TopLeft);
            var topRightPoint1 = element.PointToScreen(rect.TopRight);
            var bottomRightPoint1 = element.PointToScreen(rect.BottomRight);
            var bottomLeftPoint1 = element.PointToScreen(rect.BottomLeft);

            var minX1 = Math.Min(Math.Min(topLeftPoint1.X, topRightPoint1.X), Math.Min(bottomLeftPoint1.X, bottomRightPoint1.X));
            var minY1 = Math.Min(Math.Min(topLeftPoint1.Y, topRightPoint1.Y), Math.Min(bottomLeftPoint1.Y, bottomRightPoint1.Y));

            var topLeftPoint2 = childElement.PointToScreen(childRect.TopLeft);
            var topRightPoint2 = childElement.PointToScreen(childRect.TopRight);
            var bottomRightPoint2 = childElement.PointToScreen(childRect.BottomRight);
            var bottomLeftPoint2 = childElement.PointToScreen(childRect.BottomLeft);

            var minX2 = Math.Min(Math.Min(topLeftPoint2.X, topRightPoint2.X), Math.Min(bottomLeftPoint2.X, bottomRightPoint2.X));
            var minY2 = Math.Min(Math.Min(topLeftPoint2.Y, topRightPoint2.Y), Math.Min(bottomLeftPoint2.Y, bottomRightPoint2.Y));

            var y = Math.Abs(minY2 - minY1);
            var x = Math.Abs(minX2 - minX1);
            var width = Math.Max(Math.Abs(topLeftPoint1.X - bottomRightPoint1.X), Math.Abs(topRightPoint1.X - bottomLeftPoint1.X));
            var height = Math.Max(Math.Abs(topLeftPoint1.Y - bottomRightPoint1.Y), Math.Abs(topRightPoint1.Y - bottomLeftPoint1.Y));
            var childWidth = Math.Max(Math.Abs(topLeftPoint2.X - bottomRightPoint2.X), Math.Abs(topRightPoint2.X - bottomLeftPoint2.X));
            var childHeight = Math.Max(Math.Abs(topLeftPoint2.Y - bottomRightPoint2.Y), Math.Abs(topRightPoint2.Y - bottomLeftPoint2.Y));

            return new Rect(x / width, y / height, childWidth / width, childHeight / height);
        }

        //public static Point RotateTransform(this Point point, Point centerPoint, double angle)
        //{
        //    var radian = angle * Math.PI / 180;
        //    var rotatedX = (point.X - centerPoint.X) * Math.Cos(radian) + (point.Y - centerPoint.Y) * Math.Sin(radian) + centerPoint.X;
        //    var rotatedY = -(point.X - centerPoint.X) * Math.Sin(radian) + (point.Y - centerPoint.Y) * Math.Cos(radian) + centerPoint.Y;
        //    return new Point(rotatedX, rotatedY);
        //}
    }
}

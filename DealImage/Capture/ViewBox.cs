using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace DealImage.Capture
{
    public class ViewBox
    {
        public bool IsRotated { get; }

        public Transform RenderTransform { get; }

        public Rect ScreenshotBox { get; }

        public Rect RotateShowbox { get;}//旋转呈现框

        public Rect CropViewbox { get; }//视图裁剪框

        public ViewBox(UIElement element, double angle = 0,double scaleX = 1, double scaleY = 1)
        {
            IsRotated = !angle.Equals(0);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform(-angle, 0.5, 0.5));
            transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY, 0.5, 0.5));
            RenderTransform = transformGroup;

            var rect = VisualTreeHelper.GetDescendantBounds(element);

            var topLeft = element.TranslatePoint(new Point(0, 0), null);
            var topRight = element.TranslatePoint(new Point(rect.Width, 0), null);
            var bottomRight = element.TranslatePoint(new Point(rect.Width, rect.Height), null);
            var bottomLeft = element.TranslatePoint(new Point(0, rect.Height), null);

            var minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
            var maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
            var minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
            var maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

            ScreenshotBox = new Rect(minX, minY, maxX - minX, maxY - minY);
            
            var bevelSideLength = Math.Sqrt((maxX-minX)*(maxX - minX) + (maxY - minY)*(maxY - minY));
            RotateShowbox = new Rect(0,0, bevelSideLength, bevelSideLength);
            CropViewbox = new Rect((bevelSideLength - rect.Width) /2,(bevelSideLength - rect.Height)/2, rect.Width, rect.Height);

        }

    }
}

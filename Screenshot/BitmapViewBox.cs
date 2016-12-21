using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Screenshot
{
    public class BitmapViewBox : ViewBox
    {
        public Bitmap Bitmap { get; }

        public BitmapViewBox(UIElement element, ImageSource bitmapSource, double angle = 0, double scaleX = 1, double scaleY = 1, double correctionX = 0, double correctionY = 0) : base(element, angle, scaleX, scaleY, correctionX, correctionY)
        {
            var width = (int) Math.Round(CropViewbox.Width, 0);
            var height = (int) Math.Round(CropViewbox.Height, 0);

            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                var brush = new ImageBrush(bitmapSource);
                context.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }
            
            var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);
            Bitmap = renderBitmap.GetBitmap();

        }

    }
}
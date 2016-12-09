using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace IconMaker
{
    internal class IconBitmapFramesCollection : List<BitmapFrame>
    {
        public new void Add(BitmapFrame item)
        {
            if (item == null)
                throw new NullReferenceException("BitmapFrame cannot be null");
            if (item.PixelWidth > 256)
                throw new InvalidOperationException("The width of the frame cannot be greater than 256");
            if (item.PixelHeight > 256)
                throw new InvalidOperationException("The height of the frame cannot be greater than 256");
            if (item.PixelWidth < 16)
                throw new InvalidOperationException("The frame width cannot be less than 16");
            if (item.PixelHeight < 16)
                throw new InvalidOperationException("The frame height cannot be less than 16");
            if (item.PixelWidth != item.PixelHeight)
                throw new InvalidOperationException("BitmapFrame must be square");
            base.Add(item);
        }

        public new void Insert(int index, BitmapFrame item)
        {
            if (item == null)
                throw new NullReferenceException("BitmapFrame cannot be null");
            if (item.PixelWidth > 256)
                throw new InvalidOperationException("The width of the frame cannot be greater than 256");
            if (item.PixelHeight > 256)
                throw new InvalidOperationException("The height of the frame cannot be greater than 256");
            if (item.PixelWidth < 16)
                throw new InvalidOperationException("The frame width cannot be less than 16");
            if (item.PixelHeight < 16)
                throw new InvalidOperationException("The frame height cannot be less than 16");
            if (item.PixelWidth != item.PixelHeight)
                throw new InvalidOperationException("BitmapFrame must be square");
            base.Insert(index, item);
        }

        public void SortDescending()
        {
            Sort((x, y) =>
            {
                if (x.PixelWidth > y.PixelWidth)
                {
                    return -1;
                }
                return x.PixelWidth < y.PixelWidth ? 1 : 0;
            });
        }

        public void SortAscending()
        {
            Sort((x, y) =>
            {
                if (x.PixelWidth > y.PixelWidth)
                {
                    return 1;
                }
                return x.PixelWidth < y.PixelWidth ? -1 : 0;
            });
        }

    }
}

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace AnimatedImage.Encoding
{
    public class OctreeQuantizer : Quantizer
    {
        private readonly int _maxColors;
        private readonly Octree _octree;

        public OctreeQuantizer(int maxColors, int maxColorBits) : base(false)
        {
            if (maxColors > 0xff)
            {
                throw new ArgumentOutOfRangeException(nameof(maxColors), maxColors, "The number of colors should be less than 256");
            }
            if ((maxColorBits < 1) | (maxColorBits > 8))
            {
                throw new ArgumentOutOfRangeException(nameof(maxColorBits), maxColorBits, "This should be between 1 and 8");
            }
            _octree = new Octree(maxColorBits);
            _maxColors = maxColors;
        }

        protected override ColorPalette GetPalette(ColorPalette original)
        {
            var list = _octree.Palletize(_maxColors - 1);
            for (var i = 0; i < list.Count; i++)
            {
                original.Entries[i] = (Color)list[i];
            }
            original.Entries[_maxColors] = Color.FromArgb(0, 0, 0, 0);
            return original;
        }

        protected override unsafe void InitialQuantizePixel(Color32* pixel)
        {
            _octree.AddColor(pixel);
        }

        protected override unsafe byte QuantizePixel(Color32* pixel)
        {
            var paletteIndex = (byte)_maxColors;
            if (pixel->Alpha > 254)
            {
                paletteIndex = (byte)_octree.GetPaletteIndex(pixel);
            }
            return paletteIndex;
        }

        private class Octree
        {
            private int _leafCount;
            private readonly int _maxColorBits;
            private int _previousColor;
            private OctreeNode _previousNode;
            private readonly OctreeNode[] _reducibleNodes;
            private readonly OctreeNode _root;
            private static readonly int[] Mask = { 0x80, 0x40, 0x20, 0x10, 8, 4, 2, 1 };

            public Octree(int maxColorBits)
            {
                _maxColorBits = maxColorBits;
                _leafCount = 0;
                _reducibleNodes = new OctreeNode[9];
                _root = new OctreeNode(0, _maxColorBits, this);
                _previousColor = 0;
                _previousNode = null;
            }

            public unsafe void AddColor(Color32* pixel)
            {
                if (_previousColor == pixel->ARGB)
                {
                    if (null == _previousNode)
                    {
                        _previousColor = pixel->ARGB;
                        _root.AddColor(pixel, _maxColorBits, 0, this);
                    }
                    else
                    {
                        _previousNode.Increment(pixel);
                    }
                }
                else
                {
                    _previousColor = pixel->ARGB;
                    _root.AddColor(pixel, _maxColorBits, 0, this);
                }
            }

            public unsafe int GetPaletteIndex(Color32* pixel)
            {
                return _root.GetPaletteIndex(pixel, 0);
            }

            public ArrayList Palletize(int colorCount)
            {
                while (Leaves > colorCount)
                {
                    Reduce();
                }
                var palette = new ArrayList(Leaves);
                var paletteIndex = 0;
                _root.ConstructPalette(palette, ref paletteIndex);
                return palette;
            }

            public void Reduce()
            {
                var index = _maxColorBits - 1;
                while ((index > 0) && (null == _reducibleNodes[index]))
                {
                    index--;
                }
                var node = _reducibleNodes[index];
                _reducibleNodes[index] = node.NextReducible;
                _leafCount -= node.Reduce();
                _previousNode = null;
            }

            protected void TrackPrevious(OctreeNode node)
            {
                _previousNode = node;
            }

            public int Leaves
            {
                get
                {
                    return _leafCount;
                }
                set
                {
                    _leafCount = value;
                }
            }

            protected OctreeNode[] ReducibleNodes => _reducibleNodes;

            protected class OctreeNode
            {
                private ulong _blue;
                private ulong _green;
                private bool _leaf;
                private int _paletteIndex;
                private ulong _pixelCount;
                private ulong _red;

                public OctreeNode(int level, int colorBits, Octree octree)
                {
                    _leaf = level == colorBits;
                    _red = _green = _blue = 0;
                    _pixelCount = 0;
                    if (_leaf)
                    {
                        octree.Leaves++;
                        NextReducible = null;
                        Children = null;
                    }
                    else
                    {
                        NextReducible = octree.ReducibleNodes[level];
                        octree.ReducibleNodes[level] = this;
                        Children = new OctreeNode[8];
                    }
                }

                public unsafe void AddColor(Color32* pixel, int colorBits, int level, Octree octree)
                {
                    if (_leaf)
                    {
                        Increment(pixel);
                        octree.TrackPrevious(this);
                    }
                    else
                    {
                        var num = 7 - level;
                        var index = (((pixel->Red & Mask[level]) >> (num - 2)) | ((pixel->Green & Mask[level]) >> (num - 1))) | ((pixel->Blue & Mask[level]) >> num);
                        var node = Children[index];
                        if (null == node)
                        {
                            node = new OctreeNode(level + 1, colorBits, octree);
                            Children[index] = node;
                        }
                        node.AddColor(pixel, colorBits, level + 1, octree);
                    }
                }

                public void ConstructPalette(ArrayList palette, ref int paletteIndex)
                {
                    if (_leaf)
                    {
                        _paletteIndex = paletteIndex++;
                        palette.Add(Color.FromArgb((int)(_red / _pixelCount), (int)(_green / _pixelCount), (int)(_blue / _pixelCount)));
                    }
                    else
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            if (null != Children[i])
                            {
                                Children[i].ConstructPalette(palette, ref paletteIndex);
                            }
                        }
                    }
                }

                public unsafe int GetPaletteIndex(Color32* pixel, int level)
                {
                    var num = _paletteIndex;
                    if (_leaf)
                    {
                        return num;
                    }
                    var num2 = 7 - level;
                    var index = (((pixel->Red & Mask[level]) >> (num2 - 2)) | ((pixel->Green & Mask[level]) >> (num2 - 1))) | ((pixel->Blue & Mask[level]) >> num2);
                    if (null == Children[index])
                    {
                        throw new Exception("Didn't expect this!");
                    }
                    return Children[index].GetPaletteIndex(pixel, level + 1);
                }

                public unsafe void Increment(Color32* pixel)
                {
                    _pixelCount++;
                    _red += pixel->Red;
                    _green += pixel->Green;
                    _blue += pixel->Blue;
                }

                public int Reduce()
                {
                    _red = _green = _blue = 0;
                    var num = 0;
                    for (var i = 0; i < 8; i++)
                    {
                        if (null != Children[i])
                        {
                            _red += Children[i]._red;
                            _green += Children[i]._green;
                            _blue += Children[i]._blue;
                            _pixelCount += Children[i]._pixelCount;
                            num++;
                            Children[i] = null;
                        }
                    }
                    _leaf = true;
                    return (num - 1);
                }

                public OctreeNode[] Children { get; }

                public OctreeNode NextReducible { get; private set; }
            }
        }
    }
}

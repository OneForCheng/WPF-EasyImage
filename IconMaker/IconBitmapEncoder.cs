using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace IconMaker
{
    public class IconBitmapEncoder : DispatcherObject
    {
        #region Constructor

        public IconBitmapEncoder()
        {
            _frames = new IconBitmapFramesCollection();
        }

        #endregion

        #region Fields
        private readonly IconBitmapFramesCollection _frames;

        #endregion

        #region Properties

        public IList<BitmapFrame> Frames => _frames;

        #endregion

        #region Methods

        public void Save(Stream stream)
        {
            _frames.SortAscending();
            stream.Position = 0;
            var writer = new BinaryWriter(stream, System.Text.Encoding.UTF32);

            var framesCount = Convert.ToUInt16(_frames.Count);
            const ushort fileHeaderLength = 6;
            const ushort frameHeaderLength = 16;

            var fileHeader = new IconDir(framesCount);
            writer.Write(fileHeader.IdReserved);
            writer.Write(fileHeader.IdType);
            writer.Write(fileHeader.IdCount);

            var data = new byte[framesCount][];

            foreach (var frame in _frames)
            {
                var frameIndex = _frames.IndexOf(frame);
                data[frameIndex] = GetPngData(frame);
                //if (frame.PixelWidth == 256)
                //{
                //    data[frameIndex] = GetPngData(frame);
                //}
                //else
                //{
                //    data[frameIndex] = GetBmpData(frame);
                //}
            }

            uint frameDataOffset = fileHeaderLength;
            frameDataOffset += (uint)(frameHeaderLength * framesCount);

            foreach (var frame in _frames)
            {
                var frameIndex = _frames.IndexOf(frame);
                if (frameIndex > 0)
                {
                    frameDataOffset += Convert.ToUInt32(data[frameIndex - 1].Length);
                }
                var frameHeader = new IconDirEntry((ushort)frame.PixelWidth, (ushort)frame.PixelHeight, Convert.ToUInt16(frame.Format.BitsPerPixel), Convert.ToUInt32(data[frameIndex].Length), frameDataOffset);
                writer.Write(frameHeader.BWidth);
                writer.Write(frameHeader.BHeight);
                writer.Write(frameHeader.BColorCount);
                writer.Write(frameHeader.BReserved);
                writer.Write(frameHeader.WPlanes);
                writer.Write(frameHeader.WBitCount);
                writer.Write(frameHeader.DwBytesInRes);
                writer.Write(frameHeader.DwImageOffset);
            }

            foreach (var frameData in data)
            {
                writer.Write(frameData);
            }

        }

        private byte[] GetPngData(BitmapFrame frame)
        {
            var dataStream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame);
            encoder.Save(dataStream);
            var data = dataStream.GetBuffer();
            dataStream.Close();
            return data;
        }

        //private byte[] GetBmpData(BitmapFrame frame)
        //{
        //    var dataStream = new MemoryStream();
        //    var encoder = new BmpBitmapEncoder();
        //    encoder.Frames.Add(frame);
        //    encoder.Save(dataStream);
        //    dataStream.Position = 14;
        //    var dataStreamReader = new BinaryReader(dataStream, System.Text.Encoding.UTF32);
        //    var outDataStream = new MemoryStream();
        //    var outDataStreamWriter = new BinaryWriter(outDataStream, System.Text.Encoding.UTF32);
        //    outDataStreamWriter.Write(dataStreamReader.ReadUInt32());
        //    outDataStreamWriter.Write(dataStreamReader.ReadInt32());
        //    var height = dataStreamReader.ReadInt32();
        //    if (height > 0)
        //    {
        //        height = height * 2;
        //    }
        //    else if (height < 0)
        //    {
        //        height = -(height * 2);
        //    }
        //    else
        //    {
        //        height = 0;
        //    }
        //    outDataStreamWriter.Write(height);
        //    for (var i = 26; i <= dataStream.Length - 1; i++)
        //    {
        //        outDataStream.WriteByte((byte)(dataStream.ReadByte()));
        //    }
        //    var data = outDataStream.GetBuffer();
        //    outDataStreamWriter.Close();
        //    outDataStream.Close();
        //    dataStreamReader.Close();
        //    dataStream.Close();
        //    return data;
        //}

        #endregion

        #region Icon File Structures

        private struct IconDir
        {
            //Reserved (must be 0)
            public readonly ushort IdReserved;
            //Resource Type (1 for icons)
            public readonly ushort IdType;
            //How many images?

            public readonly ushort IdCount;
            public IconDir(ushort count)
            {
                IdReserved = Convert.ToUInt16(0);
                IdType = Convert.ToUInt16(1);
                IdCount = count;
            }

        }

        private struct IconDirEntry
        {

            //Width, in pixels, of the image
            public readonly byte BWidth;
            //Height, in pixels, of the image
            public readonly byte BHeight;
            //Number of colors in image (0 if >=8bpp)
            public readonly byte BColorCount;
            //Reserved ( must be 0)
            public readonly byte BReserved;
            //Color Planes
            public readonly ushort WPlanes;
            //Bits per pixel
            public readonly ushort WBitCount;
            //How many bytes in this resource?
            public readonly uint DwBytesInRes;
            //Where in the file is this image?

            public readonly uint DwImageOffset;

            public IconDirEntry(ushort width, ushort height, ushort bitsPerPixel, uint resSize, uint imageOffset)
            {
                BWidth = width == 256 ? Convert.ToByte(0) : Convert.ToByte(width);
                BHeight = height == 256 ? Convert.ToByte(0) : Convert.ToByte(height);
                BColorCount = Convert.ToByte(bitsPerPixel == 4 ? 16 : 0);
                BReserved = Convert.ToByte(0);
                WPlanes = Convert.ToUInt16(1);
                WBitCount = bitsPerPixel;
                DwBytesInRes = resSize;
                DwImageOffset = imageOffset;
            }

        }

        #endregion

        #region Helpers

        public static BitmapSource Get4BitImage(BitmapSource source)
        {
            var @out = new FormatConvertedBitmap(source, PixelFormats.Indexed4, BitmapPalettes.Halftone8, 0);
            return @out;
        }

        public static BitmapSource Get8BitImage(BitmapSource source)
        {
            var @out = new FormatConvertedBitmap(source, PixelFormats.Indexed8, BitmapPalettes.Halftone256, 0);
            return @out;
        }

        public static BitmapSource Get24Plus8BitImage(BitmapSource source)
        {
            var @out = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            return @out;
        }

        public static BitmapSource GetResized(BitmapSource source, int size)
        {
            var backup = source.Clone();
            try
            {
                var scaled = new TransformedBitmap();
                scaled.BeginInit();
                scaled.Source = source;
                var scX = size / (double)source.PixelWidth;
                var scy = size / (double)source.PixelHeight;
                var tr = new ScaleTransform(scX, scy, source.Width / 2, source.Height / 2);
                scaled.Transform = tr;
                scaled.EndInit();
                source = scaled;
            }
            catch
            {
                source = backup;
            }
            return source;
        }

        #endregion

    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AnimatedImage.Encoding
{
    public class GifEncoder : IDisposable
    {
        private readonly BinaryWriter _bWriter;
        private readonly MemoryStream _mStream;
        private readonly Quantizer _quantizer;

        private static readonly byte[] GifHeader = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };//GIF89a
        private static readonly byte[] LogicalScreen = { 0x70, 0x00, 0x00 };//无全局色彩表 无视背景色 无视像素纵横比
        private static readonly byte[] AppExtension = {
                0x21,0xff,0x0b, //块标志
                0x4e,0x45,0x54,0x53,0x43,0x41,0x50,0x45,0x32,0x2e,0x30, //NETSCAPE2.0
                0x03,0x01};//其他信息
        private static readonly byte[] GifEnd = { 0x3b };//结束信息


        public GifEncoder(Stream stream, int width, int height, int repeatCount = 0)
        {
            _bWriter = new BinaryWriter(stream);
            _mStream = new MemoryStream();
            _quantizer = new OctreeQuantizer(255, 8);
            WriteHeader(width, height, repeatCount);
        }

        private void WriteHeader(int width, int height, int repeatCount)
        {
            //写入gif头信息
            _bWriter.Write(GifHeader);
            _bWriter.Write((ushort)width);
            _bWriter.Write((ushort)height);
            _bWriter.Write(LogicalScreen);
            //写入其他消息
            _bWriter.Write(AppExtension);
            //写入循环标记
            _bWriter.Write((ushort)repeatCount);
            //终结
            _bWriter.Write((byte)0);
        }

        public void AppendFrame(Bitmap image, int delay, int offsetX = 0, int offsetY = 0)
        {
            _mStream.SetLength(0);
            _mStream.Position = 0;
            using (var tempGif = _quantizer.Quantize(image))
            {
                tempGif.Save(_mStream, ImageFormat.Gif);
            }

            
            var tempArray = _mStream.GetBuffer();
            // 781开始为Graphic Control Extension块 标志为21 F9 04 
            tempArray[784] = 0x09; //图像刷新时屏幕返回初始帧 貌似不赋值会出bug
            delay = delay / 10;
            tempArray[785] = (byte)(delay & 0xff);
            tempArray[786] = (byte)(delay >> 8 & 0xff); //写入2字节的帧delay 
             
            tempArray[787] = 0xff;// 787为透明色索引  788为块结尾0x00
            // 789开始为Image Descriptor块 标志位2C
            // 790~793为帧偏移大小 默认为0
            tempArray[790] = (byte)(offsetX & 0xff);
            tempArray[791] = (byte)(offsetX >> 8 & 0xff);//写入2字节水平偏移
            tempArray[792] = (byte)(offsetY & 0xff);
            tempArray[793] = (byte)(offsetY >> 8 & 0xff);//写入2字节竖直偏移

            // 794~797为帧图像大小 默认他
            tempArray[798] = (byte)(tempArray[798] | 0X87); //本地色彩表标志

            //写入到gif文件
            _bWriter.Write(tempArray, 781, 18);
            _bWriter.Write(tempArray, 13, 768);
            _bWriter.Write(tempArray, 799, (int)_mStream.Length - 800);
        }

        public void Dispose()
        {
            _bWriter.Write(GifEnd);
            _mStream?.Dispose();
        }
    }
}

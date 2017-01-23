using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Drawing
{
    internal class DrawingManager : IDisposable
    {
        #region Private fields
        private readonly WriteableBitmap _writeableBitmap;
        private readonly List<Bitmap> _recordedBitmaps;
        private TextureBrush _mosaicBrush;
        private Bitmap _currentViewBitmap;
        private Button _redoButton;
        private Button _undoButton;
        //private byte[] _bitmapBuffer;
        private int _nextUndo;

        #endregion

        #region Constructor

        public DrawingManager(Image image,Bitmap originBitmap)
        {
            _nextUndo = 0;
            _recordedBitmaps = new List<Bitmap> { originBitmap };
            _writeableBitmap = new WriteableBitmap(Imaging.CreateBitmapSourceFromHBitmap(originBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            image.Source = _writeableBitmap;
            _currentViewBitmap = (Bitmap)originBitmap.Clone();
            using (var mosaic = originBitmap.ToMosaic(10))
            {
                _mosaicBrush = new TextureBrush(mosaic);
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// 最后记录的图像
        /// </summary>
        public Bitmap LastRecordedBitmap => _recordedBitmaps.ElementAt(_nextUndo);

        /// <summary>
        /// 当前的视图图像
        /// </summary>
        public Bitmap CurrentViewBitmap => _currentViewBitmap;

        /// <summary>
        /// 能否执行撤销操作
        /// </summary>
        public bool CanUndo => (_nextUndo > 0);

        /// <summary>
        /// 能否执行反撤销操作
        /// </summary>
        public bool CanRedo => (_nextUndo != _recordedBitmaps.Count - 1);

        /// <summary>
        /// 反撤销按钮
        /// </summary>
        public Button RedoButtuon
        {
            set { _redoButton = value; }
        }

        /// <summary>
        /// 撤销按钮
        /// </summary>
        public Button UndoButton
        {
            set { _undoButton = value; }
        }

        /// <summary>
        /// 马赛克画刷
        /// </summary>
        public Brush MosaicBrush => _mosaicBrush;

        #endregion

        #region Public methods
        /// <summary>
        /// 描绘图像
        /// </summary>
        /// <param name="bitmap"></param>
        public void Drawing(Bitmap bitmap)
        {
           
            if (CanRedo)
            {
                for (var i = _recordedBitmaps.Count - 1; i > _nextUndo; i--)
                {
                    _recordedBitmaps.ElementAt(i).Dispose();
                    _recordedBitmaps.RemoveAt(i);
                }
            }
            _recordedBitmaps.Add(bitmap);
            _nextUndo++;

            _mosaicBrush.Dispose();
            using (var mosaic = bitmap.ToMosaic(10))
            {
                _mosaicBrush = new TextureBrush(mosaic);
            }

            _currentViewBitmap.Dispose();
            _currentViewBitmap = (Bitmap)bitmap.Clone();

            UpdateView(_currentViewBitmap);
            if (_redoButton != null)
            {
                _redoButton.IsEnabled = CanRedo;
            }
            if (_undoButton != null)
            {
                _undoButton.IsEnabled = CanUndo;
            }
        }

        /// <summary>
        /// 记录当前呈现图像
        /// </summary>
        public void Record()
        {
            if (CanRedo)
            {
                for (var i = _recordedBitmaps.Count - 1; i > _nextUndo; i--)
                {
                    _recordedBitmaps.ElementAt(i).Dispose();
                    _recordedBitmaps.RemoveAt(i);
                }
            }
            var bitmap = (Bitmap)_currentViewBitmap.Clone();
            _recordedBitmaps.Add(bitmap);
            _nextUndo++;
            _mosaicBrush.Dispose();
            using (var mosaic = bitmap.ToMosaic(10))
            {
                _mosaicBrush = new TextureBrush(mosaic);
            }
            if (_redoButton != null)
            {
                _redoButton.IsEnabled = CanRedo;
            }
            if (_undoButton != null)
            {
                _undoButton.IsEnabled = CanUndo;
            }
        }

        /// <summary>
        /// 撤销操作
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) return;
            _nextUndo--;
            _currentViewBitmap.Dispose();
            _currentViewBitmap = (Bitmap) _recordedBitmaps.ElementAt(_nextUndo).Clone();
            _mosaicBrush.Dispose();
            using (var mosaic = _currentViewBitmap.ToMosaic(10))
            {
                _mosaicBrush = new TextureBrush(mosaic);
            }
            UpdateView(_currentViewBitmap);
            if (_redoButton != null)
            {
                _redoButton.IsEnabled = CanRedo;
            }
            if (_undoButton != null)
            {
                _undoButton.IsEnabled = CanUndo;
            }
        }

        /// <summary>
        /// 反撤销操作
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) return;
            _nextUndo++;
            _currentViewBitmap.Dispose();
            _currentViewBitmap = (Bitmap)_recordedBitmaps.ElementAt(_nextUndo).Clone();
            _mosaicBrush.Dispose();
            using (var mosaic = _currentViewBitmap.ToMosaic(10))
            {
                _mosaicBrush = new TextureBrush(mosaic);
            }
            UpdateView(_currentViewBitmap);
            if (_redoButton != null)
            {
                _redoButton.IsEnabled = CanRedo;
            }
            if (_undoButton != null)
            {
                _undoButton.IsEnabled = CanUndo;
            }
        }

        /// <summary>
        /// 更新视图
        /// </summary>
        public void UpdateView()
        {
            UpdateView(_currentViewBitmap);
        }

        /// <summary>
        /// 重置当前视图图像
        /// </summary>
        public void ResetCurrentViewBitmap()
        {
            _currentViewBitmap.Dispose();
            _currentViewBitmap = (Bitmap)_recordedBitmaps.ElementAt(_nextUndo).Clone();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _mosaicBrush.Dispose();
            _currentViewBitmap.Dispose();
            _recordedBitmaps.ForEach(m => m.Dispose());
        }

        #endregion

        #region Private methods

        /// <summary>
        /// 更新视图
        /// </summary>
        /// <param name="bitmap"></param>
        private void UpdateView(Bitmap bitmap)
        {
            _writeableBitmap.Lock();
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            #region Safe

            //if (_bitmapBuffer == null)
            //{
            //    _bitmapBuffer = new byte[bitmap.Height * bmpData.Stride];
            //}
            //Marshal.Copy(bmpData.Scan0, _bitmapBuffer, 0, _bitmapBuffer.Length);
            //Marshal.Copy(_bitmapBuffer, 0, _writeableBitmap.BackBuffer, _bitmapBuffer.Length);

            #endregion

            #region Unsafe

            unsafe
            {
                var source = (byte*)(bmpData.Scan0);
                var destination = (byte*)(_writeableBitmap.BackBuffer);
                for (var i = 0; i < bitmap.Height * bmpData.Stride / 4; i++)
                {
                    *((int*)destination) = *((int*)source);
                    source += 4;
                    destination += 4;
                }
            }

            #endregion

            bitmap.UnlockBits(bmpData);
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _writeableBitmap.PixelWidth, _writeableBitmap.PixelHeight));
            _writeableBitmap.Unlock();
        }

        #endregion
    }
}

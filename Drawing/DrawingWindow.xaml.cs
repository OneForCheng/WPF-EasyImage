using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IPlugins;
using Brush = System.Drawing.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Color = System.Drawing.Color;
using DashStyle = System.Drawing.Drawing2D.DashStyle;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Drawing
{
    /// <summary>
    /// BinaryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DrawingWindow
    {
        #region Private fields

        private readonly DrawingManager _drawingManager;
        private Button _selectedButton;
        private DrawingTool _drawingTool;
        private bool _mouseCaptured;
        private bool _drawingChanged;
        private Point _drawStartPoint;
        private Point _drawCurrentPoint;
        private readonly List<Point> _linePoints;
        

        #endregion

        #region Constructor

        public DrawingWindow(Bitmap bitmap)
        {
            InitializeComponent();
            var resizeBitmap = ResizeBitmap(bitmap);
            //var screenHeight = SystemParameters.VirtualScreenHeight;
            //var screenWidth = SystemParameters.VirtualScreenWidth;
            var height = resizeBitmap.Height + 210.0;
            var width = resizeBitmap.Width + 40.0;
            if (height < 370)
            {
                height = 370;
            }
            //else if (height > screenHeight)
            //{
            //    height = screenHeight;
            //}
            if (width < 370)
            {
                width = 370;
            }
            //else if (width > screenWidth)
            //{
            //    width = screenWidth;
            //}
            Height = height;
            Width = width;
            _linePoints = new List<Point>();
            _drawingChanged = false;
            _selectedButton = PenToolBtn;
            _drawingTool = DrawingTool.PenTool;
            _drawingManager = new DrawingManager(TargetImage, resizeBitmap)
            {
                RedoButtuon =  RedoButton,
                UndoButton =  UndoButton,
            };
        }

        #endregion

        #region Public properties

        public HandleResult HandleResult { get; private set; }

        #endregion

        #region Event

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
            DrawingInfo_ColorChanged(null, null);
            DrawingInfo_FontChanged(null, null);
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        _drawingManager.Undo();
                        break;
                    case Key.C:
                        _drawingManager.LastRecordedBitmap.CopyImageToClipboard();
                        break;
                    case Key.Y:
                        _drawingManager.Redo();
                        break;
                }
            }
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(null, false);
            Close();
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult((Bitmap)_drawingManager.LastRecordedBitmap.Clone(), true);
            Close();
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            //...
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleResult = new HandleResult(null, false);
            Close();
        }

        private void DrawingInfo_ColorChanged(object sender, EventArgs e)
        {
            ImageTextBox.Foreground = DrawingInfo.CurrentArgbColor;
        }

        private void DrawingInfo_FontChanged(object sender, EventArgs e)
        {
            var font = DrawingInfo.CurrentFont;
            ImageTextBox.FontFamily = new System.Windows.Media.FontFamily(font.FontFamily.Name);
            ImageTextBox.FontSize = font.Size;
            if (font.Bold)
            {
                ImageTextBox.FontWeight = FontWeights.Bold;
            }
            if (font.Italic)
            {
                ImageTextBox.FontStyle = FontStyles.Italic;
            }
            var textDecorations = new TextDecorationCollection();
            if (font.Strikeout)
            {
                textDecorations.Add(TextDecorations.Strikethrough);
            }
            if (font.Underline)
            {
                textDecorations.Add(TextDecorations.Underline);
            }
            ImageTextBox.TextDecorations = textDecorations;

        }

        private void DrawingInfo_SelectedRadioChanged(object sender, EventArgs e)
        {
            if(_drawingTool != DrawingTool.PenTool)return;
            if (DrawingInfo.SelectedLeftBtn)
            {
                DrawingInfo.RightSliderLblContent = "画笔大小:";
                DrawingInfo.MinRightSliderValue = 1.0;
                DrawingInfo.MaxRightSliderValue = 20.0;
            }
            else
            {
                DrawingInfo.RightSliderLblContent = "填充偏离度:";
                DrawingInfo.MinRightSliderValue = 0;
                DrawingInfo.MaxRightSliderValue = 255; 
            }
            DrawingInfo.UpdateLeftSliderValue();
        }

        private void ExchangeBgCbx_Click(object sender, RoutedEventArgs e)
        {
            if (ExchangeBgCbx.IsChecked == true)
            {
                ImageViewGrid.Background = Brushes.White;
                ImageBorder.BorderThickness = new Thickness(0.1);
            }
            else
            {
                ImageViewGrid.Background = Brushes.Transparent;
                ImageBorder.BorderThickness = new Thickness(0);
            }
        }

        private void ExchangeBgCbx_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new ColorDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var color = dialog.Color;
            ImageViewGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(DrawingInfo.CurrentArgbColor.Color.A, color.R, color.G, color.B));
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var tag = (DrawingTool)button.Tag;
            switch (tag)
            {
                case DrawingTool.RectTool:
                case DrawingTool.EllipseTool:
                case DrawingTool.ArrowTool:
                case DrawingTool.PenTool:
                case DrawingTool.TextTool:
                case DrawingTool.EraserTool:
                    SetDrawingTool(button, tag);
                    break;
                case DrawingTool.PickerTool:

                    break;
                case DrawingTool.CopyTool:
                   _drawingManager.LastRecordedBitmap.CopyImageToClipboard();
                    break;
                case DrawingTool.UndoTool:
                    _drawingManager.Undo();
                    break;
                case DrawingTool.RedoTool:
                    _drawingManager.Redo();
                    break;
            }
        }

        private void TargetImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(TargetImage);
            _drawStartPoint.X = (int)Math.Round(position.X);
            _drawStartPoint.Y = (int)Math.Round(position.Y);
            if (_drawStartPoint.X > 0) _drawStartPoint.X--;
            if (_drawStartPoint.Y > 0) _drawStartPoint.Y--;
            if (_drawingTool == DrawingTool.TextTool)
            {
                InsertTextToImage();
                return;
            }
            _mouseCaptured = TargetImage.CaptureMouse();
            if (_drawingTool == DrawingTool.PenTool || _drawingTool == DrawingTool.EraserTool)
            {
                _linePoints.Clear();
                _linePoints.Add(new Point(_drawStartPoint.X, _drawStartPoint.Y));
                TargetImage_MouseMove(sender, e);//画点
            }
            
        }

        private void TargetImage_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(TargetImage);
            _drawCurrentPoint.X = (int)Math.Round(position.X);
            _drawCurrentPoint.Y = (int)Math.Round(position.Y);
            if (_drawCurrentPoint.X > 0) _drawCurrentPoint.X--;
            if (_drawCurrentPoint.Y > 0) _drawCurrentPoint.Y--;
            TitleLbl.Content = $"Drawing: ({_drawCurrentPoint.X},{ _drawCurrentPoint.Y})";
            if (_mouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                DrawEffects(false);
            }
            else
            {
                if(_drawingTool == DrawingTool.RectTool || _drawingTool == DrawingTool.EllipseTool || _drawingTool == DrawingTool.ArrowTool || _drawingTool == DrawingTool.PenTool || _drawingTool == DrawingTool.EraserTool) DrawEffects(true);
            }
           
        }

        private void TargetImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_mouseCaptured)
            {
                TargetImage.ReleaseMouseCapture();
                _mouseCaptured = false;
            }
            if (!_drawingChanged) return;
            _drawingChanged = false;
            _drawingManager.Record();
            
        }

        private void TargetImage_MouseLeave(object sender, MouseEventArgs e)
        {
            TitleLbl.Content = "Drawing";
            if (!_mouseCaptured && _drawingTool != DrawingTool.TextTool)//清除预览留下的痕迹
            {
                _drawingManager.ResetCurrentViewBitmap();
                _drawingManager.UpdateView();
            }
        }

        private void ImageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ImageTextBox.BorderThickness = new Thickness(0);
            if (ImageTextBox.Text.Trim() == string.Empty)
            {
                ImageTextBox.Visibility = Visibility.Hidden;
                return;
            }
            ImageTextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
               new Action(() =>
               {
                   //为了保证描绘图形时ImageTextBox边框为0，所以使用了调度器
                   var renderBitmap = new RenderTargetBitmap(_drawingManager.LastRecordedBitmap.Width, _drawingManager.LastRecordedBitmap.Height, 96, 96, PixelFormats.Pbgra32);
                   renderBitmap.Render(ImageVisulGrid);
                   _drawingManager.Drawing(renderBitmap.GetBitmap());
                   ImageTextBox.Visibility = Visibility.Hidden;
               }));
        }

        #endregion

        #region Private methods

        private Bitmap ResizeBitmap(Bitmap bitmap)
        {
            try
            {
                var width = bitmap.Width;
                var height = bitmap.Height;
                //var screenHeight = (int)SystemParameters.VirtualScreenHeight;
                //var screenWidth = (int)SystemParameters.VirtualScreenWidth;
                var resize = false;

                if (width < 330 && height < 160)
                {
                    if (width > height)
                    {
                        height = 330 * height / width;
                        width = 330;
                        resize = true;
                    }
                    else
                    {
                        width = 160 * width / height;
                        height = 160;
                        resize = true;
                    }
                }

                //if (width > screenWidth - 40)
                //{
                //    height = (screenWidth - 40) * height / width;
                //    width = screenWidth - 40;
                //    resize = true;
                //}

                //if (height > screenHeight - 200)
                //{
                //    width = (screenHeight - 200) * width / height;
                //    height = screenHeight - 200;
                //    resize = true;
                //}

                if (!resize)
                {
                    return bitmap;
                }
                var resizeBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (var bmpGraphics = Graphics.FromImage(resizeBitmap))
                {
                    bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;
                    bmpGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    bmpGraphics.CompositingQuality = CompositingQuality.GammaCorrected;
                    bmpGraphics.DrawImage(bitmap, 0, 0, width, height);
                }
                bitmap.Dispose();
                return resizeBitmap;
            }
            catch
            {
                return bitmap;
            }
        }

        private void SetDrawingTool(Button selectButton, DrawingTool drawingTool)
        {
            if (_drawingTool == drawingTool) return;
            DrawingInfo.SelectFontVisible(drawingTool == DrawingTool.TextTool);

            Selector.SetIsSelected(_selectedButton, false);
            _selectedButton = selectButton;
            Selector.SetIsSelected(_selectedButton, true);

            _drawingTool = drawingTool;
            if (drawingTool == DrawingTool.ArrowTool)
            {
                DrawingInfo.LeftRadioBtnContent = "箭头";
                DrawingInfo.RightRadioBtnContent = "直线";
            }
            else
            {
                DrawingInfo.LeftRadioBtnContent = "绘制";
                DrawingInfo.RightRadioBtnContent = "填充";
            }
            if (drawingTool == DrawingTool.PenTool && !DrawingInfo.SelectedLeftBtn)
            {
                DrawingInfo.RightSliderLblContent = "填充偏离度:";
                DrawingInfo.MinRightSliderValue = 0;
                DrawingInfo.MaxRightSliderValue = 255;
                
            }
            else
            {
                DrawingInfo.RightSliderLblContent = "画笔大小:";
                DrawingInfo.MinRightSliderValue = 1.0;
                DrawingInfo.MaxRightSliderValue = 20.0;
            }
            DrawingInfo.UpdateLeftSliderValue();
        }

        private void InsertTextToImage()
        {
            if (ImageTextBox.Visibility == Visibility.Hidden)
            {
                ImageTextBox.BorderThickness = new Thickness(1);
                ImageTextBox.Text = string.Empty;
                var width = (int) TargetImage.ActualWidth;
                var height = (int) TargetImage.ActualHeight;
                var left = _drawStartPoint.X;
                var top = _drawStartPoint.Y;
                if (left + 20 > width)
                {
                    left = width - 20;
                    ImageTextBox.MaxWidth = 20;
                }
                else
                {
                    ImageTextBox.MaxWidth = width - left;
                }
                if (top + ImageTextBox.FontSize > height)
                {
                    top = (int) (height - ImageTextBox.FontSize);
                    ImageTextBox.MaxHeight = ImageTextBox.FontSize;
                }
                else
                {
                    ImageTextBox.MaxHeight = height - top;
                }
                ImageTextBox.Margin = new Thickness(left, top, ImageTextBox.Margin.Right, ImageTextBox.Margin.Bottom);
                ImageTextBox.Visibility = Visibility.Visible;
                ImageTextBox.Focus();
            }
            else
            {
                _selectedButton.Focus();
            }
        }

        private void DrawEffects(bool isPreview)
        {
            _drawingManager.ResetCurrentViewBitmap();
            var currentColor = DrawingInfo.CurrentArgbColor.Color;
            if (isPreview)
            {
                using (var brush = new SolidBrush(Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B)))
                {
                    using (var pen = new Pen(brush, 1.5f))
                    {
                        using (var g = Graphics.FromImage(_drawingManager.CurrentViewBitmap))
                        {
                            var width = DrawingInfo.RightSliderValue;
                            var rect = new Rectangle(new Point(_drawCurrentPoint.X - (int)(width / 2), _drawCurrentPoint.Y - (int)(width / 2)), new Size((int)width, (int)width));
                            g.DrawRectangle(pen, rect);
                            g.DrawEllipse(pen, rect);
                            
                        }
                    }
                }
                
            }
            else
            {
                var color = Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);

                Brush brush;
                if (_drawingTool == DrawingTool.EraserTool)
                {
                    brush = System.Drawing.Brushes.Transparent;
                }
                else
                {
                    brush = DrawingInfo.SelectedMosaic ? _drawingManager.MosaicBrush : new SolidBrush(color);
                }
                using (var pen = new Pen(brush, (float)DrawingInfo.RightSliderValue))
                {
                    using (var g = Graphics.FromImage(_drawingManager.CurrentViewBitmap))
                    {
                        if (_drawingTool == DrawingTool.EraserTool)
                        {
                            g.CompositingMode = CompositingMode.SourceCopy;
                        }
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        Size size;
                        Point leftTopPoint;
                        switch (_drawingTool)
                        {
                            case DrawingTool.RectTool:
                                leftTopPoint = new Point(_drawStartPoint.X < _drawCurrentPoint.X ? _drawStartPoint.X : _drawCurrentPoint.X, _drawStartPoint.Y < _drawCurrentPoint.Y ? _drawStartPoint.Y : _drawCurrentPoint.Y);
                                size = new Size(Math.Abs(_drawCurrentPoint.X - _drawStartPoint.X), Math.Abs(_drawCurrentPoint.Y - _drawStartPoint.Y));
                                if (DrawingInfo.SelectedLeftBtn)
                                {
                                    g.DrawRectangle(pen, new Rectangle(leftTopPoint, size));
                                }
                                else
                                {
                                    g.FillRectangle(brush, new Rectangle(leftTopPoint, size));
                                }
                                break;
                            case DrawingTool.EllipseTool:
                                leftTopPoint = new Point(_drawStartPoint.X < _drawCurrentPoint.X ? _drawStartPoint.X : _drawCurrentPoint.X, _drawStartPoint.Y < _drawCurrentPoint.Y ? _drawStartPoint.Y : _drawCurrentPoint.Y);
                                size = new Size(Math.Abs(_drawCurrentPoint.X - _drawStartPoint.X), Math.Abs(_drawCurrentPoint.Y - _drawStartPoint.Y));
                                if (DrawingInfo.SelectedLeftBtn)
                                {
                                    g.DrawEllipse(pen, new Rectangle(leftTopPoint, size));
                                }
                                else
                                {
                                    g.FillEllipse(brush, new Rectangle(leftTopPoint, size));
                                }
                                break;
                            case DrawingTool.ArrowTool:
                                if (DrawingInfo.SelectedLeftBtn)
                                {
                                    pen.CustomEndCap = pen.Width < 5 ? new AdjustableArrowCap(4f, 5f, true) : new AdjustableArrowCap(1.5f, 2f, true);
                                }
                                g.DrawLine(pen, _drawStartPoint, _drawCurrentPoint);
                                break;
                            case DrawingTool.PenTool:
                                if (DrawingInfo.SelectedLeftBtn)
                                {
                                    if (_linePoints.Count == 1)//画点
                                    {
                                        leftTopPoint = new Point(_drawStartPoint.X - (int)(pen.Width / 2), _drawStartPoint.Y - (int)(pen.Width / 2));
                                        size = new Size((int)pen.Width, (int)pen.Width);
                                        g.FillEllipse(brush, new Rectangle(leftTopPoint, size));
                                        _linePoints.Add(new Point(_drawStartPoint.X, _drawStartPoint.Y));
                                    }
                                    else//画线
                                    {
                                        _linePoints.Add(new Point(_drawCurrentPoint.X, _drawCurrentPoint.Y));
                                        pen.StartCap = pen.EndCap = LineCap.Round;
                                        pen.LineJoin = LineJoin.Round;//很重要,默认的线连接会出现锐角
                                        g.DrawCurve(pen, _linePoints.ToArray());
                                    }
                                }
                                else
                                {
                                    _drawingManager.CurrentViewBitmap.FillFloodColor(_drawStartPoint, color, DrawingInfo.RightSliderValue);
                                }
                                break;
                            case DrawingTool.EraserTool:
                                if (_linePoints.Count == 1)//画点
                                {
                                    leftTopPoint = new Point(_drawStartPoint.X - (int)(pen.Width / 2), _drawStartPoint.Y - (int)(pen.Width / 2));
                                    size = new Size((int)pen.Width, (int)pen.Width);
                                    g.FillEllipse(brush, new Rectangle(leftTopPoint, size));
                                    _linePoints.Add(new Point(_drawStartPoint.X, _drawStartPoint.Y));
                                }
                                else//画线
                                {
                                    _linePoints.Add(new Point(_drawCurrentPoint.X, _drawCurrentPoint.Y));
                                    pen.StartCap = pen.EndCap = LineCap.Round;
                                    pen.LineJoin = LineJoin.Round;//很重要,默认的线连接会出现锐角
                                    g.DrawCurve(pen, _linePoints.ToArray());
                                }
                                break;
                        }
                    }
                }
                _drawingChanged = true;
            }
            _drawingManager.UpdateView();
        }



        #endregion

        
    }
}

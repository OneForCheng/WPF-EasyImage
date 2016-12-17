using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using EasyImage.Controls;
using EasyImage.Enum;

namespace EasyImage.Thumbs
{
    public class ResizeThumb : Thumb
    {
        private const double Tolerance = 3;
        private ImageControl _imageControl;
        private ScaleTransform _scaleTransform;
        private double _scaleVertical, _scaleHorizontal;
        private bool _turnVerticale, _turnHorizontal;
        private ThumbFlag _thumbFlag;
        private bool _isResize;
        private double _oldWitdh, _oldHeight;
        private double _oldScaleX, _oldScaleY;

        public ResizeThumb()
        {
            DragStarted += ResizeThumb_DragStarted;
            DragDelta += ResizeThumb_DragDelta;
            DragCompleted += ResizeThumb_DragCompleted;
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _imageControl = DataContext as ImageControl;
            if (_imageControl == null) return;
            _isResize = false;
            _oldWitdh = _imageControl.Width;
            _oldHeight = _imageControl.Height;
            _scaleTransform = _imageControl.GetTransform<ScaleTransform>();
            _oldScaleX = _scaleTransform.ScaleX;
            _oldScaleY = _scaleTransform.ScaleY;

            if (VerticalAlignment == VerticalAlignment.Top && HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                _thumbFlag = ThumbFlag.TopCenter;
            }
            else if (VerticalAlignment == VerticalAlignment.Stretch && HorizontalAlignment == HorizontalAlignment.Left)
            {
                _thumbFlag = ThumbFlag.LeftCenter;
            }
            else if (VerticalAlignment == VerticalAlignment.Stretch && HorizontalAlignment == HorizontalAlignment.Right)
            {   
                _thumbFlag = ThumbFlag.RightCenter;
            }
            else if (VerticalAlignment == VerticalAlignment.Bottom && HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                _thumbFlag = ThumbFlag.BottomCenter;
            }
            else if (VerticalAlignment == VerticalAlignment.Top && HorizontalAlignment == HorizontalAlignment.Left)
            {
                _thumbFlag = ThumbFlag.TopLeft;
            }
            else if (VerticalAlignment == VerticalAlignment.Top && HorizontalAlignment == HorizontalAlignment.Right)
            {
                _thumbFlag = ThumbFlag.TopRight;
            }
            else if (VerticalAlignment == VerticalAlignment.Bottom && HorizontalAlignment == HorizontalAlignment.Left)
            {
                _thumbFlag = ThumbFlag.BottomLeft;
            }
            else if (VerticalAlignment == VerticalAlignment.Bottom && HorizontalAlignment == HorizontalAlignment.Right)
            {
                _thumbFlag = ThumbFlag.BottomRight;
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_imageControl == null) return;
            _isResize = true;
            _scaleHorizontal = _scaleVertical = 1.0;
            _turnHorizontal = _turnVerticale = false;
            var relativePos = Mouse.GetPosition(_imageControl);
            double width = _imageControl.Width, height = _imageControl.Height;
            double deltaHorizontal, deltaVertical;
            switch (_thumbFlag)
            {
                case ThumbFlag.TopLeft:
                    if (relativePos.X > width && relativePos.Y - height > 0)
                    {
                        _turnVerticale = true;
                        _turnHorizontal = true;
                    }
                    else if (relativePos.X < width && relativePos.Y > height)
                    {
                        _turnVerticale = true;
                    }
                    else if (relativePos.X > width && relativePos.Y < height)
                    {
                        _turnHorizontal = true;
                    }
                    else
                    {
                        var min = Math.Min(e.VerticalChange, e.HorizontalChange);
                        var max = Math.Max(width, height);
                        if (max - min > Tolerance)
                        {
                            _scaleHorizontal = _scaleVertical = (max - min) / max;
                        }
                    }
                    break;
                case ThumbFlag.TopCenter:
                    deltaVertical = e.VerticalChange;
                    if (height - deltaVertical < Tolerance)
                    {
                        _turnVerticale = true;
                    }
                    else
                    {
                        _scaleVertical = (height - deltaVertical) / height;
                    }
                    break;
                case ThumbFlag.TopRight:
                    if (relativePos.X > 0 && relativePos.Y > height)
                    {
                        _turnVerticale = true;
                    }
                    else if (relativePos.X < 0 && relativePos.Y > height)
                    {
                        _turnVerticale = true;
                        _turnHorizontal = true;
                    }
                    else if (relativePos.X > 0 && relativePos.Y < height)
                    {
                        var min = Math.Min(e.VerticalChange, -e.HorizontalChange);
                        var max = Math.Max(width, height);
                        if (max - min > Tolerance)
                        {
                            _scaleHorizontal = _scaleVertical = (max - min) / max;
                        }
                    }
                    else
                    {
                        _turnHorizontal = true;
                    }
                    break;
                case ThumbFlag.RightCenter:
                    deltaHorizontal = -e.HorizontalChange;
                    if (width - deltaHorizontal < Tolerance)
                    {
                        _turnHorizontal = true;
                    }
                    else
                    {
                        _scaleHorizontal = (width - deltaHorizontal) / width;
                    }
                    break;
                case ThumbFlag.BottomRight:
                    if (relativePos.X > 0 && relativePos.Y > 0)
                    {
                        var min = Math.Min(-e.VerticalChange, -e.HorizontalChange);
                        var max = Math.Max(width, height);
                        if (max - min > Tolerance)
                        {
                            _scaleHorizontal = _scaleVertical = (max - min) / max;
                        }
                    }
                    else if (relativePos.X < 0 && relativePos.Y > 0)
                    {
                        _turnHorizontal = true;
                    }
                    else if (relativePos.X > 0 && relativePos.Y < 0)
                    {
                        _turnVerticale = true;
                    }
                    else
                    {
                        _turnVerticale = true;
                        _turnHorizontal = true;
                    }
                    break;
                case ThumbFlag.BottomCenter:
                     deltaVertical = -e.VerticalChange;
                    if (_imageControl.Height - deltaVertical < Tolerance)
                    {
                        _turnVerticale = true;
                    }
                    else
                    {
                        _scaleVertical = (height - deltaVertical) / height;
                    }
                    break;
                case ThumbFlag.BottomLeft:
                    if (relativePos.X > width && relativePos.Y > 0)
                    {
                        _turnHorizontal = true;
                    }
                    else if (relativePos.X < width && relativePos.Y > 0)
                    {
                        var min = Math.Min(-e.VerticalChange, e.HorizontalChange);
                        var max = Math.Max(width, height);
                        if (max - min > Tolerance)
                        {
                            _scaleHorizontal = _scaleVertical = (max - min) / max;
                        }
                    }
                    else if (relativePos.X > width && relativePos.Y < 0)
                    {
                        _turnVerticale = true;
                        _turnHorizontal = true;
                    }
                    else
                    {
                        _turnVerticale = true;
                    }
                    break;
                case ThumbFlag.LeftCenter:
                     deltaHorizontal = e.HorizontalChange;
                    if (width - deltaHorizontal < Tolerance)
                    {
                        _turnHorizontal = true;
                    }
                    else
                    {
                        _scaleHorizontal = (width - deltaHorizontal) / width;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _imageControl.ControlManager.DragResizeSelected(_scaleHorizontal, _scaleVertical, _turnHorizontal, _turnVerticale, _thumbFlag, false);
            
        }

        private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_imageControl == null) return;
            if (_isResize)
            {
                _scaleHorizontal = _oldWitdh / _imageControl.Width;
                _scaleVertical = _oldHeight / _imageControl.Height;
                _turnHorizontal = !_oldScaleX.Equals(_scaleTransform.ScaleX);
                _turnVerticale = !_oldScaleY.Equals(_scaleTransform.ScaleY);
                _imageControl.ControlManager.DragResizeSelected(_scaleHorizontal, _scaleVertical, _turnHorizontal, _turnVerticale, _thumbFlag, true);
                _isResize = false;
            }
            _imageControl = null;
        }
    }
}

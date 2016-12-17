using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using EasyImage.Controls;
using UndoFramework.Actions;

namespace EasyImage.Thumbs
{
    public class RotateThumb : Thumb
    {
        private Vector _startVector;
        private Point _centerPoint;
        private ImageControl _imageControl;
        private double _angle;
        private bool _isRotate;
        private TransactionAction _transaction;

        public RotateThumb()
        {
            DragStarted += RotateThumb_DragStarted;
            DragDelta += RotateThumb_DragDelta;
            DragCompleted += RotateThumb_DragCompleted;
        }

        private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _imageControl = DataContext as ImageControl;
            if (_imageControl == null) return;
            _angle = 0.0;
            _centerPoint = _imageControl.TranslatePoint(new Point(_imageControl.Width / 2, _imageControl.Height / 2), null);
            _startVector = Point.Subtract(Mouse.GetPosition(null), _centerPoint);
            
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_imageControl == null) return;
            if (!_isRotate)
            {
                _transaction = _imageControl.ControlManager.ReviseRotateCenterSelected;
                _transaction.Execute();
                _isRotate = true;
            }
            
            var currentPoint = Mouse.GetPosition(null);
            var deltaVector = Point.Subtract(currentPoint, _centerPoint);
            var angle = Math.Round(Vector.AngleBetween(_startVector, deltaVector), 0);
            _imageControl.ControlManager.DragRotateSelected(angle - _angle, false);
            _angle = angle;
        }

        private void RotateThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if(_imageControl == null) return;
            if (_isRotate)
            {
                _isRotate = false;
                _imageControl.ControlManager.DragRotateSelected(_angle, true, _transaction);
            }
            _imageControl = null;
        }
    }
}

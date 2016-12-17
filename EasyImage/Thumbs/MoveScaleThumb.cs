using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using EasyImage.Controls;

namespace EasyImage.Thumbs
{
    public class MoveScaleThumb: Thumb
    {
        private Point _mousePosition;
        private ImageControl _imageControl;
        private double _moveX, _moveY;
        private bool _isMove;

        public MoveScaleThumb()
        {
            DragStarted += MoveThumb_DragStarted;
            DragDelta += MoveThumb_DragDelta;
            DragCompleted += MoveThumb_DragCompleted;
        }

        private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _imageControl = DataContext as ImageControl;
            if (_imageControl == null) return;
            _moveX = 0.0;
            _moveY = 0.0;
            _mousePosition = Mouse.GetPosition(null);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_imageControl == null) return;
            if (!_isMove)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
                {
                    _imageControl.ControlManager.CloneSelected();
                }
            }
            _isMove = true;
            var curPosition = Mouse.GetPosition(null);
            var moveX = curPosition.X - _mousePosition.X;
            var moveY = curPosition.Y - _mousePosition.Y;
            _mousePosition = curPosition;
            _moveX += moveX;
            _moveY += moveY;
            _imageControl.ControlManager.DragMoveSelected(moveX, moveY, false);
        }

        private void MoveThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_imageControl == null) return;
            if (_isMove)
            {
                _isMove = false;
                _imageControl.ControlManager.DragMoveSelected(_moveX, _moveY, true);
            }
            _imageControl = null;
        }

        /// <summary>
        /// 控件的伪缩放
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var element = DataContext as ImageControl;
            if (element == null || !Selector.GetIsSelected(element)) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                element.ControlManager.WheelCenterScaleSelected(e.Delta * 0.001);
            }
            else
            {
                element.ControlManager.WheelFixedScaleElement(element, e.Delta * 0.001, e.GetPosition(element));
            }

            base.OnMouseWheel(e);
        }

    }

}

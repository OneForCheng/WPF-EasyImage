using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace Drawing.Behaviors
{
    /// <summary>
    /// 鼠标拖拽控件行为
    /// </summary>
    /// <typeparam name="T">目标控件</typeparam>
    public class MouseDragElementBehavior<T> : Behavior<FrameworkElement> where T : FrameworkElement
    {
        private Point _mousePosition;
        private T _targetElement;
        private TranslateTransform _cacheTranslateTransform;

        public Rect? MoveableRange { set; get; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
        }

        #region 鼠标事件
        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.Source as T;
            if (element == null || !element.CaptureMouse()) return;
            _mousePosition = e.GetPosition(null);
            _targetElement = element;
            _cacheTranslateTransform = element.GetTransform<TranslateTransform>();
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (_targetElement == null || e.LeftButton != MouseButtonState.Pressed) return;
            var curPosition = e.GetPosition(null);
            var moveX = curPosition.X - _mousePosition.X;
            var moveY = curPosition.Y - _mousePosition.Y;
            if (MoveableRange == null)
            {
                _cacheTranslateTransform.X += moveX;
                _cacheTranslateTransform.Y += moveY;
            }
            else
            {
                if (moveX > 0 && _cacheTranslateTransform.X + _targetElement.ActualWidth + moveX <= MoveableRange.Value.Right)
                {
                    _cacheTranslateTransform.X += moveX;
                }
                else if(moveX < 0 && _cacheTranslateTransform.X + moveX >= MoveableRange.Value.Left)
                {
                    _cacheTranslateTransform.X += moveX;
                }

                if (moveY > 0 && _cacheTranslateTransform.Y + _targetElement.ActualHeight + moveY <= MoveableRange.Value.Bottom)
                {
                    _cacheTranslateTransform.Y += moveY;
                }
                else if(moveY < 0 && _cacheTranslateTransform.Y + moveY >= MoveableRange.Value.Top)
                {
                    _cacheTranslateTransform.Y += moveY;
                }
            }
            
            _mousePosition = curPosition;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_targetElement == null) return;
            _targetElement.ReleaseMouseCapture();
            _targetElement = null;
        }

        #endregion

    }
}

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace EasyImage.Behaviors
{
    /// <summary>
    /// 鼠标拖拽控件行为
    /// </summary>
    /// <typeparam name="T">目标控件</typeparam>
    public class MouseDragElementBehavior<T> : Behavior<UIElement> where T : UIElement
    {
        private Point _mousePosition;
        private T _draggedElement;
        private TranslateTransform _translateTransform;

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
            _draggedElement = element;
            _translateTransform = element.GetTransform<TranslateTransform>();
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedElement == null) return;
            var curPosition = e.GetPosition(null);
            _translateTransform.X -= _mousePosition.X - curPosition.X;
            _translateTransform.Y -= _mousePosition.Y - curPosition.Y;
            _mousePosition = curPosition;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedElement == null) return;
            _draggedElement.ReleaseMouseCapture();
            _draggedElement = null;
        }

        #endregion

    }
}

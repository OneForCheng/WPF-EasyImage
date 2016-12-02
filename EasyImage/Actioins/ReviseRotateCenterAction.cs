using System.Windows;
using System.Windows.Media;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    internal class ReviseRotateCenterAction : AbstractBackableAction
    {
        private readonly TranslateTransform _translateTransform;
        private readonly RotateTransform _rotateTransform;
        private Point _oldCenter, _center;
        private Point _oldTranslate, _translate;

        public ReviseRotateCenterAction(FrameworkElement element)
        {
            var scaleTransform = element.GetTransform<ScaleTransform>();

            var elementCenter = element.TranslatePoint(new Point(element.Width  / 2, element.Height / 2), null);

            _rotateTransform = element.GetTransform<RotateTransform>();
            _oldCenter.X = _rotateTransform.CenterX;
            _oldCenter.Y = _rotateTransform.CenterY;
            _center.X = element.Width * scaleTransform.ScaleX / 2;
            _center.Y = element.Height * scaleTransform.ScaleY / 2;

            _translateTransform = element.GetTransform<TranslateTransform>();
            _oldTranslate.X = _translateTransform.X;
            _oldTranslate.Y = _translateTransform.Y;
            _translate.X = elementCenter.X - _center.X;
            _translate.Y = elementCenter.Y - _center.Y;
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _rotateTransform.CenterX = _center.X;
            _rotateTransform.CenterY = _center.Y;
            _translateTransform.X = _translate.X;
            _translateTransform.Y = _translate.Y;
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _rotateTransform.CenterX = _oldCenter.X;
            _rotateTransform.CenterY = _oldCenter.Y;
            _translateTransform.X = _oldTranslate.X;
            _translateTransform.Y = _oldTranslate.Y;
        }

    }
}

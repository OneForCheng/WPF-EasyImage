using System;
using System.Windows;
using System.Windows.Media;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    internal class WheelScaleAction : AbstractBackableAction
    {
        private readonly FrameworkElement _element;
        private readonly TranslateTransform _translateTransform;
        private readonly double _delta;
        private readonly double _moveX;
        private readonly double _moveY;

        public WheelScaleAction(FrameworkElement element, double delta, Point scalePoint)
        {
            _element = element;
            _delta = delta;
            _translateTransform = _element.GetTransform<TranslateTransform>();

            double deltaX = delta, deltaY = delta;
            var scaleTransform = element.GetTransform<ScaleTransform>();
            var radian = element.GetTransform<RotateTransform>().Angle / 180 * Math.PI;
            _moveX = -scalePoint.X * Math.Cos(radian) * scaleTransform.ScaleX * deltaX + scalePoint.Y * Math.Sin(radian) * scaleTransform.ScaleY * deltaY;
            _moveY = -scalePoint.X * Math.Sin(radian) * scaleTransform.ScaleX * deltaX - scalePoint.Y * Math.Cos(radian) * scaleTransform.ScaleY * deltaY;
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _element.Width = _element.Width * (1 + _delta);
            _element.Height = _element.Height * (1 + _delta);
            _translateTransform.X += _moveX;
            _translateTransform.Y += _moveY;
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _element.Width = _element.Width / (1 + _delta);
            _element.Height = _element.Height / (1 + _delta);
            _translateTransform.X -= _moveX;
            _translateTransform.Y -= _moveY;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Media;
using EasyImage.Enum;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    internal class DragResizeAction : AbstractBackableAction
    {
        private readonly FrameworkElement _element;
        private readonly TranslateTransform _translateTransform;
        private readonly ScaleTransform _scaleTransform;
        private readonly double _oldWitdh;
        private readonly double _oldHeight;
        private readonly double _width;
        private readonly double _height;
        private readonly double _oldTranslateX;
        private readonly double _oldTranslateY;
        private readonly double _translateX;
        private readonly double _translateY;
        private readonly double _oldScaleX;
        private readonly double _oldScaleY;
        private readonly double _scaleX;
        private readonly double _scaleY;

        public DragResizeAction(FrameworkElement element, double scaleHorizontal, double scaleVertical, bool turnHorizontal, bool turnVerticale, ThumbFlag thumbFlag)
        {
            _element = element;
            _translateTransform = element.GetTransform<TranslateTransform>();
            _scaleTransform = element.GetTransform<ScaleTransform>();
            _oldWitdh = _width = element.Width;
            _oldHeight = _height = element.Height;
            _oldTranslateX = _translateX = _translateTransform.X;
            _oldTranslateY = _translateY = _translateTransform.Y;
            _oldScaleX = _scaleX = _scaleTransform.ScaleX;
            _oldScaleY = _scaleY = _scaleTransform.ScaleY;

            var radian = element.GetTransform<RotateTransform>().Angle * Math.PI / 180;

            var deltaVertical = _height * (1 - scaleVertical);
            var deltaHorizontal = _width * (1 - scaleHorizontal);

            if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.TopCenter || thumbFlag == ThumbFlag.TopRight)
            {
                _translateY += deltaVertical * Math.Cos(-radian) * _scaleY;
                _translateX += deltaVertical * Math.Sin(-radian) * _scaleY;
            }

            _height = _height * scaleVertical;

            if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.LeftCenter || thumbFlag == ThumbFlag.BottomLeft)
            {
                _translateY += deltaHorizontal * Math.Sin(radian) * _scaleX;
                _translateX += deltaHorizontal * Math.Cos(radian) * _scaleX;
            }

            _width = _width * scaleHorizontal;

            if (turnVerticale)
            {
                _scaleY = -_scaleY;
                if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.TopCenter || thumbFlag == ThumbFlag.TopRight)
                {
                    _translateY -= 2 * _height * Math.Cos(-radian) * _scaleY;
                    _translateX -= 2 * _height * Math.Sin(-radian) * _scaleY;
                }
            }
            if (turnHorizontal)
            {
                _scaleX = -_scaleX;
                if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.LeftCenter || thumbFlag == ThumbFlag.BottomLeft)
                {
                    _translateY -= 2 * _width * Math.Sin(radian) * _scaleX;
                    _translateX -= 2 * _width * Math.Cos(radian) * _scaleX;
                }
            }
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _element.Height = _height;
            _element.Width = _width;
            _translateTransform.X = _translateX;
            _translateTransform.Y = _translateY;
            _scaleTransform.ScaleX = _scaleX;
            _scaleTransform.ScaleY = _scaleY;
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _element.Height = _oldHeight;
            _element.Width = _oldWitdh;
            _translateTransform.X = _oldTranslateX;
            _translateTransform.Y = _oldTranslateY;
            _scaleTransform.ScaleX = _oldScaleX;
            _scaleTransform.ScaleY = _oldScaleY;
        }

    }
}

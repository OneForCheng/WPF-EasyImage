using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UndoFramework.Abstract;
using Image = System.Windows.Controls.Image;

namespace EasyImage.Actioins
{
    internal class ExchangeImageAction : AbstractBackableAction
    {
        private readonly Image _oldImage, _image;
        private readonly ContentControl _contentControl;
        private readonly TranslateTransform _translateTransform;
        private readonly double _deltaX;
        private readonly double _deltaY;
        private readonly double _moveX;
        private readonly double _moveY;

        public ExchangeImageAction(ContentControl contentControl, Image image)
        {
            _contentControl = contentControl;
            _oldImage = contentControl.Content as Image;
            _translateTransform = contentControl.GetTransform<TranslateTransform>();
            _image = image;
            var oldHeight = contentControl.Height;
            var oldWidth = contentControl.Width;
            var imageWidth = image.Source.Width;
            var imageHeight = image.Source.Height;

            var ratio = imageHeight / imageWidth;
            imageWidth = imageWidth < oldWidth ? imageWidth : oldWidth;

            _deltaX = (imageWidth - oldWidth) /oldWidth;
            _deltaY = (imageWidth * ratio - oldHeight)/oldHeight;

            var scalePoint = new Point(oldWidth / 2, oldHeight / 2);
            var scaleTransform = contentControl.GetTransform<ScaleTransform>();
            var radian = contentControl.GetTransform<RotateTransform>().Angle / 180 * Math.PI;
            _moveX = Math.Round(-scalePoint.X * Math.Cos(radian) * scaleTransform.ScaleX * _deltaX + scalePoint.Y * Math.Sin(radian) * scaleTransform.ScaleY * _deltaY);
            _moveY = Math.Round(-scalePoint.X * Math.Sin(radian) * scaleTransform.ScaleX * _deltaX - scalePoint.Y * Math.Cos(radian) * scaleTransform.ScaleY * _deltaY);
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _contentControl.Content = _image;
            _contentControl.Height *= (1 + _deltaY);
            _contentControl.Width *= (1 + _deltaX);
            _translateTransform.X += _moveX;
            _translateTransform.Y += _moveY;
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _contentControl.Content = _oldImage;
            _contentControl.Height /= (1 + _deltaY);
            _contentControl.Width /= (1 + _deltaX);
            _translateTransform.X -= _moveX;
            _translateTransform.Y -= _moveY;
        }
    }
}

using System.Windows.Media;
using EasyImage.Controls;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    public class SetPropertyAction : AbstractBackableAction
    {
        private readonly ImageControl _element;
        private readonly TranslateTransform _translateTransform;
   
        private readonly RotateTransform _rotateTransform;
        private readonly double _oldWidth;
        private readonly double _oldHeight;
        private readonly double _newWidth;
        private readonly double _newHeight;
        private readonly double _oldAngle;
        private readonly double _newAngle;
        private readonly double _oldTranslateX;
        private readonly double _oldTranslateY;
        private readonly double _newTranslateX;
        private readonly double _newTranslateY;
        private readonly bool _oldIsLockAspect;
        private readonly bool _newIsLockAspect;

        public SetPropertyAction(ImageControl element, double oldWidth, double oldHeight, double oldAngle, bool oldIsLockAspect, double oldTranslateX, double oldTranslateY,
                                double newWidth, double newHeight, double newAngle, bool newIsLockAspect, double newTranslateX, double newTranslateY)
        {
            _element = element;
            _translateTransform = element.GetTransform<TranslateTransform>();
            _rotateTransform = element.GetTransform<RotateTransform>();
            _oldWidth = oldWidth;
            _oldHeight = oldHeight;
            _oldAngle = oldAngle;
            _oldIsLockAspect = oldIsLockAspect;
            _oldTranslateX = oldTranslateX;
            _oldTranslateY = oldTranslateY;

            _newWidth = newWidth;
            _newHeight = newHeight;
            _newAngle = newAngle;
            _newIsLockAspect = newIsLockAspect;
            _newTranslateX = newTranslateX;
            _newTranslateY = newTranslateY;
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _element.Width = _newWidth;
            _element.Height = _newHeight;
            _translateTransform.X = _newTranslateX;
            _translateTransform.Y = _newTranslateY;
            _rotateTransform.Angle = _newAngle;
            _element.IsLockAspect = _newIsLockAspect;

        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _element.Width = _oldWidth;
            _element.Height = _oldHeight;
            _translateTransform.X = _oldTranslateX;
            _translateTransform.Y = _oldTranslateY;
            _rotateTransform.Angle = _oldAngle;
            _element.IsLockAspect = _oldIsLockAspect;
        }
    }
}

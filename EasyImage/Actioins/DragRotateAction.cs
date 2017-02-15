using System.Windows.Media;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    internal class DragRotateAction : AbstractBackableAction
    {

        private readonly RotateTransform _rotateTransform;
        private readonly double _oldAngle;
        private readonly double _newAngle;

        public DragRotateAction(RotateTransform rotateTransform, double angle)
        {
            _rotateTransform = rotateTransform;
            _oldAngle = rotateTransform.Angle;
            _newAngle = (_oldAngle + angle) % 360;
            if (_newAngle < 0)
            {
                _newAngle += 360;
            }
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _rotateTransform.Angle = _newAngle;
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _rotateTransform.Angle = _oldAngle;
        }

    }
}

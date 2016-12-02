using System.Windows.Media;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    internal class DragRotateAction : AbstractBackableAction
    {

        private readonly RotateTransform _rotateTransform;
        private readonly double _angle;

        public DragRotateAction(RotateTransform rotateTransform, double angle)
        {
            _rotateTransform = rotateTransform;
            _angle = angle;
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _rotateTransform.Angle += _angle;
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _rotateTransform.Angle -= _angle;
        }

    }
}

using System.Windows.Media;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    internal class DragMoveAction : AbstractBackableAction
    {
        private readonly TranslateTransform _translateTransform;
        private readonly double _moveX;
        private readonly double _moveY;

        public DragMoveAction(TranslateTransform translateTransform, double moveX, double moveY)
        {
            _translateTransform = translateTransform;
            _moveX = moveX;
            _moveY = moveY;
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            _translateTransform.X += _moveX;
            _translateTransform.Y += _moveY;
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            _translateTransform.X -= _moveX;
            _translateTransform.Y -= _moveY;
        }
    }
}

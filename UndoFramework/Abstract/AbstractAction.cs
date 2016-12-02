using UndoFramework.Interface;

namespace UndoFramework.Abstract
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractAction : IAction
    {
        /// <summary>
        /// 操作的次数
        /// </summary>
        protected int ExecuteCount { get; set; }

        /// <summary>
        /// 执行操作
        /// </summary>
        public virtual void Execute()
        {
            if (!CanExecute)
            {
                return;
            }
            ExecuteCore();
            ExecuteCount++;
        }

        /// <summary>
        /// 执行的核心操作
        /// </summary>
        protected abstract void ExecuteCore();

        /// <summary>
        /// 能否执行
        /// </summary>
        public virtual bool CanExecute
        {
           get
            {
                return ExecuteCount == 0;
            }
            
        }
    }

}

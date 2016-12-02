using UndoFramework.Interface;

namespace UndoFramework.Abstract
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractBackableAction : IBackableAction
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
        /// 执行撤销操作
        /// </summary>
        public virtual void UnExecute()
        {
            if (!CanUnExecute)
            {
                return;
            }
            UnExecuteCore();
            ExecuteCount--;
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected abstract void ExecuteCore();

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected abstract void UnExecuteCore();

        /// <summary>
        /// 能否执行操作
        /// </summary>
        public virtual bool CanExecute => ExecuteCount == 0;

        /// <summary>
        /// 能否执行撤销操作
        /// </summary>
        public virtual bool CanUnExecute => !CanExecute;
    }

}

namespace UndoFramework.Interface
{

    /// <summary>
    /// 可撤销操作的接口
    /// </summary>
    public interface IBackableAction : IAction
    {
        /// <summary>
        /// 能否撤销操作
        /// </summary>
        bool CanUnExecute { get; }

        /// <summary>
        /// 撤销操作
        /// </summary>
        void UnExecute();
    }

 
}

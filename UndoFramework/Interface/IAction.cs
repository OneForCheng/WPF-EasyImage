namespace UndoFramework.Interface
{
    /// <summary>
    /// 操作的接口
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// 能否执行操作
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        /// 执行操作
        /// </summary>
        void Execute();
    }

}

namespace UndoFramework.Interface
{
    /// <summary>
    /// 管理撤销、反撤销操作的接口
    /// </summary>
    public interface IActionManager
    {
        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="command"></param>
        void RecordAction(IAction command);

        /// <summary>
        /// 撤销操作
        /// </summary>
        void UnExecute();

        /// <summary>
        /// 反撤销操作
        /// </summary>
        void ReExecute();

        /// <summary>
        /// 能否执行撤销操作
        /// </summary>
        bool CanUnExecute { get; }

        /// <summary>
        /// 能否执行反撤销操作
        /// </summary>
        bool CanReExecute { get; }

    }

}

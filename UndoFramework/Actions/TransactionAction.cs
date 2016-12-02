using System;
using System.Collections.Generic;
using System.Linq;
using UndoFramework.Interface;

namespace UndoFramework.Actions
{


    /// <summary>
    /// 一系列的操作组成的一个事务
    /// </summary>
    public class TransactionAction : IBackableAction
    {
        private readonly List<IBackableAction> _actionList = new List<IBackableAction>();

        /// <summary>
        /// 能否执行操作
        /// </summary>
        public bool CanExecute
        {
            get
            {
                return _actionList.All(action => action.CanExecute);
            }

        }

        /// <summary>
        /// 能否执行撤销操作
        /// </summary>
        public  bool CanUnExecute
        {
            get
            {
                return Enumerable.Reverse(_actionList).All(action => action.CanUnExecute);
            }
        }

        /// <summary>
        /// 是否有操作
        /// </summary>
        public bool HasActions => _actionList.Count != 0;

        #region Public methods

        /// <summary>
        /// 执行操作
        /// </summary>
        public void Execute()
        {
            foreach(var action in _actionList)
            {
                action.Execute();
            }
        }

        /// <summary>
        /// 撤销操作
        /// </summary>
        public void UnExecute()
        {
            foreach (var action in Enumerable.Reverse(_actionList))
            {
                action.UnExecute();
            }
        }

        /// <summary>
        /// 添加操作
        /// </summary>
        /// <param name="actionToAppend"></param>
        public void Add(IBackableAction actionToAppend)
        {
            if (actionToAppend == null)
            {
                throw new ArgumentNullException(nameof(actionToAppend));
            }
            _actionList.Add(actionToAppend);
        }

        /// <summary>
        /// 移除操作
        /// </summary>
        /// <param name="actionToCancel"></param>
        public void Remove(IBackableAction actionToCancel)
        {
            if (actionToCancel == null)
            {
                throw new ArgumentNullException(nameof(actionToCancel));
            }
            _actionList.Remove(actionToCancel);
        }

        #endregion  Public methods   
    }

}

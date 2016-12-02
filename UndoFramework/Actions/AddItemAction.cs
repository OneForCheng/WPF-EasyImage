using System;
using UndoFramework.Abstract;

namespace UndoFramework.Actions
{
    /// <summary>
    /// 添加、移除操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AddItemAction<T> : AbstractBackableAction
    {
        #region Constructors
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="adder">添加元素函数</param>
        /// <param name="remover">移除元素的函数</param>
        /// <param name="item">添加的元素</param>
        public AddItemAction(Action<T> adder, Action<T> remover, T item)
        {
            Adder = adder;
            Remover = remover;
            Item = item;
        }

        #endregion

        #region Properties and Events
        /// <summary>
        /// 添加元素函数的委托
        /// </summary>
        public Action<T> Adder { get; set; }
        /// <summary>
        /// 移除元素函数的委托
        /// </summary>
        public Action<T> Remover { get; set; }
        /// <summary>
        /// 目标元素
        /// </summary>
        public T Item { get; set; }


        #endregion Properties and Events

        #region Public methods
        /// <summary>
        /// 执行操作
        /// </summary>
        protected override void ExecuteCore()
        {
            Adder(Item);
        }
        /// <summary>
        /// 撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            Remover(Item);
        }

        #endregion Public methods

    }

}

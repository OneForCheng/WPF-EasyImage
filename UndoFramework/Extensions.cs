using System.Collections.Generic;
using UndoFramework.Actions;
using UndoFramework.Interface;

namespace UndoFramework
{

    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="action"></param>
        public static void Execute(this ActionManager actionManager, IAction action)
        {
            if (actionManager == null)
            {
                action.Execute();
            }
            else
            {
                actionManager.RecordAction(action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="parentObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetProperty(this ActionManager actionManager, object parentObject, string propertyName, object value)
        {
            var action = new SetPropertyAction(parentObject, propertyName, value);
            actionManager.Execute(action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionManager"></param>
        /// <param name="list"></param>
        /// <param name="item"></param>
        public static void AddItemAction<T>(this ActionManager actionManager, ICollection<T> list, T item)
        {
            var action = new AddItemAction<T>(list.Add, m => list.Remove(m), item);
            actionManager.Execute(action);
        }

    }
}

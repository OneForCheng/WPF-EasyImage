using System;
using System.Collections.Generic;
using System.Linq;
using UndoFramework.Interface;

namespace UndoFramework
{
    /// <summary>
    /// 
    /// </summary>
    public class ActionManager : IActionManager
    {
        #region Data
        private readonly List<Tuple<int, IBackableAction>> _actionList;
        private int _nextUndo;
        private int _maxBufferCount;
        private int _id;
        #endregion Data

        #region Constructors
        /// <summary>
        /// 构造函数
        /// </summary>
        public ActionManager()
        {
            _actionList = new List<Tuple<int, IBackableAction>>();
            _nextUndo = -1;
            _id = 0;
            _maxBufferCount = int.MaxValue;
        }

        #endregion

        #region Properties and Events
        /// <summary>
        /// 最大的缓存操作的数量
        /// </summary>
        public int MaxBufferCount
        {
            get { return _maxBufferCount; }
            set
            {
                if(value < 0)
                {
                    throw new InvalidOperationException($"{value} isn't a valid value for MaxBufferCount.");
                }
                _maxBufferCount = value;
            }
        }

        /// <summary>
        /// 当前操作管理器的状态码
        /// </summary>
        public int StatusCode => _nextUndo > -1 ? _actionList.ElementAt(_nextUndo).Item1 : 0;

        /// <summary>
        /// 能否执行撤销操作
        /// </summary>
        public bool CanUnExecute => (_nextUndo > -1);

        /// <summary>
        /// 能否执行反撤销操作
        /// </summary>
        public bool CanReExecute => (_nextUndo != _actionList.Count -1);

        #endregion Properties and Events

        #region Public methods
        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="action"></param>
        public void RecordAction(IAction action)
        {
            action.Execute();
            if (CanReExecute)
            {
                for(var i = _actionList.Count - 1; i > _nextUndo; i--)
                {
                    _actionList.RemoveAt(i);
                }
            }
            var backableCommond = action as IBackableAction;
            if (backableCommond != null)
            {
                _actionList.Add(new Tuple<int, IBackableAction>(++_id, backableCommond));
                if(_actionList.Count > _maxBufferCount)
                {
                    _actionList.RemoveAt(0);
                }
                else
                {
                    _nextUndo++;
                }
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// 撤销操作
        /// </summary>
        public  void UnExecute()
        {
            if (!CanUnExecute) return;
            _actionList.ElementAt(_nextUndo).Item2.UnExecute();
            _nextUndo--;
        }

        /// <summary>
        /// 反撤销操作
        /// </summary>
        public void ReExecute()
        {
            if (!CanReExecute) return;
            _actionList.ElementAt(_nextUndo + 1).Item2.Execute();
            _nextUndo++;
        }

        /// <summary>
        /// 清空操作
        /// </summary>
        public void Clear()
        {
            _actionList.Clear();
            _nextUndo = -1;
            _id = 0;
        }

        /// <summary>
        /// 撤销操作的集合
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBackableAction> EnumUndoableActions()
        {
            return _actionList.Select(m => m.Item2).Take(_nextUndo + 1);
        }

        /// <summary>
        /// 反撤销操作的集合
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBackableAction> EnumRedoableActions()
        {
            return _actionList.Select(m => m.Item2).Skip(_nextUndo + 1);
        }

        #endregion  Public methods

    }

}

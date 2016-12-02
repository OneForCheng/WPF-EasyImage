using System.Reflection;
using UndoFramework.Abstract;

namespace UndoFramework.Actions
{
    /// <summary>
    /// 
    /// </summary>
    public class SetPropertyAction : AbstractBackableAction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public SetPropertyAction(object parentObject, string propertyName, object value)
        {
            ParentObject = parentObject;
            Property = parentObject.GetType().GetTypeInfo().GetDeclaredProperty(propertyName);
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public object ParentObject { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected override void ExecuteCore()
        {
            OldValue = Property.GetValue(ParentObject, null);
            Property.SetValue(ParentObject, Value, null);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UnExecuteCore()
        {
            Property.SetValue(ParentObject, OldValue, null);
        }
    }
}

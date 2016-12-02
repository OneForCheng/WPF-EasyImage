using System.Windows;
using System.Windows.Controls;
using UndoFramework.Abstract;

namespace EasyImage.Actioins
{
    internal class ExchangeZIndexAction : AbstractBackableAction
    {
        private readonly UIElement _element;
        private readonly UIElement _targetElement;

        public ExchangeZIndexAction(UIElement element, UIElement targetElement)
        {
            _element = element;
            _targetElement = targetElement;
        }

        /// <summary>
        /// 执行核心操作
        /// </summary>
        protected override void ExecuteCore()
        {
            var zIndex = Panel.GetZIndex(_element);
            Panel.SetZIndex(_element, Panel.GetZIndex(_targetElement));
            Panel.SetZIndex(_targetElement, zIndex);
        }

        /// <summary>
        /// 执行核心撤销操作
        /// </summary>
        protected override void UnExecuteCore()
        {
            var zIndex = Panel.GetZIndex(_element);
            Panel.SetZIndex(_element, Panel.GetZIndex(_targetElement));
            Panel.SetZIndex(_targetElement, zIndex);
        }
    }
}

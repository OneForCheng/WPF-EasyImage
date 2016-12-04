using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DealImage.Save;
using EasyImage.Actioins;
using EasyImage.Enum;
using Microsoft.Win32;
using UndoFramework;
using UndoFramework.Actions;

namespace EasyImage
{
    public class ControlManager<T>  where T : UserControl
    {
        #region Data
        private int _maxImageZIndex;
        private readonly Panel _panelContainer;

        #endregion Data

        #region Constructors
        public ControlManager(Panel container)
        {
            _panelContainer = container;
            _maxImageZIndex = 0;
            MoveSpeed = 1.0;
            ActionManager = new ActionManager {MaxBufferCount = 20};
        }

        #endregion Constructors

        #region Properties and Events
        /// <summary>
        /// 操作管理
        /// </summary>
        public ActionManager ActionManager { get; }

        /// <summary>
        /// 移动速度
        /// </summary>
        public double MoveSpeed { get; set; }

        /// <summary>
        /// 获取选中的元素
        /// </summary>
        public IEnumerable<T> SelectedElements => _panelContainer.Children.Cast<object>().OfType<T>().Where(Selector.GetIsSelected);

        /// <summary>
        /// 获取初始化选中的元素的动作事务
        /// </summary>
        public TransactionAction ReviseRotateCenterSelected
        {
            get
            {
                var transactions = new TransactionAction();
                foreach (var item in SelectedElements)
                {
                    transactions.Add(new ReviseRotateCenterAction(item));
                }
                return transactions;
            }
        }

       
        private void Element_ZIndexTop(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderByDescending(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndex.Top)));
            }
            ActionManager.Execute(transactions);
        }

        private void Element_ZIndexTopmost(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderBy(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndex.Topmost)));
                _maxImageZIndex++;
            }
            ActionManager.Execute(transactions);
        }

        private void Element_ZIndexBottom(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderBy(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndex.Bottom)));
            }
            ActionManager.Execute(transactions);
        }

        private void Element_ZIndexBottommost(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderByDescending(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndex.Bottommost)));
            }
            ActionManager.Execute(transactions);
        }

        private void Element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as T;
            if (element == null) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                Selector.SetIsSelected(element, !Selector.GetIsSelected(element));
            }
            else
            {
                if (Selector.GetIsSelected(element)) return;
                SelectNone();
                Selector.SetIsSelected(element, true);
            }

        }

        private static void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var element = sender as T;
            var rotateControl = element?.Template.FindName("RotateThumbControl", element) as Control;
            if (rotateControl != null)
            {
                rotateControl.Visibility = element.Width < 30 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private void Menu_ExchangeImageFromClip(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
        }

        private void Menu_ExchangeImageFromFile(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var dialog = new OpenFileDialog
            {
                CheckPathExists = true,
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.gif; *.bmp; *.ico)|*.jpg; *.jpeg; *.png; *.gif; *.bmp; *.ico"
            };
            var showDialog = dialog.ShowDialog().GetValueOrDefault();
            if (!showDialog) return;
            var element = SelectedElements.First();
            if (element == null) return;
            ActionManager.Execute(new ExchangeImageAction(element, new AnimatedImage.AnimatedImage { Source = new BitmapImage(new Uri(dialog.FileName)), Stretch = Stretch.Fill }));
        }

        private void Element_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
            var element = sender as T;
            if (element == null) return;
            var count = SelectedElements.Count();
            if (count <= 0) return;
            e.Handled = false;
            var menuItem = element.ContextMenu.Items[0] as MenuItem;
            if (menuItem == null) return;
            menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Menu_SaveImage(object sender, RoutedEventArgs e)
        {
            var selectCount = SelectedElements.Count();
            if(selectCount == 0) return;
            var dialog = new SaveFileDialog
            {
                CheckPathExists = true,
                AddExtension = true,
                FilterIndex = 3,
                FileName = "图形1",
                DereferenceLinks = true,
                Filter = "GIF 可交换的图形格式 (*.gif)|*.gif|JPEG 文件交换格式 (*.jpg)|*.jpg|PNG 可移植网络图形格式 (*.png)|*.png|TIFF Tag 图像文件格式 (*.tif)|*.tif|设备无关位图 (*.bmp)|*.bmp",
                RestoreDirectory = true,
                ValidateNames = true,
            };
            var showDialog = dialog.ShowDialog().GetValueOrDefault();
            if (!showDialog) return;
            var filePath = dialog.FileName;

            if (selectCount == 1)
            {
                var element = SelectedElements.First();
                Selector.SetIsSelected(element, false);
                element.SaveChildControlToImage((Image) element.Content, filePath);
                Selector.SetIsSelected(element, true);
            }
            else
            {
                var dict = new Dictionary<FrameworkElement, FrameworkElement>();
                foreach (var element in SelectedElements.OrderBy(Panel.GetZIndex))
                {
                    dict.Add(element, (Image)element.Content);
                    Selector.SetIsSelected(element, false);
                }
                dict.SaveChildControlsToImage(filePath);
                foreach (var element in dict.Keys)
                {
                    Selector.SetIsSelected(element, true);
                }
            }

        }

        #endregion Properties and Events

        #region Public methods

        /// <summary>
        /// 添加选中元素
        /// </summary>
        /// <param name="elements"></param>
        public void AddSelected(IEnumerable<T> elements)
        {
            var enumerable = elements as IList<T> ?? elements.ToList();
            if (!enumerable.Any()) return;
            var transactions = new TransactionAction();
            foreach (var element in enumerable)
            {
                AttachProperty(element);
                transactions.Add(new AddItemAction<T>(m => _panelContainer.Children.Add(m), _panelContainer.Children.Remove, element));
            }
            ActionManager.Execute(transactions);
        }

        /// <summary>
        /// 移除选中元素
        /// </summary>
        public void RemoveSelected()
        {
            if (!SelectedElements.Any())return;
            var transactions = new TransactionAction();
            foreach (var element in SelectedElements)
            {
                transactions.Add(new AddItemAction<T>(_panelContainer.Children.Remove, m => _panelContainer.Children.Add(m),  element));
            }
            ActionManager.Execute(transactions);
        }

        /// <summary>
        /// 没有选择元素
        /// </summary>
        public void SelectNone()
        {
            foreach (T item in _panelContainer.Children)
            {
                Selector.SetIsSelected(item, false);
            }
        }

        /// <summary>
        /// 选择所有元素
        /// </summary>
        public void SelectAll()
        {
            foreach (T item in _panelContainer.Children)
            {
                Selector.SetIsSelected(item, true);
            }
        }

        /// <summary>
        /// 向指定方向移动选中的元素
        /// </summary>
        /// <param name="arrowKey">方向键</param>
        public void KeyMoveSelected(Key arrowKey)
        {
            if(!SelectedElements.Any())return;
            double moveX = 0, moveY = 0;
            switch (arrowKey)
            {
                case Key.Up:
                    moveY = -MoveSpeed;
                    break;
                case Key.Right:
                    moveX = MoveSpeed;
                    break;
                case Key.Down:
                    moveY = MoveSpeed;
                    break;
                case Key.Left:
                    moveX = -MoveSpeed;
                    break;
            }
            var transactions = new TransactionAction();
            foreach (var item in SelectedElements)
            {
                transactions.Add(new DragMoveAction(item.GetTransform<TranslateTransform>(), moveX, moveY));
            }
            ActionManager.Execute(transactions);
        }

        /// <summary>
        /// 拖动选中的元素
        /// </summary>
        /// <param name="moveX">X轴拖动距离</param>
        /// <param name="moveY">Y轴拖动距离</param>
        /// <param name="completed">是否完成拖动</param>
        public void DragMoveSelected(double moveX, double moveY, bool completed)
        {
            if (completed)
            {
                var transactions = new TransactionAction();
                foreach (var item in SelectedElements)
                {
                    var translateTransform = item.GetTransform<TranslateTransform>();
                    transactions.Add(new DragMoveAction(translateTransform, moveX, moveY));
                    translateTransform.X -= moveX;
                    translateTransform.Y -= moveY;
                }
                ActionManager.Execute(transactions);
            }
            else
            {
                foreach (var item in SelectedElements)
                {
                    var translateTransform = item.GetTransform<TranslateTransform>();
                    translateTransform.X += moveX;
                    translateTransform.Y += moveY;
                }
            }
        }

        /// <summary>
        /// 拖拽旋转选中的元素
        /// </summary>
        /// <param name="angle">旋转角度</param>
        /// <param name="completed">是否完成拖动</param>
        /// <param name="transactions">操作事务</param>
        public void DragRotateSelected(double angle, bool completed, TransactionAction transactions = null)
        {
            if (completed && transactions != null)
            {
                foreach (var item in SelectedElements)
                {
                    item.GetTransform<RotateTransform>().Angle -= angle;
                }
                transactions.UnExecute();
                foreach (var item in SelectedElements)
                {
                    transactions.Add(new DragRotateAction(item.GetTransform<RotateTransform>(), angle));
                }
                ActionManager.Execute(transactions);
            }
            else
            {
                foreach (var item in SelectedElements)
                {
                    item.GetTransform<RotateTransform>().Angle += angle;
                }
            }
        }

        /// <summary>
        /// 拉伸选中的元素
        /// </summary>
        /// <param name="scaleHorizontal">水平方向放缩的比例</param>
        /// <param name="scaleVertical">竖直方向放缩的比例</param>
        /// <param name="turnHorizontal">是否水平方向发生翻转</param>
        /// <param name="turnVerticale">是否水平竖直发生翻转</param>
        /// <param name="thumbFlag">拖拽控件的标识</param>
        /// <param name="completed">是否完成拉伸</param>
        public void DragResizeSelected(double scaleHorizontal, double scaleVertical, bool turnHorizontal, bool turnVerticale, ThumbFlag thumbFlag, bool completed)
        {
            if (completed)
            {
                var transactions = new TransactionAction();
                foreach (var element in SelectedElements)
                {
                    var translateTransform = element.GetTransform<TranslateTransform>();
                    var scaleTransform = element.GetTransform<ScaleTransform>();
                    var radian = element.GetTransform<RotateTransform>().Angle * Math.PI / 180;

                    if (turnVerticale)
                    {
                        scaleTransform.ScaleY = -scaleTransform.ScaleY;
                        if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.TopCenter || thumbFlag == ThumbFlag.TopRight)
                        {
                            translateTransform.Y -= 2 * element.Height * Math.Cos(-radian) * scaleTransform.ScaleY;
                            translateTransform.X -= 2 * element.Height * Math.Sin(-radian) * scaleTransform.ScaleY;
                        }
                    }
                    if (turnHorizontal)
                    {
                        scaleTransform.ScaleX = -scaleTransform.ScaleX;
                        if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.LeftCenter || thumbFlag == ThumbFlag.BottomLeft)
                        {
                            translateTransform.Y -= 2 * element.Width * Math.Sin(radian) * scaleTransform.ScaleX;
                            translateTransform.X -= 2 * element.Width * Math.Cos(radian) * scaleTransform.ScaleX;
                        }
                    }
           
                    var deltaVertical = element.Height * (1 - scaleVertical);
                    var deltaHorizontal = element.Width * (1 - scaleHorizontal);

                    if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.TopCenter || thumbFlag == ThumbFlag.TopRight)
                    {
                        translateTransform.Y += deltaVertical * Math.Cos(-radian) * scaleTransform.ScaleY;
                        translateTransform.X += deltaVertical * Math.Sin(-radian) * scaleTransform.ScaleY;
                    }

                    element.Height = element.Height * scaleVertical;

                    if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.LeftCenter || thumbFlag == ThumbFlag.BottomLeft)
                    {
                        translateTransform.Y += deltaHorizontal * Math.Sin(radian) * scaleTransform.ScaleX;
                        translateTransform.X += deltaHorizontal * Math.Cos(radian) * scaleTransform.ScaleX;
                    }

                    element.Width = element.Width * scaleHorizontal;

                    transactions.Add(new DragResizeAction(element, 1/scaleHorizontal, 1/scaleVertical, turnHorizontal, turnVerticale, thumbFlag));
                }

                ActionManager.Execute(transactions);
            }
            else
            {
                foreach (var element in SelectedElements)
                {
                    var translateTransform = element.GetTransform<TranslateTransform>();
                    var scaleTransform = element.GetTransform<ScaleTransform>();
                    var radian = element.GetTransform<RotateTransform>().Angle * Math.PI / 180;

                    if (!turnVerticale && !turnHorizontal)
                    {
                        var deltaVertical = element.Height * (1 - scaleVertical);
                        var deltaHorizontal = element.Width * (1 - scaleHorizontal);

                        if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.TopCenter || thumbFlag == ThumbFlag.TopRight)
                        {
                            translateTransform.Y += deltaVertical * Math.Cos(-radian) * scaleTransform.ScaleY;
                            translateTransform.X += deltaVertical * Math.Sin(-radian) * scaleTransform.ScaleY;
                        }

                        element.Height = element.Height * scaleVertical;

                        if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.LeftCenter || thumbFlag == ThumbFlag.BottomLeft)
                        {
                            translateTransform.Y += deltaHorizontal * Math.Sin(radian) * scaleTransform.ScaleX;
                            translateTransform.X += deltaHorizontal * Math.Cos(radian) * scaleTransform.ScaleX;
                        }

                        element.Width = element.Width * scaleHorizontal;
                    }
                    else
                    {
                        if (turnVerticale)
                        {
                            scaleTransform.ScaleY = -scaleTransform.ScaleY;
                            if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.TopCenter || thumbFlag == ThumbFlag.TopRight)
                            {
                                translateTransform.Y -= 2 * element.Height * Math.Cos(-radian) * scaleTransform.ScaleY;
                                translateTransform.X -= 2 * element.Height * Math.Sin(-radian) * scaleTransform.ScaleY;
                            }
                        }
                        if (turnHorizontal)
                        {
                            scaleTransform.ScaleX = -scaleTransform.ScaleX;
                            if (thumbFlag == ThumbFlag.TopLeft || thumbFlag == ThumbFlag.LeftCenter || thumbFlag == ThumbFlag.BottomLeft)
                            {
                                translateTransform.Y -= 2 * element.Width * Math.Sin(radian) * scaleTransform.ScaleX;
                                translateTransform.X -= 2 * element.Width * Math.Cos(radian) * scaleTransform.ScaleX;
                            }
                            
                        }
                        
                    }
                }
            }
           
        }

        /// <summary>
        /// 滚轮放缩元素
        /// </summary>
        /// <param name="element">缩放元素</param>
        /// <param name="delta">缩放值</param>
        /// <param name="scalePoint">放缩点(相对于元素)</param>
        public void WheelFixedScaleElement(T element, double delta, Point scalePoint)
        {
            ActionManager.Execute(new WheelScaleAction(element, delta, scalePoint));
        }

        /// <summary>
        /// 滚轮放缩选中的元素（缩放中心为控件中心）
        /// </summary>
        /// <param name="delta">缩放值</param>
        public void WheelCenterScaleSelected(double delta)
        {
            var transactions = new TransactionAction();
            foreach (var item in SelectedElements)
            {
                transactions.Add(new WheelScaleAction(item, delta, new Point(item.Width / 2, item.Height / 2)));
            }
            ActionManager.Execute(transactions);
        }

        #endregion Public methods

        #region Private methods
        private void AttachProperty(T element)
        {
            #region 设置属性
            Selector.SetIsSelected(element, true);
            _maxImageZIndex++;
            Panel.SetZIndex(element, _maxImageZIndex);

            #endregion

            #region 添加上下文菜单
            var contextMenu = new ContextMenu();
            

            #region 更改图片

            var item = new MenuItem { Header = "更改图片" };
            
            #region 二级菜单

            var subItem = new MenuItem { Header = "来自文件..." };
            subItem.Click += Menu_ExchangeImageFromFile;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "自剪贴板..." };
            subItem.Click += Menu_ExchangeImageFromClip;
            item.Items.Add(subItem);
            #endregion

            contextMenu.Items.Add(item);
            #endregion

            #region 置于顶层

            item = new MenuItem {Header = "置于顶层"};

            #region 二级菜单

            subItem = new MenuItem {Header = "置于顶层"};
            subItem.Click += Element_ZIndexTopmost;
            item.Items.Add(subItem);

            subItem = new MenuItem {Header = "上移一层"};
            subItem.Click += Element_ZIndexTop;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);
            #endregion

            #region 置于底层

            item = new MenuItem {Header = "置于底层"};

            #region 二级菜单

            subItem = new MenuItem {Header = "置于底层"};
            subItem.Click += Element_ZIndexBottommost;
            item.Items.Add(subItem);

            subItem = new MenuItem {Header = "下移一层"};
            subItem.Click += Element_ZIndexBottom;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);
            #endregion

            #region 另存为图片
            item = new MenuItem { Header = "另存为图片..." };
            item.Click += Menu_SaveImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 设置
            var separatorMenuItem = new Separator();//分割线
            contextMenu.Items.Add(separatorMenuItem);

            item = new MenuItem {Header = "设置"};
            contextMenu.Items.Add(item);
            #endregion

            element.ContextMenu = contextMenu;

            #endregion

            #region 添加事件
            element.PreviewMouseDown += Element_PreviewMouseDown;
            element.ContextMenuOpening += Element_ContextMenuOpening;
            element.SizeChanged += Element_SizeChanged;

            #endregion
        }

        private UIElement GetExchangeZIndexElement(T element, ZIndex zIndex)
        {
            var targetElement = element;
            var targetZIndex = Panel.GetZIndex(element);
            int tmp;
            var flag = 1;
            if (zIndex == ZIndex.Bottommost || zIndex == ZIndex.Bottom)
            {
                flag = -1;
            }
            if (zIndex == ZIndex.Bottommost || zIndex == ZIndex.Topmost)
            {
                foreach (T item in _panelContainer.Children)
                {
                    tmp = Panel.GetZIndex(item);
                    if ((targetZIndex - tmp)*flag >= 0) continue;
                    targetZIndex = tmp;
                    targetElement = item;
                }
                if (Equals(targetElement, element)) return targetElement;
                var newElement = new UIElement();
                Panel.SetZIndex(newElement, targetZIndex + flag);
                return newElement;
            }
            else
            {
                var nearZIndex = targetZIndex;
                foreach (T item in _panelContainer.Children)
                {
                    tmp = Panel.GetZIndex(item);
                    if (nearZIndex == targetZIndex)
                    {
                        if ((targetZIndex - tmp)*flag >= 0) continue;
                        nearZIndex = tmp;
                        targetElement = item;
                    }
                    else
                    {
                        if ((tmp - nearZIndex)*flag >= 0 || (tmp - targetZIndex)*flag <= 0) continue;
                        nearZIndex = tmp;
                        targetElement = item;
                    }
                }
                return targetElement;
            }
        }

        #endregion Private methods

    }

}

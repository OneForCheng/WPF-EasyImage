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
using DealImage;
using DealImage.Copy;
using DealImage.Paste;
using DealImage.Save;
using EasyImage.Actioins;
using EasyImage.Enum;
using IconMaker;
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
        /// 连续复制次数
        /// </summary>
        public int ContinuedPasteCount { get; set; }

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

        private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
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
            var imageSource = ImagePasteHelper.GetExchangeImageFromClip();
            if (imageSource != null)
            {
                ActionManager.Execute(new ExchangeImageAction(SelectedElements.First(), new AnimatedImage.AnimatedImage { Source = imageSource, Stretch = Stretch.Fill }));
            }
        }

        private void Menu_ExchangeImageFromFile(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var dialog = new OpenFileDialog
            {
                CheckPathExists = true,
                Filter = "所有图片 (*.ico;*.gif;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.tif;*.tiff;*.bmp;*.dib;*.rle)|*.ico;*.gif;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.tif;*.tiff;*.bmp;*.dib;*.rle"
                + "|ICO 图标格式 (*.ico)|*.ico"
                + "|GIF 可交换的图形格式 (*.gif)|*.gif"
                + "|JPEG 文件交换格式 (*.jpg;*.jpeg;*.jfif;*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe"
                + "|PNG 可移植网络图形格式 (*.png)|*.png"
                + "|TIFF Tag 图像文件格式 (*.tif;*.tiff)|*.tif;*.tiff"
                + "|设备无关位图 (*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle"
            };
            var showDialog = dialog.ShowDialog().GetValueOrDefault();
            if (!showDialog) return;
            var element = SelectedElements.First();
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
            var menuItem = element.ContextMenu.Items[2] as MenuItem;
            if (menuItem != null)
            {
                menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ExchangeImageMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var menuItem = menu?.Items[1] as MenuItem;
            if (menuItem != null)
            {
                menuItem.IsEnabled = ImagePasteHelper.CanExchangeImageFromClip();
            }
        }

        private void Menu_SaveToImage(object sender, RoutedEventArgs e)
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
                Filter =  "GIF 可交换的图形格式 (*.gif)|*.gif"
                        + "|JPEG 文件交换格式 (*.jpg)|*.jpg"
                        + "|PNG 可移植网络图形格式 (*.png)|*.png"
                        + "|TIFF Tag 图像文件格式 (*.tif)|*.tif"
                        + "|设备无关位图 (*.bmp)|*.bmp",
                ValidateNames = true,
            };
            if (selectCount == 1)
            {
                var element = SelectedElements.First();
                var bitmapSource = (element.Content as AnimatedImage.AnimatedImage)?.Source as BitmapImage;
                if (bitmapSource != null)
                {
                    var fileExt  = bitmapSource.StreamSource != null ? ImageHelper.GetFileExtension(bitmapSource.StreamSource) : ImageHelper.GetFileExtension(bitmapSource.UriSource.AbsolutePath);
                    Trace.WriteLine(fileExt);
                    switch (fileExt)
                    {
                        case FileExtension.Gif:
                            dialog.FilterIndex = 1;
                            break;
                        case FileExtension.Jpg:
                            dialog.FilterIndex = 2;
                            break;
                        case FileExtension.Png:
                            dialog.FilterIndex = 3;
                            break;
                        case FileExtension.Tif:
                            dialog.FilterIndex = 4;
                            break;
                        case FileExtension.Bmp:
                            dialog.FilterIndex = 5;
                            break;
                        default:
                            dialog.FilterIndex = 3;
                            break;
                    }
                }
            }
            var showDialog = dialog.ShowDialog().GetValueOrDefault();
            if (!showDialog) return;
            var filePath = dialog.FileName;
            var dict = SelectedElements.OrderBy(Panel.GetZIndex).ToDictionary<T, FrameworkElement, FrameworkElement>(element => element, element => (Image) element.Content);
            SetIsSelected(dict.Keys, false);
            dict.SaveChildElementsToImage(filePath);
            SetIsSelected(dict.Keys, true);

        }

        private void Menu_SaveToIcon(object sender, RoutedEventArgs e)
        {
            if (!SelectedElements.Any()) return;
            var menuItem = sender as MenuItem;
            var menu = menuItem?.Parent as MenuItem;
            if (menu != null)
            {
                var dialog = new SaveFileDialog
                {
                    CheckPathExists = true,
                    AddExtension = true,
                    FileName = "图标1",
                    DereferenceLinks = true,
                    Filter = "ICO 图标格式 (*.ico)|*.ico",
                    ValidateNames = true,
                };
                var showDialog = dialog.ShowDialog().GetValueOrDefault();
                if (!showDialog) return;
                var filePath = dialog.FileName;
                var dict = SelectedElements.OrderBy(Panel.GetZIndex).ToDictionary<T, FrameworkElement, FrameworkElement>(element => element, element => (Image)element.Content);
                SetIsSelected(dict.Keys, false);
                var imageSource = dict.GetRenderTargetBitmap();
                SetIsSelected(dict.Keys, true);
                var index = menu.Items.IndexOf(menuItem);
                Trace.WriteLine(index);
                switch (index)
                {
                    case 0:
                        imageSource.SaveIcon(filePath, IconSize.Bmp256);
                        break;
                    case 1:
                        imageSource.SaveIcon(filePath, IconSize.Bmp128);
                        break;
                    case 2:
                        imageSource.SaveIcon(filePath, IconSize.Bmp96);
                        break;
                    case 3:
                        imageSource.SaveIcon(filePath, IconSize.Bmp72);
                        break;
                    case 4:
                        imageSource.SaveIcon(filePath, IconSize.Bmp64);
                        break;
                    case 5:
                        imageSource.SaveIcon(filePath, IconSize.Bmp48);
                        break;
                    case 6:
                        imageSource.SaveIcon(filePath, IconSize.Bmp32);
                        break;
                    case 7:
                        imageSource.SaveIcon(filePath, IconSize.Bmp24);
                        break;
                    case 8:
                        imageSource.SaveIcon(filePath, IconSize.Bmp16);
                        break;
                    default:
                        imageSource.SaveIcon(filePath, IconSize.Bmp32);
                        break;
                }

            }
        }

        private void Menu_CopyImage(object sender, RoutedEventArgs e)
        {
            CopySelected();
        }

        private void Menu_ClipImage(object sender, RoutedEventArgs e)
        {
            ClipSelected();
        }

        #endregion Properties and Events

        #region Public methods

        /// <summary>
        /// 添加选中元素
        /// </summary>
        /// <param name="elements"></param>
        public void AddElements(IEnumerable<T> elements)
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
        /// 复制选中的元素
        /// </summary>
        public void CopySelected()
        {
            if (!SelectedElements.Any()) return;
            var count = SelectedElements.Count();
            var dict = new Dictionary<FrameworkElement, FrameworkElement>(count);
            var baseInfos = new List<ImageControlBaseInfo>(count);
            foreach (var element in SelectedElements.OrderBy(Panel.GetZIndex))
            {
                var image = (Image)element.Content;
                dict.Add(element, image);
                baseInfos.Add(new ImageControlBaseInfo(element));
            }
            SetIsSelected(dict.Keys, false);
            dict.CopyChildElementsToClipBoard(baseInfos);
            SetIsSelected(dict.Keys, true);
        }

        /// <summary>
        /// 剪切选中的元素
        /// </summary>
        public void ClipSelected()
        {
            CopySelected();
            RemoveSelected();
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

            #region 剪贴
            var item = new MenuItem { Header = "剪贴" };
            item.Click += Menu_ClipImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 复制
            item = new MenuItem { Header = "复制" };
            item.Click += Menu_CopyImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 更改图片

            item = new MenuItem { Header = "更改图片" };
            item.SubmenuOpened += ExchangeImageMenuItem_SubmenuOpened;
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
            item.Click += Menu_SaveToImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 另存为图标
            item = new MenuItem { Header = "另存为图标" };
            contextMenu.Items.Add(item);

            #region 二级菜单
            subItem = new MenuItem { Header = "ICO 256*256" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 128*128" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 96*96" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 72*72" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 64*64" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 48*48" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 32*32" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 24*24" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 16*16" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            #endregion

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

        private void SetIsSelected(IEnumerable<UIElement> elements,bool isSelected)
        {
            foreach (var item in elements)
            {
                Selector.SetIsSelected(item, isSelected);
            }
        }

        #endregion Private methods

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AnimatedImage;
using AnimatedImage.Encoding;
using Microsoft.Win32;
using DealImage;
using DealImage.Copy;
using DealImage.Crop;
using DealImage.Paste;
using DealImage.Save;
using EasyImage.Actioins;
using EasyImage.Controls;
using EasyImage.Enum;
using EasyImage.Windows;
using IconMaker;
using UndoFramework;
using UndoFramework.Actions;
using IPlugins;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using ImageDataFormats = DealImage.ImageDataFormats;
using Point = System.Windows.Point;

namespace EasyImage
{
    public class ControlManager
    {

        #region Constructors
        public ControlManager(Panel container)
        {
            _panelContainer = container;
            _maxControlZIndex = 0;
            MoveSpeed = 1.0;
            _actionManager = new ActionManager { MaxBufferCount = 20 };
            _statusCode = _actionManager.StatusCode;
        }

        #endregion Constructors

        #region Private fields
        private readonly Panel _panelContainer;
        private readonly ActionManager _actionManager;
        private List<Tuple<string, Bitmap, List<IFilter>>> _cachePlugins;
        private ImageControl[] _cacheSelectedElements;
        private int _statusCode;
        private int _maxControlZIndex;

        #endregion Private fields

        #region Properties
        /// <summary>
        /// 状态码是否发生改变
        /// </summary>
        public bool StatusCodeChanged => _statusCode != _actionManager.StatusCode;

        /// <summary>
        /// 移动速度
        /// </summary>
        public double MoveSpeed { get; set; }

        /// <summary>
        /// 连续添加元素的次数
        /// </summary>
        public int ContinuedAddCount { get; set; }

        /// <summary>
        /// 获取选中的元素
        /// </summary>
        public IEnumerable<ImageControl> SelectedElements => _panelContainer.Children.Cast<object>().OfType<ImageControl>().Where(Selector.GetIsSelected);

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


        #endregion Properties

        #region Public methods

        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="pluginsDir">插件目录</param>
        public void LoadPlugins(string pluginsDir)
        {
            if (pluginsDir == null)
            {
                pluginsDir = "Plugins";
            }
            if (!Directory.Exists(pluginsDir))
            {
                pluginsDir = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, pluginsDir);
                if (!Directory.Exists(pluginsDir))
                {
                    Directory.CreateDirectory(pluginsDir);
                    return;
                }
            }
            _cachePlugins = new List<Tuple<string, Bitmap, List<IFilter>>>();
            foreach (var filePath in Directory.GetFiles(pluginsDir, "*.dll"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
                try
                {
                    var list = GetPluginLists(filePath);
                    if (list != null && list.Count != 0)
                    {
                        list.ForEach(m=> _cachePlugins.Add(new Tuple<string, Bitmap, List<IFilter>>(m.GetPluginName(), m.GetPluginIcon(), m.GetIFilterList())));
                    }
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                    Extentions.ShowMessageBox($"加载 {fileName} 插件失败");
                }
            }
        }

        /// <summary>
        /// 重置所有状态
        /// </summary>
        public void Clear()
        {
            _maxControlZIndex = 0;
            _cacheSelectedElements = null;
            ContinuedAddCount = 0;
            MoveSpeed = 1;
            _actionManager.Clear();
            _statusCode = _actionManager.StatusCode;
            _panelContainer.Children.Clear();
            GC.Collect();
        }

        /// <summary>
        /// 更新状态码
        /// </summary>
        public void UpdateStatusCode()
        {
            _statusCode = _actionManager.StatusCode;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="elements"></param>
        public void Initialize(IEnumerable<ImageControl> elements)
        {
            var enumerable = elements as IList<ImageControl> ?? elements.ToList();
            if (!enumerable.Any()) return;
            foreach (var element in enumerable)
            {
                AttachProperty(element);
                Selector.SetIsSelected(element, false);
                _panelContainer.Children.Add(element);
            }
        }

        /// <summary>
        /// 撤销操作
        /// </summary>
        public void UnExecute()
        {
            _actionManager.UnExecute();
        }

        /// <summary>
        /// 反撤销操作
        /// </summary>
        public void ReExecute()
        {
            _actionManager.ReExecute();
        }

        /// <summary>
        /// 添加选中元素
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(ImageControl element)
        {
            AttachProperty(element);
            var addItemAction = new AddItemAction<ImageControl>(m => _panelContainer.Children.Add(m),
                _panelContainer.Children.Remove, element);
            _actionManager.Execute(addItemAction);
        }

        /// <summary>
        /// 添加选中元素集合
        /// </summary>
        /// <param name="elements"></param>
        public void AddElements(IEnumerable<ImageControl> elements)
        {
            var enumerable = elements as IList<ImageControl> ?? elements.ToList();
            if (!enumerable.Any()) return;
            var transactions = new TransactionAction();
            foreach (var element in enumerable)
            {
                AttachProperty(element);
                transactions.Add(new AddItemAction<ImageControl>(m => _panelContainer.Children.Add(m), _panelContainer.Children.Remove, element));
            }
            _actionManager.Execute(transactions);
        }

        /// <summary>
        /// 移除选中元素
        /// </summary>
        public void RemoveSelected()
        {
            if (!SelectedElements.Any()) return;
            var transactions = new TransactionAction();
            foreach (var element in SelectedElements)
            {
                transactions.Add(new AddItemAction<ImageControl>(_panelContainer.Children.Remove, m => _panelContainer.Children.Add(m), element));
            }
            _actionManager.Execute(transactions);
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
        /// 克隆选中的元素
        /// </summary>
        public void CloneSelected()
        {
            SelectNone();
            if (_cacheSelectedElements != null)
            {
                AddElements(_cacheSelectedElements.Select(m => m.Clone()).Cast<ImageControl>());
                _cacheSelectedElements = null;
            }

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
            foreach (ImageControl item in _panelContainer.Children)
            {
                Selector.SetIsSelected(item, false);
            }
        }

        /// <summary>
        /// 选择所有元素
        /// </summary>
        public void SelectAll()
        {
            foreach (ImageControl item in _panelContainer.Children)
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
            if (!SelectedElements.Any()) return;
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
            _actionManager.Execute(transactions);
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

                _actionManager.Execute(transactions);
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
                _actionManager.Execute(transactions);
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

                    transactions.Add(new DragResizeAction(element, 1 / scaleHorizontal, 1 / scaleVertical, turnHorizontal, turnVerticale, thumbFlag));
                }

                _actionManager.Execute(transactions);
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
        public void WheelFixedScaleElement(ImageControl element, double delta, Point scalePoint)
        {
            _actionManager.Execute(new WheelScaleAction(element, delta, scalePoint));
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
            _actionManager.Execute(transactions);
        }

        #endregion Public methods

        #region Events

        private void PluginItem_Click(object sender, EventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            element.Visibility = Visibility.Hidden;

            var bitmapSource = ((Image)element.Content).Source as BitmapSource;
            var result = ((sender as MenuItem)?.Tag as IFilter)?.ExecHandle(bitmapSource.GetResizeBitmap((int)Math.Round(element.Width), (int)Math.Round(element.Height)).GetBitmap());

            element.Visibility = Visibility.Visible;
            if (result == null) return;
            if (!result.IsSuccess)
            {
                App.Log.Error(result.Exception.ToString());
                Extentions.ShowMessageBox("程序错误,请查看日志了解详细情况!");
            }
            else if (result.IsModified)
            {
                using (var bitmap = result.ResultBitmap)
                {
                    _actionManager.Execute(new ExchangeImageAction(element, new AnimatedGif { Source = bitmap.GetBitmapSource().GetBitmapImage(), Stretch = Stretch.Fill }));
                }
            }
        }

        private void Element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as ImageControl;
            if (element == null) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                var selected = Selector.GetIsSelected(element);
                if (selected)
                {
                    _cacheSelectedElements = SelectedElements.ToArray();
                }
                Selector.SetIsSelected(element, !selected);
                if (!selected)
                {
                    _cacheSelectedElements = SelectedElements.ToArray();
                }
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
            var element = sender as ImageControl;
            var rotateControl = element?.Template.FindName("RotateThumbControl", element) as Control;
            if (rotateControl != null)
            {
                rotateControl.Visibility = element.Width < 30 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private void Menu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
            var element = sender as ImageControl;
            if (element == null) return;
            var count = SelectedElements.Count();
            if (count <= 0) return;
            e.Handled = false;

            var menuItems = element.ContextMenu?.Items?.OfType<MenuItem>().ToArray();
            if (menuItems == null) return;

            var menuItem = menuItems.Single(m => m.Tag.ToString() == "TrimSelf");
            menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;

            menuItem = menuItems.Single(m => m.Tag.ToString() == "CaptureImage");
            menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;

            menuItem = menuItems.Single(m => m.Tag.ToString() == "CutImage");
            menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;

            menuItem = menuItems.Single(m => m.Tag.ToString() == "Exchange");
            menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;

            menuItem = menuItems.Single(m => m.Tag.ToString() == "SetBackground");
            menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;

            menuItem = menuItems.Single(m => m.Tag.ToString() == "Combine");
            menuItem.IsEnabled = count > 1;

            menuItem = menuItems.SingleOrDefault(m => m.Tag.ToString() == "Plugin");
            if (menuItem != null)
            {
                menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            menuItem = menuItems.Single(m => m.Tag.ToString() == "GifDeal");
            menuItem.Visibility = count == 1 && ((AnimatedGif)element.Content).Animatable ? Visibility.Visible : Visibility.Collapsed;

            menuItem = menuItems.Single(m => m.Tag.ToString() == "Setting");
            menuItem.Visibility = count == 1 ? Visibility.Visible : Visibility.Collapsed;

        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var menuItem = menu?.Items[1] as MenuItem;
            if (menuItem != null)
            {
                menuItem.IsEnabled = ImagePaster.CanPasteImageFromClipboard();
            }
        }

        private void Menu_CopyImage(object sender, RoutedEventArgs e)
        {
            CopySelected();
        }

        private void Menu_CopyRemoveImage(object sender, RoutedEventArgs e)
        {
            ClipSelected();
        }

        private void MenuItem_CropImage(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var width = (int)Math.Round(element.Width);
            var height = (int)Math.Round(element.Height);
            if (width == 0 || height == 0)
            {
                return;
            }
            element.Visibility = Visibility.Hidden;
            var window = new GifCropWindow((AnimatedGif)element.Content, width, height);
            window.ShowDialog();
            if (window.NewAnimatedGif != null)
            {
                _actionManager.Execute(new ExchangeImageAction(element, window.NewAnimatedGif));
            }
            element.Visibility = Visibility.Visible;
        }


        private void Menu_ZIndexTop(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderByDescending(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndexFlag.Top)));
            }
            _actionManager.Execute(transactions);
        }

        private void Menu_ZIndexTopmost(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderBy(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndexFlag.Topmost)));
                _maxControlZIndex++;
            }
            _actionManager.Execute(transactions);
        }

        private void Menu_ZIndexBottom(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderBy(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndexFlag.Bottom)));
            }
            _actionManager.Execute(transactions);
        }

        private void Menu_ZIndexBottommost(object sender, RoutedEventArgs e)
        {
            var elements = SelectedElements.OrderByDescending(Panel.GetZIndex);
            var transactions = new TransactionAction();
            foreach (var item in elements)
            {
                transactions.Add(new ExchangeZIndexAction(item, GetExchangeZIndexElement(item, ZIndexFlag.Bottommost)));
            }
            _actionManager.Execute(transactions);
        }

        private async void Menu_ExchangeImageFromFile(object sender, RoutedEventArgs e)
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
            try
            {
                _actionManager.Execute(new ExchangeImageAction(element, new AnimatedGif { Source = await Extentions.GetBitmapImage(dialog.FileName), Stretch = Stretch.Fill }));
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
                Extentions.ShowMessageBox("不支持此格式的图片!");

            }

        }

        private void MenuItem_CaptureImageFromScreen(object sender, RoutedEventArgs e)
        {
            ExchangeImageFromScreen(CropStyle.Default);
        }

        private void MenuItem_ShadowCaptureImageFromScreen(object sender, RoutedEventArgs e)
        {
            ExchangeImageFromScreen(CropStyle.Shadow);
        }

        private void MenuItem_TransparentCaptureImageFromScreen(object sender, RoutedEventArgs e)
        {
            ExchangeImageFromScreen(CropStyle.Transparent);
        }

        private void MenuItem_CaptureImageFromInternal(object sender, RoutedEventArgs e)
        {
            CropImageFromInternal(CropStyle.Default);
        }

        private void MenuItem_ShadowCaptureImageFromInternal(object sender, RoutedEventArgs e)
        {
            CropImageFromInternal(CropStyle.Shadow);
        }

        private void MenuItem_TransparentCaptureImageFromInternal(object sender, RoutedEventArgs e)
        {
            CropImageFromInternal(CropStyle.Transparent);
        }

        private void MenuItem_CutImageFromInternal(object sender, RoutedEventArgs e)
        {
            CutImageFromInternal(CropStyle.Default);
        }

        private void MenuItem_ShadowCutImageFromInternal(object sender, RoutedEventArgs e)
        {
            CutImageFromInternal(CropStyle.Shadow);
        }

        private void MenuItem_TransparentCutImageFromInternal(object sender, RoutedEventArgs e)
        {
            CutImageFromInternal(CropStyle.Transparent);
        }

        private async void Menu_ExchangeImageFromClipboard(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var imageSource = (await ImagePaster.GetPasteImagesFromClipboard()).FirstOrDefault();
            if (imageSource != null)
            {
                _actionManager.Execute(new ExchangeImageAction(SelectedElements.First(), new AnimatedGif { Source = imageSource, Stretch = Stretch.Fill }));
            }
        }

        private void Menu_ExchangeImageFromInternal(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var width = (int)Math.Round(element.Width, 0);
            var height = (int)Math.Round(element.Height, 0);
            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
            }
            drawingVisual.Opacity = 0.1;
            var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);
            _actionManager.Execute(new ExchangeImageAction(SelectedElements.First(), new AnimatedGif { Source = renderBitmap.GetBitmapImage(), Stretch = Stretch.Fill }));

        }

        private async void Menu_SetPngImageBackground(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null || SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var tag = menuItem.Tag.ToString();

            BitmapSource bitmapSource;
            if (tag == "Default")
            {
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    context.DrawRectangle(Brushes.White, null, new Rect(0, 0, 10, 10));
                }
                var renderBitmap = new RenderTargetBitmap(10, 10, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawingVisual);
                bitmapSource = renderBitmap;
            }
            else if (tag == "Clipboard")
            {
                bitmapSource = (await ImagePaster.GetPasteImagesFromClipboard()).FirstOrDefault() as BitmapSource;
                if (bitmapSource == null) return;
            }
            else
            {
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
                bitmapSource = await Extentions.GetBitmapImage(dialog.FileName);
            }
            var animatedImage = (AnimatedGif)element.Content;
            if (animatedImage.Animatable)
            {
                var bitmapFrames = animatedImage.BitmapFrames;
                var pixelWidth = bitmapFrames.First().PixelWidth;
                var pixelHeight = bitmapFrames.First().PixelHeight;
                var rect = new Rectangle(0, 0, pixelWidth, pixelHeight);

                var stream = new MemoryStream();
                using (var bitmapLayer = bitmapSource.GetResizeBitmap(pixelWidth, pixelHeight).GetBitmap())
                {
                    using (var clone = (Bitmap)bitmapLayer.Clone())
                    {
                        using (var encoder = new GifEncoder(stream, pixelWidth, pixelHeight, animatedImage.RepeatCount))
                        {
                            var delays = animatedImage.Delays;
                            using (var g = Graphics.FromImage(bitmapLayer))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                for (var i = 0; i < bitmapFrames.Count; i++)
                                {
                                    g.Clear(Color.Transparent);
                                    using (var bitmap = bitmapFrames[i].GetBitmap())
                                    {
                                        g.DrawImageUnscaled(clone, rect);
                                        g.DrawImageUnscaled(bitmap, rect);
                                        encoder.AppendFrame(bitmapLayer, (int)delays[i].TotalMilliseconds);
                                    }
                                }
                            }
                        }
                    }
                }

                stream.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapSource = bitmapImage;
            }
            else
            {
                var bitmapImage = (BitmapImage)animatedImage.Source;
                var rect = new Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    context.DrawImage(bitmapSource, rect);
                    context.DrawImage(bitmapImage, rect);
                }
                var renderBitmap = new RenderTargetBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawingVisual);
                bitmapSource = renderBitmap.GetBitmapImage();
            }

            _actionManager.Execute(new ExchangeImageAction(element, new AnimatedGif { Source = bitmapSource, Stretch = Stretch.Fill }, false));

        }

        private void Menu_CombineImage(object sender, RoutedEventArgs e)
        {
            var count = SelectedElements.Count();
            if (count < 2) return;
            var dict = SelectedElements.OrderBy(Panel.GetZIndex).ToDictionary<ImageControl, FrameworkElement, FrameworkElement>(element => element, element => (Image)element.Content);
            var template = SelectedElements.First().Template;
            SetIsSelected(dict.Keys, false);

            var rect = dict.Values.GetMinContainRect();
            var bitmapImage = dict.GetCombinedBitmap(rect);//获取合并后图片（支持单张动态图的合并）
            SetIsSelected(dict.Keys, true);

            var animatedImage = new AnimatedGif { Source = bitmapImage, Stretch = Stretch.Fill };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1));
            transformGroup.Children.Add(new RotateTransform(0));
            transformGroup.Children.Add(new TranslateTransform(rect.X, rect.Y));

            var imageControl = new ImageControl(this)
            {
                Width = bitmapImage.Width,
                Height = bitmapImage.Height,
                Content = animatedImage,
                Template = template,
                RenderTransform = transformGroup,
            };
            AttachProperty(imageControl);

            var transactions = new TransactionAction();
            foreach (var element in dict.Keys.Cast<ImageControl>())
            {
                transactions.Add(new AddItemAction<ImageControl>(_panelContainer.Children.Remove, m => _panelContainer.Children.Add(m), element));
            }
            transactions.Add(new AddItemAction<ImageControl>(m => _panelContainer.Children.Add(m), _panelContainer.Children.Remove, imageControl));
            _actionManager.Execute(transactions);
        }

        private void Menu_SaveToImage(object sender, RoutedEventArgs e)
        {
            var selectCount = SelectedElements.Count();
            if (selectCount == 0) return;
            var dialog = new SaveFileDialog
            {
                CheckPathExists = true,
                AddExtension = true,
                FilterIndex = 3,
                FileName = "图形1",
                Filter = "GIF 可交换的图形格式 (*.gif)|*.gif"
                        + "|JPEG 文件交换格式 (*.jpg)|*.jpg"
                        + "|PNG 可移植网络图形格式 (*.png)|*.png"
                        + "|TIFF Tag 图像文件格式 (*.tif)|*.tif"
                        + "|设备无关位图 (*.bmp)|*.bmp",
                ValidateNames = true,
            };
            if (selectCount == 1)
            {
                var element = SelectedElements.First();
                var bitmapSource = (element.Content as AnimatedGif)?.Source as BitmapImage;
                if (bitmapSource != null)
                {
                    var fileExt = bitmapSource.StreamSource != null ? ImageHelper.GetImageExtension(bitmapSource.StreamSource) : ImageHelper.GetImageExtension(bitmapSource.UriSource.AbsolutePath);
                    switch (fileExt)
                    {
                        case ImageExtension.Gif:
                            dialog.FilterIndex = 1;
                            break;
                        case ImageExtension.Jpg:
                            dialog.FilterIndex = 2;
                            break;
                        case ImageExtension.Png:
                            dialog.FilterIndex = 3;
                            break;
                        case ImageExtension.Tif:
                            dialog.FilterIndex = 4;
                            break;
                        case ImageExtension.Bmp:
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
            var dict = SelectedElements.OrderBy(Panel.GetZIndex).ToDictionary<ImageControl, FrameworkElement, FrameworkElement>(element => element, element => (Image)element.Content);
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
                    Filter = "ICO 图标格式 (*.ico)|*.ico",
                    ValidateNames = true,
                };
                var showDialog = dialog.ShowDialog().GetValueOrDefault();
                if (!showDialog) return;
                var filePath = dialog.FileName;
                var dict = SelectedElements.OrderBy(Panel.GetZIndex).ToDictionary<ImageControl, FrameworkElement, FrameworkElement>(element => element, element => (Image)element.Content);
                SetIsSelected(dict.Keys, false);
                var imageSource = dict.GetMinContainBitmap();
                SetIsSelected(dict.Keys, true);

                if (imageSource.PixelWidth != imageSource.PixelHeight)
                {
                    var maxSideLength = imageSource.PixelWidth > imageSource.PixelHeight
                    ? imageSource.PixelWidth
                    : imageSource.PixelHeight;
                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        var brush = new ImageBrush(imageSource)
                        {
                            Stretch = Stretch.None,
                        };
                        context.DrawRectangle(brush, null, new Rect(0, 0, maxSideLength, maxSideLength));
                    }
                    var renderBitmap = new RenderTargetBitmap(maxSideLength, maxSideLength, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);
                    imageSource = renderBitmap;
                }
                

                var index = menu.Items.IndexOf(menuItem);
                switch (index)
                {
                    case 0:
                        imageSource.SaveIcon(filePath, IconSize.Bmp16);
                        break;
                    case 1:
                        imageSource.SaveIcon(filePath, IconSize.Bmp24);
                        break;
                    case 2:
                        imageSource.SaveIcon(filePath, IconSize.Bmp32);
                        break;
                    case 3:
                        imageSource.SaveIcon(filePath, IconSize.Bmp48);
                        break;
                    case 4:
                        imageSource.SaveIcon(filePath, IconSize.Bmp64);
                        break;
                    case 5:
                        imageSource.SaveIcon(filePath, IconSize.Bmp72);
                        break;
                    case 6:
                        imageSource.SaveIcon(filePath, IconSize.Bmp96);
                        break;
                    case 7:
                        imageSource.SaveIcon(filePath, IconSize.Bmp128);
                        break;
                    case 8:
                        imageSource.SaveIcon(filePath, IconSize.Bmp256);
                        break;
                    default:
                        imageSource.SaveIcon(filePath, IconSize.Bmp32);
                        break;
                }

            }
        }

        private void MenuItem_GifClip(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            MenuItem_GifCopy(null, null);
            _actionManager.Execute(new AddItemAction<ImageControl>(_panelContainer.Children.Remove, m => _panelContainer.Children.Add(m), SelectedElements.First()));
        }

        private async void MenuItem_GifCopy(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var animatedImage = (AnimatedGif)element.Content;
            var bitmapFrames = animatedImage.BitmapFrames;
            if (bitmapFrames.Count < 2) return;

            var width = (int)Math.Round(element.Width);
            var height = (int)Math.Round(element.Height);
            var angle = element.GetTransform<RotateTransform>().Angle;
            var scaleTransform = element.GetTransform<ScaleTransform>();
            var scaleX = scaleTransform.ScaleX;
            var scaleY = scaleTransform.ScaleY;

            var rect = ((Image)(element.Content)).GetMinContainRect();
            var visualWidth = (int)Math.Round(rect.Width);
            var visualHeight = (int)Math.Round(rect.Height);

            using (var stream = new MemoryStream())
            {
                using (var encoder = new GifEncoder(stream, visualWidth, visualHeight, animatedImage.RepeatCount))
                {
                    var delays = animatedImage.Delays;
                    for (var i = 0; i < bitmapFrames.Count; i++)
                    {
                        using (var bitmap = bitmapFrames[i].GetMinContainBitmap(width, height, angle, scaleX, scaleY, visualWidth, visualHeight).GetBitmap())
                        {
                            encoder.AppendFrame(bitmap, (int)delays[i].TotalMilliseconds);
                        }
                    }
                }
                stream.Position = 0;

                var tempFilePath = Path.GetTempFileName();
                using (var fs = File.OpenWrite(tempFilePath))
                {
                    var data = stream.ToArray();
                    await fs.WriteAsync(data, 0, data.Length);
                }
                var byteData = Encoding.UTF8.GetBytes("<QQRichEditFormat><Info version=\"1001\"></Info><EditElement type=\"1\" filepath=\"" + tempFilePath + "\" shortcut=\"\"></EditElement><EditElement type=\"0\"><![CDATA[]]></EditElement></QQRichEditFormat>");
                var dataObject = new DataObject();
                dataObject.SetData(ImageDataFormats.QqUnicodeRichEditFormat, new MemoryStream(byteData), true);
                dataObject.SetData(ImageDataFormats.QqRichEditFormat, new MemoryStream(byteData), true);
                dataObject.SetData(ImageDataFormats.FileDrop, new[] { tempFilePath }, true);
                dataObject.SetData(ImageDataFormats.FileNameW, new[] { tempFilePath }, true);
                dataObject.SetData(ImageDataFormats.FileName, new[] { tempFilePath }, true);

                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject, true);

            }

        }

        private async void MenuItem_GifSave(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var animatedImage = (AnimatedGif)element.Content;
            var bitmapFrames = animatedImage.BitmapFrames;
            if (bitmapFrames.Count < 2) return;

            var dialog = new SaveFileDialog
            {
                CheckPathExists = true,
                AddExtension = true,
                FileName = "图形1",
                Filter = "GIF 可交换的图形格式 (*.gif)|*.gif",
                ValidateNames = true,
            };

            var showDialog = dialog.ShowDialog().GetValueOrDefault();
            if (!showDialog) return;
            var filePath = dialog.FileName;

            var width = (int)Math.Round(element.Width);
            var height = (int)Math.Round(element.Height);
            var angle = element.GetTransform<RotateTransform>().Angle;
            var scaleTransform = element.GetTransform<ScaleTransform>();
            var scaleX = scaleTransform.ScaleX;
            var scaleY = scaleTransform.ScaleY;

            var rect = ((Image)(element.Content)).GetMinContainRect();
            var visualWidth = (int)Math.Round(rect.Width);
            var visualHeight = (int)Math.Round(rect.Height);

            using (var stream = new MemoryStream())
            {
                using (var encoder = new GifEncoder(stream, visualWidth, visualHeight, animatedImage.RepeatCount))
                {
                    var delays = animatedImage.Delays;
                    for (var i = 0; i < bitmapFrames.Count; i++)
                    {
                        using (var bitmap = bitmapFrames[i].GetMinContainBitmap(width, height, angle, scaleX, scaleY, visualWidth, visualHeight).GetBitmap())
                        {
                            encoder.AppendFrame(bitmap, (int)delays[i].TotalMilliseconds);
                        }
                    }
                }
                stream.Position = 0;
                using (var file = File.Create(filePath))
                {
                    await stream.CopyToAsync(file);
                    //stream.WriteTo(file);
                }
            }

        }

        private void MenuItem_GifSplit(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var animatedImage = (AnimatedGif)element.Content;
            var bitmapFrames = animatedImage.BitmapFrames;
            if (bitmapFrames.Count < 2) return;
            bitmapFrames.Reverse();//反置

            var transactions = new TransactionAction();
            foreach (var imageControl in bitmapFrames.Select(item => new ImageControl(this)
            {
                Width = element.Width,
                Height = element.Height,
                Content = new AnimatedGif { Source = item, Stretch = Stretch.Fill },
                Template = element.Template,
                RenderTransform = element.RenderTransform.Clone(),
            }))
            {
                AttachProperty(imageControl);
                Selector.SetIsSelected(imageControl, false);
                transactions.Add(new AddItemAction<ImageControl>(m => _panelContainer.Children.Add(m), _panelContainer.Children.Remove, imageControl));
            }
            transactions.Add(new AddItemAction<ImageControl>(_panelContainer.Children.Remove, m => _panelContainer.Children.Add(m), element));
            _actionManager.Execute(transactions);
        }

        private void MenuItem_GifReverse(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var animatedImage = (AnimatedGif)element.Content;
            var bitmapFrames = animatedImage.BitmapFrames;
            if (bitmapFrames.Count < 2) return;

            var stream = new MemoryStream();
            using (var encoder = new GifEncoder(stream, bitmapFrames.First().PixelWidth, bitmapFrames.First().PixelHeight, animatedImage.RepeatCount))
            {
                var delays = animatedImage.Delays;
                for (var i = bitmapFrames.Count - 1; i >= 0; i--)
                {
                    using (var bitmap = bitmapFrames[i].GetBitmap())
                    {
                        encoder.AppendFrame(bitmap, (int)delays[i].TotalMilliseconds);
                    }
                }
            }
            stream.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            var transactions = new TransactionAction();
            var imageControl = new ImageControl(this)
            {
                Width = element.Width,
                Height = element.Height,
                Content = new AnimatedGif { Source = bitmapImage, Stretch = Stretch.Fill },
                Template = element.Template,
                RenderTransform = element.RenderTransform.Clone(),
            };
            AttachProperty(imageControl);
            transactions.Add(new AddItemAction<ImageControl>(m => _panelContainer.Children.Add(m), _panelContainer.Children.Remove, imageControl));
            transactions.Add(new AddItemAction<ImageControl>(_panelContainer.Children.Remove, m => _panelContainer.Children.Add(m), element));
            _actionManager.Execute(transactions);
        }

        private void MenuItem_GifDrawing(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            var width = (int) Math.Round(element.Width);
            var height = (int) Math.Round(element.Height);
            if (width == 0 || height == 0)
            {
                return;
            }
            var animatedImage = (AnimatedGif)element.Content;
            var bitmapFrames = animatedImage.BitmapFrames;
            if (bitmapFrames.Count < 2) return;
            element.Visibility = Visibility.Hidden;

            var window = new GifDrawing.GifDrawingWindow(animatedImage, width, height);
            window.ShowDialog();
            if (window.NewAnimatedGif != null)
            {
                _actionManager.Execute(new ExchangeImageAction(element, window.NewAnimatedGif));
            }
            element.Visibility = Visibility.Visible;
        }

        private void Menu_Setting(object sender, RoutedEventArgs e)
        {
            if (SelectedElements.Count() != 1) return;
            var window = new ImageSettingWindow(SelectedElements.First());
            window.ShowDialog();
            if (window.SetPropertyAction != null)
            {
                _actionManager.Execute(window.SetPropertyAction);
            }
        }

        #endregion Properties and Events

        #region Private methods
        private void AttachProperty(ImageControl element)
        {
            #region 设置属性
            Selector.SetIsSelected(element, true);
            _maxControlZIndex++;
            Panel.SetZIndex(element, _maxControlZIndex);

            #endregion

            #region 添加上下文菜单
            var contextMenu = new ContextMenu();

            #region 剪切
            var item = new MenuItem { Header = "剪切", Tag = "CopyRemove" };
            item.Click += Menu_CopyRemoveImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 复制
            item = new MenuItem { Header = "复制", Tag = "Copy" };
            item.Click += Menu_CopyImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 裁剪
            item = new MenuItem { Header = "裁剪", Tag = "TrimSelf" };
            item.Click += MenuItem_CropImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 截屏

            item = new MenuItem
            {
                Header = "截屏",
                Tag = "CaptureScreen",
            };

            #region 二级菜单

            var subItem = new MenuItem
            {
                Header = "普通截屏"
            };

            subItem.Click += MenuItem_CaptureImageFromScreen;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "影子截屏",
                ToolTip = "用该图的不透明部分截取屏幕(适用于透明图)"
            };
            subItem.Click += MenuItem_ShadowCaptureImageFromScreen;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "透明截屏",
                ToolTip = "用该图的透明部分截取屏幕(适用于透明图)"

            };
            subItem.Click += MenuItem_TransparentCaptureImageFromScreen;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);

            #endregion

            #region 截图
            item = new MenuItem { Header = "截图", Tag = "CaptureImage" };

            #region 二级菜单

            subItem = new MenuItem { Header = "普通截图" };
            subItem.Click += MenuItem_CaptureImageFromInternal;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "影子截图",
                ToolTip = "用该图的不透明部分截取加载图像(适用于透明图)"
            };
            subItem.Click += MenuItem_ShadowCaptureImageFromInternal;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "透明截图",
                ToolTip = "用该图的不透明部分截取加载图像(适用于透明图)"

            };
            subItem.Click += MenuItem_TransparentCaptureImageFromInternal;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);

            #endregion

            #region 抠图
            item = new MenuItem { Header = "抠图", Tag = "CutImage" };

            #region 二级菜单

            subItem = new MenuItem { Header = "普通抠图" };
            subItem.Click += MenuItem_CutImageFromInternal;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "影子抠图",
                ToolTip = "用该图的不透明部分抠取加载图像(适用于透明图)"

            };
            subItem.Click += MenuItem_ShadowCutImageFromInternal;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "透明抠图",
                ToolTip = "用该图的不透明部分抠取加载图像(适用于透明图)"

            };
            subItem.Click += MenuItem_TransparentCutImageFromInternal;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);

            #endregion

            #region 更改图片

            item = new MenuItem { Header = "更改图片", Tag = "Exchange" };
            item.SubmenuOpened += MenuItem_SubmenuOpened;
            #region 二级菜单

            subItem = new MenuItem { Header = "默认图片" };
            subItem.Click += Menu_ExchangeImageFromInternal;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "自剪贴板" };
            subItem.Click += Menu_ExchangeImageFromClipboard;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "来自文件..." };
            subItem.Click += Menu_ExchangeImageFromFile;
            item.Items.Add(subItem);
            #endregion

            contextMenu.Items.Add(item);
            #endregion

            #region 设置背景

            item = new MenuItem { Header = "设置背景", Tag = "SetBackground", ToolTip = "给透明图设置背景" };
            item.SubmenuOpened += MenuItem_SubmenuOpened;
            #region 二级菜单

            subItem = new MenuItem { Header = "默认背景", Tag = "Default" };
            subItem.Click += Menu_SetPngImageBackground;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "自剪贴板", Tag = "Clipboard" };
            subItem.Click += Menu_SetPngImageBackground;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "来自文件...", Tag = "File" };
            subItem.Click += Menu_SetPngImageBackground;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);
            #endregion

            #region 组合图片
            item = new MenuItem { Header = "组合", Tag = "Combine" };
            item.Click += Menu_CombineImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 置于顶层

            item = new MenuItem { Header = "置于顶层", Tag = "ZIndexTop" };

            #region 二级菜单

            subItem = new MenuItem { Header = "置于顶层" };
            subItem.Click += Menu_ZIndexTopmost;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "上移一层" };
            subItem.Click += Menu_ZIndexTop;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);
            #endregion

            #region 置于底层

            item = new MenuItem { Header = "置于底层", Tag = "ZIndexBottom" };

            #region 二级菜单

            subItem = new MenuItem { Header = "置于底层" };
            subItem.Click += Menu_ZIndexBottommost;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "下移一层" };
            subItem.Click += Menu_ZIndexBottom;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);
            #endregion

            #region 另存为图片
            item = new MenuItem { Header = "另存为图片", Tag = "SaveToImage" };
            item.Click += Menu_SaveToImage;
            contextMenu.Items.Add(item);

            #endregion

            #region 另存为图标
            item = new MenuItem { Header = "另存为图标", Tag = "SaveToIcon" };
            contextMenu.Items.Add(item);

            #region 二级菜单
            subItem = new MenuItem { Header = "ICO 16*16" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 24*24" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 32*32" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 48*48" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 64*64" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 72*72" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 96*96" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 128*128" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            subItem = new MenuItem { Header = "ICO 256*256" };
            subItem.Click += Menu_SaveToIcon;
            item.Items.Add(subItem);

            #endregion

            #endregion

            #region Gif处理

            item = new MenuItem
            {
                Header = "Gif处理",
                Tag = "GifDeal",
                ToolTip = "处理GIF动态图"
            };

            #region 二级菜单

            subItem = new MenuItem
            {
                Header = "剪切",
                Tag = "GifClip",
                ToolTip = "剪切当前GIF动态图"
            };
            subItem.Click += MenuItem_GifClip;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "复制",
                Tag = "GifCopy",
                ToolTip = "复制当前GIF动态图"
            };
            subItem.Click += MenuItem_GifCopy;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "保存",
                Tag = "GifSave",
                ToolTip = "保存当前GIF动态图"
            };
            subItem.Click += MenuItem_GifSave;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "分离",
                Tag = "GifSplit",
                ToolTip = "将GIF动态图分解成多张图像"
            };
            subItem.Click += MenuItem_GifSplit;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "反转",
                Tag = "GifReverse",
                ToolTip = "将GIF动态图播放顺序反转"
            };
            subItem.Click += MenuItem_GifReverse;
            item.Items.Add(subItem);

            subItem = new MenuItem
            {
                Header = "涂鸦",
                Tag = "GifDrawing",
                ToolTip = "在GIF动态图上涂鸦"
            };
            subItem.Click += MenuItem_GifDrawing;
            item.Items.Add(subItem);

            #endregion

            contextMenu.Items.Add(item);

            #endregion

            #region 插件

            if (_cachePlugins != null && _cachePlugins.Count > 0)
            {
                item = new MenuItem
                {
                    Header = "插件",
                    Tag = "Plugin"
                };
                foreach (var plugin in _cachePlugins)
                {
                    if (plugin.Item3.Count == 1)
                    {
                        subItem = new MenuItem
                        {
                            Header = plugin.Item3.First().GetPluginName(),
                            Tag = plugin.Item3.First()
                        };
                        var bitmap = plugin.Item3.First().GetPluginIcon();
                        if (bitmap != null)
                        {
                            subItem.Icon = new Image { Source = bitmap.GetBitmapSource() };
                        }
                        subItem.Click += PluginItem_Click;
                    }
                    else
                    {
                        if(plugin.Item3 == null) continue;
                       
                        subItem = new MenuItem { Header = plugin.Item1 };
                        if (plugin.Item2 != null)
                        {
                            subItem.Icon = new Image { Source = plugin.Item2.GetBitmapSource() };
                        }
                        foreach (var ihanle in plugin.Item3)
                        {
                            var thirdItem = new MenuItem
                            {
                                Header = ihanle.GetPluginName(),
                                Tag = ihanle
                            };

                            var bitmap = ihanle.GetPluginIcon();
                            if (bitmap != null)
                            {
                                thirdItem.Icon = new Image { Source = bitmap.GetBitmapSource() };
                            }
                            thirdItem.Click += PluginItem_Click;
                            subItem.Items.Add(thirdItem);
                        }
                    }
                    item.Items.Add(subItem);
                }
                contextMenu.Items.Add(item);
            }

            #endregion

            #region 设置
            contextMenu.Items.Add(new Separator());//分割线

            item = new MenuItem { Header = "设置", Tag = "Setting" };
            item.Click += Menu_Setting;
            contextMenu.Items.Add(item);
            #endregion

            element.ContextMenu = contextMenu;

            #endregion

            #region 添加事件
            element.PreviewMouseDown += Element_PreviewMouseDown;
            element.MouseDoubleClick += MenuItem_CaptureImageFromScreen;
            element.ContextMenuOpening += Menu_ContextMenuOpening;
            element.SizeChanged += Element_SizeChanged;

            #endregion
        }

        private List<IFilterList> GetPluginLists(string filePath)
        {
            var list = new List<IFilterList>();
            var assembly = Assembly.LoadFile(Path.GetFullPath(filePath));

            foreach (var iFilterList in assembly.GetTypes().Where(m => m.GetInterface("IFilterList") != null).Select(type => (IFilterList)Activator.CreateInstance(type)))
            {
                iFilterList.GetIFilterList()?.ForEach(m => m.InitPlugin(AppDomain.CurrentDomain.SetupInformation.ApplicationBase));
                list.Add(iFilterList);
            }

            return list;
        }

        private UIElement GetExchangeZIndexElement(ImageControl element, ZIndexFlag zIndexFlag)
        {
            var targetElement = element;
            var targetZIndex = Panel.GetZIndex(element);
            int tmp;
            var flag = 1;
            if (zIndexFlag == ZIndexFlag.Bottommost || zIndexFlag == ZIndexFlag.Bottom)
            {
                flag = -1;
            }
            if (zIndexFlag == ZIndexFlag.Bottommost || zIndexFlag == ZIndexFlag.Topmost)
            {
                foreach (ImageControl item in _panelContainer.Children)
                {
                    tmp = Panel.GetZIndex(item);
                    if ((targetZIndex - tmp) * flag >= 0) continue;
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
                foreach (ImageControl item in _panelContainer.Children)
                {
                    tmp = Panel.GetZIndex(item);
                    if (nearZIndex == targetZIndex)
                    {
                        if ((targetZIndex - tmp) * flag >= 0) continue;
                        nearZIndex = tmp;
                        targetElement = item;
                    }
                    else
                    {
                        if ((tmp - nearZIndex) * flag >= 0 || (tmp - targetZIndex) * flag <= 0) continue;
                        nearZIndex = tmp;
                        targetElement = item;
                    }
                }
                return targetElement;
            }
        }

        private void SetIsSelected(IEnumerable<UIElement> elements, bool isSelected)
        {
            foreach (var item in elements)
            {
                Selector.SetIsSelected(item, isSelected);
            }
        }

        private async void ExchangeImageFromScreen(CropStyle cropStyle)
        {
            if (!SelectedElements.Any()) return;
            var elements = SelectedElements.ToList();

            elements.ForEach(m => m.Visibility = Visibility.Hidden);

            await Task.Delay(300);//保证上下文菜单已消失

            foreach (var element in SelectedElements)
            {
                var scaleTransform = element.GetTransform<ScaleTransform>();
                var imageSource = ScreenCropper.CropScreen(new CropViewBox((Image)element.Content, element.GetTransform<RotateTransform>().Angle, scaleTransform.ScaleX, scaleTransform.ScaleY));

                if (imageSource != null && cropStyle != CropStyle.Default)
                {
                    imageSource = ImageCropper.CropBitmapSource(imageSource,
                        ((BitmapSource)((Image)element.Content).Source).GetResizeBitmap(imageSource.PixelWidth,
                            imageSource.PixelHeight), cropStyle);
                }

                if (imageSource != null)
                {
                    _actionManager.Execute(new ExchangeImageAction(element,
                        new AnimatedGif
                        {
                            Source = imageSource.GetBitmapImage(),
                            Stretch = Stretch.Fill
                        }));
                }
                else
                {
                    Extentions.ShowMessageBox("不合法的截屏操作!");
                }
            }

            elements.ForEach(m => m.Visibility = Visibility.Visible);

        }

        private void CropImageFromInternal(CropStyle cropStyle)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            element.Visibility = Visibility.Hidden;

            element.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var renderBitmap = new RenderTargetBitmap((int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(_panelContainer);
                var scaleTransform = element.GetTransform<ScaleTransform>();
                var imageSource = ScreenCropper.CropScreen(renderBitmap, new CropViewBox((Image)element.Content, element.GetTransform<RotateTransform>().Angle, scaleTransform.ScaleX, scaleTransform.ScaleY));
                if (imageSource != null && cropStyle != CropStyle.Default)
                {
                    imageSource = ImageCropper.CropBitmapSource(imageSource, ((BitmapSource)((Image)element.Content).Source).GetResizeBitmap(imageSource.PixelWidth, imageSource.PixelHeight), cropStyle);
                }
                if (imageSource != null)
                {
                    _actionManager.Execute(new ExchangeImageAction(element, new AnimatedGif { Source = imageSource.GetBitmapImage(), Stretch = Stretch.Fill }));
                }
                else
                {
                    Extentions.ShowMessageBox("不合法的截图操作!");
                }
                element.Visibility = Visibility.Visible;
            }));
        }

        private void CutImageFromInternal(CropStyle cropStyle)
        {
            if (SelectedElements.Count() != 1) return;
            var element = SelectedElements.First();
            element.Visibility = Visibility.Hidden;

            element.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var renderBitmap = new RenderTargetBitmap((int)SystemParameters.VirtualScreenWidth,
                    (int)SystemParameters.VirtualScreenHeight, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(_panelContainer);
                var scaleTransform = element.GetTransform<ScaleTransform>();
                var cropViewBox = new CropViewBox((Image)element.Content, element.GetTransform<RotateTransform>().Angle,
                    scaleTransform.ScaleX, scaleTransform.ScaleY);
                var imageSource = ScreenCropper.CropScreen(renderBitmap, cropViewBox);
                if (imageSource != null && cropStyle != CropStyle.Default)
                {
                    imageSource = ImageCropper.CropBitmapSource(imageSource,
                        ((BitmapSource)((Image)element.Content).Source).GetResizeBitmap(imageSource.PixelWidth,
                            imageSource.PixelHeight), cropStyle);
                }
                element.Visibility = Visibility.Visible;
                if (imageSource == null)
                {
                    Extentions.ShowMessageBox("不合法的抠图操作!");
                    return;
                }
                var unSelectedElements =
                    _panelContainer.Children.Cast<object>()
                        .OfType<ImageControl>()
                        .Where(m => !Selector.GetIsSelected(m))
                        .ToList();
                var intersectElements =
                    unSelectedElements.Where(m => ((Image)element.Content).IsOverlapped((Image)m.Content)).ToArray();

                var fullScreenBitmap = GetFullScreenBitmap(element, cropStyle);
                var transactions = new TransactionAction();
                foreach (var item in intersectElements)
                {
                    scaleTransform = item.GetTransform<ScaleTransform>();
                    cropViewBox = new CropViewBox((Image)item.Content, item.GetTransform<RotateTransform>().Angle,
                        scaleTransform.ScaleX, scaleTransform.ScaleY);
                    var bitmapSource = ScreenCropper.CropScreen(fullScreenBitmap, cropViewBox);
                    var cutBitmapSource =
                        ImageCropper.CropBitmapSource(
                            ((BitmapSource)((Image)item.Content).Source).GetResizeBitmap(bitmapSource.PixelWidth,
                                bitmapSource.PixelHeight), bitmapSource, CropStyle.Transparent);
                    transactions.Add(new ExchangeImageAction(item,
                        new AnimatedGif
                        {
                            Source = cutBitmapSource.GetBitmapImage(),
                            Stretch = Stretch.Fill
                        }));
                }
                transactions.Add(new ExchangeImageAction(element,
                    new AnimatedGif { Source = imageSource.GetBitmapImage(), Stretch = Stretch.Fill }));
                _actionManager.Execute(transactions);
            }));
        }

        private BitmapSource GetFullScreenBitmap(ImageControl imageControl, CropStyle cropStyle)
        {
            var rect = ((Image)(imageControl.Content)).GetMinContainRect();
            var centerPoint = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            var rotateTransform = imageControl.GetTransform<RotateTransform>();
            var scaleTransform = imageControl.GetTransform<ScaleTransform>();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scaleTransform.ScaleX, scaleTransform.ScaleY, 0.5, 0.5));
            transformGroup.Children.Add(new RotateTransform(rotateTransform.Angle, 0.5, 0.5));

            var bitmapSource = GetCropStyleBitmap(((AnimatedGif)imageControl.Content).Source as BitmapSource, (int)Math.Round(imageControl.Width, 0), (int)Math.Round(imageControl.Height, 0), cropStyle);
            var bevelSideLength = Math.Sqrt(bitmapSource.Width * bitmapSource.Width + bitmapSource.Height * bitmapSource.Height);
            var screenWidth = (int)SystemParameters.VirtualScreenWidth;
            var screenHeight = (int)SystemParameters.VirtualScreenHeight;

            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                var brush = new ImageBrush(bitmapSource)
                {
                    Stretch = Stretch.None,
                    RelativeTransform = transformGroup,
                };
                context.DrawRectangle(brush, null, new Rect((centerPoint.X - bevelSideLength / 2), (centerPoint.Y - bevelSideLength / 2), bevelSideLength, bevelSideLength));
            }
            drawingVisual.Opacity = imageControl.Opacity;
            var renderBitmap = new RenderTargetBitmap(screenWidth, screenHeight, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);
            return renderBitmap;
        }

        private BitmapSource GetCropStyleBitmap(BitmapSource bitmapSource, int width, int height, CropStyle cropStyle)
        {
            if (cropStyle == CropStyle.Default)
            {
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                }
                var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawingVisual);
                return renderBitmap;
            }
            var resizeBitmap = bitmapSource.GetResizeBitmap(width, height);
            return cropStyle == CropStyle.Shadow ? resizeBitmap : resizeBitmap.GetBitmap().ShadowSwapTransparent(Color.White).GetBitmapSource();
        }

        #endregion Private methods
    }

}

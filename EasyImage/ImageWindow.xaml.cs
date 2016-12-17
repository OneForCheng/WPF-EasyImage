using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EasyImage.Behaviors;
using EasyImage.Config;
using EasyImage.Enum;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DealImage.Paste;
using NHotkey;
using NHotkey.Wpf;

namespace EasyImage
{

    /// <summary>
    /// ImageWinodw.xaml 的交互逻辑
    /// </summary>
    public partial class ImageWindow
    {
        private UserConfig _userConfigution;
        private ControlManager _controlManager;
        private ClipboardMonitor _clipboardMonitor;

        public ImageWindow()
        {
            InitializeComponent();
        }

        #region 主窗口事件

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            #region 成员变量初始化

            ImageCanvas.Width = Width;
            ImageCanvas.Height = Height;
            _userConfigution = ((MainWindow) Owner).UserConfigution;
            _controlManager = new ControlManager(ImageCanvas);
            _clipboardMonitor = new ClipboardMonitor();
            _clipboardMonitor.OnClipboardContentChanged += OnClipboardContentChanged;

            #endregion

            #region 加载其它配置

            this.RemoveSystemMenuItems(SystemMenuItems.All); //去除窗口指定的系统菜单
            
            //https://github.com/thomaslevesque/NHotkey
            HotkeyManager.Current.AddOrReplace("GlobalPasteFromClipboard", Key.V,
                    ModifierKeys.Control | ModifierKeys.Alt, GlobalPasteFromClipboard);
            HotkeyManager.Current.AddOrReplace("GlobalAddCanvas", Key.N,
                    ModifierKeys.Control | ModifierKeys.Alt, GlobalAddCanvas);

            InitMainMenu();

            #endregion

        }

        private void HiddenWindow(object sender, ExecutedRoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Left)
            {
                _controlManager.KeyMoveSelected(e.Key);
                if (_controlManager.MoveSpeed <= 10)
                {
                    _controlManager.MoveSpeed += 0.5;
                }
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        _controlManager.ActionManager.UnExecute();
                        break;
                    case Key.A:
                        _controlManager.SelectAll();
                        break;
                    case Key.C:
                        _controlManager.CopySelected();
                        break;
                    case Key.V:
                        PasteImagesFromClipboard(null, null);
                        break;
                    case Key.X:
                        _controlManager.ClipSelected();
                        break;
                    case Key.Y:
                        _controlManager.ActionManager.ReExecute();
                        break;
                }
            }
            else if (e.Key == Key.Delete)
            {
                _controlManager.RemoveSelected();
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            _controlManager.MoveSpeed = 1.0;
        }

        #endregion

        #region 主菜单事件

        private void MainMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _controlManager.SelectNone();
        }

        private void MainMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var element = sender as Image;
            var menuItem = element?.ContextMenu.Items[2] as MenuItem;
            if (menuItem != null)
            {
                menuItem.IsEnabled = ImagePasteHelper.CanPasteImageFromClipboard();
            }
        }

        private void AddImagesFromFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter =
                    "所有图片 (*.ico;*.gif;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.tif;*.tiff;*.bmp;*.dib;*.rle)|*.ico;*.gif;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.tif;*.tiff;*.bmp;*.dib;*.rle"
                    + "|ICO 图标格式 (*.ico)|*.ico"
                    + "|GIF 可交换的图形格式 (*.gif)|*.gif"
                    + "|JPEG 文件交换格式 (*.jpg;*.jpeg;*.jfif;*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe"
                    + "|PNG 可移植网络图形格式 (*.png)|*.png"
                    + "|TIFF Tag 图像文件格式 (*.tif;*.tiff)|*.tif;*.tiff"
                    + "|设备无关位图 (*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle"
            };

            var showDialog = dialog.ShowDialog();
            if (showDialog != null && !(bool) showDialog) return;
            _controlManager.SelectNone();
            _controlManager.ContinuedPasteCount = 0;
            var controls = new List<ImageControl>(dialog.FileNames.Length);
            foreach (var file in dialog.FileNames)
            {
                var stream = new MemoryStream();
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(stream);
                }
                try
                {
                    stream.Position = 0;
                    var imageSource = new BitmapImage();
                    imageSource.BeginInit();
                    imageSource.StreamSource = stream;
                    imageSource.EndInit();
                    controls.Add(PackageImageToControl(new AnimatedImage.AnimatedImage { Source = imageSource, Stretch = Stretch.Fill }));
                }
                catch (Exception ex)
                {
                    throw new Exception("不支持此图片格式");
                }
            }
            _controlManager.AddElements(controls);
        }

        private void PasteImagesFromClipboard(object sender, RoutedEventArgs e)
        {
            if (ImagePasteHelper.CanInternalPasteFromClipboard())
            {
                var baseInfos = ImagePasteHelper.GetInternalPasteDataFromClipboard() as List<ImageControlBaseInfo>;
                if (baseInfos != null)
                {
                    _controlManager.SelectNone();
                    _controlManager.ContinuedPasteCount++;
                    _controlManager.AddElements(baseInfos.Select(PackageBaseInfoToControl));
                    return;
                }
            }

            if (!ImagePasteHelper.CanPasteImageFromClipboard()) return;
            try
            {
                var imageSources = ImagePasteHelper.GetPasteImagesFromClipboard();
                _controlManager.SelectNone();
                _controlManager.ContinuedPasteCount++;
                var enumerable = imageSources as IList<ImageSource> ?? imageSources.ToList();
                var controls = new List<ImageControl>(enumerable.Count);
                controls.AddRange(enumerable.Select(imageSource => PackageImageToControl(new AnimatedImage.AnimatedImage { Source = imageSource, Stretch = Stretch.Fill })));
                _controlManager.AddElements(controls);
            }
            catch (Exception ex)
            {
                //无效的粘贴
                Trace.WriteLine(ex);
            }
            
        }

        private void AddImageFromInternal(object sender, RoutedEventArgs e)
        {
            var size = _userConfigution.ImageSetting.InitMaxImgSize / 2;
            if (size < 1 || size > SystemParameters.PrimaryScreenHeight)
            {
                size = SystemParameters.PrimaryScreenHeight/2;
            }
            var rect = new Rect(0, 0, size, size);
            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(Brushes.White, null, rect);
            }
            drawingVisual.Opacity = 0.1;
            var renderBitmap = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            var stream = new MemoryStream();
            encoder.Save(stream);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            _controlManager.SelectNone();
            _controlManager.AddElement(PackageImageSourceToControl(bitmapImage));
        }

        #endregion

        #region 初始化操作

        private void InitMainMenu()
        {
            #region 初始化属性
            MainMenuIcon.Width = 30;
            MainMenuIcon.Height = 30;
            MainMenuIcon.ToolTip = "EasyImage主菜单";
            MainMenuIcon.Cursor = Cursors.SizeAll;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1));
            transformGroup.Children.Add(new RotateTransform(0));
            transformGroup.Children.Add(new TranslateTransform((SystemParameters.PrimaryScreenWidth - MainMenuIcon.Width) / 2, (SystemParameters.PrimaryScreenHeight - MainMenuIcon.Height) / 2));
            MainMenuIcon.RenderTransform = transformGroup;

            var dragBehavior = new MouseDragElementBehavior<Image>();
            dragBehavior.Attach(MainMenuIcon);

            #endregion

            #region 添加上下文菜单
            var contextMenu = new ContextMenu();
            var item = new MenuItem {Header = "新建"};
            item.Click += AddImageFromInternal;
            contextMenu.Items.Add(item);

            item = new MenuItem { Header = "添加" };
            item.Click += AddImagesFromFile;
            contextMenu.Items.Add(item);

            item = new MenuItem { Header = "粘贴" };
            item.Click += PasteImagesFromClipboard;
            contextMenu.Items.Add(item);

            item = new MenuItem { Header = "退出" };
            item.Click += (sender, args) =>
            {
                Owner.Close();
            };
            contextMenu.Items.Add(item);

            MainMenuIcon.ContextMenu = contextMenu;
            #endregion

            #region 添加事件
            MainMenuIcon.MouseDown += MainMenu_MouseDown;
            MainMenuIcon.ContextMenuOpening += MainMenu_ContextMenuOpening;

            #endregion
        }

        private ImageControl PackageImageToControl(AnimatedImage.AnimatedImage image)
        {
            var imageControl = new ImageControl(_controlManager);

            var width = imageControl.Width = image.Source.Width;
            var height = imageControl.Height = image.Source.Height;
            
            imageControl.Content = image;
            imageControl.Template = (ControlTemplate)Resources["MoveResizeRotateTemplate"];

            //调整大小
            var initMaxImgSize = _userConfigution.ImageSetting.InitMaxImgSize;
            if (width > height && width > initMaxImgSize)
            {
                imageControl.Height = initMaxImgSize * height / width;
                imageControl.Width = initMaxImgSize;
            }
            else if (height > width && height > initMaxImgSize)
            {
                imageControl.Height = initMaxImgSize;
                imageControl.Width = initMaxImgSize * width / height;
            }

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1));
            transformGroup.Children.Add(new RotateTransform(0));
            transformGroup.Children.Add(new TranslateTransform((SystemParameters.PrimaryScreenWidth - imageControl.Width) / 2 + _userConfigution.ImageSetting.PasteMoveUnitDistace * _controlManager.ContinuedPasteCount, (SystemParameters.PrimaryScreenHeight - imageControl.Height) / 2 + _userConfigution.ImageSetting.PasteMoveUnitDistace * _controlManager.ContinuedPasteCount));
            imageControl.RenderTransform = transformGroup;
            
            return imageControl;
        }

        private ImageControl PackageImageSourceToControl(ImageSource imageSource)
        {
            var animatedImage = new AnimatedImage.AnimatedImage { Source = imageSource, Stretch = Stretch.Fill };
            var imageControl = new ImageControl(_controlManager)
            {
                Width = imageSource.Width,
                Height = imageSource.Height,
                Content = animatedImage,
                Template = (ControlTemplate)Resources["MoveResizeRotateTemplate"],
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1));
            transformGroup.Children.Add(new RotateTransform(0));
            transformGroup.Children.Add(new TranslateTransform((SystemParameters.PrimaryScreenWidth - imageControl.Width) / 2, (SystemParameters.PrimaryScreenHeight - imageControl.Height) / 2));
            imageControl.RenderTransform = transformGroup;

            return imageControl;
        }

        public ImageControl PackageBaseInfoToControl(ImageControlBaseInfo baseInfo)
        {
            var animatedImage = new AnimatedImage.AnimatedImage { Source = baseInfo.ImageSource, Stretch = Stretch.Fill };
            var imageControl = new ImageControl(_controlManager)
            {
                Width = baseInfo.Width,
                Height = baseInfo.Height,
                Content = animatedImage,
                Template = (ControlTemplate) Resources["MoveResizeRotateTemplate"],
                RenderTransform = baseInfo.RenderTransform,
            };

            var translateTransform = imageControl.GetTransform<TranslateTransform>();
            translateTransform.X += _userConfigution.ImageSetting.PasteMoveUnitDistace * _controlManager.ContinuedPasteCount;
            translateTransform.Y += _userConfigution.ImageSetting.PasteMoveUnitDistace * _controlManager.ContinuedPasteCount;

            return imageControl;
        }

        #endregion

        #region 全局事件

        private void GlobalPasteFromClipboard(object sender, HotkeyEventArgs e)
        {
            PasteImagesFromClipboard(null, null);
        }

        private void GlobalAddCanvas(object sender, HotkeyEventArgs e)
        {
            AddImageFromInternal(null,null);
        }

        private void OnClipboardContentChanged(object sender, EventArgs e)
        {
            _controlManager.ContinuedPasteCount = 0;
        }

        #endregion

    }
}

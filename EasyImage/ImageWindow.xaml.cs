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
using DealImage.Paste;

namespace EasyImage
{
    
    /// <summary>
    /// ImageWinodw.xaml 的交互逻辑
    /// </summary>
    public partial class ImageWindow 
    {
        private UserConfig _userConfigution;
        private ControlManager<ImageControl> _controlManager;

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
            _userConfigution = ((MainWindow)Owner).UserConfigution;
            _controlManager = new ControlManager<ImageControl>(ImageCanvas);
            #endregion

            #region 加载其它配置
            this.RemoveSystemMenuItems(SystemMenuItems.All);//去除窗口指定的系统菜单
            InitMainMenu();

            #endregion

        }

        private void HiddenWindow(object sender, RoutedEventArgs e)
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
            else if((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Z )
            {
                _controlManager.ActionManager.UnExecute();
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A)
            {
                _controlManager.SelectAll();
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                _controlManager.CopySelected();
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                AddImagesFromClipboard(null, null);
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Y)
            {
                _controlManager.ActionManager.ReExecute();
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
            var menuItem = element?.ContextMenu.Items[1] as MenuItem;
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
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.gif; *.bmp; *.ico)|*.jpg; *.jpeg; *.png; *.gif; *.bmp; *.ico"
            };
            var showDialog = dialog.ShowDialog();
            if (showDialog != null && !(bool) showDialog) return;
            if(dialog.FileName.Any())
            {
                _controlManager.SelectNone();
            }
            var controls = new List<ImageControl>(dialog.FileNames.Length);
            controls.AddRange(from file in dialog.FileNames select new BitmapImage(new Uri(file)) into bitmap select new AnimatedImage.AnimatedImage { Source = bitmap, Stretch = Stretch.Fill} into image select PackageImageToControl(image));
            _controlManager.AddElements(controls);
        }

        private void AddImagesFromClipboard(object sender, RoutedEventArgs e)
        {
            if (!ImagePasteHelper.CanPasteImageFromClipboard()) return;
            var imageSources = ImagePasteHelper.PasteImageFromClipboard();
            var enumerable = imageSources as IList<ImageSource> ?? imageSources.ToList();
            var controls = new List<ImageControl>(enumerable.Count);
            controls.AddRange(from imageSource in enumerable  select new AnimatedImage.AnimatedImage { Source = imageSource, Stretch = Stretch.Fill } into image select PackageImageToControl(image));
            _controlManager.AddElements(controls);
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
            transformGroup.Children.Add(new TranslateTransform((SystemParameters.VirtualScreenWidth - MainMenuIcon.Width) / 2, (SystemParameters.VirtualScreenHeight - MainMenuIcon.Height) / 2));
            MainMenuIcon.RenderTransform = transformGroup;

            var dragBehavior = new MouseDragElementBehavior<Image>();
            dragBehavior.Attach(MainMenuIcon);

            #endregion

            #region 添加上下文菜单
            var contextMenu = new ContextMenu();
            var item = new MenuItem {Header = "添加"};
            item.Click += AddImagesFromFile;
            contextMenu.Items.Add(item);

            item = new MenuItem { Header = "粘贴" };
            item.Click += AddImagesFromClipboard;
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
            transformGroup.Children.Add(new TranslateTransform((SystemParameters.VirtualScreenWidth - imageControl.Width) / 2, (SystemParameters.VirtualScreenHeight - imageControl.Height) / 2));
            imageControl.RenderTransform = transformGroup;

            return imageControl;
        }

        #endregion

       
    }
}

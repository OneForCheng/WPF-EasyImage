using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EasyImage.Config;
using EasyImage.Enum;
using EasyImage.UnmanagedToolkit;
using Microsoft.Win32;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Shortcut = EasyImage.Config.Shortcut;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace EasyImage.Windows
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow
    {
        private readonly UserConfig _userConfig;
        private readonly UserControl _imageControl;
        private readonly TranslateTransform _translateTransform;
        private AnimatedImage.AnimatedGif _oldAnimatedGif;
        private Shortcut _oldGlobelAddShortcut, _oldGlobelPasteShortcut;
        private bool _oldAutoRun;
        private string _oldPath;
        private double _oldWidth, _oldHeight, _oldTranslateX, _oldTranslateY, _oldInitMaxImgSize;
        private bool _textChanged;
        private bool _canTextChange;
        private TextboxFlag _textboxFlag;
        
        public SettingWindow(UserConfig userConfig, UserControl imageControl, bool isHide)
        {
            InitializeComponent();
            _userConfig = userConfig;
            _imageControl = imageControl;
            _translateTransform = imageControl.GetTransform<TranslateTransform>();
            if (isHide)
            {
                HeightTbx.IsEnabled =
                    WidthTbx.IsEnabled =
                        LocationXTbx.IsEnabled =
                            LocationYTbx.IsEnabled = false;
            }
            _textChanged = false;
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
           
            _oldWidth = Math.Round(_imageControl.Width, 2);
            _oldHeight = Math.Round(_imageControl.Height, 2);
            _oldTranslateX = Math.Round(_translateTransform.X, 2);
            _oldTranslateY = Math.Round(_translateTransform.Y, 2);
            _oldInitMaxImgSize = Math.Round(_userConfig.ImageSetting.InitMaxImgSize);
            _oldAnimatedGif = (AnimatedImage.AnimatedGif)_imageControl.Content;
            _oldPath = _userConfig.ImageSetting.MainMenuInfo.Path;
            _oldGlobelAddShortcut = (Shortcut)_userConfig.ShortcutSetting.GlobelAddShortcut.Clone();
            _oldGlobelPasteShortcut = (Shortcut)_userConfig.ShortcutSetting.GlobelPasteShortcut.Clone();
            _oldAutoRun = _userConfig.AppSetting.AutoRun;

            HeightTbx.Text = _oldHeight.ToString(CultureInfo.InvariantCulture);
            WidthTbx.Text = _oldWidth.ToString(CultureInfo.InvariantCulture);
            LocationXTbx.Text = _oldTranslateX.ToString(CultureInfo.InvariantCulture);
            LocationYTbx.Text = _oldTranslateY.ToString(CultureInfo.InvariantCulture);
            MaxSizeTbx.Text = _oldInitMaxImgSize.ToString(CultureInfo.InvariantCulture);
            AutoRunCk.IsChecked = _oldAutoRun;

            CtrlCbx1.IsChecked = _oldGlobelAddShortcut.IsCtrl;
            AltCbx1.IsChecked = _oldGlobelAddShortcut.IsAlt;
            ShiftCbx1.IsChecked = _oldGlobelAddShortcut.IsShift;
            KeyTbx1.Text = _oldGlobelAddShortcut.Key.ToString();

            CtrlCbx2.IsChecked = _oldGlobelPasteShortcut.IsCtrl;
            AltCbx2.IsChecked = _oldGlobelPasteShortcut.IsAlt;
            ShiftCbx2.IsChecked = _oldGlobelPasteShortcut.IsShift;
            KeyTbx2.Text = _oldGlobelPasteShortcut.Key.ToString();

            _canTextChange = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var autoRun = AutoRunCk.IsChecked.GetValueOrDefault();
            var fileName = AppDomain.CurrentDomain.SetupInformation.ApplicationName;
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            if (autoRun)
            {
                var registry = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true) ??
                               Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                registry?.SetValue(fileName,Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, AppDomain.CurrentDomain.SetupInformation.ApplicationName));
            }
            else
            {
                var registry = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                registry?.DeleteValue(fileName, false);
            }
        }
        
        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            Close();
        }

        private async void ReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                Filter =
                    "所有图片 (*.ico;*.gif;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.tif;*.tiff;*.bmp;*.dib;*.rle)|*.ico;*.gif;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.tif;*.tiff;*.bmp;*.dib;*.rle"
                    + "|ICO 图标格式 (*.ico)|*.ico"
                    + "|GIF 可交换的图形格式 (*.gif)|*.gif"
                    + "|JPEG 文件交换格式 (*.jpg;*.jpeg;*.jfif;*.jpe)|*.jpg;*.jpeg;*.jfif;*.jpe"
                    + "|PNG 可移植网络图形格式 (*.png)|*.png"
                    + "|TIFF Tag 图像文件格式 (*.tif;*.tiff)|*.tif;*.tiff"
                    + "|设备无关位图 (*.bmp;*.dib;*.rle)|*.bmp;*.dib;*.rle"
            };

            var showDialog = dialog.ShowDialog().GetValueOrDefault();
            if (!showDialog) return;
            try
            {
                var fileFullName = dialog.FileName;
                var fileName = $"MenuItemIcon{fileFullName.Substring(fileFullName.LastIndexOf('.'))}";
                File.Copy(fileFullName, Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, fileName ), true);
                var imageSource = await Extentions.GetBitmapImage(fileFullName);
                _imageControl.Content = new AnimatedImage.AnimatedGif
                {
                    Source = imageSource,
                    Stretch = Stretch.Fill,
                };
               
                _userConfig.ImageSetting.MainMenuInfo.Path = fileName;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
                Extentions.ShowMessageBox("出现错误，更改图像失败!");
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!_canTextChange) return;
            var textbox = sender as TextBox;
            if (textbox == null) return;
            _textboxFlag = (TextboxFlag)textbox.Tag;
            _textChanged = true;
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!_textChanged) return;
            SetValue();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_textChanged) return;
            SetValue();
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if(checkbox == null)return;
            var flag = (int) checkbox.Tag;
            var isCheck = checkbox.IsChecked.GetValueOrDefault();
            switch (flag)
            {
                case 0:
                    _userConfig.ShortcutSetting.GlobelAddShortcut.IsCtrl = isCheck;
                    break;
                case 1:
                    _userConfig.ShortcutSetting.GlobelAddShortcut.IsAlt = isCheck;
                    break;
                case 2:
                    _userConfig.ShortcutSetting.GlobelAddShortcut.IsShift = isCheck;
                    break;
                case 3:
                    _userConfig.ShortcutSetting.GlobelPasteShortcut.IsCtrl = isCheck;
                    break;
                case 4:
                    _userConfig.ShortcutSetting.GlobelPasteShortcut.IsAlt = isCheck;
                    break;
                case 5:
                    _userConfig.ShortcutSetting.GlobelPasteShortcut.IsShift = isCheck;
                    break;
            }
        }

        private void KeyTbx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox == null) return;
            e.Handled = true;
            if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
            {
                Extentions.ShowMessageBox("不能有[Ctrl,Alt,Shift]等按键");
                return;
            }
           
            var flag = (int)textbox.Tag;
            switch (flag)
            {
                case 0:
                    if (e.Key != Key.None && e.Key != Key.ImeProcessed)
                    {
                        _userConfig.ShortcutSetting.GlobelAddShortcut.Key = e.Key;
                        KeyTbx1.Text = e.Key.ToString();
                    }
                    else if (e.ImeProcessedKey != Key.None)
                    {
                        _userConfig.ShortcutSetting.GlobelAddShortcut.Key = e.ImeProcessedKey;
                        KeyTbx1.Text = e.ImeProcessedKey.ToString();
                    }
                    break;
                case 1:
                    if (e.Key != Key.None && e.Key != Key.ImeProcessed)
                    {
                        _userConfig.ShortcutSetting.GlobelPasteShortcut.Key = e.Key;
                        KeyTbx2.Text = e.Key.ToString();
                    }
                    else if (e.ImeProcessedKey != Key.None)
                    {
                        _userConfig.ShortcutSetting.GlobelPasteShortcut.Key = e.ImeProcessedKey;
                        KeyTbx2.Text = e.ImeProcessedKey.ToString();
                    }
                    break;
            }
            
        }

        private void AutoRunCk_Click(object sender, RoutedEventArgs e)
        {
            _userConfig.AppSetting.AutoRun = AutoRunCk.IsChecked.GetValueOrDefault();
        }

        private void SetHeightValue()
        {
            double height;
            if (double.TryParse(HeightTbx.Text, out height))
            {
                height = Math.Round(height, 2);
                if (height > 0)
                {
                    _imageControl.Height = height;
                }
            }
            _canTextChange = false;
            HeightTbx.Text = Math.Round(_imageControl.Height, 2).ToString(CultureInfo.InvariantCulture);
            _canTextChange = true;
        }

        private void SetWidthValue()
        {
           
            double width;

            if (double.TryParse(WidthTbx.Text, out width))
            {
                width = Math.Round(width, 2);
                if (width > 0)
                {
                    _imageControl.Width = width;
                }
            }
            _canTextChange = false;
            WidthTbx.Text = Math.Round(_imageControl.Width, 2).ToString(CultureInfo.InvariantCulture);
            _canTextChange = true;
        }

        private void SetTranslateXValue()
        {
            
            double translateX;
            if (double.TryParse(LocationXTbx.Text, out translateX))
            {
                _translateTransform.X = Math.Round(translateX, 2);
            }
            _canTextChange = false;
            LocationXTbx.Text = Math.Round(_translateTransform.X, 2).ToString(CultureInfo.InvariantCulture);
            _canTextChange = true;

        }

        private void SetTranslateYValue()
        {
            
            double translateY;
            if (double.TryParse(LocationYTbx.Text, out translateY))
            {
                _translateTransform.Y = Math.Round(translateY, 2);
            }
            _canTextChange = false;
            LocationYTbx.Text = Math.Round(_translateTransform.Y, 2).ToString(CultureInfo.InvariantCulture);
            _canTextChange = true;

        }

        private void SetInitMaxImgSize()
        {
            double initMaxImgSize;
            if (double.TryParse(MaxSizeTbx.Text, out initMaxImgSize))
            {
                initMaxImgSize = Math.Round(initMaxImgSize);
                if (initMaxImgSize > 0)
                {
                    _userConfig.ImageSetting.InitMaxImgSize = initMaxImgSize;
                }
            }
            _canTextChange = false;
            MaxSizeTbx.Text = _userConfig.ImageSetting.InitMaxImgSize.ToString(CultureInfo.InvariantCulture);
            _canTextChange = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NormalTbm.IsSelected)
            {
                NormalGrid.Visibility = Visibility.Visible;
                ShortcutGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                ShortcutGrid.Visibility = Visibility.Visible;
                NormalGrid.Visibility = Visibility.Hidden;
            }
        }

        private void SetValue()
        {
            try
            {
                switch (_textboxFlag)
                {
                    case TextboxFlag.First:
                        SetHeightValue();
                        break;
                    case TextboxFlag.Second:
                        SetWidthValue();
                        break;
                    case TextboxFlag.Third:
                        SetTranslateXValue();
                        break;
                    case TextboxFlag.Forth:
                        SetTranslateYValue();
                        break;
                    case TextboxFlag.Fifth:
                        SetInitMaxImgSize();
                        break;
                }
                _textChanged = false;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
                Extentions.ShowMessageBox("无效的输入!");
            }
        }

        private void Reset()
        {
            _canTextChange = false;

            _imageControl.Height = _oldHeight;
            _imageControl.Width = _oldWidth;
            _translateTransform.X = _oldTranslateX;
            _translateTransform.Y = _oldTranslateY;
            _userConfig.ImageSetting.InitMaxImgSize = _oldInitMaxImgSize;
            _userConfig.ImageSetting.MainMenuInfo.Path = _oldPath;
            _userConfig.ShortcutSetting.GlobelAddShortcut = (Shortcut)_oldGlobelAddShortcut.Clone();
            _userConfig.ShortcutSetting.GlobelPasteShortcut = (Shortcut)_oldGlobelPasteShortcut.Clone();
            _userConfig.AppSetting.AutoRun = _oldAutoRun;

            _imageControl.Content = _oldAnimatedGif;
            HeightTbx.Text = _oldHeight.ToString(CultureInfo.InvariantCulture);
            WidthTbx.Text = _oldWidth.ToString(CultureInfo.InvariantCulture);
            LocationXTbx.Text = _oldTranslateX.ToString(CultureInfo.InvariantCulture);
            LocationYTbx.Text = _oldTranslateY.ToString(CultureInfo.InvariantCulture);
            MaxSizeTbx.Text = _oldInitMaxImgSize.ToString(CultureInfo.InvariantCulture);
            AutoRunCk.IsChecked = _oldAutoRun;

            CtrlCbx1.IsChecked = _oldGlobelAddShortcut.IsCtrl;
            AltCbx1.IsChecked = _oldGlobelAddShortcut.IsAlt;
            ShiftCbx1.IsChecked = _oldGlobelAddShortcut.IsShift;
            KeyTbx1.Text = _oldGlobelAddShortcut.Key.ToString();

            CtrlCbx2.IsChecked = _oldGlobelPasteShortcut.IsCtrl;
            AltCbx2.IsChecked = _oldGlobelPasteShortcut.IsAlt;
            ShiftCbx2.IsChecked = _oldGlobelPasteShortcut.IsShift;
            KeyTbx2.Text = _oldGlobelPasteShortcut.Key.ToString();

            _canTextChange = true;
        }
    }
}

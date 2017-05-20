using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using DealImage;
using DealImage.Paste;
using EasyImage.Behaviors;
using Point = System.Drawing.Point;
using EasyImage.UnmanagedToolkit;

namespace EasyImage.Windows
{
    /// <summary>
    /// ImageFavoritesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageFavoritesWindow
    {
        #region Private fields
        private AutoHideWindowBehavior _autoHideBehavior;
        private bool _isDragDrop;

        #endregion

        #region Constructor
        public ImageFavoritesWindow()
        {
            InitializeComponent();
            ImageItemsSource = new ObservableCollection<Image>();
        }

        #endregion

        #region Protect methods

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource?.AddHook(WndProc);
        }

        protected const int WmNchittest = 0x0084;
        protected const int AgWidth = 12;
        protected const int BThickness = 4;

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WmNchittest:
                    var mousePoint = new Point(lParam.ToInt32() & 0xFFFF, lParam.ToInt32() >> 16);
                    if (mousePoint.Y - Top <= AgWidth
                       && mousePoint.X - Left <= AgWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Httopleft);
                    }
                    else if (ActualHeight + Top - mousePoint.Y <= AgWidth
                       && mousePoint.X - Left <= AgWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Htbottomleft);
                    }
                    else if (mousePoint.Y - Top <= AgWidth
                       && ActualWidth + Left - mousePoint.X <= AgWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Httopright);
                    }
                    else if (ActualWidth + Left - mousePoint.X <= AgWidth
                       && ActualHeight + Top - mousePoint.Y <= AgWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Htbottomright);
                    }
                    else if (mousePoint.X - Left <= BThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Htleft);
                    }
                    else if (ActualWidth + Left - mousePoint.X <= BThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Htright);
                    }
                    else if (mousePoint.Y - Top <= BThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Httop);
                    }
                    else if (ActualHeight + Top - mousePoint.Y <= BThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.Htbottom);
                    }
                    else
                    {
                        handled = false;
                        return IntPtr.Zero;
                    }
            }
            return IntPtr.Zero;
        }

        protected enum HitTest
        {
            Hterror = -2,
            Httransparent = -1,
            Htnowhere = 0,
            Htclient = 1,
            Htcaption = 2,
            Htsysmenu = 3,
            Htgrowbox = 4,
            Htsize = Htgrowbox,
            Htmenu = 5,
            Hthscroll = 6,
            Htvscroll = 7,
            Htminbutton = 8,
            Htmaxbutton = 9,
            Htleft = 10,
            Htright = 11,
            Httop = 12,
            Httopleft = 13,
            Httopright = 14,
            Htbottom = 15,
            Htbottomleft = 16,
            Htbottomright = 17,
            Htborder = 18,
            Htreduce = Htminbutton,
            Htzoom = Htmaxbutton,
            Htsizefirst = Htleft,
            Htsizelast = Htbottomright,
            Htobject = 19,
            Htclose = 20,
            Hthelp = 21,
        }

        #endregion

        #region Public properties

        public ObservableCollection<Image> ImageItemsSource { get; }

        public void ShowWindow()
        {
            Show();
            if (_autoHideBehavior != null && _autoHideBehavior.IsHide)
            {
                _autoHideBehavior.Show();
            }
        }

        public async void LoadCollectedImages(string path)
        {
            if (!Directory.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                if(!Directory.Exists(path))return;
            }
            var folder = new DirectoryInfo(path);
            foreach (var item in folder.GetFiles())
            {
                try
                {
                    ImageItemsSource.Add(new Image
                    {
                        Source = await Extentions.GetBitmapImage(item.FullName)
                    });
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                }
            }
        }

        public async void SaveCollectedImages(string path)
        {
            if (!Directory.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "./Favorites");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
            }
            else
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            foreach (var item in ImageItemsSource)
            {
                var bitmapImage = item?.Source as BitmapImage;
                if (bitmapImage != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        bitmapImage.StreamSource.Position = 0;
                        await bitmapImage.StreamSource.CopyToAsync(stream);

                        var filePath = Path.Combine(path, Guid.NewGuid().ToString("N"));
                        using (var fs = File.OpenWrite(filePath))
                        {
                            var data = stream.ToArray();
                            fs.Write(data, 0, data.Length);
                        }
                    }
                }
            }

        }

        #endregion

        #region 主窗口事件
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {this.DisableMaxmize(true); //禁用窗口最大化功能
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.Restore | Win32.SystemMenuItems.Minimize | Win32.SystemMenuItems.Maximize | Win32.SystemMenuItems.SpliteLine | Win32.SystemMenuItems.Close); //去除窗口指定的系统菜单

            _autoHideBehavior = new AutoHideWindowBehavior();
            _autoHideBehavior.Attach(this);

            ImageListBox.ItemsSource = ImageItemsSource;

        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var index = ImageListBox.SelectedIndex;
                if (index != -1)
                {
                    ImageItemsSource.RemoveAt(index);
                }
            }
        }

        private async void WindowDrop(object sender, DragEventArgs e)
        {
            if (!_isDragDrop)
            {
                try
                {
                    Win32.Point curPosition;
                    Win32.GetCursorPos(out curPosition);
                    var imageSources = await ImagePaster.GetImageFromIDataObject(e.Data);
                    if (imageSources.Count <= 0) return;
                    imageSources.ForEach(m => ImageItemsSource.Add(new Image
                    {
                        Source = m,
                    }));
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                    Extentions.ShowMessageBox("无效的粘贴!");
                }
            }
            
        }

        private void WindowDragEnter(object sender, DragEventArgs e)
        {
            Activate();
        }

        #endregion


        #region 控件事件

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void DragImage(object sender, MouseButtonEventArgs e)
        {
            var image = e.Source as Image;

            var bitmapImage = image?.Source as BitmapImage;
            if (bitmapImage != null)
            {
                _isDragDrop = true;
                using (var stream = new MemoryStream())
                {

                    bitmapImage.StreamSource.Position = 0;
                    await bitmapImage.StreamSource.CopyToAsync(stream);

                    var dataObject = new DataObject();

                    //普通图片格式
                    dataObject.SetData(ImageDataFormats.Bitmap, bitmapImage, true);

                    //若想取消注释，则不能释放stream，否则会报错
                    ////兼容PNG透明格式图片
                    //dataObject.SetData(ImageDataFormats.Png, stream, true);

                    //兼容QQ
                    var tempFilePath = Path.GetTempFileName();
                    using (var fs = File.OpenWrite(tempFilePath))
                    {
                        var data = stream.ToArray();
                        fs.Write(data, 0, data.Length);
                    }

                    var byteData = Encoding.UTF8.GetBytes("<QQRichEditFormat><Info version=\"1001\"></Info><EditElement type=\"1\" filepath=\"" + tempFilePath + "\" shortcut=\"\"></EditElement><EditElement type=\"0\"><![CDATA[]]></EditElement></QQRichEditFormat>");
                    dataObject.SetData(ImageDataFormats.QqUnicodeRichEditFormat, new MemoryStream(byteData), true);
                    dataObject.SetData(ImageDataFormats.QqRichEditFormat, new MemoryStream(byteData), true);
                    dataObject.SetData(ImageDataFormats.FileDrop, new[] { tempFilePath }, true);
                    dataObject.SetData(ImageDataFormats.FileNameW, new[] { tempFilePath }, true);
                    dataObject.SetData(ImageDataFormats.FileName, new[] { tempFilePath }, true);
                    DragDrop.DoDragDrop(image, dataObject, DragDropEffects.Copy);

                }
                _isDragDrop = false;
            }
        }


        //private void CloseBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}
        #endregion


    }
}

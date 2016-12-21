using System.Configuration;
using System.Windows;
using EasyImage.Config;

namespace EasyImage
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private ImageWindow _imgWin;
   
        public UserConfig UserConfigution { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            UserConfigution = new UserConfig();
            UserConfigution.LoadConfigFromXml(ConfigurationManager.AppSettings["UserConfigPath"]);
        }

        #region 主窗口事件
        /// <summary>
        /// 窗口加载完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _imgWin = new ImageWindow() { Owner = this };
            _imgWin.Show();
            _trayIcon = InitTrayIcon();//初始化系统托盘
        }

        /// <summary>
        /// 窗口关闭时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserConfigution.SaveConfigToXml();
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
            }
        }
        
        #endregion

        #region 初始化操作
        /// <summary>
        /// 初始化系统托盘
        /// </summary>
        private System.Windows.Forms.NotifyIcon InitTrayIcon()
        {
            var trayIcon = new System.Windows.Forms.NotifyIcon()
            {
                BalloonTipText = Properties.Resources.ApplicationName,
                Text = Properties.Resources.ApplicationName,
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath) /*读取程序图标，来作为托盘图标*/
            };
            trayIcon.MouseClick += TrayIcon_MouseClick;

            var conntextMenu = new System.Windows.Forms.ContextMenuStrip();

            var item1 = new System.Windows.Forms.ToolStripMenuItem("设置");
            item1.Click += (sender, e) =>
                           {
                           };
            conntextMenu.Items.Add(item1);

            var tsp = new System.Windows.Forms.ToolStripSeparator();
            conntextMenu.Items.Add(tsp);

            var item2 = new System.Windows.Forms.ToolStripMenuItem("退出");
            item2.Click += (sender, e) =>
                           {
                                _imgWin.Close();
                           };
            conntextMenu.Items.Add(item2);

            trayIcon.ContextMenuStrip = conntextMenu;
            trayIcon.ShowBalloonTip(500);//设置显示提示气球时间
            trayIcon.Visible = true;

            return trayIcon;
        }

        private void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left  && _imgWin != null)
            {
                _imgWin.Visibility = Visibility.Visible;
            }
        }

        #endregion

    }
}

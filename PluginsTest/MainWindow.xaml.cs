using Drawing;
using System.Windows;

namespace PluginsTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var window = new DrawingWindow(Properties.Resources.TestImage);
            window.ShowDialog();
        }
    }
}

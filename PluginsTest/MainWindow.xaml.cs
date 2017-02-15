using Beauty;
using Drawing;
using System;
using System.Windows;
using System.Windows.Interop;
using ArtDeal;
using Property;

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
            var window = new PropertiesWindow(Properties.Resources.TestImage);
            window.ShowDialog();
        }
    }
}

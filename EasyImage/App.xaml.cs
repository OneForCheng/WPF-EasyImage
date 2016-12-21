using System.Linq;
using System.Threading;
using System.Windows;

namespace EasyImage
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        private Mutex _instance;//同步基元，保证不被回收

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            bool createNew;//返回是否赋予了使用线程的互斥体初始所属权
            _instance = new Mutex(true, "EasyImage", out createNew);//同步基元变量

            if (createNew)//控制程序只启动一次
            {
                var mainWindow = new MainWindow();
                if (e.Args.Length > 0)
                {
                    var filePath = e.Args.First();
                    if (System.IO.File.Exists(filePath) && filePath.LastIndexOf('.') > -1 && filePath.Split('.').Last().ToUpper() == "EI")
                    {
                        mainWindow.UserConfigution.WindowState.InitEasyImagePath = filePath;
                    }
                }
                mainWindow.Show();
            }
            else
            {
                MessageBox.Show("已经启动了该程序!");
                Current.Shutdown(0);
            }
        }
    }
}

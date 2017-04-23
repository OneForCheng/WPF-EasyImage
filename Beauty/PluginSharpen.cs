using System.Drawing;
using IPlugins;

namespace Beauty
{
    public class PluginSharpen : IFilter
    {
        public string GetPluginName()
        {
            return "锐化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.SharpenIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new SharpenWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

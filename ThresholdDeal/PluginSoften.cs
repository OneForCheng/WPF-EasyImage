using System;
using IPlugins;
using System.Drawing;

namespace ThresholdDeal
{
    public class PluginSoften : IHandle
    {
        public string GetPluginName()
        {
            return "柔化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.BlurIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new SoftenWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

using System;
using IPlugins;
using System.Drawing;

namespace ThresholdDeal
{
    public class PluginBinary : IHandle
    {
        public string GetPluginName()
        {
            return "二值化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.BinaryIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new BinaryWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

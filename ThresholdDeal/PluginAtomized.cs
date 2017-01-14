using System;
using IPlugins;
using System.Drawing;

namespace ThresholdDeal
{
    public class PluginAtomized : IHandle
    {
        public string GetPluginName()
        {
            return "雾化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.AtomizedIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {

            var window = new AtomizedWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

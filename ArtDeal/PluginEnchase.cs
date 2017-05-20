using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginEnchase : IMultiFilter
    {
        public string GetPluginName()
        {
            return "浮雕化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.EnchaseIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new EnchaseWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

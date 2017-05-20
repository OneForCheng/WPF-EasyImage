using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginSpherize : IMultiFilter
    {
        public string GetPluginName()
        {
            return "球面化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.SpherizeIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new SpherizeWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

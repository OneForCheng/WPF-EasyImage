using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginMosaic : IMultiFilter
    {
        public string GetPluginName()
        {
            return "马赛克处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.MosaicIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new MosaicWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

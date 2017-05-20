using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginBinary : IMultiFilter
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

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new BinaryWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginAtomized : IMultiFilter
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

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {

            var window = new AtomizedWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginMagicMirror : IMultiFilter
    {
        public string GetPluginName()
        {
            return "哈哈镜处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.GlassesIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new MagicMirrorWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

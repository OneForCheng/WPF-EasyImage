using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Beauty
{
    public class PluginSoften : IMultiFilter
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

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new SoftenWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

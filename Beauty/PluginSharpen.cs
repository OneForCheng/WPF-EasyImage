using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Beauty
{
    public class PluginSharpen : IMultiFilter
    {
        public string GetPluginName()
        {
            return "锐化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.SharpenIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new SharpenWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

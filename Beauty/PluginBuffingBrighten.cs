using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Beauty
{
    public class PluginBuffingBrighten : IMultiFilter
    {
        public string GetPluginName()
        {
            return "磨皮美白处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.BeautyIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new BuffingBrightenWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

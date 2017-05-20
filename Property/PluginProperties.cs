using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Property
{
    public class PluginProperties : IMultiFilter
    {
        public string GetPluginName()
        {
            return "属性调整[GIF]";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.SettingsIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(IEnumerable<Bitmap> bitmaps)
        {
            var window = new PropertiesWindow(bitmaps);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

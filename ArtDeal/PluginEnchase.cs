using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginEnchase : IHandle
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

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new EnchaseWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginSpherize : IHandle
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

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new SpherizeWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

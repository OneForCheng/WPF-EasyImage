using System.Drawing;
using IPlugins;

namespace Property
{
    public class PluginProperties : IHandle
    {
        public string GetPluginName()
        {
            return "Properties";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.SettingsIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new PropertiesWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

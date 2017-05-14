using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    public class PluginMagicMirror : ISingleFilter
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

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new MagicMirrorWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

using System.Drawing;
using IPlugins;

namespace Beauty
{
    public class PluginBuffingBrighten : ISingleFilter
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

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new BuffingBrightenWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;

        }
    }
}

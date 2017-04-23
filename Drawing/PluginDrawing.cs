using System.Drawing;
using IPlugins;

namespace Drawing
{
    public class PluginDrawing : IFilter
    {
        public string GetPluginName()
        {
            return "涂鸦";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.DrawingIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new DrawingWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

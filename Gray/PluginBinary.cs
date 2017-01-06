using IPlugins;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Gray
{
    public class PluginBinary : IHandle
    {
        public string GetPluginName()
        {
            return "二值化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.BinaryIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var binaryWin = new BinaryWindow(bitmap);
            return binaryWin.ShowDialog().GetValueOrDefault() ? new HandleResult(binaryWin.ResultBitmap, true) : new HandleResult(null, false);
        }
    }
}

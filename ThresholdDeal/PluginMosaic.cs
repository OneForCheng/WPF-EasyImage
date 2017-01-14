using System;
using IPlugins;
using System.Drawing;

namespace ThresholdDeal
{
    public class PluginMosaic : IHandle
    {
        public string GetPluginName()
        {
            return "马赛克处理";
        }

        public Bitmap GetPluginIcon()
        {
            return Properties.Resources.MosiacIcon;
        }

        public void InitPlugin(string appStartupPath)
        {

        }

        public HandleResult ExecHandle(Bitmap bitmap)
        {
            var window = new MosaicWindow(bitmap);
            window.ShowDialog();
            return window.HandleResult;
        }
    }
}

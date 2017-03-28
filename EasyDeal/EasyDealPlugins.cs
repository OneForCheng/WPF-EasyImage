using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace EasyDeal
{
    class EasyDealPlugins : IHandleList
    {
        public string GetPluginName()
        {
            return "简单处理";
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IHandle> GetIHandleList()
        {
            return new List<IHandle>
            {
                new PluginBlackWhite(),
                new PluginEnBlackWhite(),
                new PluginInvertColor(),
                new PluginSobelEdge()
            };
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace EasyDeal
{
    class EasyDealPlugins : IFilterList
    {
        public string GetPluginName()
        {
            return "简单处理[GIF]";
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IFilter> GetIFilterList()
        {
            return new List<IFilter>
            {
                new PluginBlackWhite(),
                new PluginEnBlackWhite(),
                new PluginInvertColor(),
                new PluginSobelEdge()
            };
        }
    }
}

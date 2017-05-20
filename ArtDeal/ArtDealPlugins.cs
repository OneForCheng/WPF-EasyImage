using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    class ArtDealPlugins : IFilterList
    {
        public string GetPluginName()
        {
            return "艺术处理[GIF]";
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IFilter> GetIFilterList()
        {
            return new List<IFilter>
            {
                new PluginBinary(),
                new PluginEnchase(),
                new PluginAtomized(),
                new PluginMosaic(),
                new PluginMagicMirror(),
                new PluginSpherize()
            };
        }
    }
}

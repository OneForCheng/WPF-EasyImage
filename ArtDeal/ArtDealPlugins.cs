using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace ArtDeal
{
    class ArtDealPlugins : IHandleList
    {
        public string GetPluginName()
        {
            return "艺术处理";
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IHandle> GetIHandleList()
        {
            return new List<IHandle>
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

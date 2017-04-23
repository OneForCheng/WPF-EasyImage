using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Beauty
{
    class BeautyPlugins : IFilterList
    {
        public string GetPluginName()
        {
            return "美化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IFilter> GetIFilterList()
        {
            return new List<IFilter>
            {
                new PluginBuffingBrighten(),
                new PluginSharpen(),
                new PluginSoften()
            };
        }
    }
}

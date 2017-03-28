using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Beauty
{
    class BeautyPlugins : IHandleList
    {
        public string GetPluginName()
        {
            return "美化处理";
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IHandle> GetIHandleList()
        {
            return new List<IHandle>
            {
                new PluginBuffingBrighten(),
                new PluginSharpen(),
                new PluginSoften()
            };
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Property
{
    class PropertyPlugins : IHandleList
    {
        public string GetPluginName()
        {
            return string.Empty;
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IHandle> GetIHandleList()
        {
            return new List<IHandle>
            {
                new PluginProperties(),
            };
        }
    }
}

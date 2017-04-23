using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Drawing
{
    class DrawingPlugins : IFilterList
    {
        public string GetPluginName()
        {
            return string.Empty;
        }

        public Bitmap GetPluginIcon()
        {
            return null;
        }

        public List<IFilter> GetIFilterList()
        {
            return new List<IFilter>
            {
                new PluginDrawing(),
            };
        }
    }
}

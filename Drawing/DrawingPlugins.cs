using System.Collections.Generic;
using System.Drawing;
using IPlugins;

namespace Drawing
{
    class DrawingPlugins : IHandleList
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
                new PluginDrawing(),
            };
        }
    }
}

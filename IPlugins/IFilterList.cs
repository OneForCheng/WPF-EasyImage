using System.Collections.Generic;
using System.Drawing;

namespace IPlugins
{
    public interface IFilterList
    {
        /// <summary>
        /// 获取插件显示的名字
        /// </summary>
        /// <returns>插件名字</returns>
        string GetPluginName();

        /// <summary>
        /// 获取插件在菜单上显示的图标 若不需要显示则返回null
        /// </summary>
        /// <returns>图标 否则 null</returns>
        Bitmap GetPluginIcon();

        /// <summary>
        /// 获取插件集合
        /// </summary>
        /// <returns></returns>
        List<IFilter> GetIFilterList();

    }
}
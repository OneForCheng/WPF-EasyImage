using System.Drawing;

namespace IPlugins
{
    public interface IHandle
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
        /// 用于加载插件时候初始化调用
        /// </summary>
        /// <param name="appStartupPath">主程序启动路径</param>
        void InitPlugin(string appStartupPath);

        /// <summary>
        /// 处理图片
        /// </summary>
        /// <param name="bitmap">需要处理的图片</param>
        /// <returns>处理结果</returns>
        HandleResult ExecHandle(Bitmap bitmap);
    }
}

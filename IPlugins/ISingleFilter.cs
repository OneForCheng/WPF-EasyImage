using System.Drawing;

namespace IPlugins
{
    public interface ISingleFilter : IFilter
    {
        /// <summary>
        /// 处理单张图片
        /// </summary>
        /// <param name="bitmap">需要处理的图片</param>
        /// <returns>处理结果</returns>
        HandleResult ExecHandle(Bitmap bitmap);
    }
}
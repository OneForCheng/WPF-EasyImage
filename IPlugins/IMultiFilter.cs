using System.Collections.Generic;
using System.Drawing;

namespace IPlugins
{
    public interface IMultiFilter : IFilter
    {
        /// <summary>
        /// 处理多张图片
        /// </summary>
        /// <param name="bitmap">需要处理的图片</param>
        /// <returns>处理结果</returns>
        HandleResult ExecHandle(IEnumerable<Bitmap> bitmap);
    }
}
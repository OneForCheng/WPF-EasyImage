using System;
using System.Collections.Generic;
using System.Drawing;

namespace IPlugins
{
    public class HandleResult : IDisposable
    {
        public IEnumerable<Bitmap> ResultBitmaps { get; }

        public bool IsModified { get; }

        public bool IsSuccess { get; }

        public Exception Exception { get; }

        /// <summary>
        /// 记录异常的构造函数
        /// </summary>
        /// <param name="exception"></param>
        public HandleResult(Exception exception)
        {
            IsSuccess = false;
            Exception = exception;
        }

        /// <summary>
        /// 正常构造函数
        /// </summary>
        /// <param name="bitmaps"></param>
        /// <param name="isModified"></param>
        public HandleResult(IEnumerable<Bitmap> bitmaps, bool isModified)
        {
            ResultBitmaps = bitmaps;
            IsModified = isModified;
            IsSuccess = true;
        }

        public void Dispose()
        {
            if (ResultBitmaps != null)
            {
                foreach (var item in ResultBitmaps)
                {
                    item.Dispose();
                }
            }
        }
    }
}

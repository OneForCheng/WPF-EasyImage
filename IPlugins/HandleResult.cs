using System;
using System.Drawing;

namespace IPlugins
{
    public class HandleResult : IDisposable
    {
        public Bitmap ResultBitmap { get; }

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
        /// <param name="bitmap"></param>
        /// <param name="isModified"></param>
        public HandleResult(Bitmap bitmap, bool isModified)
        {
            ResultBitmap = bitmap;
            IsModified = isModified;
            IsSuccess = true;
        }

        public void Dispose()
        {
            ResultBitmap?.Dispose();
        }
    }
}

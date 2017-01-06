using System;
using System.Drawing;

namespace IPlugins
{
    public class HandleResult : IDisposable
    {
        public Bitmap ResultBitmap { get; }

        public bool IsModified { get; }

        public HandleResult(Bitmap bitmap, bool isModified)
        {
            ResultBitmap = bitmap;
            IsModified = isModified;
        }

        public void Dispose()
        {
            ResultBitmap?.Dispose();
        }
    }
}

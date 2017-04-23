using System;

namespace EasyImage.Enum
{
    /// <summary>
    /// 堆叠顺序标识
    /// </summary>
    [Flags]
    public enum ZIndexFlag
    {
        Bottommost,
        Bottom,
        Top,
        Topmost,
    }
}

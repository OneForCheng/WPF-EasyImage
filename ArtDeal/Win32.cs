using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ArtDeal
{
    internal static class Win32
    {
        /// <summary>
        /// 系统菜单选项
        /// </summary>
        public enum SystemMenuItems
        {
            /// <summary>
            /// 还原
            /// </summary>
            Restore = 1,
            /// <summary>
            /// 移动
            /// </summary>
            Move = 2,
            /// <summary>
            /// 大小
            /// </summary>
            Size = 4,
            /// <summary>
            /// 最小化
            /// </summary>
            Minimize = 8,
            /// <summary>
            /// 最大化
            /// </summary>
            Maximize = 16,
            /// <summary>
            /// 分割线
            /// </summary>
            SpliteLine = 32,
            /// <summary>
            /// 关闭
            /// </summary>
            Close = 64,
            /// <summary>
            /// 所有
            /// </summary>
            All = 127
        }

        /// <summary>
        /// 去除窗体的指定的系统菜单
        /// </summary>
        /// <param name="window"></param>
        /// <param name="menuItems"></param>
        public static void RemoveSystemMenuItems(this Window window, SystemMenuItems menuItems)
        {
            if (window == null)
            {
                return;
            }
            var winHelper = new WindowInteropHelper(window);
            var hwnd = winHelper.Handle;
            var sysMenu = GetSystemMenu(hwnd, 0);
            const uint mfByposition = 0x00000400;
            // 6 close, 5 splite line, 4 Maximize, 3 Minimize, 2 Size, 1 Move, 0 Restore
            // notes that, delete from bigger index to smaller
            var flag = (int)menuItems;
            var bit = 128;
            for (uint i = 6; ; i--)
            {
                bit = bit / 2;
                if ((flag & bit) != 0)
                {
                    DeleteMenu(sysMenu, i, mfByposition);
                }
                if (bit == 1)
                {
                    break;
                }
            }
            DrawMenuBar(sysMenu);
        }

        /// <summary>
        /// 获取系统菜单句柄
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="bRevert"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, Int32 bRevert);

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="hMenu"></param>
        /// <param name="uPosition"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        /// <summary>
        /// 重绘菜单
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        internal static extern int DrawMenuBar(IntPtr hWnd);

    }
}

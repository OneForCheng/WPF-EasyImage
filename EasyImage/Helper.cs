using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using EasyImage.Enum;

namespace EasyImage
{
    public static class Helper
    {
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
            for (uint i = 6;; i--)
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
        /// 从指定UI元素中获取指定位置转换信息
        /// </summary>
        /// <typeparam name="T">位置转换信息</typeparam>
        /// <param name="element">UI元素</param>
        /// <returns></returns>
        public static T GetTransform<T>(this UIElement element) where T : Transform, new()
        {
            var transform = element.RenderTransform;
            var targetTransform = transform as T;
            if (targetTransform != null)
            {
                return targetTransform;
            }
            else
            {
                var group = transform as TransformGroup;
                if (group != null)
                {
                    var count = group.Children.Count;
                    for (var i = count - 1; i >= 0; i--)
                    {
                        targetTransform = group.Children[i] as T;
                        if (targetTransform != null)
                        {
                            break;
                        }
                    }
                    if (targetTransform != null) return targetTransform;
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    return targetTransform;
                }
                else
                {
                    group = new TransformGroup();
                    if (transform != null)
                    {
                        group.Children.Add(transform);
                    }
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    element.RenderTransform = group;

                    return targetTransform;
                }
            }
        }

        #region 系统API函数
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

        #endregion

    }
}

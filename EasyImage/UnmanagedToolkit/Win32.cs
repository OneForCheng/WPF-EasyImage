using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace EasyImage.UnmanagedToolkit
{
    public static class Win32
    {

        #region 1、去除系统菜单
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

        #endregion

        #region 2、窗体间发送消息
        public const int WmCopydata = 0x004A;

        [StructLayout(LayoutKind.Sequential)]
        public struct CopyDataStruct
        {
            public IntPtr dwData;
            public int cbData;//字符串长度
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;//字符串
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="windowName">window的title，建议加上GUID，不会重复</param>
        /// <param name="strMsg">要发送的字符串</param>
        public static void SendMessage(string windowName, string strMsg)
        {
            if (strMsg == null) return;
            var hwnd = FindWindow(null, windowName);
            if (hwnd != IntPtr.Zero)
            {
                CopyDataStruct cds;
                cds.dwData = IntPtr.Zero;
                cds.lpData = strMsg;
                //注意：长度为字节数
                cds.cbData = System.Text.Encoding.Default.GetBytes(strMsg).Length + 1;
                // 消息来源窗体
                var fromWindowHandler = 0;
                SendMessage(hwnd, WmCopydata, fromWindowHandler, ref cds);
            }
        }

        [DllImport("user32")]
        public static extern bool ChangeWindowMessageFilter(uint msg, int flags);


        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 给指定窗体发送消息
        /// </summary>
        /// <param name="hWnd">目标窗体句柄</param>
        /// <param name="msg">消息标识</param>
        /// <param name="wParam">自定义数值</param>
        /// <param name="lParam">结构体</param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref CopyDataStruct lParam);

        #endregion

        #region MyRegion


        //获取鼠标坐标
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out Point pt);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        #endregion
    }

}

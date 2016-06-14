using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace stdOldW.WinFormHelper
{
    /// <summary>
    /// 与窗口操作相关的函数，以及鼠标、键盘的操作命令
    /// </summary>
    public class WindowsUtil
    {
        /// <summary>
        /// 向任意进程发送键盘消息
        /// </summary>
        /// <param name="bVk">按键的虚拟键值，如回车键为vk_return, tab键为vk_tab,可以参考常用模拟键的键值对照表，也可以通过System.Windows.Forms.Keys枚举来查看。</param>
        /// <param name="bScan">扫描码，一般不用设置，用0代替就行；</param>
        /// <param name="dwFlags">选项标志，如果为keydown则置0即可，如果为keyup则设成数值2，即常数 KEYEVENTF_KEYUP；</param>
        /// <param name="dwExtraInfo">一般也是置0即可。</param>
        /// <remarks>
        ///  调用案例1：
        /// keybd_event(System.Windows.Forms.Keys.Escape, 0, 0, 0)  ' 按下 ESCAPE键
        /// keybd_event(System.Windows.Forms.Keys.NumLock, 0, KEYEVENTF_KEYUP, 0)  ' 按键弹起，其中 KEYEVENTF_KEYUP=2
        ///  调用案例2：    模拟按下 'ALT+F4'键
        /// keybd_event(18, 0, 0, 0);
        /// keybd_event(115, 0, 0, 0);
        /// keybd_event(115, 0, KEYEVENTF_KEYUP, 0);
        /// keybd_event(18, 0, KEYEVENTF_KEYUP, 0);
        ///</remarks>
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, UInt32 dwFlags, UIntPtr dwExtraInfo);

        //声明方式二： Public Declare Sub keybd_event Lib "user32" (ByVal bVk As Byte, ByVal bScan As Byte, ByVal dwFlags As Integer, ByVal dwExtraInfo As Integer)
        // Public Const

        // 调用案例1：
        //keybd_event(System.Windows.Forms.Keys.Escape, 0, 0, 0)  ' 按下 ESCAPE键
        //keybd_event(System.Windows.Forms.Keys.NumLock, 0, KEYEVENTF_KEYUP, 0)  ' 按键弹起

        // 调用案例2：    模拟按下 'ALT+F4'键
        //keybd_event(18, 0, 0, 0);
        //keybd_event(115, 0, 0, 0);
        //keybd_event(115, 0, KEYEVENTF_KEYUP, 0);
        //keybd_event(18, 0, KEYEVENTF_KEYUP, 0);

        /// <summary> 发送鼠标消息 </summary>
        /// <param name="dwFlags"> (位编码)要如何操作鼠标。如果不指定 MOUSEEVENTF_ABSOLUTE，则是相对于当前的鼠标位置。 </param>
        /// <param name="dx">根据MOUSEEVENTF_ABSOLUTE标志，指定x，y方向的绝对位置或相对位置 </param>
        /// <param name="dy">根据MOUSEEVENTF_ABSOLUTE标志，指定x，y方向的绝对位置或相对位置 </param>
        /// <param name="dwData">没有使用，直接赋值为0 </param>
        /// <param name="dwExtraInfo">没有使用，直接赋值为0 </param>
        [DllImport("user32.dll")]
        public static extern void mouse_event(MouseOperation dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        [Flags]
        public enum MouseOperation : uint
        {
            /// <Summary> 移动鼠标 </Summary>
            MouseEventF_Move = 0x0001,      // 移动鼠标

            /// <Summary> 模拟鼠标左键按下 </Summary>
            MouseEventF_LeftDown = 0x0002,  // 模拟鼠标左键按下

            /// <Summary> 模拟鼠标左键抬起 </Summary>
            MouseEventF_LeftUp = 0x0004,    // 模拟鼠标左键抬起

            /// <Summary> 模拟鼠标右键按下 </Summary>
            MouseEventF_RightDown = 0x0008, // 模拟鼠标右键按下

            /// <Summary> 模拟鼠标右键抬起 </Summary>
            MouseEventF_RightUp = 0x0010,   // 模拟鼠标右键抬起

            /// <Summary> 模拟鼠标中键按下 </Summary>
            MouseEventF_MiddleDown = 0x0020,// 模拟鼠标中键按下

            /// <Summary> 模拟鼠标中键抬起 </Summary>
            MouseEventF_MiddleUp = 0x0040,  // 模拟鼠标中键抬起

            /// <Summary> 标示是否采用绝对坐标 </Summary>
            MouseEventF_Absolute = 0x8000,  // 标示是否采用绝对坐标
        }
    }

}
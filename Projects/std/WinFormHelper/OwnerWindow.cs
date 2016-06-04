using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stdOldW.WinFormHelper
{
    /// <summary> 将任意一个窗体作为Form的父窗口，并用在Form.Show(IWin32Window)中 </summary>
    public class OwnerWindow : IWin32Window
    {
        private IntPtr hand;

        /// <summary> 父窗口的句柄值 </summary>
        public IntPtr Handle
        {
            get { return hand; }
        }

        /// <summary> 构造函数 </summary>
        /// <param name="Handle">作为父窗体的窗口句柄值</param>
        /// <remarks></remarks>
        public OwnerWindow(IntPtr Handle)
        {
            this.hand = Handle;
        }

        /// <summary> 通过当前运行的进程名称来获得对应的主窗体 </summary>
        /// <param name="ProcessName">当前运行的进程名称，此名称可以通过“Windows任务管理器 -> 进程”进行查看。</param>
        public static OwnerWindow CreateFromProcessName(string ProcessName)
        {
            Process[] procs = Process.GetProcessesByName(ProcessName);
            if (procs.Length > 0)
            {
                return new OwnerWindow(procs[0].MainWindowHandle);
            }
            else
            {
                throw new NullReferenceException(string.Format("指定的进程\"{0}\"未找到！", ProcessName));
            }
        }
    }
}

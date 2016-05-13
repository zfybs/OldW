Imports System.Runtime.InteropServices

Namespace std_ez
    ''' <summary>
    ''' 与窗口操作相关的函数，以及鼠标、键盘的操作命令
    ''' </summary>
    Public Class WindowsUtil

        ''' <summary>
        ''' 向任意进程发送键盘消息
        ''' </summary>
        ''' <param name="bVk">按键的虚拟键值，如回车键为vk_return, tab键为vk_tab,可以参考常用模拟键的键值对照表，也可以通过System.Windows.Forms.Keys枚举来查看。</param>
        ''' <param name="bScan">扫描码，一般不用设置，用0代替就行；</param>
        ''' <param name="dwFlags">选项标志，如果为keydown则置0即可，如果为keyup则设成数值2，即常数 KEYEVENTF_KEYUP；</param>
        ''' <param name="dwExtraInfo">一般也是置0即可。</param>
        ''' <remarks>
        '''  调用案例1：
        ''' keybd_event(System.Windows.Forms.Keys.Escape, 0, 0, 0)  ' 按下 ESCAPE键
        ''' keybd_event(System.Windows.Forms.Keys.NumLock, 0, KEYEVENTF_KEYUP, 0)  ' 按键弹起，其中 KEYEVENTF_KEYUP=2 
        '''  调用案例2：    模拟按下 'ALT+F4'键
        ''' keybd_event(18, 0, 0, 0);
        ''' keybd_event(115, 0, 0, 0);
        ''' keybd_event(115, 0, KEYEVENTF_KEYUP, 0); 
        ''' keybd_event(18, 0, KEYEVENTF_KEYUP, 0);
        '''</remarks>
        <DllImport("user32.dll")>
        Public Shared Sub keybd_event(bVk As Byte, bScan As Byte, dwFlags As UInteger, dwExtraInfo As UIntPtr)
            '声明方式二： Public Declare Sub keybd_event Lib "user32" (ByVal bVk As Byte, ByVal bScan As Byte, ByVal dwFlags As Integer, ByVal dwExtraInfo As Integer)
            ' Public Const 

            ' 调用案例1：
            'keybd_event(System.Windows.Forms.Keys.Escape, 0, 0, 0)  ' 按下 ESCAPE键
            'keybd_event(System.Windows.Forms.Keys.NumLock, 0, KEYEVENTF_KEYUP, 0)  ' 按键弹起

            ' 调用案例2：    模拟按下 'ALT+F4'键
            'keybd_event(18, 0, 0, 0);
            'keybd_event(115, 0, 0, 0);
            'keybd_event(115, 0, KEYEVENTF_KEYUP, 0); 
            'keybd_event(18, 0, KEYEVENTF_KEYUP, 0);

        End Sub


    End Class

    ''' <summary> 将任意一个窗体作为Form的父窗口，并用在Form.Show(IWin32Window)中 </summary>
    Public Class OwnerWindow
        Implements IWin32Window

        Private hand As IntPtr
        ''' <summary> 父窗口的句柄值 </summary>
        Public ReadOnly Property Handle As IntPtr Implements IWin32Window.Handle
            Get
                Return hand
            End Get
        End Property

        ''' <summary> 构造函数 </summary>
        ''' <param name="Handle">作为父窗体的窗口句柄值</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal Handle As IntPtr)
            Me.hand = Handle
        End Sub

        ''' <summary> 通过当前运行的进程名称来获得对应的主窗体 </summary>
        ''' <param name="ProcessName">当前运行的进程名称，此名称可以通过“Windows任务管理器 -> 进程”进行查看。</param>
        Public Shared Function CreateFromProcessName(ByVal ProcessName As String) As OwnerWindow
            Dim procs As Process() = Process.GetProcessesByName(ProcessName)
            If procs.Length > 0 Then
                Return New OwnerWindow(procs(0).MainWindowHandle)
            Else
                Throw New NullReferenceException(String.Format("指定的进程""{0}""未找到！", ProcessName))
            End If
        End Function

    End Class
End Namespace
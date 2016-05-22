Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices

Namespace std_ez

    ''' <summary>
    ''' 提供一些基础性的操作工具
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Utils

#Region "弹框显示集合中的某些属性或者字段的值"


        ''' <summary>
        ''' 将集合中的每一个元素的ToString函数的结果组合到一个字符串中进行显示
        ''' </summary>
        ''' <param name="V"></param>
        ''' <remarks></remarks>
        Public Shared Sub ShowEnumerable(ByVal V As IEnumerable, Optional ByVal Title As String = "集合中的元素")
            Dim str As String = ""
            For Each o As Object In V
                str = str & o.ToString & vbCrLf
            Next
            MessageBox.Show(str, Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        ''' <summary>
        ''' 将集合中的每一个元素的指定属性的ToString函数的结果组合到一个字符串中进行显示
        ''' </summary>
        ''' <param name="V"></param>
        ''' <param name="PropertyName">要读取的属性的名称，注意，此属性不能带参数。</param>
        ''' <remarks></remarks>
        Public Shared Sub ShowEnumerableP(ByVal V As IEnumerable, ByVal PropertyName As String, Optional ByVal Title As String = "集合中的元素")
            Dim str As String = ""
            Dim tp As Type
            Dim MdInfo As Reflection.MethodInfo
            Dim res As String
            For Each obj As Object In V
                tp = obj.GetType
                MdInfo = tp.GetProperty(PropertyName).GetMethod
                res = MdInfo.Invoke(obj, Nothing).ToString()
                '
                str = str & res & vbCrLf
            Next
            MessageBox.Show(str, Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        ''' <summary>
        ''' 将集合中的每一个元素的指定字段的ToString函数的结果组合到一个字符串中进行显示
        ''' </summary>
        ''' <param name="V"></param>
        ''' <param name="FieldName">要读取的字段的名称。</param>
        ''' <remarks></remarks>
        Public Shared Sub ShowEnumerableF(ByVal V As IEnumerable, ByVal FieldName As String, Optional ByVal Title As String = "集合中的元素")
            Dim str As String = ""
            Dim tp As Type

            Dim res As String
            For Each obj As Object In V
                tp = obj.GetType
                res = tp.GetField(FieldName).GetValue(obj).ToString
                '
                str = str & res & vbCrLf
            Next
            MessageBox.Show(str, Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

#End Region

        ''' <summary>
        ''' 将字符转换为日期。除了.NET能够识别的日期格式外，
        ''' 还增加了20160406（ 即 2016/04/06），以及 201604061330（即 2016/04/06 13:30）
        ''' </summary>
        ''' <param name="text">要转换为日期的字符。</param>
        ''' <returns></returns>
        Public Shared Function String2Date(ByVal text As String, ByRef ResultedDate As Nullable(Of Date)) As Boolean
            Dim blnSucceed As Boolean
            ' 模式1. 正常的日期格式
            If Date.TryParse(text, ResultedDate) Then
                Return True
            End If

            ' 模式2. 20160406 ， 即 2016/04/06
            If text.Length = 8 Then
                Try
                    ResultedDate = New Date(Integer.Parse(text.Substring(0, 4)),
                                            Integer.Parse(text.Substring(4, 2)),
                                            Integer.Parse(text.Substring(6, 2)))
                    Return True
                Catch ex As Exception
                    ResultedDate = Nothing
                    Return False
                End Try
            End If

            ' 模式3. 201604061330 ， 即 2016/04/06 13:30
            If text.Length = 12 Then
                Try
                    ResultedDate = New Date(Integer.Parse(text.Substring(0, 4)),
                                            Integer.Parse(text.Substring(4, 2)),
                                            Integer.Parse(text.Substring(6, 2)),
                                            Integer.Parse(text.Substring(8, 2)),
                                            Integer.Parse(text.Substring(10, 2)), 0)
                    Return True
                Catch ex As Exception
                    ResultedDate = Nothing
                    Return False
                End Try
            End If
            Return blnSucceed
        End Function

        ''' <summary>
        ''' 将指定的文件夹添加到此程序的DLL文件的搜索路径中。
        ''' 这个函数只用来在开发时用于AddinManager调试之用，在最终的Release版本中，此函数中的内容可以直接删除。
        ''' </summary>
        ''' <param name="SearchPath">要添加的文件夹路径</param>
        ''' <remarks></remarks>
        Public Shared Sub SetDllDirectory(ByVal SearchPath As String)
            If Not SetDllDirectoryW(SearchPath) Then
                Throw New ArgumentException("无法将路径"" " & SearchPath & " "" 添加到DLL的索引路径中！")
            End If
        End Sub
        ''' <summary>
        ''' 将指定的文件夹添加到此程序的DLL文件的搜索路径中.
        ''' adds a directory to the search path used to locate DLLs for the application.
        ''' </summary>
        ''' <param name="lpPathName">要添加的文件夹路径</param>
        ''' <remarks>Pretty straight-forward to use. Obviously, is usually going to be called before calling LoadLibraryEx().
        ''' 另外,在PInvoke中,只有SetDllDirectory这个函数,但是它的真实的名称是SetDllDirectoryW.</remarks>
        Private Declare Function SetDllDirectoryW Lib "kernel32.dll" (ByVal lpPathName As String) As Boolean



        ''' <summary>
        ''' 装载指定的动态链接库，并为当前进程把它映射到地址空间。一旦载入，就可以访问库内保存的资源。一旦不需要，用FreeLibrary函数释放DLL
        ''' </summary>
        ''' <param name="lpFileName">指定要载入的动态链接库的名称。采用与CreateProcess函数的lpCommandLine参数指定的同样的搜索顺序</param>
        ''' <param name="hReservedNull">未用，设为零</param>
        ''' <param name="dwFlags"></param>
        ''' <returns>成功则返回库模块的句柄，零表示失败。会设置GetLastError</returns>
        ''' <remarks>参考 http://www.pinvoke.net/default.aspx/kernel32/LoadLibraryEx.html .
        ''' If you only want to load resources from the library, specify LoadLibraryFlags.LoadLibraryAsDatafile as dwFlags. 
        ''' In this case, nothing is done to execute or prepare to execute the mapped file.</remarks>
        <DllImport("kernel32.dll")>
        Public Shared Function LoadLibraryEx(lpFileName As String, hReservedNull As IntPtr, dwFlags As LoadLibraryFlags) As IntPtr
        End Function

        ''' <summary>
        ''' 用在 LoadLibraryEx 函数中
        ''' </summary>
        ''' <remarks></remarks>
        <System.Flags>
        Public Enum LoadLibraryFlags As UInteger
            ''' <summary> 不对DLL进行初始化，仅用于NT </summary>
            DONT_RESOLVE_DLL_REFERENCES = &H1
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = &H10
            ''' <summary> 不准备DLL执行。如装载一个DLL只是为了访问它的资源，就可以改善一部分性能 </summary>
            LOAD_LIBRARY_AS_DATAFILE = &H2
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = &H40
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = &H20
            ''' <summary> 指定搜索的路径 </summary>
            LOAD_WITH_ALTERED_SEARCH_PATH = &H8
        End Enum

    End Class
End Namespace
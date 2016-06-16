using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace stdOldW
{
    /// <summary>
    /// 提供一些基础性的操作工具
    /// </summary>
    /// <remarks></remarks>
    public static class Utils
    {
        #region 弹框显示集合中的某些属性或者字段的值

        /// <summary>
        /// 将集合中的每一个元素的ToString函数的结果组合到一个字符串中进行显示
        /// </summary>
        /// <param name="V"></param>
        /// <remarks></remarks>
        public static void ShowEnumerable(IEnumerable V, string Title = "集合中的元素")
        {
            string str = "";
            foreach (object o in V)
            {
                str = str + o.ToString() + "\r\n";
            }
            MessageBox.Show(str, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 将集合中的每一个元素的指定属性的ToString函数的结果组合到一个字符串中进行显示
        /// </summary>
        /// <param name="V"></param>
        /// <param name="PropertyName">要读取的属性的名称，注意，此属性不能带参数。</param>
        /// <remarks></remarks>
        public static void ShowEnumerableProperty(IEnumerable V, string PropertyName, string Title = "集合中的元素")
        {
            string str = "";
            Type tp = default(Type);
            MethodInfo MdInfo = default(MethodInfo);
            string res = "";
            foreach (object obj in V)
            {
                tp = obj.GetType();
                MdInfo = tp.GetProperty(PropertyName).GetMethod;
                res = Convert.ToString(MdInfo.Invoke(obj, null).ToString());
                //
                str = str + res + "\r\n";
            }
            MessageBox.Show(str, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 将集合中的每一个元素的指定字段的ToString函数的结果组合到一个字符串中进行显示
        /// </summary>
        /// <param name="V"></param>
        /// <param name="FieldName">要读取的字段的名称。</param>
        /// <remarks></remarks>
        public static void ShowEnumerableField(IEnumerable V, string FieldName, string Title = "集合中的元素")
        {
            string str = "";
            Type tp = default(Type);

            string res = "";
            foreach (object obj in V)
            {
                tp = obj.GetType();
                res = tp.GetField(FieldName).GetValue(obj).ToString();
                //
                str = str + res + "\r\n";
            }
            MessageBox.Show(str, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        /// <summary>
        /// 在调试阶段，为每一种报错显示对应的报错信息及出错位置。
        /// 在软件发布前，应将此方法中的内容修改为常规的报错提醒。
        /// </summary>
        /// <param name="ex"> Catch 块中的 Exception 对象</param>
        /// <param name="message">报错信息提示</param>
        /// <param name="title"> 报错对话框的标题 </param>
        public static void ShowDebugCatch(Exception ex, string message, string title = "出错")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);

            // 一直向下提取InnerException
            Exception exIn = ex.InnerException;
            Exception exStack = ex;
            while (exIn != null)
            {
                exStack = exIn;
                sb.AppendLine(exIn.Message);
                exIn = exIn.InnerException;
            }
            // 最底层的出错位置
            sb.AppendLine("\r\n" + exStack.StackTrace);

            MessageBox.Show(sb.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 返回Nullable所对应的泛型。如果不是Nullable泛型，则返回null。
        /// </summary>
        /// <param name="typeIn"></param>
        /// <returns></returns>
        public static Type GetNullableGenericArgurment(Type typeIn)
        {
            // We need to check whether the property is NULLABLE
            if (typeIn.IsGenericType && typeIn.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                return typeIn.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }
        }

        #region 与 DllDirectory 的设置相关的API函数


        /// <summary>
        /// 将指定的文件夹添加到此程序的DLL文件的搜索路径中。
        /// 这个函数只用来在开发时用于AddinManager调试之用，在最终的Release版本中，此函数中的内容可以直接删除。
        /// </summary>
        /// <param name="SearchPath">要添加的文件夹路径</param>
        /// <remarks></remarks>
        public static void SetDllDirectory(string SearchPath)
        {
            if (!SetDllDirectoryW(SearchPath))
            {
                throw new ArgumentException("无法将路径\" " + SearchPath + " \" 添加到DLL的索引路径中！");
            }
        }

        /// <summary>
        /// 将指定的文件夹添加到此程序的DLL文件的搜索路径中.
        /// adds a directory to the search path used to locate DLLs for the application.
        /// </summary>
        /// <param name="lpPathName">要添加的文件夹路径</param>
        /// <remarks>Pretty straight-forward to use. Obviously, is usually going to be called before calling LoadLibraryEx().
        /// 另外,在PInvoke中,只有SetDllDirectory这个函数,但是它的真实的名称是SetDllDirectoryW.</remarks>
        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool SetDllDirectoryW(string lpPathName);

        /// <summary>
        /// 装载指定的动态链接库，并为当前进程把它映射到地址空间。一旦载入，就可以访问库内保存的资源。一旦不需要，用FreeLibrary函数释放DLL
        /// </summary>
        /// <param name="lpFileName">指定要载入的动态链接库的名称。采用与CreateProcess函数的lpCommandLine参数指定的同样的搜索顺序</param>
        /// <param name="hReservedNull">未用，设为零</param>
        /// <param name="dwFlags"></param>
        /// <returns>成功则返回库模块的句柄，零表示失败。会设置GetLastError</returns>
        /// <remarks>参考 http://www.pinvoke.net/default.aspx/kernel32/LoadLibraryEx.html .
        /// If you only want to load resources from the library, specify LoadLibraryFlags.LoadLibraryAsDatafile as dwFlags.
        /// In this case, nothing is done to execute or prepare to execute the mapped file.</remarks>
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        /// <summary>
        /// 用在 LoadLibraryEx 函数中
        /// </summary>
        /// <remarks></remarks>
        [Flags]
        public enum LoadLibraryFlags
        {
            /// <summary> 不对DLL进行初始化，仅用于NT </summary>
            DONT_RESOLVE_DLL_REFERENCES = 0x1,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x10,

            /// <summary> 不准备DLL执行。如装载一个DLL只是为了访问它的资源，就可以改善一部分性能 </summary>
            LOAD_LIBRARY_AS_DATAFILE = 0x2,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x40,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20,

            /// <summary> 指定搜索的路径 </summary>
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x8
        }
        #endregion
    }
}
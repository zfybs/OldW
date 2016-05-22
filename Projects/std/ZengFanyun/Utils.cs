// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Linq;
// End of VB project level imports

using System.Text.RegularExpressions;
using System.Runtime.InteropServices;


namespace std_ez
{
	
	/// <summary>
	/// 提供一些基础性的操作工具
	/// </summary>
	/// <remarks></remarks>
	public class Utils
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
		public static void ShowEnumerableP(IEnumerable V, string PropertyName, string Title = "集合中的元素")
		{
			string str = "";
			Type tp = default(Type);
			System.Reflection.MethodInfo MdInfo = default(System.Reflection.MethodInfo);
			string res = "";
			foreach (object obj in V)
			{
				tp = obj.GetType();
				MdInfo = tp.GetProperty(PropertyName).GetMethod;
				res = System.Convert.ToString(MdInfo.Invoke(obj, null).ToString());
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
		public static void ShowEnumerableF(IEnumerable V, string FieldName, string Title = "集合中的元素")
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
		/// 将字符转换为日期。除了.NET能够识别的日期格式外，
		/// 还增加了20160406（ 即 2016/04/06），以及 201604061330（即 2016/04/06 13:30）
		/// </summary>
		/// <param name="text">要转换为日期的字符。</param>
		/// <returns></returns>
		public static bool String2Date(string text, ref Nullable<DateTime> ResultedDate)
		{
			bool blnSucceed = false;
			// 模式1. 正常的日期格式
		    DateTime outDateTime;
			if (DateTime.TryParse(text, out outDateTime))
			{
			    ResultedDate = outDateTime;
                return true;
			}
			
			// 模式2. 20160406 ， 即 2016/04/06
			if (text.Length == 8)
			{
				try
				{
					ResultedDate = new DateTime(int.Parse(text.Substring(0, 4)),
						int.Parse(text.Substring(4, 2)),
						int.Parse(text.Substring(6, 2)));
					return true;
				}
				catch (Exception)
				{
					ResultedDate = null;
					return false;
				}
			}
			
			// 模式3. 201604061330 ， 即 2016/04/06 13:30
			if (text.Length == 12)
			{
				try
				{
					ResultedDate = new DateTime(int.Parse(text.Substring(0, 4)),
						int.Parse(text.Substring(4, 2)),
						int.Parse(text.Substring(6, 2)),
						int.Parse(text.Substring(8, 2)),
						int.Parse(text.Substring(10, 2)), 0);
					return true;
				}
				catch (Exception)
				{
					ResultedDate = null;
					return false;
				}
			}
			return blnSucceed;
		}
		
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
				throw (new ArgumentException("无法将路径\" " + SearchPath + " \" 添加到DLL的索引路径中！"));
			}
		}
		/// <summary>
		/// 将指定的文件夹添加到此程序的DLL文件的搜索路径中.
		/// adds a directory to the search path used to locate DLLs for the application.
		/// </summary>
		/// <param name="lpPathName">要添加的文件夹路径</param>
		/// <remarks>Pretty straight-forward to use. Obviously, is usually going to be called before calling LoadLibraryEx().
		/// 另外,在PInvoke中,只有SetDllDirectory这个函数,但是它的真实的名称是SetDllDirectoryW.</remarks>
		[DllImport("kernel32.dll", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
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
		[DllImport("kernel32.dll")]public static  extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);
		
		/// <summary>
		/// 用在 LoadLibraryEx 函数中
		/// </summary>
		/// <remarks></remarks>
		[System.Flags]public enum LoadLibraryFlags
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
		
	}
}

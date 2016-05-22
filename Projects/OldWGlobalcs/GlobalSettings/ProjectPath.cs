// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Linq;
// End of VB project level imports

//using Autodesk.Revit.DB;
	//using Autodesk.Revit.UI;
		using System.IO;
using Autodesk.Revit.UI;


namespace OldW.GlobalSettings
{
	
	/// <summary>
	/// 项目中各种文件的路径
	/// </summary>
	/// <remarks></remarks>
	public class ProjectPath
	{
		public ProjectPath()
		{
			// VBConversions Note: Non-static class variable initialization is below.  Class variables cannot be initially assigned non-static values in C#.
			Path_Solution = new DirectoryInfo(Path_Dlls).Parent.FullName;
			Path_icons = Path.Combine(new DirectoryInfo(Path_Dlls).Parent.FullName, "Resources\\icons");
			Path_Projects = Path.Combine(new DirectoryInfo(Path_Dlls).Parent.FullName, "Projects");
			Path_family = Path.Combine(new DirectoryInfo(Path_Dlls).Parent.FullName, "Resources\\Family");
			Path_SharedParametersFile = Path.Combine(Path.Combine(new DirectoryInfo(Path_Dlls).Parent.FullName, "Resources\\Family"), "global.txt");
			Path_data = Path.Combine(new DirectoryInfo(Path_Dlls).Parent.FullName, "Resources\\Data");
			Path_WarningValueUsing = Path.Combine(Path.Combine(new DirectoryInfo(Path_Dlls).Parent.FullName, "Resources\\Data"), "WarningValueUsing.dat");
			}
		/// <summary>
		/// Application的Dll所对应的路径，也就是“bin”文件夹的目录。
		/// </summary>
		/// <remarks>等效于：Dim thisAssemblyPath As String = System.Reflection.Assembly.GetExecutingAssembly().Location</remarks>
private static string Path_Dlls
		{
			get
			{
				// 在最终的发布中，Path_Dlls的值是等于“ My.Application.Info.DirectoryPath”的，但是，在调试过程中，如果使用Revit的AddinManager插件进行调试，
				// 则在每次调试时，AddinManager都会将对应的dll复制到一个新的临时文件夹中，如果将此复制后的dll的路作为Path_Dlls的值，那么其后面的Path_Solution等路径都是无效路径了。
				if (Directory.Exists("F:\\Software\\Revit\\RevitDevelop\\OldW\\bin"))
				{
					return "F:\\Software\\Revit\\RevitDevelop\\OldW\\bin";
				}
				else
				{
					return System.Reflection.Assembly.GetExecutingAssembly().Location;
                    // 等效于：Dim thisAssemblyPath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
                    // 等效于：(new Microsoft.VisualBasic.ApplicationServices.ConsoleApplicationBase()).Info.DirectoryPath
                }
            }
		}
		/// <summary>
		/// Solution文件所在的文件夹
		/// </summary>
		private static string Path_Solution; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		/// <summary>
		/// 存放图标的文件夹
		/// </summary>
		private static string Path_icons; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		/// <summary>
		/// 存放不同项目的文件夹
		/// </summary>
		private static string Path_Projects; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		
		
		/// <summary>
		/// 存放族与族共享参数的文件夹
		/// </summary>
		public static string Path_family; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		/// <summary>
		///   族共享参数的文件的绝对路径（不是文件夹的路径）。
		/// </summary>
		/// <remarks>先通过application.SharedParametersFilename来设置当前的SharedParametersFilename的路径，
		/// 然后通过application.OpenSharedParameterFile方法来返回对应的DefinitionFile对象。</remarks>
		public static string Path_SharedParametersFile; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		/// <summary>
		/// 存放数据文件
		/// </summary>
		/// <remarks></remarks>
		public static string Path_data; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		
		/// <summary>
		/// 监测警戒规范的绝对文件路径
		/// </summary>
		public static string Path_WarningValueUsing; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
		
	}
	
	
	
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using std_ez;

namespace rvtTools_ez
{
	
	/// <summary>
	/// Revit中的一些常规性操作工具
	/// </summary>
	/// <remarks></remarks>
	public class rvtTools
	{
		
		/// <summary>
		/// 获取外部共享参数文件中的参数组“OldW”，然后可以通过DefinitionGroup.Definitions.Item(name As String)来索引其中的共享参数，
		/// 也可以通过DefinitionGroup.Definitions.Create(name As String)来创建新的共享参数。
		/// </summary>
		/// <param name="App"></param>
		public static DefinitionGroup GetOldWDefinitionGroup(Autodesk.Revit.ApplicationServices.Application App)
		{
			
			// 打开共享文件
			string OriginalSharedFileName = App.SharedParametersFilename; // Revit程序中，原来的共享文件路径
			App.SharedParametersFilename = ProjectPath.Path_SharedParametersFile;
			DefinitionFile myDefinitionFile = App.OpenSharedParameterFile(); // 如果没有找到对应的文件，则打开时不会报错，而是直接返回Nothing
			App.SharedParametersFilename = OriginalSharedFileName; // 将Revit程序中的共享文件路径还原，以隐藏插件程序中的共享文件路径
			
			if (myDefinitionFile == null)
			{
				throw (new NullReferenceException("The specified shared parameter file \"" + ProjectPath.Path_SharedParametersFile + "\" is not found!"));
			}
			
			// 索引到共享文件中的指定共享参数
			DefinitionGroups myGroups = myDefinitionFile.Groups;
			DefinitionGroup myGroup = myGroups.get_Item(Constants.SP_GroupName);
			return myGroup;
		}
		
#region    ---   搜索文档中的元素
		
		/// <summary>
		/// Helper function: find a list of element with the given Class, Name and Category (optional).
		/// </summary>
		/// <remarks></remarks>
		public static IList<Element> FindElements(Document rvtDoc, Type 
			targetType, BuiltInCategory targetCategory = BuiltInCategory.INVALID, string targetName = null)
		{
			
			//'  first, narrow down to the elements of the given type and category
			var collector = new FilteredElementCollector(rvtDoc).OfClass(targetType);
			
			// 是否要按类别搜索
			if (!(targetCategory == BuiltInCategory.INVALID))
			{
				collector.OfCategory(targetCategory);
			}
			
			// 是否要按名称搜索
			if (targetName != null)
			{
				//'  using LINQ query here.
				var elems = from element in collector 
					where element.Name.Equals(targetName) 
					select element;
				
				//'  put the result as a list of element for accessibility.
				return elems.ToList();
			}
			return collector.ToElements();
		}
		
		/// <summary>
		///  Helper function: find a list of element with the given Class, Name and Category (optional).
		/// </summary>
		/// <param name="rvtDoc">要进行搜索的Revit文档</param>
		/// <param name="SourceElements">要从文档中的哪个集合中来进行搜索</param>
		/// <param name="targetType"></param>
		/// <param name="targetCategory"></param>
		/// <param name="targetName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static IList<Element> FindElements(Document rvtDoc, ICollection<ElementId> SourceElements, Type 
			targetType, BuiltInCategory targetCategory = BuiltInCategory.INVALID, string targetName = null)
		{
			
			
			var collector = new FilteredElementCollector(rvtDoc, SourceElements);
			
			// 搜索类型
			collector.OfClass(targetType);
			
			// 是否要搜索类别
			if (!(targetCategory == BuiltInCategory.INVALID))
			{
				collector.OfCategory(targetCategory);
			}
			
			// 是否要搜索名称
			if (targetName != null)
			{
				IEnumerable<Element> elems = default(IEnumerable<Element>);
				//'  parse the collection for the given names
				//'  using LINQ query here.
				elems = from element in collector 
					where element.Name.Equals(targetName) 
					select element;
				return elems.ToList();
			}
			
			return collector.ToElements();
		}
		
		public static Element FindElement(Document rvtDoc, Type 
			targetType, BuiltInCategory targetCategory = BuiltInCategory.INVALID, string targetName = null)
		{
			
			//'  find a list of elements using the overloaded method.
			IList<Element> elems = FindElements(rvtDoc, targetType, targetCategory, targetName);
			
			//'  return the first one from the result.
			if (elems.Count > 0)
			{
				return elems[0];
			}
			return null;
		}
		public static Element FindElement(Document rvtDoc, ICollection<ElementId> SourceElements, Type 
			targetType, BuiltInCategory targetCategory = BuiltInCategory.INVALID, string targetName = null)
		{
			//'  find a list of elements using the overloaded method.
			IList<Element> elems = FindElements(rvtDoc, targetType, targetCategory, targetName);
			
			//'  return the first one from the result.
			if (elems.Count > 0)
			{
				return elems[0];
			}
			return null;
		}
		
#endregion
		
		/// <summary>
		/// 撤消 Revit 的操作
		/// </summary>
		public static void Undo()
		{
            UIntPtr ptr0=new UIntPtr(0);
            // 第一步，先取消当前的所有操作
            // 在Revit UI界面中退出绘制，即按下ESCAPE键
            WindowsUtil.keybd_event((byte) 27, (byte) 0, 0, ptr0); // 按下 ESCAPE键
			WindowsUtil.keybd_event((byte) 27, (byte) 0, 0x2, ptr0); // 按键弹起
			
			// 第二步，按下 Ctrl + Z
			// 在Revit UI界面中退出绘制
			WindowsUtil.keybd_event((byte) 17, (byte) 0, 0, ptr0); // 按下 Control 键
			WindowsUtil.keybd_event((byte) 90, (byte) 0, 0, ptr0); // 按下 Z 键
			
			WindowsUtil.keybd_event((byte) 90, (byte) 0, 2, ptr0);
			WindowsUtil.keybd_event((byte) 17, (byte) 0, 2, ptr0); // 按键弹起
			
		}
	}
}

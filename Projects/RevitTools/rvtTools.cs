using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using stdOldW.WinFormHelper;

namespace rvtTools
{
	/// <summary>
	/// Revit中的一些常规性操作工具
	/// </summary>
	/// <remarks></remarks>
	public static class RvtTools
	{
		
		/// <summary>
		/// 获取外部共享参数文件中的参数组“OldW”，然后可以通过DefinitionGroup.Definitions.Item(name As String)来索引其中的共享参数，
		/// 也可以通过DefinitionGroup.Definitions.Create(name As String)来创建新的共享参数。
		/// </summary>
		/// <param name="app"></param>
		public static DefinitionGroup GetOldWDefinitionGroup(Autodesk.Revit.ApplicationServices.Application app)
		{
            // 打开共享文件
            string OriginalSharedFileName = app.SharedParametersFilename; // Revit程序中，原来的共享文件路径

            // 将新共享文件赋值给Revit
            app.SharedParametersFilename = ProjectPath.Path_SharedParametersFile;
            DefinitionFile myDefinitionFile = app.OpenSharedParameterFile(); // 如果没有找到对应的文件，则打开时不会报错，而是直接返回Nothing
            app.SharedParametersFilename = OriginalSharedFileName; // 将Revit程序中的共享文件路径还原，以隐藏插件程序中的共享文件路径

            if (myDefinitionFile == null)
            {
                throw (new NullReferenceException("The specified shared parameter file \"" + ProjectPath.Path_SharedParametersFile + "\" is not found!"));
            }

            // 索引到共享文件中的指定共享参数
            DefinitionGroups myGroups = myDefinitionFile.Groups;
            DefinitionGroup myGroup = myGroups.get_Item(Constants.SP_GroupName);
            return myGroup;
        }

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

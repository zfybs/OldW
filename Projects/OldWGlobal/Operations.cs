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

//using Autodesk.Revit.UI;
	//using Autodesk.Revit.DB;
		//using Autodesk.Revit.UI.Selection;
			//using Autodesk.Revit.DB.Architecture;
				using OldW.GlobalSettings;
using System.Threading;
using Autodesk.Revit.UI;


namespace OldW.GlobalSettings
{
	public class Operations
	{
		
		/// <summary>
		/// 将程序插件中的所有面板禁用
		/// </summary>
		public static void DeActivateControls(UIApplication uiApp)
		{
			var Panels = uiApp.GetRibbonPanels(Constants.AppName);
			foreach (RibbonPanel p in Panels)
			{
				p.Enabled = false;
			}
		}
		
		/// <summary>
		/// 将程序插件中的所有面板激活
		/// </summary>
		public static void ActivateControls(UIApplication uiApp)
		{
			var Panels = uiApp.GetRibbonPanels(Constants.AppName);
			foreach (RibbonPanel p in Panels)
			{
				p.Enabled = true;
			}
		}
		
	}
}

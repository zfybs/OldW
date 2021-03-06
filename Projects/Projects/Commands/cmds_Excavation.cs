using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitStd;
using DllActivator;

namespace OldW.Commands
{
	
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_Excavation : IExternalCommand
		{
		
		private static OldW.Excavation.frm_DrawExcavation Frm;
		
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			
			DllActivator_Projects dat = new DllActivator_Projects();
			dat.ActivateReferences();
			
			UIApplication uiApp = commandData.Application;
			Document doc = uiApp.ActiveUIDocument.Document;

			//
			OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
		    OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc);// OldWDocument.SearchOrCreate(WApp, doc);
			//
			OldW.Excavation.ExcavationDoc ExcavDoc = new OldW.Excavation.ExcavationDoc(WDoc);
			
			if (Frm == null || Frm.IsDisposed)
			{
				Frm = new OldW.Excavation.frm_DrawExcavation(ExcavDoc);
			}
			Frm.Show(null);
			
			//
			return Result.Succeeded;
		}
		
	}
	
	/// <summary> 提取模型中的开挖土体信息 </summary>
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_ExcavationInfo : IExternalCommand
		{
		
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
			dat.ActivateReferences();
			
			UIApplication uiApp = commandData.Application;
			Document doc = uiApp.ActiveUIDocument.Document;
			//
            
			OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
			OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc);// OldWDocument.SearchOrCreate(WApp, doc);
			//
			OldW.Excavation.ExcavationDoc ExcavDoc = new OldW.Excavation.ExcavationDoc(WDoc);
			
			OldW.Excavation.frm_ExcavationInfo frm = new OldW.Excavation.frm_ExcavationInfo(ExcavDoc);
			frm.Show(null);
			
			
			return Result.Succeeded;
			//
			
		}
		
	}
	
	
}

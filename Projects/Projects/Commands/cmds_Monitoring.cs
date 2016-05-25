// VBConversions Note: VB project level imports
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.Instrumentations;

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports


namespace OldW.Commands
{
	
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]public class cmd_ElementDataManager : IExternalCommand
		{
		
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Document doc = uiApp.ActiveUIDocument.Document;
			ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();

		    ICollection<Instrumentation> ins = Instrumentation.Lookup(doc, eleIds);
            OldW.DataManager.ElementDataManager frm = new OldW.DataManager.ElementDataManager(ins, doc);
			frm.ShowDialog();
			return Result.Succeeded;
		}
		
	}
	
}


// VBConversions Note: VB project level imports

using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Application = Autodesk.Revit.ApplicationServices.Application;

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;

// End of VB project level imports

//using Autodesk.Revit.ApplicationServices;
//using Autodesk.Revit.DB.Events;


namespace OldW.Commands
		{
			
			[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]public class cmd_Test : IExternalCommand
				{
				private UIApplication uiApp;
		private UIDocument uiDoc;
		private Application App;
		
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			App = commandData.Application.Application;
			uiDoc = commandData.Application.ActiveUIDocument;
			
			var a = uiDoc.Selection.PickObjects(ObjectType.Element);
			MessageBox.Show(System.Convert.ToString(a.Count));
			return Result.Cancelled;
		}
		
		
	}
}


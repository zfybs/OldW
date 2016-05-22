// VBConversions Note: VB project level imports

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports

//using OldW.Instrumentation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace OldW.Commands
{
	
	
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_Analyze : IExternalCommand
		{
		
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			OldWApplication WApp = OldWApplication.Create(uiApp.Application);
			OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, uiApp.ActiveUIDocument.Document);
			
			Document doc = uiApp.ActiveUIDocument.Document;
			//
			Element inclineEle = doc.GetElement(new ElementId(460115));
			OldW.Instrumentation.Instrum_Incline Incline = new OldW.Instrumentation.Instrum_Incline((FamilyInstance)inclineEle);
			//
			FamilyInstance eleEarht = (FamilyInstance) doc.GetElement(new ElementId(460116));
			OldW.Excavation.ExcavationDoc exca = new OldW.Excavation.ExcavationDoc(WDoc);
			
			OldW.Soil.Soil_Model soil = exca.FindSoilModel();
			Incline.FindAdjacentEarthElevation(soil.Soil);
			
			return Result.Succeeded;
		}
		}
	
	/// <summary> 查看指定日期的开挖工况 </summary>
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_ViewStage : IExternalCommand
		{
		
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Document doc = uiApp.ActiveUIDocument.Document;
			//
			
			OldWApplication WApp = OldWApplication.Create(uiApp.Application);
			OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, doc);
			//
			ConstructionReview f = new ConstructionReview();
			f.Show(null);
			
			return Result.Succeeded;
			//
			
		}
		
	}
}


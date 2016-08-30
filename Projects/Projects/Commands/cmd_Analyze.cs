using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.DynamicStages;
using OldW.Excavation;
using OldW.Instrumentations;

namespace OldW.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class cmd_Analyze : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(uiApp.ActiveUIDocument.Document);//OldWDocument.SearchOrCreate(WApp, uiApp.ActiveUIDocument.Document);

            Document doc = uiApp.ActiveUIDocument.Document;
            //
            Element inclineEle = doc.GetElement(new ElementId(460115));
            Instrum_WallIncline Incline = new Instrum_WallIncline((FamilyInstance) inclineEle);
            //
            FamilyInstance eleEarht = (FamilyInstance) doc.GetElement(new ElementId(460116));
            ExcavationDoc exca = new ExcavationDoc(WDoc);

            Soil_Model soil = exca.FindSoilModel();
            Incline.FindAdjacentEarthElevation(soil.Soil);

            return Result.Succeeded;
        }
    }
}
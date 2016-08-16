using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.DynamicReview;
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
            OldWApplication WApp = OldWApplication.Create(uiApp.Application);
            OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, uiApp.ActiveUIDocument.Document);

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

    /// <summary> 查看指定日期的开挖工况 </summary>
    [Transaction(TransactionMode.Manual)]
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.Excavation;
using eZstd;
using eZstd.Miscellaneous;

namespace OldW.Commands
{
    /// <summary> 修复开挖土体对模型土体的剪切关系，并将其放置到同一个组中。 </summary>
    /// <remarks>
    /// 修改所有开挖土体对于基坑土体的剪切关系。
    /// 在某些情况下（比如用新的模型土体替换了旧的模型土体时），
    /// 开挖土体可能会失去对于模型土体的剪切关系，以致于开挖土体虽然位于模型土体的组中，但是模型土体并没有被切割。
    /// </remarks>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_ExcavSoilReCut : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
            dat.ActivateReferences();

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            //
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc); // OldWDocument.SearchOrCreate(WApp, doc);
            //
            ExcavationDoc ExcavDoc = new OldW.Excavation.ExcavationDoc(WDoc);
            //

            Soil_Model sm = ExcavDoc.FindSoilModel();
            var excavSoils = ExcavDoc.FindExcavSoils(sm);
            //
            using (Transaction tran = new Transaction(doc, "重新设置土体剪切"))
            {
                try
                {
                    tran.Start();
                    //
                    sm.RemoveSoils(tran, excavSoils);
                    //
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.RollBack();
                    message = ex.Message;
                    // DebugUtils.ShowDebugCatch(ex, "重新设置土体剪切时出错");
                    return Result.Failed;
                }
            }
            return Result.Succeeded;
        }

    }

    /// <summary> 将模型土体或者开挖土体族中，没有对应实例的那些族及对应的族类型删除 </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_DeleteRedundantExcavations : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
            dat.ActivateReferences();

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            //
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc); // OldWDocument.SearchOrCreate(WApp, doc);
            //
            ExcavationDoc ExcavDoc = new OldW.Excavation.ExcavationDoc(WDoc);
            //
            using (Transaction tran = new Transaction(doc, "删除没有实例的土体族及对应的族类型"))
            {
                try
                {
                    tran.Start();
                    //
                    ExcavDoc.DeleteEmptySoilFamily(tran);
                    //
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.RollBack();
                    message = DebugUtils.GetDebugMessage(ex, "重新设置土体剪切时出错");
                    return Result.Failed;
                }
            }
            return Result.Succeeded;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.Excavation;
using OldW.Instrumentations;

namespace OldW.Commands
{

    /// <summary> 查看Revit的Panel中指定的某一天的开挖工况 </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_ShowOldWModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
            dat.ActivateReferences();
            //
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            //

            //
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument oldWDoc = WApp.SearchOrCreateOldWDocument(doc);// OldWDocument.SearchOrCreate(WApp, doc);
            ExcavationDoc exDoc = new ExcavationDoc(oldWDoc);

            // 所有最终要显示出来的土体与测点单元
            List<ElementId> oldWElementIds = new List<ElementId>();

            // 搜索 模型土体与开挖土体
            var soilm = exDoc.FindSoilModel();
            if (soilm != null)
            {
                oldWElementIds.Add(soilm.Soil.Id);

                // 开挖土体
                var soile = exDoc.FindExcavSoils(soilm);
                oldWElementIds.AddRange(soile.Select(r => r.Soil.Id));
            }

            // 搜索所有的测点单元
            ICollection<Instrumentation> ins = Instrumentation.Lookup(doc);
            oldWElementIds.AddRange(ins.Select(r => r.Monitor.Id));

            // 

            using (Transaction tranDoc = new Transaction(doc, "显示 OldW 基坑模型"))
            {
                try
                {
                    tranDoc.Start();
                    //
                    View v = uiApp.ActiveUIDocument.ActiveView;
                    v.UnhideElements(oldWElementIds);

                    //
                    tranDoc.Commit();
                }
                catch (Exception ex)
                {
                    tranDoc.RollBack();
                    message = ex.Message;
                    return Result.Failed;
                }
            }



            return Result.Succeeded;
        }
    }
}

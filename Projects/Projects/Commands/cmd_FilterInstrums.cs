using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DllActivator;
using OldW.Instrumentations;
using OldW.Instrumentations.MonitorSetterGetter;

namespace OldW.Commands
{
    /// <summary>
    /// 监测数据管理系统
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_FilterInstrums : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();
            //
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc);//OldWDocument.SearchOrCreate(WApp, doc);
            InstrumDoc insDoc = new InstrumDoc(WDoc);


            ICollection<Instrumentation> ins;
            //
            ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();
            //
            // 如果没有执行任何单元，则从整个文档中进行搜索 
            ins = eleIds.Count == 0
                ? Instrumentation.Lookup(doc)
                : Instrumentation.Lookup(doc, eleIds);

            PickInstrums frm = new PickInstrums(ins, insDoc);
            var res = frm.ShowDialog();
            //
            if (res == DialogResult.OK)
            {
                uiApp.ActiveUIDocument.Selection.SetElementIds(frm.FilterdInstrumentations);
            }
            return Result.Succeeded;
        }
    }
}

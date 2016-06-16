#region

using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.DataManager;
using OldW.DllActivator;
using OldW.Instrumentations;

#endregion

namespace OldW.Commands
{
    /// <summary>
    /// 监测数据管理系统
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_DataManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();
            //
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            OldWApplication WApp = OldWApplication.Create(uiApp.Application);
            OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, doc);
            InstrumDoc insDoc = new InstrumDoc(WDoc);


            ICollection<Instrumentation> ins;
            //
            ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();
            //
            // 如果没有执行任何单元，则从整个文档中进行搜索 
            ins = eleIds.Count == 0
                ? Instrumentation.Lookup(doc)
                : Instrumentation.Lookup(doc, eleIds);
            
            ElementDataManager frm = new ElementDataManager(ins, insDoc);
            frm.ShowDialog();
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// 从Excel中导入监测数据
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_DataImport : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();
            //
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            OldWApplication WApp = OldWApplication.Create(uiApp.Application);
            OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, doc);
            InstrumDoc insDoc = new InstrumDoc(WDoc);

            // 
            ICollection<Instrumentation> ins;
            ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();

            //
            // 如果没有执行任何单元，则从整个文档中进行搜索 
            ins = eleIds.Count == 0 
                ? Instrumentation.Lookup(doc) 
                : Instrumentation.Lookup(doc, eleIds);

            DataImport frm = new DataImport(ins, insDoc);
            frm.ShowDialog();

            return Result.Succeeded;
        }
    }

    /// <summary>
    /// 将数据导出到Excel
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_DataExport : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();
            //
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();

            return Result.Succeeded;
        }
    }
}
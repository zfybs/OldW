using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DllActivator;
using OldW.DataManager;
using DllActivator;
using OldW.Instrumentations;



namespace OldW.Commands
{
    /// <summary>
    /// ������ݹ���ϵͳ
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
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc);// OldWDocument.SearchOrCreate(WApp, doc);
            InstrumDoc insDoc = new InstrumDoc(WDoc);

            ICollection<Instrumentation> ins;
            //
            ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();
            //
            // ���û��ִ���κε�Ԫ����������ĵ��н������� 
            ins = eleIds.Count == 0
                ? Instrumentation.Lookup(doc)
                : Instrumentation.Lookup(doc, eleIds);

            ElementDataManager frm = new ElementDataManager(ins, insDoc);
            frm.ShowDialog();
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// ��Excel�е���������
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
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc);// OldWDocument.SearchOrCreate(WApp, doc);
            InstrumDoc insDoc = new InstrumDoc(WDoc);

            // 
            ICollection<Instrumentation> ins;
            ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();

            //
            // ���û��ִ���κε�Ԫ����������ĵ��н������� 
            ins = eleIds.Count == 0
                ? Instrumentation.Lookup(doc)
                : Instrumentation.Lookup(doc, eleIds);
            if (ins.Any())
            {
                DataImport frm = new DataImport(ins, insDoc);
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show(@"�� Revit ģ����û���ҵ��κβ�㵥Ԫ�����ȷ��ò�㡣", "��ʾ", MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// �����ݵ�����Excel
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
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc);// OldWDocument.SearchOrCreate(WApp, doc);
            InstrumDoc insDoc = new InstrumDoc(WDoc);

            // 
            ICollection<Instrumentation> ins;
            ICollection<ElementId> eleIds = uiApp.ActiveUIDocument.Selection.GetElementIds();

            //
            // ���û��ִ���κε�Ԫ����������ĵ��н������� 
            ins = eleIds.Count == 0
                ? Instrumentation.Lookup(doc)
                : Instrumentation.Lookup(doc, eleIds);

            DataExport frm = new DataExport(ins, insDoc);
            frm.ShowDialog();
            //
            return Result.Succeeded;
        }
    }
}
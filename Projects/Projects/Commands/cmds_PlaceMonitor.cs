using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.DllActivator;
using OldW.Instrumentations;

namespace OldW.Commands
{
    /// <summary>
    /// 测斜管建模
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_SetFamilyIncli : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            //
            OldWApplication WApp = OldWApplication.Create(uiApp.Application);
            OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, doc);
            //
            InstrumDoc MonitorDoc = new InstrumDoc(WDoc);
            try
            {
                MonitorDoc.SetFamily(InstrumentationType.测斜);
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
    

    /// <summary>
    /// 沉降测点建模
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_SetFamilySettlement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            //
            OldWApplication WApp = OldWApplication.Create(uiApp.Application);
            OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, doc);
            //
            InstrumDoc MonitorDoc = new InstrumDoc(WDoc);
            try
            {
                MonitorDoc.SetFamily(InstrumentationType.地表隆沉);
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }

    /// <summary>
    /// 轴力测点建模
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_SetFamilyForce : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            //
            OldWApplication WApp = OldWApplication.Create(uiApp.Application);
            OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, doc);
            //
            InstrumDoc MonitorDoc = new InstrumDoc(WDoc);
            try
            {
                MonitorDoc.SetFamily(InstrumentationType.支撑轴力);
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }





}
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.DllActivator;
using OldW.Instrumentations;

namespace OldW.Commands
{

    /// <summary>
    /// 墙体测斜
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_PlaceWallIncline : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.墙体测斜);
        }
    }

    /// <summary>
    /// 土体测斜
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_PlaceSoilIncline : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.土体测斜);
        }
    }


    /// <summary>
    /// 墙顶位移
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_PlaceWallTop : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.墙顶位移);
        }
    }


    /// <summary>
    /// 地表隆沉
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_PlaceGroundSettlement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.地表隆沉);
        }
    }

    /// <summary>
    /// 立柱隆沉
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_PlaceColumnHeave : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.立柱隆沉);
        }
    }

    /// <summary>
    /// 支撑轴力
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_PlaceStrutForce : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.支撑轴力);
        }
    }

    /// <summary>
    /// 水位
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_PlaceWaterTable : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.水位);
        }
    }

    /// <summary>
    /// 其他点测点
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_PlaceOtherPoint : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.其他点测点);
        }
    }

    /// <summary>
    /// 其他线测点
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_PlaceOtherLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return PlaceMonitor.Execute(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.其他线测点);
        }
    }


    /// <summary>
    /// 放置任意类型的监测测点
    /// </summary>
    public static class PlaceMonitor
    {
        public static Result Execute(ref ExternalCommandData commandData, ref string message, ref ElementSet elements, InstrumentationType type)
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
                MonitorDoc.SetFamily(type);
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
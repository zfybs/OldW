using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using DllActivator;
using OldW.Instrumentations;
using OldW.Instrumentations.MonitorSetterGetter;

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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
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
            return PlaceMonitor.PlaceInstrumentation(
                commandData: ref commandData,
                message: ref message,
                elements: ref elements,
                type: InstrumentationType.其他线测点);
        }
    }
}
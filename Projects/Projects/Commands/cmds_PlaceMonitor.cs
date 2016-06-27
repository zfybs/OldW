using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using OldW.DllActivator;
using OldW.Instrumentations;
using stdOldW;

namespace OldW.Commands
{

    #region ---   放置不同的测点族实例

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

    #endregion

    /// <summary>
    /// 放置任意类型的监测测点
    /// </summary>
    public class PlaceMonitor
    {
        private static Autodesk.Revit.ApplicationServices.Application _app;
        private static bool _setNameAfterPlaced;
        private static UIApplication _uiApp;

        /// <summary> 放置各种类型的监测测点 </summary>
        public static Result PlaceInstrumentation(ref ExternalCommandData commandData, ref string message, ref ElementSet elements, InstrumentationType type)
        {
            DllActivator_Projects dat = new DllActivator_Projects();
            dat.ActivateReferences();

            _uiApp = commandData.Application;
            _app = _uiApp.Application;
            Document doc = _uiApp.ActiveUIDocument.Document;

            // 事件关联
            _app.DocumentChanged += ApplicationOnDocumentChanged;
            _uiApp.Idling += idling_RenameAddedInstrums;

            //
            OldWApplication WApp = OldWApplication.Create(_uiApp.Application);
            OldWDocument WDoc = OldWDocument.SearchOrCreate(WApp, doc);
            //
            InstrumDoc MonitorDoc = new InstrumDoc(WDoc);
            try
            {
                //
                instrumsToBeRenamed = new Dictionary<Instrumentation, string>();
                // 执行测点的放置操作
                MonitorDoc.SetFamily(type);
            }
            catch (Exception ex)
            {
                Utils.ShowDebugCatch(ex, "放置测点出错。");
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        private static void ApplicationOnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {

            if (e.GetAddedElementIds().Count + e.GetModifiedElementIds().Count + e.GetDeletedElementIds().Count == 0)
            {
                return;
            }
            var added = e.GetAddedElementIds();
            if (added.Count == 0)  // 每次放置测点的操作并不一定只添加一个Element。
            {
                _app.DocumentChanged -= ApplicationOnDocumentChanged;
                return;
            }

            var instrums = Instrumentation.Lookup(e.GetDocument(), added);
            if (instrums.Count != 1)
            {
                _app.DocumentChanged -= ApplicationOnDocumentChanged;
                return;
            }

            // MessageBox.Show( instrums[0].Monitor.Id.ToString());
            // 监测单元的初始化
            InitializeInstrumentation(instrums[0]);
        }

        /// <summary> 在闲置事件中对新添加的测点进行重命名。 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void idling_RenameAddedInstrums(object sender, IdlingEventArgs e)
        {
            // MessageBox.Show($"idle {instrumsToBeRenamed.Count}");
            if (instrumsToBeRenamed.Count > 0)
            {
                // 
                using (Transaction tran = new Transaction(((UIApplication)sender).ActiveUIDocument.Document, "测点重命名"))
                {
                    try
                    {
                        tran.Start();
                        foreach (var ins in instrumsToBeRenamed)
                        {
                            if (ins.Key.Monitor.IsValidObject)
                            {
                                ins.Key.SetMonitorName(tran, ins.Value);
                            }
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowDebugCatch(ex, "测点重命名出错");
                        // tran.RollBack();
                    }
                }
                instrumsToBeRenamed.Clear();
            }
            instrumsToBeRenamed = null;
            // 取消事件关联
            _uiApp.Idling -= idling_RenameAddedInstrums;
            _app.DocumentChanged -= ApplicationOnDocumentChanged;
        }

        private static Dictionary<Instrumentation, string> instrumsToBeRenamed;
        /// <summary>
        /// 监测单元的初始化
        /// </summary>
        /// <param name="ins"></param>
        private static void InitializeInstrumentation(Instrumentation ins)
        {
            ElementInitializer formInitializer = new ElementInitializer(ins.GetMonitorName());
            formInitializer.ShowDialog();

            if (ins.GetMonitorName() != formInitializer.MonitorName)
            {
                instrumsToBeRenamed.Add(ins, formInitializer.MonitorName);
            }
        }
    }

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
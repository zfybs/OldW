using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using OldW.Commands;
using OldW.DllActivator;
using eZstd;
using eZstd.Miscellaneous;

namespace OldW.Instrumentations.MonitorSetterGetter
{
    /// <summary> 放置任意类型的监测测点 </summary>
    public class PlaceMonitor
    {
        private static Autodesk.Revit.ApplicationServices.Application _app;
        private static bool _setNameAfterPlaced;
        private static UIApplication _uiApp;

        /// <summary> 放置各种类型的监测测点 </summary>
        /// <param name="type"> 要放置的测点类型 </param>
        public static Result PlaceInstrumentation(ref ExternalCommandData commandData, ref string message,
            ref ElementSet elements, InstrumentationType type)
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
            OldWApplication WApp = OldWApplication.GetUniqueApplication(_uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc);//OldWDocument.SearchOrCreate(WApp, doc);
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
                // DebugUtils.ShowDebugCatch(ex, "放置测点出错。");
                message = ex.Message + "\n\r" + ex.StackTrace;
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
                        DebugUtils.ShowDebugCatch(ex, "测点重命名出错");
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

}

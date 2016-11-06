using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forms = System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.DynamicStages;
using OldW.Excavation;
using OldW.GlobalSettings;
using RevitStd;
using eZstd;
using eZstd.Miscellaneous;
using TextBox = Autodesk.Revit.UI.TextBox;

namespace OldW.Commands
{

    /// <summary> 查看Revit的Panel中指定的某一天的开挖工况 </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_ViewStage : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
            dat.ActivateReferences();
            //
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            //

            DateTime? constructionTime = ReadTimeFromPanel(uiApp);
            if (!constructionTime.HasValue)
            {
                Forms.MessageBox.Show(@"请先在选项卡中设置有效的时间格式。比如 “ 2016/6/6 13:06 ”", "提示",
                    Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return Result.Cancelled;
            }

            //
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument oldWDoc = WApp.SearchOrCreateOldWDocument(doc);// OldWDocument.SearchOrCreate(WApp, doc);
            ExcavationDoc exDoc = new ExcavationDoc(oldWDoc);
            //
            // 对开挖土体按时间进行开挖状态的分类
            Soil_Model sm = exDoc.FindSoilModel();
            var excavSoilCollections = exDoc.FindExcavSoils(sm);

            View view = uiApp.ActiveUIDocument.ActiveGraphicalView;
            using (Transaction tranDoc = new Transaction(doc, "施工工况展示"))
            {
                try
                {
                    tranDoc.Start();

                    // 根据施工日期刷新模型
                    //
                    ReviewDoc reviewDoc = new ReviewDoc(oldWDoc, excavSoilCollections);
                    reviewDoc.ShowExcavation(tranDoc, constructionTime.Value, view);

                    //
                    tranDoc.Commit();
                }
                catch (Exception ex)
                {
                    tranDoc.RollBack();
                    DebugUtils.ShowDebugCatch(ex, "施工工况动态展示出错");
                }
            }
            
            return Result.Succeeded;
        }

        private DateTime? ReadTimeFromPanel(UIApplication uiApp)
        {
            TextBox txt = GlobalSettings.Operations.GetRibbonItem(uiApp, "工况展示", "CurrentDate") as TextBox;
            string tm = txt.Value.ToString();

            return DateTimeHelper.String2Date(tm);
        }
    }


    /// <summary> 手动查看指定日期的开挖工况 </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_ViewStageManually : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
            dat.ActivateReferences();
            //

            var wpf = new ViewStageManually(commandData.Application);
            wpf.Show(null);

            return Result.Succeeded;
        }
    }

    /// <summary> 自动动态查看指定日期的开挖工况 </summary>
    [Transaction(TransactionMode.Manual)]
    public class cmd_ViewStageDynamically : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
            dat.ActivateReferences();
            //

            //
            UIApplication uiApp = commandData.Application;
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument oldWDoc = WApp.SearchOrCreateOldWDocument(uiApp.ActiveUIDocument.Document);// OldWDocument.SearchOrCreate(WApp, doc);

            var wpf = new ViewStageDynamically(oldWDoc);
            wpf.Show(null);

            return Result.Succeeded;
        }
    }



}

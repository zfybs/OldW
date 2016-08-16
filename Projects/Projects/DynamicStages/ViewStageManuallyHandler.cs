using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.Excavation;
using stdOldW;

namespace OldW.DynamicStages
{
    internal class ViewStageManuallyHandler : IExternalEventHandler
    {
        #region ---   Fields

        private ReviewDoc _reviewDoc;
       
        #endregion

        #region ---   构造函数

        string IExternalEventHandler.GetName()
        {
            return "基坑群施工工况动态展示";
        }

        #endregion

        #region ---   Execute 的参数

        /// <summary> Execute 的参数 </summary>
        public object ExternalEventArgs;

        public void SetExcavSoilCollections(UIApplication uiApp)
        {
            Document doc = uiApp.ActiveUIDocument.Document;
            //
            _reviewDoc  = ReviewDoc.CreateFromActiveDocument(uiApp,doc);
        }

        #endregion

        #region ---   ConstructionReviewHandler.Execute

        void IExternalEventHandler.Execute(UIApplication uiApp)
        {
            Autodesk.Revit.DB.View view = uiApp.ActiveUIDocument.ActiveGraphicalView;
            Document doc = uiApp.ActiveUIDocument.Document;
            using (Transaction tranDoc = new Transaction(doc, "施工工况动态展示"))
            {
                try
                {
                    tranDoc.Start();

                    // 根据施工日期刷新模型
                    DateTime time = (DateTime)ExternalEventArgs;
                    OnConstructionDateChanged(tranDoc, view, time);

                    //
                    tranDoc.Commit();
                }
                catch (Exception ex)
                {
                    tranDoc.RollBack();
                    DebugUtils.ShowDebugCatch(ex, "施工工况动态展示出错");
                }
            }
        }

        /// <summary> 根据施工日期刷新模型 </summary>
        /// <param name="tranDoc"></param>
        /// <param name="uiApp"></param>
        /// <param name="constructionTime"></param>
        private void OnConstructionDateChanged(Transaction tranDoc, Autodesk.Revit.DB.View view, DateTime constructionTime)
        {
            //
            _reviewDoc.ShowExcavation(tranDoc, constructionTime, view);
        }

        #endregion
    }



}

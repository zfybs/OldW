using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using OldW.Excavation;

namespace OldW.DynamicStages
{
    /// <summary> 在基坑开挖模拟中，对 指定的开挖土体集合 进行不同时间下的动态开挖展示 </summary>
    /// <remarks></remarks>
    public class ReviewDoc : OldWDocument
    {
        /// <summary> 用来进行开挖预览的开挖土体单元 </summary>
        private readonly ICollection<Soil_Excav> _excavSoils;
        #region ---   Fields

        private Soil_Model _modelSoil;

        /// <summary> 用来进行开挖模拟 </summary>
        private ExcavationDoc _excavDoc;

        #endregion

        #region ---   构造函数

        /// <summary> 构造函数 </summary>
        /// <param name="oldWDoc"></param>
        /// <param name="excavSoils">用来进行开挖预览的开挖土体单元</param>
        public ReviewDoc(OldWDocument oldWDoc, ICollection<Soil_Excav> excavSoils)
            : base(oldWDoc.Document)
        {
            if (!excavSoils.Any())
            {
                throw new System.ArgumentException("开挖土体集合中至少要包含一个开挖土体单元。");
            }
            _excavDoc = new ExcavationDoc(this);
            _excavSoils = excavSoils;
            _modelSoil = excavSoils.First().ModelSoil;
        }


        /// <summary> 将指定的文档中的 所有开挖土体 都进行开挖工况的动画展示 </summary>
        /// <param name="uiApp"></param>
        /// <param name="doc">将会提取此文档中的所有开挖土体所组成的集合，来进行开挖工况的动画展示</param>
        /// <returns></returns>
        public static ReviewDoc CreateFromActiveDocument(UIApplication uiApp, Document doc)
        {
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument oldWDoc = WApp.SearchOrCreateOldWDocument(doc);//OldWDocument.SearchOrCreate(WApp, doc);
            ExcavationDoc exDoc = new ExcavationDoc(oldWDoc);
            //
            // 对开挖土体按时间进行开挖状态的分类
            Soil_Model sm = exDoc.FindSoilModel();
            var excavSoilCollections = exDoc.FindExcavSoils(sm);
            var reviewDoc = new ReviewDoc(oldWDoc, excavSoilCollections);

            return reviewDoc;
        }

        #endregion

        /// <summary>
        /// 根据指定的时间在Revit视图中显示出每一个开挖分块的开挖状态
        /// </summary>
        /// <param name="constructionTime"></param>
        /// <param name="view"></param>
        public void ShowExcavation(Transaction tranDoc, DateTime constructionTime, View view)
        {
            // 根据施工日期确定每一个开挖分块的开挖状态
            Dictionary<Soil_Excav, ExcavationStage> soilsStage = _excavDoc.FilterExcavSoils(_excavSoils, constructionTime);

            // 工况动态显示
            tranDoc.SetName("工况动态显示");

            // 先将临时隐藏的土体显示出来
            view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

            foreach (var ss in soilsStage)
            {
                Soil_Excav es = ss.Key;
                ExcavationStage stage = ss.Value;

                // 将每一个开挖土体根据其开挖状态来设置其显示样式
                es.RefreshByStage(tranDoc, view, stage);
            }
        }
    }
}

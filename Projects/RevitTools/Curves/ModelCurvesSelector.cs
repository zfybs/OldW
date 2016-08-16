using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace rvtTools.Curves
{
    /// <summary>
    /// 通过各种方法来构造出封闭的曲线
    /// </summary>
    public class ModelCurveSelector
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiDoc"> 进行模型线选择的那个文档 </param>
        public ModelCurveSelector(UIDocument uiDoc)
        {
            ModelCurveSelector with_1 = this;
            with_1.uiDoc = uiDoc;
        }

        #region    ---   Properties

        #endregion

        #region    ---   Fields

        protected UIDocument uiDoc;

        /// <summary>
        /// 模型线选择器中已经选择了的曲线
        /// </summary>
        private List<Curve> SelectedCurves;

        /// <summary>
        /// 模型线选择器中已经选择了的曲线的Id值
        /// </summary>
        private List<ElementId> SelectedCurvesId;

        #endregion

        #region    ---   在界面中选择出封闭的曲线

        /// <summary>
        /// 选择模型中的模型线
        /// </summary>
        /// <returns></returns>
        protected IList<Reference> SelectModelCurve()
        {
            IList<Reference> boundaries;
            try
            {
                boundaries = uiDoc.Selection.PickObjects(ObjectType.Element, new CurveSelectionFilter(),
                           "选择一组封闭的模型线。");
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message + "\n\r"+  ex.GetType().Name);
                boundaries = new List<Reference>();
            }

            return boundaries;
        }

        /// <summary>
        /// 曲线选择过滤器
        /// </summary>
        /// <remarks></remarks>
        private class CurveSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                bool bln = false;
                if (element is ModelCurve)
                {
                    return true;
                }
                return bln;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

        #endregion

        #region  扩展区：曲线连续性要求的检测 以及对应的界面响应

        #endregion
    }
}
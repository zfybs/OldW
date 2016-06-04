// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Linq;
// End of VB project level imports

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using stdOldW;


namespace rvtTools
{
	/// <summary>
	/// 通过各种方法来构造出封闭的曲线
	/// </summary>
	public class ModelCurveSelector
	{
		
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
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="uiDoc"> 进行模型线选择的那个文档 </param>
		public ModelCurveSelector(UIDocument uiDoc)
		{
			ModelCurveSelector with_1 = this;
			with_1.uiDoc = uiDoc;
		}
		
#region    ---   在界面中选择出封闭的曲线
		
		/// <summary>
		/// 选择模型中的模型线
		/// </summary>
		/// <returns></returns>
		protected IList<Reference> SelectModelCurve()
		{
			IList<Reference> boundaries = uiDoc.Selection.PickObjects(ObjectType.Element, new CurveSelectionFilter(), "选择一组封闭的模型线。");
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
		
		/// <summary>
		/// 从选择的Curve Elements中，获得连续排列的多段曲线（不一定要封闭）。
		/// </summary>
		/// <param name="doc">曲线所在文档</param>
		/// <param name="SelectedCurves">多条曲线元素所对应的Reference，可以通过Selection.PickObjects返回。
		/// 注意，SelectedCurves中每一条曲线都必须是有界的（IsBound），否则，其GetEndPoint会报错。</param>
		/// <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
		/// 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
		protected IList<Curve> GetContiguousCurvesFromSelectedCurveElements(Document doc, IList<Reference> SelectedCurves)
		{
			IList<Curve> curves = new List<Curve>();
			
			// Build a list of curves from the curve elements
			foreach (Reference reference in SelectedCurves)
			{
				CurveElement curveElement = doc.GetElement(reference) as CurveElement;
				curves.Add(curveElement.GeometryCurve.Clone());
			}
			//
			curves = CurvesFormator.GetContiguousCurvesFromCurves(curves);
			return curves;
		}
		
#endregion
		
#region  扩展区：曲线连续性要求的检测 以及对应的界面响应
		
#endregion
		
	}
	
}

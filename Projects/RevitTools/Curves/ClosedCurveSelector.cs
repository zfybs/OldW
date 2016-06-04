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
using Autodesk.Revit.UI;
using stdOldW;


namespace rvtTools
{
	/// <summary>
	/// 在Revit界面中选择出多个封闭的模型线
	/// </summary>
	public class ClosedCurveSelector : ModelCurveSelector
	{
		
		/// <summary>
		/// 是否要分次选择多个封闭的模型曲线链
		/// </summary>
		private bool MultipleClosed;
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="uiDoc">进行模型线选择的那个文档</param>
		/// <param name="Multiple"> 是否要分次选择多个封闭的模型曲线链</param>
		public ClosedCurveSelector(UIDocument uiDoc, bool Multiple) : base(uiDoc)
		{
			this.MultipleClosed = Multiple;
		}
		
		/// <summary>
		/// 开启同步操作：在Revit UI 界面中选择封闭的模型曲线链
		/// </summary>
		/// <returns></returns>
		public CurveArrArray SendSelect()
		{
			CurveArrArray Profiles = new CurveArrArray(); // 每一次创建开挖土体时，在NewExtrusion方法中，要创建的实体的轮廓
			bool blnStop = false;
			do
			{
				blnStop = true;
				CurveLoop cvLoop = GetLoopedCurve();
				// 检验并添加
				if (cvLoop.Count() > 0)
				{
					CurveArray cvArr = new CurveArray();
					foreach (Curve c in cvLoop)
					{
						cvArr.Append(c);
					}
					Profiles.Append(cvArr);
					
					// 是否要继续添加
					if (this.MultipleClosed)
					{
						DialogResult res = MessageBox.Show("曲线添加成功，是否还要继续添加？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (res == DialogResult.Yes)
						{
							blnStop = false;
						}
					}
				}
			} while (!blnStop);
			return Profiles;
		}
		
		/// <summary>
		/// 获取一组连续封闭的模型线
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		private CurveLoop GetLoopedCurve()
		{
			Document Doc = this.uiDoc.Document;
			//
			IList<Reference> Boundaries = base.SelectModelCurve();
			
			CurveLoop cvLoop = new CurveLoop();
			try
			{
				if (Boundaries.Count == 1) // 要么是封闭的圆或圆弧，要么就不封闭
				{
					Curve c = ((ModelCurve) (Doc.GetElement(Boundaries[0]))).GeometryCurve;
					if (((c is Arc) || (c is Ellipse)) && (!c.IsBound))
					{
						cvLoop.Append(c);
					}
					else
					{
						throw (new InvalidOperationException("选择的一条圆弧线或者椭圆线并不封闭。"));
					}
				}
				else
				{
					// 对于选择了多条曲线的情况
					IList<Curve> cs = GetContiguousCurvesFromSelectedCurveElements(Doc, Boundaries);
					if (cs != null)
					{
						foreach (Curve c in cs)
						{
							cvLoop.Append(c);
						}
					}
					else
					{
						// 显示出选择的每一条线的两个端点
						int nn = Boundaries.Count;
						Curve c = default(Curve);
						string[] cvs = new string[nn - 1 + 1];
						for (var i = 0; i <= nn - 1; i++)
						{
							c = (Doc.GetElement(Boundaries[i]) as CurveElement).GeometryCurve;
							cvs[i] = c.GetEndPoint(0).ToString() + c.GetEndPoint(1).ToString();
						}
						Utils.ShowEnumerable(cvs);
						
						throw (new InvalidOperationException("所选择的曲线不连续。"));
					}
					
					if (cvLoop.IsOpen())
					{
						throw (new InvalidOperationException("所选择的曲线不能形成封闭的曲线。"));
					}
					else if (!cvLoop.HasPlane())
					{
						throw (new InvalidOperationException("所选择的曲线不在同一个平面上。"));
					}
					else
					{
						return cvLoop;
					}
				}
			}
			catch (Exception ex)
			{
				DialogResult res = MessageBox.Show(ex.Message + " 点击是以重新选择，点击否以退出绘制。" + "\r\n" + "当前选择的曲线条数为：" + System.Convert.ToString(Boundaries.Count) + "条。" +
					"\r\n" + ex.StackTrace, "Warnning", MessageBoxButtons.OKCancel);
				if (res == DialogResult.OK)
				{
					cvLoop = GetLoopedCurve();
				}
				else
				{
					cvLoop = new CurveLoop();
					return cvLoop;
				}
			}
			return cvLoop;
		}
		
		///' <summary>
		///' 检测当前集合中的曲线是否符合指定的连续性要求
		///' </summary>
		///' <param name="curvesIn"> 用户选择的用于检测的曲线集合 </param>
		///' <param name="curvesOut"> 进行检测与重新排列后的新的曲线集合 </param>
		///' <returns></returns>
		//Private Function ValidateCurves(ByVal curvesIn As List(Of Curve), ByRef curvesOut As List(Of Curve)) As CurveCheckState
		//    curvesOut = Nothing
		//    Dim cs As CurveCheckState = CurveCheckState.Invalid_Exit
		
		//    ' 根据不同的模式进行不同的检测
		//    Select Case Me.CheckMode
		//        Case CurveCheckMode.Connected  ' 一条连续曲线链
		//            curvesOut = CurvesFormator.GetContiguousCurvesFromCurves(curvesIn)
		//            If curvesOut IsNot Nothing Then
		//                cs = CurveCheckState.Valid_Continue
		//            Else  ' 说明根本不连续
		//                cs = CurveCheckState.Invalid_InquireForUndo
		//            End If
		
		//        Case CurveCheckMode.Closed
		
		//            curvesOut = CurvesFormator.GetContiguousCurvesFromCurves(curvesIn)
		//            If curvesOut Is Nothing Then   ' 说明根本就不连续
		//                cs = CurveCheckState.Invalid_InquireForUndo
		//            Else  ' 说明起码是连续的
		//                If curvesOut.First.GetEndPoint(0).DistanceTo(curvesOut.Last.GetEndPoint(1)) < GeoHelper.VertexTolerance Then
		//                    ' 说明整个连续曲线是首尾相接，即是闭合的。此时就不需要再继续选择了
		//                    cs = CurveCheckState.Valid_Exit
		//                Else
		//                    ' 说明整个曲线是连续的，但是还没有闭合。此时应该重新选择
		//                    cs = CurveCheckState.Invalid_InquireForUndo
		//                End If
		//            End If
		
		//        Case CurveCheckMode.MultiClosed
		
		//            curvesOut = CurvesFormator.GetContiguousCurvesFromCurves(curvesIn)
		//            If curvesOut Is Nothing Then   ' 说明根本就不连续
		//                cs = CurveCheckState.Invalid_InquireForUndo
		//            Else  ' 说明起码是连续的
		//                If curvesOut.First.GetEndPoint(0).DistanceTo(curvesOut.Last.GetEndPoint(1)) < GeoHelper.VertexTolerance Then
		//                    ' 说明整个连续曲线是首尾相接，即是闭合的。此时应该询问用户是否要继续绘制新的闭合曲线
		//                    cs = CurveCheckState.Valid_InquireForContinue
		//                Else
		//                    ' 说明整个曲线是连续的，但是还没有闭合。此时应该重新选择
		//                    cs = CurveCheckState.Invalid_InquireForUndo
		//                End If
		//            End If
		
		
		//        Case CurveCheckMode.HorizontalPlan Or CurveCheckMode.Closed
		//            If CurvesFormator.IsInOnePlan(curvesIn, New XYZ(0, 0, 1)) Then
		
		//            End If
		//            Return False
		//        Case CurveCheckMode.Seperated
		//            ' 不用检测，直接符合
		//            cs = CurveCheckState.Valid_InquireForContinue
		//    End Select
		//    Return cs
		//End Function
		
	}
}

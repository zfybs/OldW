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
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using rvtTools_ez;
using std_ez;


namespace rvtTools_ez
{
	
	public partial class CurvesFormator
	{
		
		
#region  曲线连续性 的 实现方法
		
		/// <summary>
		/// 从指定的Curve集合中中，获得连续排列的多段曲线（不一定要封闭）。
		/// 此方法必须保证集合中的第一个元素为连续曲线链中的最左端的那一根曲线。
		/// </summary>
		/// <param name="curves">多条曲线元素所对应的集合
		/// 注意，curves 集合中每一条曲线都必须是有界的（IsBound），否则，其 GetEndPoint 会报错。</param>
		/// <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
		/// 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
		public static IList<Curve> GetContiguousCurvesFromCurves_OneDirection(IList<Curve> curves)
		{
			
			XYZ endPoint = default(XYZ); // 每一条线的终点，用来与剩下的线段的起点或者终点进行比较
			bool blnHasCont = false; // 此终点是否有对应的点与之对应，如果没有，则说明所有的线段中都不能形成连续的多段线
			
			// Walk through each curve (after the first) to match up the curves in order
			for (int ThisCurveId = 0; ThisCurveId <= curves.Count - 2; ThisCurveId++)
			{
				Curve ThisCurve = curves[ThisCurveId];
				blnHasCont = false;
				endPoint = ThisCurve.GetEndPoint(1); // 第i条线的终点
				Curve tmpCurve = curves[ThisCurveId + 1]; // 当有其余的曲线放置在当ThisCurveId + 1位置时，要将当前状态下的ThisCurveId + 1位置的曲线与对应的曲线对调。
				
				// 从剩下的曲线中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
				for (int NextCurveId = ThisCurveId + 1; NextCurveId <= curves.Count - 1; NextCurveId++)
				{
					
					// Is there a match end->start, if so this is the next curve
					if (GeoHelper.IsAlmostEqualTo(curves[NextCurveId].GetEndPoint(0), endPoint, GeoHelper.VertexTolerance))
					{
						blnHasCont = true;
						// 向上对换
						curves[ThisCurveId + 1] = curves[NextCurveId];
						curves[NextCurveId] = tmpCurve;
						continue;
						
						// Is there a match end->end, if so, reverse the next curve
					}
					else if (GeoHelper.IsAlmostEqualTo(curves[NextCurveId].GetEndPoint(1), endPoint, GeoHelper.VertexTolerance))
					{
						blnHasCont = true;
						// 向上对换
						curves[ThisCurveId + 1] = curves[NextCurveId].CreateReversed();
						if (NextCurveId != ThisCurveId + 1)
						{
							// 如果 NextCurveId = ThisCurveId + 1 ，说明 ThisCurveId + 1 就是接下来的那条线，只不过方向反了。
							// 这样就不可以将反转前的那条线放回去，而只需要执行上面的反转操作就可以了。
							curves[NextCurveId] = tmpCurve;
						}
						
						continue;
					}
					
				}
				if (!blnHasCont) // 说明不可能形成连续的多段线了
				{
					return default(IList<Curve>);
				}
			}
			
			return curves;
		}
		
		/// <summary>
		/// 从指定的Curve集合中中，获得连续排列的多段曲线（不一定要封闭）。如果不连续，则返回Nothing。
		/// </summary>
		/// <param name="curves">多条曲线元素所对应的集合
		/// 注意，curves 集合中每一条曲线都必须是有界的（IsBound），否则，其 GetEndPoint 会报错。</param>
		/// <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
		/// 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
		/// <remarks>GetContiguousCurvesFromCurves2与函数GetContiguousCurvesFromCurves2的功能完全相同，只是GetContiguousCurvesFromCurves1是
		/// 通过数值的方法来实现，而GetContiguousCurvesFromCurves2是通过类与逻辑的判断来实现。所以GetContiguousCurvesFromCurves1的执行速度可能会快一点点，
		/// 而GetContiguousCurvesFromCurves2的扩展性会好得多。</remarks>
		public static IList<Curve> GetContiguousCurvesFromCurves(IList<Curve> curves)
		{
			
			Dictionary<int, Curve> CurvesLeft = new Dictionary<int, Curve>();
			ContiguousCurves cc = default(ContiguousCurves);
			//
			if (curves.Count >= 1)
			{
				cc = new ContiguousCurves(curves[0]);
				for (var i = 1; i <= curves.Count - 1; i++)
				{
					CurvesLeft.Add(i, curves[System.Convert.ToInt32(i)]);
				}
			}
			else
			{
				return default(IList<Curve>);
			}
			//
			Nullable<int> foundedIndex = null;
			// 先向右端延伸搜索
			for (var i = 0; i <= CurvesLeft.Count - 1; i++)
			{
				foundedIndex = cc.CheckForward(CurvesLeft);
				if (foundedIndex != null) // 说明找到了相连的曲线
				{
					cc.ConnectForward(CurvesLeft[foundedIndex.Value]);
					CurvesLeft.Remove(foundedIndex.Value);
					
				}
				else // 说明剩下的曲线中，没有任何一条曲线能与当前连续链的右端相连了
				{
					break;
				}
			}
			
			// 再向左端延伸搜索
			for (var i = 0; i <= CurvesLeft.Count - 1; i++)
			{
				foundedIndex = cc.CheckBackward(CurvesLeft);
				if (foundedIndex != null) // 说明找到了相连的曲线
				{
					cc.ConnectBackward(CurvesLeft[foundedIndex.Value]);
					CurvesLeft.Remove(foundedIndex.Value);
					
				}
				else // 说明剩下的曲线中，没有任何一条曲线能与当前连续链的右端相连了
				{
					break;
				}
			}
			
			//
			if (cc.Curves.Count != curves.Count)
			{
				return default(IList<Curve>);
			}
			return cc.Curves;
		}
		
#endregion
		
		/// <summary>
		/// 模拟一段从左向右的连续性曲线链集合，集合中的第一个元素表示最左边的曲线；end0 与 end1 分别代表整个连续曲线段的最左端点与最右端点。
		/// </summary>
		public class ContiguousCurves
		{
			
			private List<Curve> CurvesChain;
			/// <summary> 连续性曲线链，此集合中的曲线肯定是首尾相连的。且第一个元素表示最左边的那条曲线。 </summary>
public List<Curve> Curves
			{
				get
				{
					return CurvesChain;
				}
			}
			
			/// <summary> 整个连续性曲线链的最左端点的坐标 </summary>
			private XYZ end0;
			
			/// <summary> 整个连续性曲线链的最右端点的坐标 </summary>
			private XYZ end1;
			
			/// <summary>
			/// 从一条曲线开始构造连续曲线链
			/// </summary>
			/// <param name="BaseCurve"></param>
			public ContiguousCurves(Curve BaseCurve)
			{
				CurvesChain = new List<Curve>();
				CurvesChain.Add(BaseCurve);
				this.end0 = BaseCurve.GetEndPoint(0);
				this.end1 = BaseCurve.GetEndPoint(1);
			}
			
#region 检测连续性
			
			/// <summary>
			/// 从一组曲线中找到一条与连续链右端点相接的曲线，并且在适当的情况下，对搜索到的曲线进行反转。
			/// </summary>
			/// <param name="curves">进行搜索的曲线集合。在此函数中，可能会对连接到的那条曲线进行反转。
			/// IDictionary中的键值表示每一条Curve的Id值，这个值并不一定是从1开始递增的。
			/// </param>
			/// <returns>
			/// 与连续曲线链的最右端相连的那一条曲线在输入的曲线集合中所对应的Id键值。
			/// 如果没有找到连接的曲线，则返回Nothing！</returns>
			public Nullable<int> CheckForward(Dictionary<int, Curve> curves)
			{
				// 搜索到的那一条曲线所对应的Id值
				Nullable<int> ConnectedCurveIndex = new Nullable<int>();
				ConnectedCurveIndex = null; // 如果没有找到，则返回Nothing
				
				//
				var Ids = curves.Keys.ToArray();
				var Cvs = curves.Values;
				//
				int tempId = 0;
				Curve tempCurve = default(Curve);
				
				// 从曲线集合中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
				for (int i = 0; i <= Ids.Length - 1; i++)
				{
					tempId = Ids[i];
					tempCurve = curves[tempId];
					
					// Is there a match end->start, if so this is the next curve
					if (GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(0), this.end1, GeoHelper.VertexTolerance))
					{
						return tempId;
						// Is there a match end->end, if so, reverse the next curve
					}
					else if (GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(1), this.end1, GeoHelper.VertexTolerance))
					{
						// 将曲线进行反转
						curves[tempId] = tempCurve.CreateReversed(); // 将反转后的曲线替换掉原来的曲线
						return tempId;
					}
				}
				//
				return ConnectedCurveIndex;
			}
			
			/// <summary>
			/// 从一组曲线中找到一条与连续链左端点相接的曲线，并且在适当的情况下，对搜索到的曲线进行反转。
			/// </summary>
			/// <param name="curves">进行搜索的曲线集合。在此函数中，可能会对连接到的那条曲线进行反转。
			/// IDictionary中的键值表示每一条Curve的Id值，这个值并不一定是从1开始递增的。</param>
			/// <returns>与连续曲线链的最右端相连的那一条曲线在输入的曲线集合中所对应的Id键值。
			/// 如果没有找到连接的曲线，则返回Nothing！</returns>
			public Nullable<int> CheckBackward(Dictionary<int, Curve> curves)
			{
				// 搜索到的那一条曲线所对应的Id值
				Nullable<int> ConnectedCurveIndex = new Nullable<int>();
				ConnectedCurveIndex = null; // 如果没有找到，则返回Nothing
				//
				var Ids = curves.Keys.ToArray();
				var Cvs = curves.Values;
				//
				int tempId = 0;
				Curve tempCurve = default(Curve);
				
				// 从曲线集合中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
				for (int i = 0; i <= Ids.Length - 1; i++)
				{
					tempId = Ids[i];
					tempCurve = curves[tempId];
					
					// Is there a match end->start, if so this is the next curve
					if (GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(0), this.end0, GeoHelper.VertexTolerance))
					{
						// 将曲线进行反转
						curves[tempId] = tempCurve.CreateReversed();
						return tempId;
						
					}
					else if (GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(1), this.end0, GeoHelper.VertexTolerance))
					{
						return tempId;
					}
				}
				//
				return ConnectedCurveIndex;
			}
			
#endregion
			
#region 连接
			
			/// <summary>
			/// 将曲线添加到连续曲线链的右端
			/// </summary>
			/// <param name="c">请自行确保添加的曲线是可以与当前的连续链首尾相接的。
			/// 如果不能确保，请通过CheckForward函数进行检测。</param>
			public void ConnectForward(Curve c)
			{
				CurvesChain.Add(c);
				end1 = c.GetEndPoint(1);
			}
			
			/// <summary>
			/// 将曲线添加到连续曲线链的左端
			/// </summary>
			/// <param name="c">请自行确保添加的曲线是可以与当前的连续链首尾相接的。
			/// 如果不能确保，请通过CheckBackward函数进行检测。</param>
			public void ConnectBackward(Curve c)
			{
				CurvesChain.Insert(0, c);
				end0 = c.GetEndPoint(0);
			}
			
#endregion
		}
		
	}
	
}

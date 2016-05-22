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
	
	/// <summary>
	/// 在UI界面中按指定的要求绘制模型线，并将这些模型线保存在对应的列表中。
	/// 在绘制完成后，必须手动执行 Dispose 方法，以清空数据，以及取消事件的关联。
	/// </summary>
	public class ModelCurvesDrawer
	{
		
		/// <summary>
		/// 在模型线绘制完成时，触发此事件。
		/// </summary>
		/// <param name="AddedCurves">添加的模型线</param>
		/// <param name="FinishedExternally">画笔是否是由外部程序强制关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为 True。</param>
		/// <param name="Succeeded">AddedCurves集合中的曲线集合是否满足指定的连续性条件</param>
		public delegate void DrawingCompletedEventHandler(List<ElementId> AddedCurves, bool FinishedExternally,  bool Succeeded);
		private DrawingCompletedEventHandler DrawingCompletedEvent;
		
		public event DrawingCompletedEventHandler DrawingCompleted
		{
			add
			{
				DrawingCompletedEvent = (DrawingCompletedEventHandler) System.Delegate.Combine(DrawingCompletedEvent, value);
			}
			remove
			{
				DrawingCompletedEvent = (DrawingCompletedEventHandler) System.Delegate.Remove(DrawingCompletedEvent, value);
			}
		}
		
		
#region    ---   Types
		
		public enum CurvesState
		{
			
			/// <summary> 当前的曲线不满足检查要求，应该退出或者撤消绘制 </summary>
			Invalid = 0,
			
			/// <summary> 当前的曲线还未满足检查要求，但是还可以继续绘制。
			/// 比如要求绘制一个封闭的曲线链，则在未封闭的过程中，只要其是连续的，就还可以继续绘制。 </summary>
			Validating,
			
			/// <summary> 当前的曲线已经满足了指定的要求，但此时并不一定要退出。 </summary>
			Validated
			
		}
		
#endregion
		
#region    ---   Properties
		
		private CurveCheckMode F_CheckMode;
		/// <summary>
		/// 所绘制的曲线要符合何种连续性条件
		/// </summary>
		/// <returns></returns>
public CurveCheckMode CheckMode
		{
			get
			{
				return F_CheckMode;
			}
		}
		
		private bool F_CheckInTime;
		/// <summary>
		/// 是否在每一步绘制时都检测所绘制的曲线是否符合指定的要求，如果为False，则在绘制操作退出后进行统一检测。
		/// </summary>
		/// <returns></returns>
public bool CheckInTime
		{
			get
			{
				return F_CheckInTime;
			}
		}
		
		/// <summary> 模型线绘制器是否正在使用中。
		/// 注意：每次只能有一个实例正在绘制曲线。</summary>
		public static bool IsBeenUsed {get; set;}
#endregion
		
#region    ---   Fields
		
		private UIApplication rvtUiApp;
		private Autodesk.Revit.ApplicationServices.Application rvtApp;
		private Document doc;
		
		// Public Property AddedModelCurves As List(Of ModelCurve)
		/// <summary>
		/// 已经绘制的所有模型线
		/// </summary>
		/// <returns></returns>
		private List<ElementId> AddedModelCurvesId;
		
#endregion
		
#region    ---   实例的构造与回收
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="uiApp">进行模型线绘制的Revit程序</param>
		/// <param name="CheckMode">所绘制的曲线要符合何种连续性条件</param>
		/// <param name="CheckInTime">是否在每一步绘制时都检测所绘制的曲线是否符合指定的要求，如果为False，则在绘制操作退出后进行统一检测。</param>
		/// <param name="BaseCurves">
		/// 在新绘制之前，先指定一组基准曲线集合，而新绘制的曲线将与基准曲线一起来进行连续性条件的检测。
		/// </param>
		public ModelCurvesDrawer(UIApplication uiApp, CurveCheckMode CheckMode, bool CheckInTime, List<ElementId> BaseCurves = null)
			{
			
			// 变量初始化
			rvtUiApp = uiApp;
			rvtApp = uiApp.Application;
			this.F_CheckMode = CheckMode;
			this.F_CheckInTime = CheckInTime;
			
			// 处理已经添加好的基准曲线集合
			this.AddedModelCurvesId = BaseCurves;
			if (this.AddedModelCurvesId == null)
			{
				this.AddedModelCurvesId = new List<ElementId>();
			}
			//
			rvtApp.DocumentChanged += new System.EventHandler<DocumentChangedEventArgs>(app_DocumentChanged);
		}
		
		// 绘图与判断处理
		/// <summary>
		/// 在UI界面中绘制模型线。此方法为异步操作，程序并不会等待 PostDraw 方法执行完成才继续向下执行。
		/// </summary>
		public void PostDraw()
		{
			if (ModelCurvesDrawer.IsBeenUsed)
			{
				throw (new InvalidOperationException("有其他的对象正在绘制模型线，请等待其绘制完成后再次启动。"));
			}
			else
			{
				
				ActiveDraw();
				ModelCurvesDrawer.IsBeenUsed = true;
			}
		}
		
		/// <summary>
		/// 取消模型线的绘制
		/// </summary>
		public void Cancel()
		{
			// 最后再检测一次
			bool blnContinueDraw = false;
			CurvesState cs = ValidateCurves(ref blnContinueDraw);
			RefreshUI(cs, blnContinueDraw);
			
			if (cs == CurvesState.Validated)
			{
				this.Finish(true, true);
			}
			else
			{
				// 不管是 Validating 还是 Invalid，说明在此刻都没有成功
				this.Finish(FinishedExternally: true, Succeeded: false);
			}
		}
		
		/// <summary>
		/// 绘制完成，并关闭绘制模式
		/// </summary>
		/// <param name="FinishedExternally">画笔是否是由外部程序强制关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为 True。</param>
		/// <param name="Succeeded">AddedModelCurves 集合中的曲线集合是否满足指定的连续性条件</param>
		private void Finish(bool FinishedExternally, bool Succeeded)
		{
			
			// 注意以下几步操作的先后顺序
			//
			ModelCurvesDrawer.IsBeenUsed = false;
			if (DrawingCompletedEvent != null)
				DrawingCompletedEvent(AddedModelCurvesId, FinishedExternally, Succeeded);
			
			// 数据清空
			AddedModelCurvesId = new List<ElementId>();
			DeactiveDraw();
		}
		
		public void Dispose()
		{
			// 注意以下几步操作的先后顺序
			//
			rvtApp.DocumentChanged -= new System.EventHandler<DocumentChangedEventArgs>(app_DocumentChanged);
			ModelCurvesDrawer.IsBeenUsed = false;
			
			// 变量清空
			rvtUiApp = null;
			rvtApp = null;
			AddedModelCurvesId = null;
			//
			DeactiveDraw();
		}
		
#endregion
		
		/// <summary>
		/// DocumentChanged事件，并针对不同的绘制情况而进行不同的处理
		/// </summary>
		/// <param name="sender">Application对象</param>
		/// <param name="e"></param>
		private void app_DocumentChanged(object sender, DocumentChangedEventArgs e)
		{
			
			if (e.Operation == UndoOperation.TransactionCommitted ||
					e.Operation == UndoOperation.TransactionUndone ||
					e.Operation == UndoOperation.TransactionRedone)
					{
					
					doc = e.GetDocument();
				bool blnContinueDraw = false; // 在检查连续性后是否要继续绘制
				try
				{
					
					// 先考察添加的对象：如果添加了新对象，则要么是 DrawNewLines ，要么是 DrawOtherObjects
					int addedCount = 0;
					Element addedElement = default(Element);
					foreach (ElementId eid in e.GetAddedElementIds())
					{
						addedElement = doc.GetElement(eid);
						if (addedElement is ModelCurve)
						{
							AddedModelCurvesId.Add(eid);
							addedCount++;
						}
					}
					
					if (addedCount > 0) // 说明绘制了新的模型线
					{
						// 检测当前集合中的曲线是否符合指定的连续性要求
						if (this.CheckInTime)
						{
							CurvesState cs = ValidateCurves(ref blnContinueDraw);
							RefreshUI(cs, blnContinueDraw);
						}
						else // 说明不进行实时检测，而直接继续绘制
						{
							
						}
						//
						return;
					}
					
					//
					// 再考察删除对象的情况
					List<ElementId> deleted = e.GetDeletedElementIds().ToList();
					if (deleted.Count > 0)
					{
						
						// 先将被删除的曲线从曲线链集合中剔除掉
						int id_Chain = 0; // 曲线链中的元素下标
						int id_deleted = 0; // 删除的模型线集合中的元素下标
						for (id_Chain = AddedModelCurvesId.Count - 1; id_Chain >= 0; id_Chain--) // 曲线链中的元素下标
						{
							//
							id_deleted = deleted.IndexOf(AddedModelCurvesId[id_Chain]); // 找到对应的项
							//
							if (id_deleted >= 0)
							{
								deleted.RemoveAt(id_deleted);
								AddedModelCurvesId.RemoveAt(id_Chain);
							}
						}
						
						// 检测剔除后的集合中的曲线是否符合指定的连续性要求
						if (this.CheckInTime)
						{
							if (this.CheckInTime)
							{
								CurvesState cs = ValidateCurves(ref blnContinueDraw);
								RefreshUI(cs, blnContinueDraw);
							}
							else // 说明不进行实时检测，而直接继续绘制
							{
								
							}
						}
						else // 说明不进行实时检测，而直接继续绘制
						{
							
						}
						//
						return;
					}
					
					// 再考察修改对象的情况（因为在添加对象或者删除对象时，都有可能伴随有修改对象）：在没有添加新对象，只作了修改的情况下，要么是对
					int modifiedCount = 0;
					Element modifiedCountElement = default(Element);
					foreach (ElementId eid in e.GetModifiedElementIds())
					{
						modifiedCountElement = doc.GetElement(eid);
						if (modifiedCountElement is ModelCurve)
						{
							modifiedCount++;
						}
					}
					if (modifiedCount > 0)
					{
						
						// 检测剔除后的集合中的曲线是否符合指定的连续性要求
						if (this.CheckInTime)
						{
							CurvesState cs = ValidateCurves(ref blnContinueDraw);
							RefreshUI(cs, blnContinueDraw);
							
						}
						else // 说明不进行实时检测，而直接继续绘制
						{
							
						}
						//
						return;
					}
					
				}
				catch (Exception ex)
				{
					MessageBox.Show("在绘制模型线及连续性判断时出问题啦~~~" + "\r\n" +
						ex.Message + ex.GetType().FullName + "\r\n" +
						ex.StackTrace);
					// 结束绘制
					this.Finish(false, false);
				}
			}
			
		}
		
#region    ---   曲线绘制的激活、取消
		
		// 绘图操作的启动与终止
		/// <summary>
		/// 启动绘图操作
		/// </summary>
		private void ActiveDraw()
		{
			rvtUiApp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.ModelLine));
		}
		
		/// <summary>
		/// 绘图结束后的操作。注意，此操作必须要放在Messagebox.Show（或者是其他通过ESC键就可以对窗口进行某些操作的情况，
		/// 比如关闭窗口等）之后。如果放在Messagebox.Show之前，则会模拟通过按下ESC键而将模态窗口关闭的操作，则模态窗口就只
		/// 会闪现一下，或者根本就看不见。
		/// </summary>
		private void DeactiveDraw()
		{
		   UIntPtr ptr0=new UIntPtr(0);
			// 在Revit UI界面中退出绘制，即按下ESCAPE键
			WindowsUtil.keybd_event((byte) 27, (byte) 0, 0, ptr0); // 按下 ESCAPE键
			WindowsUtil.keybd_event((byte) 27, (byte) 0, 0x2, ptr0); // 按键弹起
			
			// 再按一次
			WindowsUtil.keybd_event((byte) 27, (byte) 0, 0, ptr0);
			WindowsUtil.keybd_event((byte) 27, (byte) 0, 0x2, ptr0);
		}
		
#endregion
		
#region  扩展区：曲线连续性要求的检测 以及对应的界面响应
		
		/// <summary>
		/// 检测当前集合中的曲线是否符合指定的连续性要求
		/// </summary>
		/// <param name="continueDraw">在检查连续性后是否要继续绘制</param>
		/// <returns></returns>
		private CurvesState ValidateCurves(ref bool continueDraw)
		{
			CurvesState cs = CurvesState.Invalid;
			continueDraw = false;
			List<Curve> curves = new List<Curve>();
			
			//将ElementId转换为对应的 Curve 对象
			foreach (var id in AddedModelCurvesId)
			{
				curves.Add(((ModelCurve) (doc.GetElement(id))).GeometryCurve);
			}
			
			// 根据不同的模式进行不同的检测
			if (this.F_CheckMode == CurveCheckMode.Connected) // 一条连续曲线链
			{
				if (CurvesFormator.GetContiguousCurvesFromCurves(curves) != null)
				{
					cs = CurvesState.Validated;
					continueDraw = true;
				}
				else // 说明根本不连续
				{
					cs = CurvesState.Invalid;
					continueDraw = false;
				}
			}
			else if (this.F_CheckMode == CurveCheckMode.Closed)
			{
				IList<Curve> CurveChain = default(List<Curve>);
				
				CurveChain = CurvesFormator.GetContiguousCurvesFromCurves(curves);
				if (CurveChain == null) // 说明根本就不连续
				{
					cs = CurvesState.Invalid;
					continueDraw = false;
					
				}
				else // 说明起码是连续的
				{
					if (CurveChain.First().GetEndPoint(0).DistanceTo(CurveChain.Last().GetEndPoint(1)) < GeoHelper.VertexTolerance)
					{
						// 说明整个连续曲线是首尾相接，即是闭合的。此时就不需要再继续绘制下去了
						cs = CurvesState.Validated;
						continueDraw = false;
					}
					else
					{
						// 说明整个曲线是连续的，但是还没有闭合。此时就可以继续绘制下去
						cs = CurvesState.Validating;
						continueDraw = true;
					}
				}
			}
			else if (this.F_CheckMode == (CurveCheckMode.HorizontalPlan | CurveCheckMode.Closed))
			{
				if (CurvesFormator.IsInOnePlan(curves, new XYZ(0, 0, 1)))
				{
					
				}
				return CurvesState.Invalid ;
			}
			else if (this.F_CheckMode == CurveCheckMode.Seperated)
			{
				// 不用检测，直接符合
				cs = CurvesState.Validated;
				continueDraw = true;
			}
			return cs;
		}
		
		/// <summary>
		/// 根据当前曲线的连续性状态，以及是否可以继续绘制，来作出相应的UI更新
		/// </summary>
		/// <param name="cs"></param>
		/// <param name="ContinueDraw"></param>
		private void RefreshUI(CurvesState cs, bool ContinueDraw)
		{
			
			switch (cs)
			{
				case CurvesState.Validated:
					if (ContinueDraw) // 说明是绘制连续线时满足条件
					{
						// 继续绘制即可
					}
					else // 说明是绘制封闭线时终于封闭成功了
					{
						// 此时直接绘制绘制就可以了，而不用考虑撤消的问题
						this.Finish(false, true);
						return;
					}
					break;
					
				case CurvesState.Validating:
					if (ContinueDraw) // 说明是绘制封闭线时还未封闭，但是所绘制的曲线都是连续的
					{
						// 继续绘制即可
					}
					else // 暂时没有考虑到何时会出现此种情况
					{
						// 不需要任何实现方法
					}
					return;
				case CurvesState.Invalid:
					if (InquireUndo())
					{
						rvtTools.Undo();
					}
					else
					{
						// 结束绘制
						this.Finish(false, false);
					}
					return;
			}
		}
		
#endregion
		
		/// <summary> 询问用户是否要撤消操作 </summary>
		private bool InquireUndo()
		{
			DialogResult res = MessageBox.Show("当前操作使得绘制的模型线不满足要求，是否要撤消此操作？", "提示", MessageBoxButtons.YesNo,
				MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
			if (res == DialogResult.Yes)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
	}
}

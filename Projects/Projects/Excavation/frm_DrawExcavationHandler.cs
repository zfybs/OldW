// VBConversions Note: VB project level imports

using System;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports


namespace OldW.Excavation
{
	
	public partial class frm_DrawExcavation
	{
		
#region    ---   Types
		
		/// <summary>
		/// 每一个外部事件调用时所提出的需求，为了在Execute方法中充分获取窗口的需求，
		/// 所以将调用外部事件的窗口控件以及对应的触发事件参数也传入Execute方法中。
		/// </summary>
		/// <remarks></remarks>
		private class RequestParameter
		{
			
			private object F_sender;
			/// <summary> 引发Form事件控件对象 </summary>
public dynamic sender
			{
				get
				{
					return F_sender;
				}
			}
			
			private EventArgs F_e;
			/// <summary> Form中的事件所对应的事件参数 </summary>
public EventArgs e
			{
				get
				{
					return F_e;
				}
			}
			
			private Request F_Id;
			/// <summary> 具体的需求 </summary>
public Request Id
			{
				get
				{
					return F_Id;
				}
			}
			
			/// <summary>
			/// 定义事件需求与窗口中引发此事件的控件对象及对应的事件参数
			/// </summary>
			/// <param name="RequestId">具体的需求</param>
			/// <param name="e">Form中的事件所对应的事件参数</param>
			/// <param name="sender">引发Form事件控件对象</param>
			/// <remarks></remarks>
			public RequestParameter(Request RequestId, EventArgs e = null, object sender = null)
			{
				RequestParameter with_1 = this;
				with_1.F_sender = sender;
				with_1.F_e = e;
				with_1.F_Id = RequestId;
			}
		}
		
		/// <summary>
		/// ModelessForm的操作需求，用来从窗口向IExternalEventHandler对象传递需求。
		/// </summary>
		/// <remarks></remarks>
		private enum Request
		{
			
			/// <summary>
			/// 通过在UI界面绘制模型线来作为土体的轮廓
			/// </summary>
			DrawCurves,
			
			/// <summary>
			/// 删除绘制好的模型线并清空曲线集合数据
			/// </summary>
			DeleteCurves,
			
			/// <summary>
			/// 开始建模
			/// </summary>
			StartModeling
		}
		
#endregion
		
	}
}

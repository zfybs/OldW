// VBConversions Note: VB project level imports

using System;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
using  OldW.GlobalSettings;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
// End of VB project level imports
using std_ez;


namespace OldW.Instrumentation
{
	
	/// <summary>
	/// 所有类型的监测点，包括地表沉降、立柱隆起、支撑轴力等，但不包括测斜管
	/// </summary>
	/// <remarks></remarks>
	public class Instrum_Point : Instrumentation
	{
		
#region    ---   Properties
		
		private MonitorData_Point F_MonitorData;
		/// <summary>
		/// 点测点的整个施工阶段中的监测数据
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
public MonitorData_Point MonitorData
		{
			get
			{
				if (F_MonitorData == null)
				{
					F_MonitorData = GetData();
				}
				return this.F_MonitorData;
			}
			set
			{
				this.F_MonitorData = value;
			}
		}
		
#endregion
		
#region    ---   构造函数
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="MonitorPoint">所有类型的监测点，包括地表沉降、立柱隆起、支撑轴力等，但不包括测斜管</param>
		/// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
		/// <remarks></remarks>
		internal Instrum_Point(FamilyInstance MonitorPoint, InstrumentationType Type) : base(MonitorPoint, Type)
		{
			
		}
		
#endregion
		
		private MonitorData_Point GetData()
		{
			string strData = Monitor.get_Parameter(Constants.SP_Monitor_Guid).AsString();
			MonitorData_Point MData = null;
			if (strData.Length > 0)
			{
				try
				{
					MData = (MonitorData_Point) (StringSerializer.Decode64(strData));
					if ((MData.arrDate == null) || (MData.arrValue == null))
					{
						throw (new Exception());
					}
					return MData;
				}
				catch (Exception)
				{
					TaskDialog.Show("Error", this.Monitor.Name + " (" + this.Monitor.Id.IntegerValue.ToString() + ")" + " 中的监测数据不能正确地提取。", TaskDialogCommonButtons.Ok, TaskDialogResult.Ok);
					return null;
				}
			}
			return MData;
		}
		
		
		/// <summary>
		/// 将监测数据以序列化字符串保存到对应的Parameter对象中。
		/// </summary>
		/// <remarks></remarks>
		public bool SaveData()
		{
			if (this.MonitorData != null)
			{
				// 将数据序列化为字符串
				string strData = StringSerializer.Encode64(this.MonitorData);
				Parameter para = Monitor.get_Parameter(GlobalSettings.Constants.SP_Monitor_Guid); // ActiveElement.Parameter_MonitorData
				using (Transaction Tran = new Transaction(Doc, "保存表格中的数据到Element的参数中"))
				{
					try
					{
						Tran.Start();
						para.Set(strData);
						Tran.Commit();
						return true;
					}
					catch (Exception)
					{
						Tran.RollBack();
						return false;
					}
				}
				
			}
			else
			{
				
				return false;
			}
		}
		
	}
}

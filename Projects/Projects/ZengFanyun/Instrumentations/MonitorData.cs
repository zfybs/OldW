// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports


namespace OldW.Instrumentation
{
	
#region   ---   监测数据类
	/// <summary>
	///
	/// </summary>
	/// <remarks></remarks>
	[System.Serializable()]public class MonitorData_Point
	{
		public DateTime[] arrDate {get; set;}
		public object[] arrValue {get; set;}
		public MonitorData_Point(DateTime[] ArrayDate, object[] ArrayValue)
		{
			MonitorData_Point with_1 = this;
			with_1.arrDate = ArrayDate;
			with_1.arrValue = ArrayValue;
		}
	}
	
	/// <summary>
	/// 线测点中的每一天的监测数据
	/// </summary>
	/// <remarks></remarks>
	[System.Serializable()]public class MonitorData_Line
	{
		
		private SortedDictionary<DateTime, MonitorData_Length> AllData;
public SortedDictionary<DateTime, MonitorData_Length> Data
		{
			get
			{
				return AllData;
			}
		}
		
		public MonitorData_Line(SortedDictionary<DateTime, MonitorData_Length> AllData)
		{
			this.AllData = AllData;
		}
	}
	
	/// <summary>
	///
	/// </summary>
	/// <remarks></remarks>
	[System.Serializable()]public class MonitorData_Length
	{
		public float[] arrDistance {get; set;}
		public object[] arrValue {get; set;}
		public MonitorData_Length(float[] ArrayDistance, object[] ArrayValue)
		{
			MonitorData_Length with_1 = this;
			with_1.arrDistance = ArrayDistance;
			with_1.arrValue = ArrayValue;
		}
	}
#endregion
	
}

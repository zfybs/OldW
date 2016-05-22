// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports
using OldW.GlobalSettings;
using Autodesk.Revit.DB;

namespace OldW.Instrumentation
{
	
	/// <summary>
	/// 所有类型的线监测，包括测斜管
	/// </summary>
	/// <remarks></remarks>
	public class Instrum_Line : Instrumentation
	{
#region    ---   Properties
		
		/// <summary>
		/// 线测点的整个施工阶段中的监测数据
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public SortedDictionary<DateTime, Dictionary<float, object>> MonitorData {get; set;}
		
#endregion
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="MonitorLine">所有类型的监测管线，包括测斜管，但不包括地表沉降、立柱隆起、支撑轴力等</param>
		/// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
		/// <remarks></remarks>
		public Instrum_Line(FamilyInstance MonitorLine, InstrumentationType Type = InstrumentationType.墙体测斜) : base(MonitorLine, Type)
		{
		}
		
		
		public MonitorData_Line GetData()
		{
			return null;
		}
		
		
	}
}

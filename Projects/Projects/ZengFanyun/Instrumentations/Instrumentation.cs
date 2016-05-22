// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports

using OldW.GlobalSettings;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace OldW.Instrumentation
{
	
	/// <summary>
	/// 监测测点：包括线测点（测斜管）或点测点（地表沉降、立柱隆起、支撑轴力）等
	/// </summary>
	/// <remarks>
	/// 对于点测点而言，其监测数据是在不同的时间记录的，每一个时间上都只有一个数据。所以其监测数据是一个两列的表格，第一列为时间，第二列为监测数据。
	/// 对于线测点而言（比如测斜管），在每一个时间上都有两列数据，用来记录这一时间上，线测点中每一个位置的监测值。
	/// </remarks>
	public abstract class Instrumentation
	{
		
#region    ---   Properties
		
		private Document F_Doc;
public Document Doc
		{
			get
			{
				return F_Doc;
			}
		}
		
		private UIDocument F_UIDoc;
public UIDocument UIDoc
		{
			get
			{
				return F_UIDoc;
			}
		}
		
		private FamilyInstance F_Monitor;
		/// <summary>
		/// 监测仪器，对于点测点，其包括地表沉降、立柱隆起、支撑轴力等；
		/// 对于线测点，包括测斜管
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
public FamilyInstance Monitor
		{
			get
			{
				return F_Monitor;
			}
		}
		
		
		private InstrumentationType F_Type;
		/// <summary> 监测点的测点类型，也是测点所属的族的名称 </summary>
public InstrumentationType Type
		{
			get
			{
				return F_Type;
			}
		}
		
		/// <summary> 每一个测点的名称，比如 CX1，LZ2等 </summary>
		public string Name {get; set;}
		
#endregion
		
#region    ---   构造函数
		
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Instrumentation">所有类型的监测仪器，包括线测点（测斜管）或点测点（地表沉降、立柱隆起、支撑轴力）等</param>
		/// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
		/// <remarks></remarks>
		internal Instrumentation(FamilyInstance Instrumentation, InstrumentationType Type)
		{
			
			if (Instrumentation != null)
			{
				this.F_Monitor = Instrumentation;
				this.F_Doc = Instrumentation.Document;
				this.F_UIDoc = new UIDocument(this.Doc);
				this.F_Type = Type;
				//
				
			}
			else
			{
				throw (new NullReferenceException("The specified element is not valid as an instrumentation."));
			}
			
		}
		
#endregion
		
		
#region    ---   从Element集合中过滤出监测点对象
		
		/// <summary>
		/// 从指定的Element集合中，找出所有的监测点元素
		/// </summary>
		/// <param name="Elements"> 要进行搜索过滤的Element集合</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static List<Instrumentation> FilterInstrumentations(Document Doc, ICollection<ElementId> Elements)
		{
			List<Instrumentation> Instrus = new List<Instrumentation>();
			FilteredElementCollector Coll = new FilteredElementCollector(Doc, Elements);
			// 集合中的族实例
			Coll = Coll.OfClass(typeof(FamilyInstance));
			
			// 找到指定的Element集合中，所有的族实例
			FilteredElementIterator FEI = Coll.GetElementIterator();
			string strName = "";
			FEI.Reset();
			while (FEI.MoveNext())
			{
				//add level to list
				FamilyInstance fi = FEI.Current as FamilyInstance;
				if (fi != null)
				{
					// 一个Element所对应的族的名称
					strName = System.Convert.ToString(fi.Symbol.FamilyName);
					InstrumentationType Tp = default(InstrumentationType);
					if (Enum.TryParse(value:  strName, result: out Tp))
					{
						switch (Tp)
						{
							case InstrumentationType.墙体测斜:
								Instrus.Add(new Instrum_Incline(fi));
								break;
							case InstrumentationType.支撑轴力:
								Instrus.Add(new Instrum_StrutAxialForce(fi));
								break;
							case InstrumentationType.地表隆沉:
								Instrus.Add(new Instrum_GroundSettlement(fi));
								break;
							case InstrumentationType.立柱隆沉:
								Instrus.Add(new Instrum_ColumnHeave(fi));
								break;
						}
					}
				}
			}
			return Instrus;
		}
		
		/// <summary>
		/// 从指定的Element集合中，找出所有的点测点元素
		/// </summary>
		/// <param name="Elements"> 要进行搜索过滤的Element集合</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static List<Instrum_Point> FilterInstru_Point(Document Doc, ICollection<ElementId> Elements)
		{
			List<Instrum_Point> Instrus = new List<Instrum_Point>();
			FilteredElementCollector Coll = new FilteredElementCollector(Doc, Elements);
			// 找到指定的Element集合中，所有的族实例
			FilteredElementIterator FEI = Coll.OfClass(typeof(FamilyInstance)).GetElementIterator();
			string strName = "";
			FEI.Reset();
			while (FEI.MoveNext())
			{
				//add level to list
				FamilyInstance fi = FEI.Current as FamilyInstance;
				if (fi != null)
				{
					// 一个Element所对应的族的名称
					strName = System.Convert.ToString(fi.Symbol.FamilyName);
					InstrumentationType Tp = default(InstrumentationType);
					if (Enum.TryParse(value: strName, result: out Tp))
					{
						switch (Tp)
						{
							case InstrumentationType.支撑轴力:
								Instrus.Add(new Instrum_StrutAxialForce(fi));
								break;
							case InstrumentationType.地表隆沉:
								Instrus.Add(new Instrum_GroundSettlement(fi));
								break;
							case InstrumentationType.立柱隆沉:
								Instrus.Add(new Instrum_ColumnHeave(fi));
								break;
						}
					}
				}
			}
			return Instrus;
		}
		
#endregion
		
	}
	
}



// VBConversions Note: VB project level imports

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports

//using rvtTools_ez.ExtensionMethods;
//using System.Math;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;



namespace OldW.Instrumentation
{
	/// <summary>
	/// 测点_地表垂直位移
	/// </summary>
	/// <remarks></remarks>
	public class Instrum_GroundSettlement : Instrum_Point
	{
		/// <summary> 构造函数 </summary>
		/// <param name="GroundSettlementElement">地表垂直位移测点所对应的图元</param>
		public Instrum_GroundSettlement(FamilyInstance GroundSettlementElement) : base(GroundSettlementElement, InstrumentationType.地表隆沉)
		{
			
			
		}
		
	}
}

// VBConversions Note: VB project level imports

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports

//using rvtTools_ez.ExtensionMethods;
//using System.Math;


using Autodesk.Revit.DB;
using OldW.Instrumentations;

namespace OldW.Instrumentations
{
	/// <summary>
	/// 测点_支撑轴力
	/// </summary>
	/// <remarks></remarks>
	public class Instrum_StrutAxialForce : Instrum_Point
	{
		/// <summary> 构造函数 </summary>
		/// <param name="StrutAxialForceElement"> 支撑轴力测点所对应的图元</param>
		public Instrum_StrutAxialForce(FamilyInstance StrutAxialForceElement) : base(StrutAxialForceElement, InstrumentationType.支撑轴力)
		{
			
		}
		
	}
}

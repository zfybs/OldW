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
	/// 测点_立柱垂直位移
	/// </summary>
	/// <remarks></remarks>
	public class Instrum_ColumnHeave : Instrum_Point
	{
		/// <summary> 构造函数 </summary>
		/// <param name="ColumnHeaveElement">立柱垂直位移测点所对应的图元</param>
		public Instrum_ColumnHeave(FamilyInstance ColumnHeaveElement) : base(ColumnHeaveElement, InstrumentationType.立柱隆沉)
		{
			
			
		}
		
	}
}

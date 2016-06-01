// VBConversions Note: VB project level imports

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;

// End of VB project level imports


namespace OldW.DllActivator
{
	public class DllActivator_Projects : IDllActivator
	{
		/// <summary>
		/// 激活本DLL所引用的那些DLLs
		/// </summary>
		public void ActivateReferences()
		{
			IDllActivator dat;
			//
			dat = new DllActivator_std();
			dat.ActivateReferences();
			//
			dat = new DllActivator_OldWGlobal();
			dat.ActivateReferences();
			//
			dat = new DllActivator_RevitTools();
			dat.ActivateReferences();
		}
	}
    }

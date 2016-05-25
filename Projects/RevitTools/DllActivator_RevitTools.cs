using System;
namespace OldW.DllActivator
{
    public class DllActivator_RevitTools : IDllActivator
    {
        /// <summary>
        /// 激活本DLL所引用的那些DLLs
        /// </summary>
        void IDllActivator.ActivateReferences()
        {
            IDllActivator dat;
            dat = new DllActivator_OldWGlobal();
            dat.ActivateReferences();
            
            //
            dat = new DllActivator_std();
            dat.ActivateReferences();
            //
        }
    }
}
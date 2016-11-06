using System;
using DllActivator;


namespace DllActivator
{
    public class DllActivator_Projects : IDllActivator_RevitStd
    {
        /// <summary>
        /// 激活本DLL所引用的那些DLLs
        /// </summary>
        public void ActivateReferences()
        {
            IDllActivator_std dat;
            //
            dat = new DllActivator_std();
            dat.ActivateReferences();
            //
            dat = new DllActivator_OldWGlobal();
            dat.ActivateReferences();

            //
            var dat2 = new DllActivator_RevitStd();
            dat2.ActivateReferences();
        }
    }
}
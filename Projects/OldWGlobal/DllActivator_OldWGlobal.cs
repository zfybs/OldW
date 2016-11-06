using DllActivator;

namespace DllActivator
{
    public class DllActivator_OldWGlobal : IDllActivator_std
    {
        /// <summary>
        /// 激活本DLL所引用的那些DLLs
        /// </summary>
        public void ActivateReferences()
        {
            IDllActivator_std dat ;
            dat = new DllActivator_std();
            dat.ActivateReferences();
        }
    }
}
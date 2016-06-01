namespace OldW.DllActivator
{
    public class DllActivator_OldWGlobal : IDllActivator
    {
        /// <summary>
        /// 激活本DLL所引用的那些DLLs
        /// </summary>
        public void ActivateReferences()
        {
            IDllActivator dat = default(IDllActivator);
            dat = new DllActivator_std();
            dat.ActivateReferences();
        }
    }
}
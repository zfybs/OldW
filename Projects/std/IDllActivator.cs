namespace OldW.DllActivator
{
    /// <summary>
    /// 用于在打开非模态窗口的的IExternalCommand.Execute方法中，
    /// </summary>
    public interface IDllActivator
    {
        /// <summary> 激活本DLL所引用的那些DLLs </summary>
        void ActivateReferences();
    }

    /// <summary>
    ///
    /// </summary>
    public class DllActivator_std : IDllActivator
    {
        /// <summary>
        /// 激活本DLL所引用的那些DLLs
        /// </summary>
        public void ActivateReferences()
        {
        }
    }
}
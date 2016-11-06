using System;
namespace OldW.ProjectInfo
{
    /// <summary>
    /// 一个OldWDocument的模型中，与基坑相关的一些信息，这些信息是被序列化到模型的项目信息中的参数“OldW_Project”中的。
    /// </summary>
    /// <remarks></remarks>
    [Serializable()]
    public class OldWProjectInfo
    {
        #region    ---   Properties
        /// <summary> 基坑开挖的开始日期 </summary>
        public DateTime ExcavStart;
        /// <summary> 基坑开挖的结束日期 </summary>
        public DateTime ExcavFinish;
        
        #endregion

        /// <summary> 构造函数 </summary>
        public OldWProjectInfo()
        {
            // 默认的开挖起止时间都是今天
            ExcavStart = DateTime.Today;
            ExcavFinish = DateTime.Today;
        }
    }

}
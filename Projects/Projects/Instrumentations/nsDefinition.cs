using System;

namespace OldW.Instrumentations // 与 OldW.Instrumentation 命名空间相关的一些接口、枚举等的定义
{
    /// <summary>
    /// 监测仪器的族名称（也是族文件的名称），同时也作为监测仪器的类型判断
    /// </summary>
    /// <remarks>从枚举值返回对应的枚举字符的方法：GlobalSettings.InstrumentationType.沉降测点.ToString</remarks>
    public enum InstrumentationType
    {
        /// <summary> 并不是任何一种已经识别的测点类型 </summary>
        其他,

        /// <summary> 比如地下连续墙的水平位移 </summary>
        墙体测斜,

        /// <summary> 比如基坑外地表的垂直位移 </summary>
        地表隆沉,

        /// <summary> 比如基坑中支撑的轴力 </summary>
        支撑轴力,

        /// <summary> 比如基坑中立柱的垂直位移 </summary>
        立柱隆沉
    }

    /// <summary> 监测数据类，表示点测点中的每一天的监测数据 </summary>
    [Serializable()]
    public class MonitorData_Point
    {
        /// <summary>
        /// 监测日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 监测数据，如果当天没有数据，则为null
        /// </summary>
        public Single? Value { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="Value"></param>
        public MonitorData_Point(DateTime Date, Single? Value)
        {
            this.Date = Date;
            this.Value = Value;
        }
    }
}
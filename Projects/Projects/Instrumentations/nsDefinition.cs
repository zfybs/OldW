using System;
using System.Collections.Generic;

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


    /// <summary>
    /// 线测点中的每一天的监测数据
    /// </summary>
    /// <remarks></remarks>
    [Serializable()]
    public class MonitorData_Line
    {
        /// <summary>
        /// 线测点上的每一个子节点的深度（相对于线测点的顶端或起点而言）
        /// </summary>
        public Single[] Nodes { get; set; }

        private readonly SortedDictionary<DateTime, float?[]> _monitorData;
        /// <summary>
        /// 测斜管在每一天的监测数据。其中，SortedDictionary 中的Value项 为一个数组，
        /// 它代表对应的日期下，Depths中每一个深度处所对应的监测数据值，
        /// 所以，此数组中元素的个数必须要与Depths数组中元素的个数相同。
        /// </summary>
        public SortedDictionary<DateTime, float?[]> MonitorData
        {
            get { return _monitorData; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodes">线测点上的每一个子节点的深度（相对于线测点的顶端或起点而言）</param>
        public MonitorData_Line(Single[] nodes)
        {
            _monitorData = new SortedDictionary<DateTime, float?[]>();
            Nodes = nodes;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodes">线测点上的每一个子节点的深度（相对于线测点的顶端或起点而言）</param>
        /// <param name="monitoredData">已经记录好的监测数据</param>
        public MonitorData_Line(Single[] nodes, SortedDictionary<DateTime, float?[]> monitoredData)
        {
            _monitorData = monitoredData;
            Nodes = nodes;
        }
    }
}
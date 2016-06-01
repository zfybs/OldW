using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using OldW.Instrumentations;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 所有类型的线监测，包括测斜管
    /// </summary>
    /// <remarks></remarks>
    public class Instrum_Line : Instrumentation
    {
        #region    ---   Properties

        /// <summary>
        /// 线测点的整个施工阶段中的监测数据
        /// </summary>
        /// <Value></Value>
        /// <returns></returns>
        /// <remarks></remarks>
        public SortedDictionary<DateTime, Dictionary<float, object>> MonitorData { get; set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="MonitorLine">所有类型的监测管线，包括测斜管，但不包括地表沉降、立柱隆起、支撑轴力等</param>
        /// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
        /// <remarks></remarks>
        public Instrum_Line(FamilyInstance MonitorLine, InstrumentationType Type = InstrumentationType.墙体测斜)
            : base(MonitorLine, Type)
        {
        }

        /// <summary>
        /// 从指定的Element集合中，找出所有的点测点元素
        /// </summary>
        /// <param name="Elements"> 要进行搜索过滤的Element集合</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public new static List<Instrum_Line> Lookup(Document Doc, ICollection<ElementId> Elements)
        {
            List<Instrum_Line> lines = new List<Instrum_Line>();

            List<Instrumentation> instrus;
            instrus = Instrumentation.Lookup(Doc, Elements);
            foreach (var VARIABLE in instrus)
            {
                if (VARIABLE is Instrum_Line)
                {
                    lines.Add(VARIABLE as Instrum_Line);
                }
            }
            return lines;
        }

        public MonitorData_Line GetData()
        {
            return null;
        }

        #region   ---   监测数据类
        
        /// <summary>
        /// 线测点中的每一天的监测数据
        /// </summary>
        /// <remarks></remarks>
        [System.Serializable()]
        public class MonitorData_Line
        {
            private SortedDictionary<DateTime, MonitorData_Length> AllData;
            public SortedDictionary<DateTime, MonitorData_Length> Data
            {
                get
                {
                    return AllData;
                }
            }

            public MonitorData_Line(SortedDictionary<DateTime, MonitorData_Length> AllData)
            {
                this.AllData = AllData;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        [System.Serializable()]
        public class MonitorData_Length
        {

            public float[] arrDistance { get; set; }
            public object[] arrValue { get; set; }
            public MonitorData_Length(float[] ArrayDistance, object[] ArrayValue)
            {
                MonitorData_Length with_1 = this;
                with_1.arrDistance = ArrayDistance;
                with_1.arrValue = ArrayValue;
            }
        }
        #endregion

    }
}
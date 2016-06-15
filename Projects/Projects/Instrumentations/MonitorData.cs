using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using stdOldW.DAL;

namespace OldW.Instrumentations // 与 OldW.Instrumentation 命名空间相关的一些接口、枚举等的定义
{
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
        public float? Value { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="Value"></param>
        public MonitorData_Point(DateTime Date, float? Value)
        {
            this.Date = Date;
            this.Value = Value;
        }

        /// <summary>
        /// 从 DataTable 对象中指定的两个字段提取出点测点的监测数据的实体类集合
        /// </summary>
        /// <param name="table"> 要进行数据提取的表格 </param>
        /// <param name="indexDate"> 日期数据在 table 中所在的列的列号 </param>
        /// <param name="indexValue"> 监测数据在 table 中所在的列的列号 </param>
        /// <returns> 实体类集合，用来作为 Instrum_Point.SetMonitorData 的输入参数 </returns>
        public static List<MonitorData_Point> FromDataTable(DataTable table, int indexDate, int indexValue)
        {
            return (from DataRow row in table.Rows
                    select Convert.IsDBNull(row[indexValue])
                    ? new MonitorData_Point((DateTime)row[indexDate], null)
                    : new MonitorData_Point((DateTime)row[indexDate], Convert.ToSingle(row[indexValue]))).ToList();
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
        /// 因为从概念上来说，所谓的线测点，其子节点们不一定是表示深度的数值区分，而是指一个测点中有多个监测项目。
        /// 比如测斜管是在一个测点中的不同深度下有多个监测数据，而墙顶位移是一个测点中有水平与竖直两个监测数据。
        /// </summary>
        private string[] _nodes { get; set; }
        
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
        /// 子节点的名称是否有数值意义
        /// </summary>
        /// <remarks> 线测点的子节点是广义上的同一个测点中所监测的不同类型的数据，比如墙顶位移测点就有“墙顶垂直位移”与 “墙顶水平位移”两个子节点。
        /// 但是对于测斜管这类线测点，其每一个字段都是有严格的数值意义的，即代表了此子节点距离管顶的深度。</remarks>
        public  bool NodesDigital;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodes">线测点上的每一个子节点的深度（相对于线测点的顶端或起点而言）</param>
        /// <param name="nodesDigital">子节点的名称是否有数值意义</param>
        public MonitorData_Line(string[] nodes, bool nodesDigital)
        {
            _monitorData = new SortedDictionary<DateTime, float?[]>();
            _nodes = nodes;
            NodesDigital = nodesDigital;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodes">线测点上的每一个子节点的深度（相对于线测点的顶端或起点而言）</param>
        /// <param name="monitoredData">已经记录好的监测数据</param>
        /// <param name="nodesDigital">子节点的名称是否有数值意义</param>
        public MonitorData_Line(string[] nodes, SortedDictionary<DateTime, float?[]> monitoredData, bool nodesDigital)
        {
            _monitorData = monitoredData;
            NodesDigital = nodesDigital;
            _nodes = nodes;
        }

        /// <summary> 获取线测点的子节点的名称所对应的数值。如果不能转换为数值，则给出报错。 </summary>
        /// <returns>对于测斜管这类线测点，其每一个字段都是有严格的数值意义的，即代表了此子节点距离管顶的深度。</returns>
        public float[] GetDigitalNodes()
        {
            if (NodesDigital)
            {
                return _nodes.Select(Convert.ToSingle).ToArray();
            }
            throw new InvalidCastException("此类监测数据的节点并不是数值类型，无法转换为数值。");
        }

        /// <summary> 获取线测点的子节点的名称 </summary>
        public string[] GetStringNodes()
        {
            return _nodes;
        }

        /// <summary>
        /// 从 DataTable 对象提取出此线测点的所有监测数据
        /// </summary>
        /// <param name="table"> 要进行数据提取的表格的第一个字段必须是用来存储时间信息的主键 </param>
        /// <param name="convertStringToSingle"> 对于测斜管这类线测点，其每一个字段都是有严格的数值意义的，即代表了此子节点距离管顶的深度，
        /// 所以在Excel工作表中，这些子节点的字段名的格式为“2#50”，这时就要将其转换为对应的可以表示数值的“2.50”。 </param>
        /// <returns> 实体类，用来作为 Instrum_Line.SetMonitorData 的输入参数 </returns>
        public static MonitorData_Line FromDataTable(DataTable table, bool convertStringToSingle)
        {
            int nodesCount = table.Columns.Count - 1;

            // 提取子节点深度
            string[] nodes = new string[nodesCount];
            if (convertStringToSingle)
            {
                for (int i = 0; i < nodesCount; i++)
                {
                    // 列名格式为：“0#50”，即表示深度为0.50处的子节点，所以这里要先将其转换为数值
                    nodes[i] = table.Columns[i + 1].ColumnName.Replace("#", ".");
                }

            }
            else
            {
                for (int i = 0; i < nodesCount; i++)
                {
                    // 列名格式为：“0#50”，即表示深度为0.50处的子节点，所以这里要先将其转换为数值
                    nodes[i] = table.Columns[i + 1].ColumnName;
                }
            }


            // 构造监测数据的集合
            SortedDictionary<DateTime, float?[]> monitoredData = new SortedDictionary<DateTime, float?[]>();
            foreach (DataRow row in table.Rows)
            {
                float?[] values = new float?[nodesCount];
                for (int i = 0; i < nodesCount; i++)
                {
                    // 列名格式为：“0#50”，即表示深度为0.50处的子节点，所以这里要先将其转换为数值
                    if (Convert.IsDBNull(row[i + 1]))
                    {
                        values[i] = null;
                    }
                    else
                    {
                        values[i] = Convert.ToSingle(row[i + 1]);
                    }
                }
                // 添加一条记录
                monitoredData.Add((DateTime)row[0], values);
            }
            return new MonitorData_Line(nodes, monitoredData,convertStringToSingle);
        }

    }
}

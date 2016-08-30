using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using eZstd;
using eZstd.Data;

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
        private MonitorData_Line _monitorData;

        /// <summary>
        /// 子节点的名称是否有数值意义
        /// </summary>
        /// <remarks> 线测点的子节点是广义上的同一个测点中所监测的不同类型的数据，比如墙顶位移测点就有“墙顶垂直位移”与 “墙顶水平位移”两个子节点。
        /// 但是对于测斜管这类线测点，其每一个字段都是有严格的数值意义的，即代表了此子节点距离管顶的深度。</remarks>
        public readonly bool NodesDigital;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="MonitorLine">所有类型的监测管线，包括测斜管，但不包括地表沉降、立柱隆起、支撑轴力等</param>
        /// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
        /// <param name="nodesDigital"> 子节点的名称是否有数值意义 </param>
        /// <remarks></remarks>
        public Instrum_Line(FamilyInstance MonitorLine, InstrumentationType Type, bool nodesDigital)
            : base(MonitorLine, Type)
        {
            NodesDigital = nodesDigital;
        }

        /// <summary>
        /// 从指定的Element集合中，找出所有的点测点元素
        /// </summary>
        /// <param name="elements"> 要进行搜索过滤的Element集合</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public new static List<Instrum_Line> Lookup(Document doc, ICollection<ElementId> elements)
        {
            List<Instrum_Line> lines = new List<Instrum_Line>();

            List<Instrumentation> instrus;
            instrus = Instrumentation.Lookup(doc, elements);
            foreach (var ins in instrus)
            {
                if (ins is Instrum_Line)
                {
                    lines.Add(ins as Instrum_Line);
                }
            }
            return lines;
        }

        #region   ---   监测数据的提取与保存

        /// <summary>
        /// 将测点对象中的监测数据提取为具体的序列化类。
        /// 其中包括线测点的每一个子节点的数据，以及整个线测点在整个施工过程中所有的测点数据。
        /// </summary>
        /// <returns>如果在派生类中将此方法进行重写，则一定要对应地对 <see cref="SetMonitorData"/> 方法进行重写。</returns>
        public virtual MonitorData_Line GetMonitorData()
        {
            if (_monitorData != null)
            {
                return _monitorData;
            }

            string strData = base.GetMonitorDataString();
            MonitorData_Line mData = null;
            if (strData.Length > 0)
            {
                try
                {
                    mData = (MonitorData_Line)StringSerializer.Decode64(strData);
                    return mData;
                }
                catch (Exception)
                {
                    TaskDialog.Show("Error",
                        this.Monitor.Name + " (" + this.Monitor.Id.IntegerValue.ToString() + ")" + " 中的监测数据不能正确地提取。",
                        TaskDialogCommonButtons.Ok, TaskDialogResult.Ok);
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 将监测数据以序列化字符串保存到对应的Parameter对象中。
        /// </summary>
        /// <remarks>如果在派生类中将此方法进行重写，则一定要对应地对 <see cref="GetMonitorData"/> 方法进行重写。</remarks>
        public virtual void SetMonitorData(Transaction tran, MonitorData_Line data)
        {

            // 将数据序列化为字符串
            // 如果data为空，则表示将原来的监测数据清空，所以也是可以的
            string strData = StringSerializer.Encode64(data);
            base.SetMonitorDataString(tran, strData);

            // 将数据在类实例中保存下来
            _monitorData = data;

        }

        #endregion

        #region   ---   与Excel数据库的数据交互

        /// <summary> 从Excel中导入监测数据 </summary>
        /// <param name="tran"> 已经start的Revit事务 </param>
        /// <param name="conn">连接到Excel工作簿</param>
        /// <param name="sheetName">监测数据所在的Excel工作表，表名称中应该包含后缀$ </param>
        /// <param name="fieldName">监测数据在工作表中的哪个字段下。对于线测点，其fieldName是不必要的。</param>
        public override void ImportFromExcel(Transaction tran, OleDbConnection conn, string sheetName, string fieldName)
        {
            var dt = ExcelDbHelper.GetDataFromSheet(conn, sheetName);
            var data = MonitorData_Line.FromDataTable(dt, NodesDigital);
            SetMonitorData(tran, data);
        }

        /// <summary> 将Revit中的线测点的监测数据导出到Excel工作表中。 </summary>
        /// <param name="conn">连接到Excel工作簿</param>
        /// <returns> 如果此测点中没有监测数据，则返回 null </returns>
        /// <remarks>对于线测点的子节点有数值意义的情况（墙体测斜），每一个线测点都对应一个工作表；
        /// 对于线测点的子节点无数值意义的情况（墙顶位移），每一个子节点类型对应一个工作表；</remarks>
        public virtual DataTable ConvertToDatatable(OleDbConnection conn)
        {
            if (!NodesDigital)
            {
                throw new InvalidOperationException("线测斜对象的子节点无数值意义，不能单独存储在一张Excel工作表中。");
            }
            DataTable table = new DataTable();

            // 添加时间列
            DataColumn colDate = new DataColumn()
            {
                ColumnName = Constants.ExcelDatabasePrimaryKeyName,
                DataType = typeof(DateTime),
                AllowDBNull = false,
            };
            table.Columns.Add(colDate);

            var data = GetMonitorData();

            if (data == null)
            {
                // 如果没有监测数据，则返回一个只有主键列“时间”的表。
                return table;
            }

            // 添加数值型子节点列
            float[] nodes = data.GetDigitalNodes();
            foreach (var nd in nodes)
            {
                DataColumn colField = new DataColumn()
                {
                    // 将小数点进行替换
                    ColumnName = nd.ToString(CultureInfo.InvariantCulture).Replace(".", Constants.ExcelDatabaseDot),

                    DataType = typeof(float),
                    AllowDBNull = true,
                };
                table.Columns.Add(colField);
            }

            // 写入数据
            foreach (var oneData in data.MonitorData)
            {
                var dd = new object[nodes.Length + 1];
                dd[0] = oneData.Key;  // 日期数据
                //
                var colIndex = 1;
                foreach (float? d in oneData.Value)
                {
                    if (d == null)
                    {
                        dd[colIndex] = DBNull.Value;
                    }
                    else
                    {
                        dd[colIndex] = d;
                    }
                    colIndex += 1;
                }
                table.Rows.Add(dd);
            }
            return table;
        }

        /// <summary> 将Revit中的线测点的监测数据导出到Excel工作表中。 </summary>
        /// <param name="lines"> <see cref="NodesDigital"/>属性为false的线测点的集合。
        /// 集合中的测点必须是同一种测点类型，而且每个测点中的子节点数目要相同。因为要将其放到同一个Excel工作表内 </param>
        /// <remarks>对于线测点的子节点有数值意义的情况（墙体测斜），每一个线测点都对应一个工作表；
        /// 对于线测点的子节点无数值意义的情况（墙顶位移），每一个子节点类型对应一个工作表；</remarks>
        public static DataTable[] ConvertToDatatables(IList<Instrum_Line> lines)
        {
            // 确保集合中至少有一个具有有效数据的测点
            if (lines == null || lines.Count == 0) throw new NullReferenceException("集合中没有线测点");

            // ---------------------------------------------------------------
            // 所有测点集合中的所有监测数据
            List<Dictionary<string, List<MonitorData_Point>>> dataSheets = new List<Dictionary<string, List<MonitorData_Point>>>();
            // ---------------------------------------------------------------


            // 根据第一个测点的状态确定集合中的所有测点的类型（默认此集合中所有的线测点都是属性同一种类型）
            Instrum_Line line1 = lines[0];
            InstrumentationType originType = line1.Type;
            if (line1.NodesDigital) throw new InvalidOperationException("线测斜对象的子节点有明显的数值意义，不能将每一个子节点放置在一个工作表中。");

            // 先提取第一个线测点中的数据，并构建基本表格数据
            string[] nodes1 = new string[] { };  // 第一个线测点的子节点集合。正常情况下，集合中其余所有的线测点都具有相同的子节点集合。
            foreach (var line in lines)
            {
                if (line.GetMonitorData() != null)
                {
                    nodes1 = line.GetMonitorData().GetStringNodes();
                    break;
                }
            }
            if (nodes1 == null || nodes1.Length == 0) throw new NullReferenceException("未找到有效的子节点");

            // var data1 = line1.GetMonitorData();

            int nodeIndex;
            for (nodeIndex = 0; nodeIndex < nodes1.Length; nodeIndex++)// 每一个node对应了一个 Excel 工作表
            {
                // 每一个节点类型对应一个工作表，键代表每一个测点的名称
                Dictionary<string, List<MonitorData_Point>> nodesData = new Dictionary<string, List<MonitorData_Point>>();  // 一类子节点中的每一个线测点的监测数据
                dataSheets.Add(nodesData);  // 每一个节点类型对应一个工作表

                // ---------------------------------------------------------------
                // 此时，dataSheets中应该已经为每一个节点类型创建一个表（Dictionary<string, List<MonitorData_Point>>）
                // 而每一个表中都有了两个字段：第一个字段为主键“时间”，每二个字段为此线测点在工作表所对应的节点类型下的测点数据。
                // ---------------------------------------------------------------
            }

            // 将后面其他的线测点的每一个节点的数据放置到与节点类型对应的表格中。
            foreach (Instrum_Line line in lines)
            {
                if (line.Type != originType) throw new InvalidOperationException("集合中的线测点必须是同一种监测类型");

                var data = line.GetMonitorData();

                // 如果此测点没有监测数据的话，则将此测点的名称添加到每一个子节点工作表的字段名中
                if (data == null)
                {
                    data = new MonitorData_Line(nodes1, new SortedDictionary<DateTime, float?[]>(), false);
                }
                else
                {
                    // 检测测点的子节点是否与其他测点的子节点不相同
                    var nodes = data.GetStringNodes();  // 第 lineIndex 个线测点的所有子节点
                    if (nodes.Length != nodes1.Length) throw new InvalidOperationException("集合中的线测点元素必须包含相同数目的子节点");
                }

                // 将此线测点中每一个子节点的数据放置到对应的字典表格中

                // 提取并重构监测数据
                for (nodeIndex = 0; nodeIndex < nodes1.Length; nodeIndex++)// 每一个node对应了一个 Excel 工作表
                {
                    // 每一个节点类型对应一个工作表，键代表每一个测点的名称
                    Dictionary<string, List<MonitorData_Point>> nodesData = dataSheets[nodeIndex];  // 一类子节点中的每一个线测点的监测数据

                    List<MonitorData_Point> oneNodeData = new List<MonitorData_Point>();
                    foreach (var oneDayData in data.MonitorData)
                    {
                        oneNodeData.Add(new MonitorData_Point(oneDayData.Key, oneDayData.Value[nodeIndex]));

                    } // 某线测点的某个子节点的下一天的监测数据

                    nodesData.Add(line.GetMonitorName(), oneNodeData);
                }  // 某线测点的下一个子节点
            }

            // 转换为表格 DataTable
            DataTable[] tables = new DataTable[nodes1.Length];

            for (int i = 0; i < nodes1.Length; i++)
            {
                tables[i] = InstrumDoc.PointsToDatatable(dataSheets[i], nodes1[i]);
            }
            return tables;
        }
        #endregion
    }
}
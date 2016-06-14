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
    #region ---   enum(InstrumentationType) ：监测仪器的族名称（也是族文件的名称），同时也作为监测仪器的类型判断 

    /// <summary>
    /// (位编码)监测仪器的族名称（也是族文件的名称），同时也作为监测仪器的类型判断
    /// </summary>
    /// <remarks>从枚举值返回对应的枚举字符的方法：GlobalSettings.InstrumentationType.沉降测点.ToString</remarks>
    [Flags()]
    public enum InstrumentationType : int
    {

        /// <summary> 并不是任何一种已经指定的线测点或者点测点类型 </summary>
        未指定 = 0,

        /// <summary> 并不是任何一种已经特殊处理过的点测点类型 </summary>
        其他点测点 = 1,

        /// <summary> 并不是任何一种已经特殊处理过的线测点类型 </summary>
        其他线测点 = 2,

        /// <summary> 比如地下连续墙的水平位移 </summary>
        墙体测斜 = 4,

        /// <summary> 比如土体中的测斜管的水平位移，它与墙体测斜的区别在于墙体测斜是嵌在地下连续墙中，
        /// 而且墙体测斜与土体测斜的安全警戒准则并不相同。 </summary>
        土体测斜 = 8,

        /// <summary> 墙顶位移的监测数据包括水平位移与垂直位移。
        /// 在Excel中通过两张表“墙顶水平位移”与“墙顶垂直位移”来保存。 </summary>
        墙顶位移 = 16,

        /// <summary> 比如基坑外地表的垂直位移 </summary>
        地表隆沉 = 32,

        /// <summary> 比如基坑中立柱的垂直位移 </summary>
        立柱隆沉 = 64,

        /// <summary> 比如基坑中支撑的轴力 </summary>
        支撑轴力 = 128,

        /// <summary> 比如基坑中水位测点处的水位高低 </summary>
        水位 = 256,
    }

    #endregion

    #region ---   Class(MonitorData_Point/MonitorData_Line)：Revit中测点监测数据的序列化类 

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
        public MonitorData_Line(float[] nodes, SortedDictionary<DateTime, float?[]> monitoredData)
        {
            _monitorData = monitoredData;
            Nodes = nodes;
        }
        
        /// <summary>
        /// 从 DataTable 对象提取出此线测点的所有监测数据
        /// </summary>
        /// <param name="table"> 要进行数据提取的表格的第一个字段必须是用来存储时间信息的主键，并且后面的每一个字段的名称都是“2#50”的格式。 </param>
        /// <returns> 实体类，用来作为 Instrum_Line.SetMonitorData 的输入参数 </returns>
        public static MonitorData_Line FromDataTable(DataTable table)
        {
            int nodesCount = table.Columns.Count - 1;

            // 提取子节点深度
            float[] nodes = new float[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                // 列名格式为：“0#50”，即表示深度为0.50处的子节点，所以这里要先将其转换为数值
                nodes[i] = Convert.ToSingle(table.Columns[i + 1].ColumnName.Replace("#", "."));
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
            return new MonitorData_Line(nodes,monitoredData);
        }
    }
    #endregion

    #region ---   Class(InstrumCollector)：测点收集器，用来对测点集合进行分类管理

    /// <summary>
    /// 测点收集器，用来对测点集合进行分类管理
    /// </summary>
    public struct InstrumCollector
    {
        /// <summary> 所有的测点的集合 </summary>
        List<Instrumentation> _allInstrumentations;
        /// <summary> 所有的测点的集合 </summary>
        public List<Instrumentation> AllInstrumentations
        {
            get { return _allInstrumentations; }
        }

        #region ---   不同的测点集合 

        /// <summary> 立柱隆沉测点 </summary>
        public readonly List<Instrum_ColumnHeave> ColumnHeave;

        /// <summary> 地表隆沉测点 </summary>
        public readonly List<Instrum_GroundSettlement> GroundSettlement;

        /// <summary> 测斜点 </summary>
        public readonly List<Instrum_WallIncline> Incline;

        /// <summary> 支撑轴力点 </summary>
        public readonly List<Instrum_StrutAxialForce> StrutAxialForce;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="instrums"> 要进行测点分类的测点集合 </param>
        public InstrumCollector(IEnumerable<Instrumentation> instrums)
        {
            ColumnHeave = new List<Instrum_ColumnHeave>();
            GroundSettlement = new List<Instrum_GroundSettlement>();
            Incline = new List<Instrum_WallIncline>();
            StrutAxialForce = new List<Instrum_StrutAxialForce>();
            //
            _allInstrumentations = new List<Instrumentation>();

            // 
            Truncate(instrums);
        }

        /// <summary>
        /// 清空原测点集合中的元素，并重新添加新的元素
        /// </summary>
        /// <param name="instrums"></param>
        public void Truncate(IEnumerable<Instrumentation> instrums)
        {
            ColumnHeave.Clear();
            GroundSettlement.Clear();
            Incline.Clear();
            StrutAxialForce.Clear();
            //
            _allInstrumentations.Clear();

            Append(instrums);
        }

        /// <summary>
        /// 直接向集合中附加新的测点
        /// </summary>
        /// <param name="instrums"></param>
        public void Append(IEnumerable<Instrumentation> instrums)
        {
            foreach (Instrumentation inst in instrums)
            {
                if (inst is Instrum_ColumnHeave)
                {
                    ColumnHeave.Add((Instrum_ColumnHeave)inst);
                }
                else if (inst is Instrum_GroundSettlement)
                {
                    GroundSettlement.Add((Instrum_GroundSettlement)inst);
                }
                else if (inst is Instrum_WallIncline)
                {
                    Incline.Add((Instrum_WallIncline)inst);
                }
                else if (inst is Instrum_StrutAxialForce)
                {
                    StrutAxialForce.Add((Instrum_StrutAxialForce)inst);
                }
            }
            _allInstrumentations.AddRange(instrums);

        }

        /// <summary> 按指定的类型过滤出集合中所有的测点 </summary>
        /// <param name="type"> 多种监测类型的按位组合 </param>
        public List<Instrumentation> GetMonitors(InstrumentationType type)
        {
            var q = from Instrumentation r in AllInstrumentations
                    where (r.Type & type) > 0
                    select r;
            return q.ToList();
        }

        #region ---   过滤出线测点 

        /// <summary> 过滤出集合中所有的线测点 </summary>
        public List<Instrum_Line> GetLineMonitors()
        {
            var q = from Instrumentation r in AllInstrumentations
                    where r is Instrum_Line
                    select (Instrum_Line)r;
            return q.ToList();
        }

        /// <summary> 按指定的选项过滤出集合中所有的线测点 </summary>
        /// <param name="type"> 多种监测类型的按位组合 </param>
        public List<Instrum_Line> GetLineMonitors(InstrumentationType type)
        {
            var q = from Instrumentation r in AllInstrumentations
                    where (r is Instrum_Line) && (r.Type & type) > 0
                    select (Instrum_Line)r;
            return q.ToList();
        }


        #endregion

        #region ---   过滤出点测点 

        /// <summary> 过滤出集合中所有的线测点 </summary>
        public List<Instrum_Point> GetPointMonitors()
        {
            var q = from Instrumentation r in AllInstrumentations
                    where r is Instrum_Point
                    select (Instrum_Point)r;
            return q.ToList();
        }

        /// <summary> 按指定的选项过滤出集合中所有的点测点 </summary>
        /// <param name="type"> 多种监测类型的按位组合 </param>
        public List<Instrum_Point> GetPointMonitors(InstrumentationType type)
        {
            var q = from Instrumentation r in AllInstrumentations
                    where r is Instrum_Point && (r.Type & type) > 0
                    select (Instrum_Point)r;
            return q.ToList();
        }

        #endregion
    }

    #endregion

    #region ---   Class(InstrumTypeMapping)将不同的Excel工作表字段的名称映射到Revit对应的测点中去 

    /// <summary>
    /// 将不同的Excel工作表字段的名称映射到Revit对应的测点中去
    /// </summary>
    public struct InstrumTypeMapping
    {

        /// <summary> 墙体测斜,每一个测斜点的数据用一个工作表来保存 </summary>
        private const string SheetWallIncline = "CX";
        /// <summary> 土体测斜,每一个测斜点的数据用一个工作表来保存 </summary>
        private const string SheetSoilIncine = "TX";

        private const string SheetWallTopH = "墙顶水平位移";
        private const string SheetWallTopV = "墙顶垂直位移";
        private const string SheetGroundHeave = "地表隆沉";
        private const string SheetColumnHeave = "立柱隆沉";
        private const string SheetStrut = "支撑轴力";
        private const string SheetWaterTable = "水位";

        /// <summary> 其他未在上面标记过的测点类型，其每一个点测点的监测数据都保存在工作表中的某个字段下。 </summary>
        private const string SheetOtherPoint = "PM";
        /// <summary> 其他未在上面标记过的测点类型，其每一个线测点中有多个子节点（类似于测斜管），
        /// 一个测点的监测数据保存在一张工作表，而表中的每一个字段代表此线测点中的一个子节点。 </summary>
        private const string SheetOtherLine = "LM";

        #region ---   工作表名 相关的匹配

        /// <summary> 指定的Excel工作表中是包含多个测点（点测点）还是只包含一个测点（线测点） </summary>
        /// <param name="excelSheetName"></param>
        /// <returns> 如果此工作表中有多个点测点，比如“地表隆沉”，则返回true；
        /// 如果此工作表就代表一个线测点，则返回false。 </returns>
        public static bool MultiPointsInSheet(string excelSheetName)
        {
            if (excelSheetName.StartsWith(SheetSoilIncine, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(SheetWallIncline, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(SheetOtherLine, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        /// <summary> 此工作表名称所对应的测点类型在Revit中是否是一个线测点类型 </summary>
        /// <param name="excelSheetName"></param>
        /// <returns> 如果此工作表名称代表一类测点，比如“地表隆沉”，则返回true；
        /// 如果此工作表就代表一个测斜测点，则返回false。 </returns>
        public static bool IsRevitLine(string excelSheetName)
        {
            if (excelSheetName.StartsWith(SheetSoilIncine, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(SheetWallIncline, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(SheetWallTopH, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(SheetWallTopV, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(SheetOtherLine, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 由字段名称来匹配出对应的测点类型
        /// </summary>
        /// <param name="excelSheetName"> Excel工作表的名称，不包含后缀$</param>
        /// <returns></returns>
        public static InstrumentationType MapToType(string excelSheetName)
        {
            InstrumentationType tp;
            if (excelSheetName.StartsWith(SheetColumnHeave))
            {
                tp = InstrumentationType.立柱隆沉;
            }
            else if (excelSheetName.StartsWith(SheetGroundHeave))
            {
                tp = InstrumentationType.地表隆沉;
            }
            else if (excelSheetName.StartsWith(SheetOtherLine))
            {
                tp = InstrumentationType.其他线测点;
            }
            else if (excelSheetName.StartsWith(SheetOtherPoint))
            {
                tp = InstrumentationType.其他点测点;
            }
            else if (excelSheetName.StartsWith(SheetSoilIncine))
            {
                tp = InstrumentationType.土体测斜;
            }
            else if (excelSheetName.StartsWith(SheetStrut))
            {
                tp = InstrumentationType.支撑轴力;
            }
            else if (excelSheetName.StartsWith(SheetWallIncline))
            {
                tp = InstrumentationType.墙体测斜;
            }
            else if (excelSheetName.StartsWith(SheetWallTopH))
            {
                tp = InstrumentationType.墙顶位移;
            }
            else if (excelSheetName.StartsWith(SheetWallTopV))
            {
                tp = InstrumentationType.墙顶位移;
            }
            else if (excelSheetName.StartsWith(SheetWaterTable))
            {
                tp = InstrumentationType.水位;
            }
            else
            {
                tp = InstrumentationType.其他点测点;
            }
            return tp;
        }

        #endregion

        #region ---   点测点监测数据中字段名 相关的匹配

        /// <summary> 根据给出的字段名匹配出对应的测点编号 </summary>
        /// <param name="excelPointOrLineName"> Excel中的测点编号，此编号可以了线测点的编号如CX01，也可以是点测点的编号如DB01。 </param>
        /// <returns> 如果能匹配出编号12或者编号2-12这两种模式，则返回对应的数值字符，如果不能匹配，则返回空字符</returns>
        public static string GetNumber(string excelPointOrLineName)
        {
            if (string.IsNullOrEmpty(excelPointOrLineName)) { return ""; }

            excelPointOrLineName = excelPointOrLineName.Trim(); // 清除前后空白
            string strNum = "";

            string Pattern1 = @"\d+-\d+$"; // DB2-12 模式

            strNum = Regex.Match(excelPointOrLineName, Pattern1).Value;
            if (string.IsNullOrEmpty(strNum)) // 如果不能匹配，再看是否能匹配 CX12 模式
            {
                string Pattern2 = @"\d+$"; // CX12 模式
                strNum = Regex.Match(excelPointOrLineName, Pattern2).Value;

                if (string.IsNullOrEmpty(strNum)) // 如果不能匹配，再看是否能
                {
                    return "";  // 如果不能匹配，则返回空字符
                }
                // 成功匹配
                strNum = int.Parse(strNum).ToString(); // 将字符“010”转换为“10”
            }
            else
            {
                // 成功匹配
                var s = strNum.Split('-');
                strNum = int.Parse(s[0]) + "-" + int.Parse(s[1]); // 将字符“010-012”转换为“10-12”
            }
            return strNum;
        }
        #endregion
    }
    #endregion
}

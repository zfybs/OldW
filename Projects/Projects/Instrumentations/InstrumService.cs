using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using stdOldW.DAL;
using OldW.GlobalSettings;

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

        /// <summary> 通过位运算进行组合的非数值线测点的集合。不包括墙体测斜这种子节点有数值意义的线测点 </summary>
        非数值线测点集合 = 墙顶位移,

        /// <summary> 通过位运算进行组合的数值线测点的集合。不包括墙顶位移这种子节点没有数值意义的线测点 </summary>
        数值线测点集合 = 其他线测点 | 墙体测斜 | 土体测斜,

        /// <summary> 通过位运算进行组合的所有线测点的集合。包括墙顶位移 </summary>
        线测点集合 = 数值线测点集合 | 非数值线测点集合,

        /// <summary> 通过位运算进行组合的所有点测点的集合。 </summary>
        点测点集合 = 其他点测点 | 地表隆沉 | 立柱隆沉 | 支撑轴力 | 水位,


    }

    #endregion

    #region ---   Class(InstrumTypeMapping)将不同的Excel工作表字段的名称映射到Revit对应的测点中去 

    /// <summary>
    /// 将不同的Excel工作表字段的名称映射到Revit对应的测点中去
    /// </summary>
    public static class InstrumTypeMappingExcel
    {
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
            if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_SoilIncine, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallIncline, StringComparison.OrdinalIgnoreCase)
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
            if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_SoilIncine, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallIncline, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallTopH, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallTopV, StringComparison.OrdinalIgnoreCase)
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
            if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_ColumnHeave))
            {
                tp = InstrumentationType.立柱隆沉;
            }
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_GroundHeave))
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
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_SoilIncine))
            {
                tp = InstrumentationType.土体测斜;
            }
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_StrutForce))
            {
                tp = InstrumentationType.支撑轴力;
            }
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallIncline))
            {
                tp = InstrumentationType.墙体测斜;
            }
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallTopH))
            {
                tp = InstrumentationType.墙顶位移;
            }
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallTopV))
            {
                tp = InstrumentationType.墙顶位移;
            }
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WaterTable))
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

    #region ---   Class(InstrumCollector)：测点收集器，用来对测点集合进行分类管理

    /// <summary>
    /// 测点收集器，用来对测点集合进行分类管理
    /// </summary>
    public class InstrumCollector
    {
        /// <summary> 所有的测点的集合 </summary>
        List<Instrumentation> _allInstrumentations;
        /// <summary> 所有的测点的集合 </summary>
        public List<Instrumentation> AllInstrumentations
        {
            get { return _allInstrumentations; }
        }

        #region ---   不同的测点集合 

        ///// <summary> 立柱隆沉测点 </summary>
        //public readonly List<Instrum_ColumnHeave> ColumnHeave;

        ///// <summary> 地表隆沉测点 </summary>
        //public readonly List<Instrum_GroundSettlement> GroundSettlement;

        ///// <summary> 测斜点 </summary>
        //public readonly List<Instrum_WallIncline> Incline;

        ///// <summary> 支撑轴力点 </summary>
        //public readonly List<Instrum_StrutAxialForce> StrutAxialForce;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="instrums"> 要进行测点分类的测点集合 </param>
        public InstrumCollector(IEnumerable<Instrumentation> instrums)
        {
            //ColumnHeave = new List<Instrum_ColumnHeave>();
            //GroundSettlement = new List<Instrum_GroundSettlement>();
            //Incline = new List<Instrum_WallIncline>();
            //StrutAxialForce = new List<Instrum_StrutAxialForce>();
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
            //ColumnHeave.Clear();
            //GroundSettlement.Clear();
            //Incline.Clear();
            //StrutAxialForce.Clear();
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
            //foreach (Instrumentation inst in instrums)
            //{
            //    if (inst is Instrum_ColumnHeave)
            //    {
            //        ColumnHeave.Add((Instrum_ColumnHeave)inst);
            //    }
            //    else if (inst is Instrum_GroundSettlement)
            //    {
            //        GroundSettlement.Add((Instrum_GroundSettlement)inst);
            //    }
            //    else if (inst is Instrum_WallIncline)
            //    {
            //        Incline.Add((Instrum_WallIncline)inst);
            //    }
            //    else if (inst is Instrum_StrutAxialForce)
            //    {
            //        StrutAxialForce.Add((Instrum_StrutAxialForce)inst);
            //    }
            //}
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

}
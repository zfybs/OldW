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

    #region ---   Class(InstrumTypeMapping)将不同的 Excel 工作表字段的名称映射到Revit对应的测点中去 

    /// <summary>
    /// 将不同的Excel工作表字段的名称映射到Revit对应的测点中去
    /// </summary>
    public static class ExcelMapping
    {

        #region ---   工作表名 相关的匹配

        /// <summary>
        /// 验证给定的字符是否可以作为 Excel 的工作表的名称，如果不行，则按某种规则转换为有效的 Excel 工作表名称
        /// </summary>
        /// <param name="originalName"></param>
        /// <returns> 转换后的有效的 Excel 工作表名称 </returns>
        public static string ValidateSheetName(string originalName)
        {
            string newName = originalName.Trim();

            // 特殊字符替换
            newName = newName.Replace("-", "_");
            newName = newName.Replace(".", "_");
            newName = newName.Replace("^", "_");

            // 其他 Excel 工作表名称的规则
            Match m;

            // 1. 不能以数字开头
            string pattern = @"\b\d+";
            m = Regex.Match(newName, pattern);
            if (m.Success)
            {
                newName = "_" + newName;
            }

            // 2. 如果表名要以“非数值 + 数值”的格式命名，则前面的非数值部分至少要多于三个字符，比如CXCX2是可以的，但是CXC1会给出报错.
            pattern = @"\b\D{1,3}(\d+)\b";
            m = Regex.Match(newName, pattern);
            if (m.Success)
            {
                var ind = m.Groups[1].Index;  // 数字所在的位置
                newName = newName.Insert(ind, "_");  // 在数字前面添加一个下划线
            }

            return newName;
        }


        /// <summary> 指定的Excel工作表中是包含多个测点（点测点）还是只包含一个测点（线测点） </summary>
        /// <param name="excelSheetName"></param>
        /// <returns> 如果此工作表中有多个点测点，比如“地表隆沉”，则返回true；
        /// 如果此工作表就代表一个线测点，则返回false。 </returns>
        public static bool MultiPointsInSheet(string excelSheetName)
        {
            if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_SoilIncine, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_WallIncline, StringComparison.OrdinalIgnoreCase)
                || excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_OtherLine, StringComparison.OrdinalIgnoreCase))
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
                || excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_OtherLine, StringComparison.OrdinalIgnoreCase))
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
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_OtherLine))
            {
                tp = InstrumentationType.其他线测点;
            }
            else if (excelSheetName.StartsWith(Constants.ExcelDatabaseSheet_OtherPoint))
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
        public static string GetNumberFromField(string excelPointOrLineName)
        {
            if (string.IsNullOrEmpty(excelPointOrLineName)) { return ""; }

            excelPointOrLineName = excelPointOrLineName.Trim(); // 清除前后空白
            string strNum = "";

            // DB2-12 模式 或者 DB2_12 
            string Pattern1 = @"\d+(-|_)\d+$";  // $ 表示匹配必须出现在字符串或者一行的结尾。
            Match match = Regex.Match(excelPointOrLineName, Pattern1);

            if (match.Success) // 如果不能匹配，再看是否能匹配 CX12 模式
            {  // 成功匹配
                strNum = match.Value;

                var s = strNum.Split(match.Groups[1].Value.ToCharArray());
                strNum = int.Parse(s[0]) + "-" + int.Parse(s[1]); // 将字符“010-012”转换为“10-12”
                return strNum;
            }

            string Pattern2 = @"\d+$"; // CX12 模式
            strNum = Regex.Match(excelPointOrLineName, Pattern2).Value;

            if (string.IsNullOrEmpty(strNum)) // 如果不能匹配，再看是否能
            {
                return "";  // 如果不能匹配，则返回空字符
            }
            // 成功匹配
            strNum = int.Parse(strNum).ToString(); // 将字符“010”转换为“10”

            return strNum;
        }
        #endregion

        #region ---   线测点监测数据中 字段名 相关的匹配

        /// <summary>
        /// 将数值子节点型的线测点在Excel中的子节点名称转换为对应的数值。如果不能转换成功，则报错。
        /// </summary>
        /// <param name="digitalNodeName"> 对于测斜管这类线测点，其每一个字段都是有严格的数值意义的，即代表了此子节点距离管顶的深度，
        /// 所以在Excel工作表中，这些子节点的字段名的格式为“123、0#50、0.5、0dot5”，这时就要将其转换为对应的可以表示数值的“2.50”。 </param>
        /// <returns> 进行转换后的数值字符，如"2.50" </returns>
        public static string GetDigitalNodeName(string digitalNodeName)
        {
            // 列名格式为：“123、0#50、0.5、0dot5”，即表示深度为0.50处的子节点，所以这里要先将其转换为数值
            const string pattern = @"\b\s*\d*(\.|#|" + Constants.ExcelDatabaseDot + @")??\d*\s*\b";

            var match = Regex.Match(digitalNodeName, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var dot = match.Groups[1].Value;
                // 如果是整数就直接返回整数就可以了。
                return string.IsNullOrEmpty(dot) ? match.Value : match.Value.Replace(dot, ".");
            }
            else
            {
                throw new InvalidCastException("Excel 工作表中表示节点的字段名不能转换为数值！");
            }
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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="instrums"> 要进行测点分类的测点集合 </param>
        public InstrumCollector(IEnumerable<Instrumentation> instrums)
        {
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
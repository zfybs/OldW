using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OldW.GlobalSettings;

namespace OldW.Instrumentations
{
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
                throw new ArgumentException($"工作表名称“{ excelSheetName }”不能匹配到任何一种测点类型。请检查Excel工作表的名称是否符合指定测点类型的命名规范。");
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
            const string pattern = @"\b\s*\d*(\.|#|" + Constants.ExcelDatabaseDot + @")?\d*\s*\b";

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
}

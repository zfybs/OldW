using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using OldW.Instrumentations;


namespace OldW.DataManager
{
    partial class DataImport
    {
        /// <summary>
        /// DataGridViewExcel中所对应的实体类
        /// </summary>
        private class MonitorEntityExcel
        {

            #region ---   数据库实体对象的属性

            /// <summary> 数据库中的此测点是否要进行传输，即导入或者导出 </summary>
            private bool _transport;
            /// <summary> 数据库中的此测点是否要进行传输，即导入或者导出 </summary>
            public bool Transport
            {
                get { return _transport; }
                set
                {
                    if (value)
                    {
                        MatchInstrum(_possibleMatches);
                    }
                    else
                    {
                        MappedItem = null;
                    }
                    _transport = value;
                }
            }

            /// <summary> 此测点在Excel的哪一个工作表内，工作表名称包含后缀$ </summary>
            public string SheetName { get; }

            /// <summary> 此测点在Excel工作表内对应哪一个字段 </summary>
            public string FieldName { get; }

            /// <summary> 此测点在Revit中绑定的对应测点。也是后面要进行实际的数据导入的对应测点 </summary>
            public Instrumentation MappedItem { get; set; }

            #endregion

            /// <summary>  如果此测点的数据是保存在一整张工作表中（比如测斜数据），则为 false，
            /// 而如果此测点的数据是保存在一个工作表的某字段下，则为true。 </summary>
            [Browsable(false)]
            public bool StoredInField { get; }

            /// <summary> 此测点的类型 </summary>
            [Browsable(false)]
            public InstrumentationType MonitorType { get; }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="sheetName">此测点在Excel的哪一个工作表内，工作表名称包含后缀$</param>
            /// <param name="fieldName">此测点在Excel工作表内对应哪一个字段</param>
            /// <param name="storedInField"> 如果此测点的数据是保存在一整张工作表中（比如测斜数据），则为 false，
            /// 而如果此测点的数据是保存在一个工作表的某字段下，则为true。 </param>
            /// <param name="possibleMatches"> 在进行字段匹配时，此测点可能匹配到的所有测点的集合。 </param>
            public MonitorEntityExcel(string sheetName, string fieldName, bool storedInField, InstrumCollector possibleMatches)
            {
                _possibleMatches = possibleMatches;
                FieldName = fieldName;
                SheetName = sheetName;
                StoredInField = storedInField;
                Transport = true;
                //
            }

            #region ---   数据库字段到Revit测点单元的模糊匹配

            /// <summary> 在进行字段匹配时，此测点可能匹配到的所有测点的集合。 </summary>
            private readonly InstrumCollector _possibleMatches;

            /// <summary>
            /// 将此数据库对象在Revit中找到一个可能的匹配测点单元
            /// </summary>
            /// <param name="instrumsToMatch"> 在Revit中要进行匹配的测点集合 </param>
            /// <remarks> 在修改完与Datagridview绑定的属性后，记得通过Datagridview.Refresh()方法刷新界面。 </remarks>
            public void MatchInstrum(InstrumCollector instrumsToMatch)
            {
                IEnumerable<Instrumentation> searchingInstrums = instrumsToMatch.AllInstrumentations;

                // 1. 匹配具体的类型
                searchingInstrums = instrumsToMatch.GetMonitors(type: InstrumTypeMappingExcel.MapToType(SheetName));

                // 2. 如果不能匹配到具体的类型，则匹配大的区间：线测点还是点测点
                if (!searchingInstrums.Any())
                {
                    if (InstrumTypeMappingExcel.IsRevitLine(SheetName))
                    {
                        searchingInstrums = instrumsToMatch.GetLineMonitors();
                    }
                    else
                    {
                        searchingInstrums = instrumsToMatch.GetPointMonitors();
                    }
                    // 如果还是没有大概的匹配项，则将整个集合作为搜索区间
                    if (!searchingInstrums.Any())
                    {
                        searchingInstrums = instrumsToMatch.AllInstrumentations;
                    }
                }


                // 3. 匹配可能的字段名
                Instrumentation possibleInstrum = searchingInstrums.First();
                var num1 = InstrumTypeMappingExcel.GetNumber(FieldName);
                foreach (Instrumentation ins in searchingInstrums)
                {
                    if (string.Compare(num1, InstrumTypeMappingExcel.GetNumber(ins.GetMonitorName()), StringComparison.Ordinal) == 0)
                    {
                        possibleInstrum = ins;
                        break;
                    }
                }

                // 将可能的对应测点类型进行赋值
                MappedItem = possibleInstrum;
            }

            #endregion
        }
    }
}
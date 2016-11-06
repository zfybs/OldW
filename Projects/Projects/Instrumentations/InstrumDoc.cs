using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using OldW.Instrumentations.MonitorSetterGetter;
using RevitStd;
using eZstd.Data;
using eZstd.Miscellaneous;
using eZstd.UserControls;
using Forms = System.Windows.Forms;

namespace OldW.Instrumentations
{
    /// <summary> 用来执行基坑中的测点布置，监测数据管理等操作 </summary>
    /// <remarks></remarks>
    public class InstrumDoc : OldWDocument
    {
        #region    ---   Types

        #endregion

        #region    ---   Fields

        private Application App;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="OldWDoc"></param>
        public InstrumDoc(OldWDocument OldWDoc) : base(OldWDoc.Document)
        {
            this.App = Document.Application;
        }

        /// <summary>
        /// 放置测点
        /// </summary>
        /// <param name="monitorType"> 要放置的测点类型 </param>
        /// <returns></returns>
        public void SetFamily(InstrumentationType monitorType)
        {
            UIDocument uidoc = new UIDocument(Document);
            ElementId foundId = null;

            //是否存在族
            string MonitorFamilyName = Enum.GetName(typeof(InstrumentationType), monitorType);  // 要放置的测点的名称
            Family family = uidoc.Document.FindFamily(MonitorFamilyName);
            //如果存在，获得文件该族 ； 如果不存在，载入族
            if (family == null)
            {
                using (Transaction trans = new Transaction(uidoc.Document, "trans"))
                {
                    string rfaPath = Path.Combine(ProjectPath.Path_family,
                        MonitorFamilyName + ".rfa");
                    try
                    {
                        trans.Start();
                        uidoc.Document.LoadFamily(rfaPath, out family);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        throw new FileNotFoundException("未找到指定的测点族所对应的族样板文件：\n\r" + rfaPath);
                    }
                }
            }

            //获得该族的族类型，并且放置族实例
            var symbols = family.GetFamilySymbolIds();
            FamilySymbol symbol;
            if (symbols.Count == 0)
            {
                throw new InvalidOperationException("导入的测点族中没有有效的族类型。");
            }
            else if (symbols.Count == 1)
            {
                symbol = uidoc.Document.GetElement(symbols.ElementAt(0)) as FamilySymbol;
            }
            else
            {
                // 比如测斜管族中有多种不同规格的测斜管

                // 设计一个UI界面，让用户选择要放置哪一种规格的测点。
                // 并且提示用户，可以先自行的Revit界面中复制出多个不同规格参数的族类型，以供放置测点时选择。
                ChooseFamilySymbol formChooseFamilySymbol = new ChooseFamilySymbol(Document, symbols);
                var rr = formChooseFamilySymbol.ShowDialog();
                symbol = formChooseFamilySymbol.Symbol;
            }
            uidoc.PostRequestForElementTypePlacement(symbol);
        }

        #region    ---   组合列表框 ComboBox 的操作

        /// <summary>
        /// 将指定的测点集合对象转换为组合列表框Combox控件的DataSource类型。
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <param name="comboboxControl"> ComboBox 控件对象或者 DataGridViewComboBoxCell 控件对象  </param>
        /// <returns> </returns>
        public void FillCombobox(IEnumerable<Instrumentation> elementCollection, object comboboxControl)
        {
            ListControlValue<Instrumentation>[] arr = GetComboboxDatasource(elementCollection);

            if (comboboxControl.GetType() == typeof(Forms.ComboBox))
            {
                Forms.ComboBox cmb = (Forms.ComboBox)comboboxControl;
                // ComboBox设置
                cmb.DataSource = null;
                cmb.DisplayMember = ListControlValue<Instrumentation>.DisplayMember;
                cmb.ValueMember = ListControlValue<Instrumentation>.ValueMember;
                cmb.DataSource = arr;
            }

            if (comboboxControl.GetType() == typeof(Forms.DataGridViewComboBoxCell))
            {
                Forms.DataGridViewComboBoxCell cmb = (Forms.DataGridViewComboBoxCell)comboboxControl;

                // ComboBox设置
                cmb.DataSource = null;
                cmb.DisplayMember = ListControlValue<Instrumentation>.DisplayMember;
                cmb.ValueMember = ListControlValue<Instrumentation>.ValueMember;
                cmb.DataSource = arr;
            }

        }

        /// <summary>
        /// 将指定的测点集合对象转换为组合列表框Combox控件的DataSource类型。
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <returns> </returns>
        public ListControlValue<Instrumentation>[] GetComboboxDatasource(IEnumerable<Instrumentation> elementCollection)
        {
            return elementCollection.Select(eleid =>
            new ListControlValue<Instrumentation>(
                DisplayedText: eleid.IdName,
                Value: eleid)).ToArray();
        }

        #endregion

        /// <summary> 将多个点测点（或者线测点中的多个子节点）的监测数据Revit中导出到Excel中。
        /// 如果指定的点测点（或者线测点中的多个子节点）中没有数据，则返回的表格中只有一个“时间”字段 </summary>
        /// <param name="fieldPoints"> 字典中的每一键代表多个点测点（或者线测点中的多个子节点）的测点名称，值代表此测点或者子节点每一天的监测数据 </param>
        /// <param name="tableName"> 表格的标题名称，此名称在后期自然是可以修改的。 </param>
        public static DataTable PointsToDatatable(Dictionary<string, List<MonitorData_Point>> fieldPoints, string tableName)
        {
            DataTable table = new DataTable()
            {
                TableName = ExcelMapping.ValidateSheetName(tableName)
            };

            DataColumn colDate = new DataColumn // 主键的日期字段
            {
                ColumnName = Constants.ExcelDatabasePrimaryKeyName,
                DataType = typeof(DateTime),
                AllowDBNull = false,
            };

            table.Columns.Add(colDate);

            SortedSet<DateTime> id = new SortedSet<DateTime>(); // 维护表格的主键

            //
            foreach (var point in fieldPoints) // 每一个测点
            {
                DataColumn colField = new DataColumn // 每一个测点的监测字段
                {
                    ColumnName = point.Key,
                    DataType = typeof(float),
                    AllowDBNull = true,
                };

                table.Columns.Add(colField);

                List<MonitorData_Point> data = point.Value;
                if (data == null)
                {
                    // 说明此测点没有数据，因为只添加一个空的数据列
                    continue;
                }
                foreach (MonitorData_Point dataPoint in data) // 测点每一天的监测数据
                {
                    var index = id.IndexOf(dataPoint.Date);

                    if (index >= 0) // 说明有匹配项，此时应该在匹配下标所对应的行中添加数据
                    {
                        // 不用修改监测日期值，因为日期是主键，在添加数据行时已经赋值了。
                        table.Rows[index][colField] = DataTableHelper.FilterNull(dataPoint.Value);      // 监测数据值 

                    }
                    else // 说明没有匹配项，此时应该在表格中添加一行新的数据
                    {
                        id.Add(dataPoint.Date);
                        int newIndex = id.IndexOf(dataPoint.Date); // 在添加数据后，新添加的数据并不一定会在最后一行，而有可能被Sort到其他行去了。

                        // Instantiate a new row using the NewRow method.
                        DataRow dtRow = table.NewRow();


                        dtRow[colDate] = dataPoint.Date;
                        // 监测数据值 
                        dtRow[colField] = DataTableHelper.FilterNull(dataPoint.Value);

                        // 先为数据行赋值，再将数据行添加到表格中，以避免出现不允许空值的列中出现空值。
                        table.Rows.InsertAt(dtRow, newIndex);
                    }
                }
            }

            return table;
        }
    }
}
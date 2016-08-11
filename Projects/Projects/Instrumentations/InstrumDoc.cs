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
using rvtTools;
using stdOldW;
using stdOldW.DAL;
using stdOldW.WinFormHelper;
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
            Boolean found = (uidoc.Document.FindFamily(monitorType.ToString()) != null);

            Family family = uidoc.Document.FindFamily(monitorType.ToString());
            //如果存在，获得文件该族 ； 如果不存在，载入族
            if (family == null)
            {
                using (Transaction trans = new Transaction(uidoc.Document, "trans"))
                {
                    try
                    {
                        trans.Start();
                        uidoc.Document.LoadFamily(Path.Combine(ProjectPath.Path_family,
                            monitorType.ToString() + ".rfa"), out family);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }

            //获得该族的族类型，并且放置族实例
            FamilySymbol symbol = uidoc.Document.GetElement(family.GetFamilySymbolIds().ElementAt(0)) as FamilySymbol;
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
            LstbxValue<Instrumentation>[] arr = GetComboboxDatasource(elementCollection);

            if (comboboxControl.GetType() == typeof(Forms.ComboBox))
            {
                Forms.ComboBox cmb = (Forms.ComboBox)comboboxControl;
                // ComboBox设置
                cmb.DataSource = null;
                cmb.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
                cmb.ValueMember = LstbxValue<Instrumentation>.ValueMember;
                cmb.DataSource = arr;
            }

            if (comboboxControl.GetType() == typeof(Forms.DataGridViewComboBoxCell))
            {
                Forms.DataGridViewComboBoxCell cmb = (Forms.DataGridViewComboBoxCell)comboboxControl;

                // ComboBox设置
                cmb.DataSource = null;
                cmb.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
                cmb.ValueMember = LstbxValue<Instrumentation>.ValueMember;
                cmb.DataSource = arr;
            }

        }

        /// <summary>
        /// 将指定的测点集合对象转换为组合列表框Combox控件的DataSource类型。
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <returns> </returns>
        public LstbxValue<Instrumentation>[] GetComboboxDatasource(IEnumerable<Instrumentation> elementCollection)
        {
            return elementCollection.Select(eleid =>
            new LstbxValue<Instrumentation>(
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
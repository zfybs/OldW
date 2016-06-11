#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OldW.Instrumentations;
using stdOldW;
using stdOldW.DAL;
using stdOldW.WinFormHelper;

#endregion

namespace OldW.DataManager
{
    public partial class DataImport
    {
        #region ---   Fields

        private readonly InstrumDoc _document;

        /// <summary> 进入测点监测数据管理窗口时，所有选择的测点 </summary>
        private InstrumDoc.InstrumCollector selectedInstrum;

        /// <summary> 与DataGridView相绑定的数据源集合 </summary>
        private BindingList<MonitorEntityExcel> BindedExcelPoints;

        /// <summary> Excel数据库的连接 </summary>
        OleDbConnection excelConnection;

        /// <summary>  Excel数据库中，每一个工作表的第一个字段名称必须是“时间” </summary>
        private const string PrimaryKeyName = "时间";

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="eleIdCollection">所有要进行处理的测点元素的Id集合</param>
        /// <param name="document"></param>
        /// <remarks></remarks>
        public DataImport(ICollection<Instrumentation> eleIdCollection, InstrumDoc document)
        {
            InitializeComponent();

            //
            _document = document;
            selectedInstrum = new InstrumDoc.InstrumCollector(eleIdCollection);

            //
            BindedExcelPoints = new BindingList<MonitorEntityExcel>();
            ConstructdataGridViewExcel();
        }

        #region ---   DataGridView的创建与事件处理

        private void ConstructdataGridViewExcel()
        {
            //-------------------- 设置数据源的集合 -------------------------------------
            BindedExcelPoints.AllowNew = false;
            dataGridViewExcel.AllowUserToAddRows = false;

            // AutoGenerateColumns属性要在为DataSource赋值之前进行设置；
            // 如果AutoGenerateColumns 为False，那么当设置DataSource 后，用户必须要手动为指定的属性值添加数据列。参考：第六篇2.1.6的AutoGenerateColumns：自动生成数据列。
            dataGridViewExcel.AutoGenerateColumns = false;
            dataGridViewExcel.AutoSize = false;
            dataGridViewExcel.DataSource = BindedExcelPoints;


            //-------- 将已有的数据源集合中的每一个元素的不同属性在不同的列中显示出来 -------
            //
            // ColumnImport
            // 
            var ColumnImport = new DataGridViewCheckBoxColumn();
            ColumnImport.Name = "ColumnImport";
            ColumnImport.DataPropertyName = "Transport";
            ColumnImport.HeaderText = "导入";
            ColumnImport.Width = 50;
            // 
            // ColumnDrawData
            // 
            DataGridViewButtonColumn ColumnDrawData;
            ColumnDrawData = new DataGridViewButtonColumn();
            ColumnDrawData.Name = "ColumnDrawData";
            ColumnDrawData.HeaderText = "查看";
            ColumnDrawData.Text = "查看";
            ColumnDrawData.UseColumnTextForButtonValue = true;
            ColumnDrawData.Width = 50;
            // 
            // ColumnSheet
            // 
            DataGridViewTextBoxColumn ColumnSheet = new DataGridViewTextBoxColumn();
            ColumnSheet.DataPropertyName = "SheetName";
            ColumnSheet.HeaderText = "工作表";
            ColumnSheet.Name = "ColumnSheet";
            ColumnSheet.ReadOnly = true;
            // 
            // ColumnField
            // 
            DataGridViewTextBoxColumn ColumnField = new DataGridViewTextBoxColumn();
            ColumnField.DataPropertyName = "FieldName";
            ColumnField.HeaderText = "测点";
            ColumnField.Name = "ColumnField";
            ColumnField.ReadOnly = true;
            // 
            // ColumnMappedItem
            // 
            DataGridViewComboBoxColumn ColumnMappedItem = new DataGridViewComboBoxColumn();
            ColumnMappedItem.DataPropertyName = "MappedItem";
            ColumnMappedItem.Name = "ColumnMappedItem";
            ColumnMappedItem.HeaderText = "目标";

            //
            ColumnMappedItem.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
            ColumnMappedItem.ValueMember = LstbxValue<Instrumentation>.ValueMember;
            ColumnMappedItem.DataSource = _document.GetComboboxDatasource(selectedInstrum.AllInstrumentations); ;
            ColumnMappedItem.Width = 250;

            this.dataGridViewExcel.Columns.AddRange(new DataGridViewColumn[]
            {
                ColumnImport,
                ColumnDrawData,
                ColumnSheet,
                ColumnField,
                ColumnMappedItem
            });

            // --------------- 事件关联  --------------- 
            dataGridViewExcel.DataError += DataGridViewExcelOnDataError;
            dataGridViewExcel.CellContentClick += DataGridViewExcelOnCellContentClick;
            dataGridViewExcel.CellValueChanged += DataGridViewExcelOnCellValueChanged;
            dataGridViewExcel.CurrentCellDirtyStateChanged += DataGridViewExcelOnCurrentCellDirtyStateChanged;
        }


        /// <summary>
        /// DataGridViewExcel中所对应的实体类
        /// </summary>
        internal class MonitorEntityExcel
        {
            /// <summary> 数据库中的此测点是否要进行传输，即导入或者导出 </summary>
            public bool Transport { get; set; }

            /// <summary> 此测点在Excel的哪一个工作表内 </summary>
            public string SheetName { get; }

            /// <summary> 此测点在Excel工作表内对应哪一个字段 </summary>
            public string FieldName { get; }

            /// <summary> 此测点在Revit中绑定的对应测点。也是后面要进行实际的数据导入的对应测点 </summary>
            public Instrumentation MappedItem { get; set; }

            /// <summary>  如果此测点的数据是保存在一整张工作表中（比如测斜数据），则为true，
            /// 而如果此测点的数据是保存在一个工作表的某字段下，则为false。 </summary>
            [Browsable(false)]
            public bool IsSheetField { get; }

            /// <summary> 此测点的类型 </summary>
            [Browsable(false)]
            public InstrumentationType MonitorType { get; }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="sheetName">此测点在Excel的哪一个工作表内</param>
            /// <param name="fieldName">此测点在Excel工作表内对应哪一个字段</param>
            /// <param name="isSheetField"> 如果此测点的数据是保存在一整张工作表中（比如测斜数据），则为true，
            /// 而如果此测点的数据是保存在一个工作表的某字段下，则为false。 </param>
            public MonitorEntityExcel(string sheetName, string fieldName, bool isSheetField)
            {
                FieldName = fieldName;
                SheetName = sheetName;
                IsSheetField = isSheetField;
                Transport = true;
                MappedItem = null;
                //
            }

            /// <summary>
            /// 将此数据库对象在Revit中找到一个可能的匹配测点单元
            /// </summary>
            /// <param name="instrumsToMatch"> 在Revit中要进行匹配的测点集合 </param>
            /// <returns></returns>
            public void MatchInstrum(InstrumDoc.InstrumCollector instrumsToMatch)
            {

                // _listInstrum[2].Value
                MappedItem= instrumsToMatch.AllInstrumentations[3];
            }

        }


        #endregion

        #region ---   数据库字段与测点的匹配映射

        /// <summary> 准备将选择的工作表中的监测数据输入Datagridview控件，并重新刷新界面 </summary>
        /// <param name="sender"></param> <param name="e"></param>
        private void buttonMapping_Click(object sender, EventArgs e)
        {
            string workbookPath = textBox1.Text;
            if (File.Exists(workbookPath))
            {
                string strExt = Path.GetExtension(workbookPath);
                if (string.Compare(strExt, ".xlsx", StringComparison.OrdinalIgnoreCase) == 0
                    || string.Compare(strExt, ".xls", StringComparison.OrdinalIgnoreCase) == 0
                    || string.Compare(strExt, ".xlsb", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Mapping(workbookPath);
                }
                else
                {
                    MessageBox.Show(@"请选择合适的Excel工作簿进行数据映射。", @"文件名后缀不匹配", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(@"请选择合适的Excel工作簿进行数据映射。", @"文件未找到", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary> 将选择的工作表中的监测数据输入Datagridview控件，并重新刷新界面 </summary>
        /// <param name="WorkbookName"> 监测数据工作簿的绝对路径 </param>
        private void Mapping(string WorkbookName)
        {
            // 先关闭前面的数据库连接
            if (excelConnection != null && excelConnection.State == ConnectionState.Open)
            {
                excelConnection.Close();
            }
            // 打开新的数据库连接
            excelConnection = ExcelDbHelper.ConnectToExcel(WorkbookName, 1);
            excelConnection.Open();

            //

            List<MonitorEntityExcel> points = new List<MonitorEntityExcel>();

            // 工作簿中所有工作表的名称
            var sheetsName = ExcelDbHelper.GetSheetsName(excelConnection);

            foreach (var shtName in sheetsName)
            {
                // 如果工作表的名称能够匹配出相对的测点类型，则开始搜索字段
                if (InstrumDoc.InstrumTypeMapping.IsMonitorType(shtName.Remove(shtName.Length - 1, 1)))
                {
                    var ptNames = GetPointNames(excelConnection, shtName);
                    foreach (var ptname in ptNames)
                    {
                        points.Add(new MonitorEntityExcel(shtName, ptname, false));
                    }
                }
                else
                {
                    // 对于测斜管的工作表，其工作表名与测点名称相同
                    points.Add(new MonitorEntityExcel(
                        sheetName: shtName,
                        fieldName: shtName.Remove(shtName.Length - 1, 1),
                        isSheetField: true));
                }
            }

            // 最后检查无误，开始刷新界面
            BindedExcelPoints.Clear();
            BindedExcelPoints.AddRange(points);
        }

        /// <summary>
        /// 检查工作表中的字段是否符合要求，并获取工作表中除第一个字段“时间”以外的所有字段的名称
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"> 要在哪一个工作表中提取字段信息，表名的格式为“Sheet1$”</param>
        /// <remarks></remarks>
        private IList<string> GetPointNames(OleDbConnection conn, string tableName)
        {
            DataTable dt = conn.GetSchema("Columns", new string[] { null, null, tableName });

            // MessageBox.Show( DataTableHelper.PrintTableOrView(dt, "tableName").ToString());

            // 检查工作表中是否有有效的字段
            if (dt.Rows.Count <= 0)
            {
                return new List<string>();
            }

            // 检查工作表中是否有“时间”字段
            var fieldNames = DataTableHelper.GetValue(dt, "COLUMN_NAME");
            var indexTime = fieldNames.IndexOf(PrimaryKeyName);
            if (indexTime < 0)
            {
                return new List<string>();
            }

            // 检查工作表中“时间”字段的数据类型是否为时间类型（对应的数值为5）
            int dataTypeIndex;
            int.TryParse(dt.Rows[indexTime]["DATA_TYPE"].ToString(), out dataTypeIndex);
            if (dataTypeIndex != 7)
            {
                return new List<string>();
            }

            // 一切检查通过，开始提取字段名
            fieldNames.RemoveAt(indexTime);

            return fieldNames.Select(r => r.ToString()).ToList();
        }

        #endregion

        #region ---   DataGridView 界面UI操作与集合数据更新

        private void DataGridViewExcelOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string columnName = dataGridViewExcel.Columns[e.ColumnIndex].Name;

            // 绘制指定监测点的曲线图
            if (e.RowIndex >= 0 && columnName == "ColumnDrawData")
            {
                var rData = (MonitorEntityExcel)dataGridViewExcel.Rows[e.RowIndex].DataBoundItem;

                if (rData.IsSheetField)
                {
                    // 说明不能绘图
                    DataGridViewButtonCell btn = (DataGridViewButtonCell)dataGridViewExcel.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    btn.UseColumnTextForButtonValue = false;
                }
                else
                {
                    DrawMonitorData(excelConnection, rData.SheetName, rData.FieldName);
                }
            }
        }


        private void DataGridViewExcelOnCurrentCellDirtyStateChanged(object sender, EventArgs eventArgs)
        {
            if (dataGridViewExcel.CurrentCell.GetType() == typeof(DataGridViewCheckBoxCell)
                && dataGridViewExcel.IsCurrentCellDirty)
            {
                dataGridViewExcel.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridViewExcelOnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            string columnName = dataGridViewExcel.Columns[e.ColumnIndex].Name;

            // 在修改是否要将此测点导入到Revit中时，改变对应的测点匹配项
            if (e.RowIndex >= 0 && columnName == "ColumnImport")
            {
                DataGridViewCheckBoxCell checkBox = (DataGridViewCheckBoxCell)dataGridViewExcel.Rows[e.RowIndex].Cells["ColumnImport"];
                DataGridViewComboBoxCell cmb = (DataGridViewComboBoxCell)dataGridViewExcel.Rows[e.RowIndex].Cells["ColumnMappedItem"];
                //
                if ((bool)checkBox.Value)
                {
                    var rData = (MonitorEntityExcel)dataGridViewExcel.Rows[e.RowIndex].DataBoundItem;
                    rData.MatchInstrum(selectedInstrum);
                    dataGridViewExcel.Refresh();
                }
                else
                {
                    // 也可以直接通过 cmb.Value = null; 来实现，因为 MappedItem 属性与此组合列表框的选择项是绑定的
                    var rData = (MonitorEntityExcel)dataGridViewExcel.Rows[e.RowIndex].DataBoundItem;
                    rData.MappedItem = null;
                    dataGridViewExcel.Refresh();
                }

            }
        }

        private void DataGridViewExcelOnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                MessageBox.Show($"不能转换为指定类型的数据: \n\r ", @"数据格式转换出错。", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        private void DrawMonitorData(OleDbConnection conn, string sheetName, string fieldName)
        {
            DataTable dt = ExcelDbHelper.GetDataFromExcel(conn, sheetName, PrimaryKeyName, fieldName);
            var f = new Chart_MonitorData(InstrumentationType.点测点);
            Series s = f.AddLineSeries(fieldName);
            s.Points.DataBindXY(DataTableHelper.GetValue(dt, PrimaryKeyName), DataTableHelper.GetValue(dt, fieldName));
            f.ShowDialog();
        }

        #endregion

        #region ---   一般事件处理

        /// <summary> 关闭窗口事件 </summary>
        private void DataImport_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (excelConnection != null && excelConnection.State == ConnectionState.Open)
            {
                excelConnection.Close();
            }
        }

        #endregion
    }
}
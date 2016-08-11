#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
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
        private InstrumCollector _selectedInstrum;

        /// <summary> 与DataGridView相绑定的数据源集合 </summary>
        private BindingList<MonitorEntityExcel> _BindedExcelPoints;

        /// <summary> Excel数据库的连接 </summary>
        OleDbConnection _excelConnection;

        /// <summary> 执行数据导入操作的后台线程 </summary>
        readonly BackgroundWorker _backgroundWorker;
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
            _selectedInstrum = new InstrumCollector(eleIdCollection);

            //
            _BindedExcelPoints = new BindingList<MonitorEntityExcel>();
            ConstructdataGridViewExcel();

            //

            // BackgroundWorker 的设置
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;

            // 事件关联
            _backgroundWorker.DoWork += BackgroundWorkerOnDoWork_ImportFromExcel;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;
            _backgroundWorker.ProgressChanged += BackgroundWorkerOnProgressChanged;
        }

        #region ---   DataGridView的创建与事件处理

        private void ConstructdataGridViewExcel()
        {
            //-------------------- 设置数据源的集合 -------------------------------------
            _BindedExcelPoints.AllowNew = false;
            dataGridViewExcel.AllowUserToAddRows = false;

            // AutoGenerateColumns属性要在为DataSource赋值之前进行设置；
            // 如果AutoGenerateColumns 为False，那么当设置DataSource 后，用户必须要手动为指定的属性值添加数据列。参考：第六篇2.1.6的AutoGenerateColumns：自动生成数据列。
            dataGridViewExcel.AutoGenerateColumns = false;
            dataGridViewExcel.DataSource = _BindedExcelPoints;


            //-------- 将已有的数据源集合中的每一个元素的不同属性在不同的列中显示出来 -------
            //
            // ColumnImport
            // 
            var ColumnImport = new DataGridViewCheckBoxColumn();
            ColumnImport.Name = "ColumnImport";
            ColumnImport.DataPropertyName = "Transport";
            ColumnImport.HeaderText = "导入";
            ColumnImport.Width = 50;
            ColumnImport.Resizable = DataGridViewTriState.False;
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
            ColumnDrawData.Resizable = DataGridViewTriState.False;
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
            ColumnMappedItem.Name = "ColumnMappedItem";
            ColumnMappedItem.DataPropertyName = "MappedItem";
            ColumnMappedItem.HeaderText = "目标";
            //
            ColumnMappedItem.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
            ColumnMappedItem.ValueMember = LstbxValue<Instrumentation>.ValueMember;
            ColumnMappedItem.DataSource = _document.GetComboboxDatasource(_selectedInstrum.AllInstrumentations); ;
            ColumnMappedItem.Width = 250;

            this.dataGridViewExcel.Columns.AddRange(new DataGridViewColumn[]
            {
                ColumnImport,
                ColumnDrawData,
                ColumnSheet,
                ColumnField,
                ColumnMappedItem
            });

            // --------------- 控件属性设置  --------------- 
            dataGridViewExcel.AutoSize = false;
            dataGridViewExcel.AllowUserToResizeRows = false;
            dataGridViewExcel.EditMode = DataGridViewEditMode.EditOnEnter; // 为了解决DataGridViewComboBoxCell要点击三次才出现下拉框的问题。

            // --------------- 事件关联  --------------- 
            dataGridViewExcel.DataError += DataGridViewExcelOnDataError;
            dataGridViewExcel.CellContentClick += DataGridViewExcelOnCellContentClick;
            dataGridViewExcel.CurrentCellDirtyStateChanged += DataGridViewExcelOnCurrentCellDirtyStateChanged;
        }

        #endregion

        #region ---   Mapping 数据库字段与测点的匹配映射

        /// <summary> 准备将选择的工作表中的监测数据输入Datagridview控件，并重新刷新界面 </summary>
        /// <param name="sender"></param> <param name="e"></param>
        private void buttonMapping_Click(object sender, EventArgs e)
        {
            string workbookPath = textBoxFilePath.Text;
            if (File.Exists(workbookPath))
            {
                string strExt = Path.GetExtension(workbookPath);
                if (string.Compare(strExt, ".xlsx", StringComparison.OrdinalIgnoreCase) == 0
                    || string.Compare(strExt, ".xls", StringComparison.OrdinalIgnoreCase) == 0
                    || string.Compare(strExt, ".xlsb", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Mapping(workbookPath);

                    // 刷新界面
                    ButtonImport.Enabled = true;
                    buttonCheckMultiple.Visible = true;
                    buttonUnCheckMultiple.Visible = true;

                    // 刷新进度条
                    labelProgress.Text = "";
                    progressBar1.Value = 0;

                    // 下次再按Enter就执行导入操作
                    AcceptButton = ButtonImport;
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
            if (_excelConnection != null && _excelConnection.State == ConnectionState.Open)
            {
                _excelConnection.Close();
            }
            // 打开新的数据库连接
            _excelConnection = ExcelDbHelper.ConnectToExcel(WorkbookName, 1);
            _excelConnection.Open();

            //

            List<MonitorEntityExcel> points = new List<MonitorEntityExcel>();

            // 工作簿中所有工作表的名称
            var sheetsName = ExcelDbHelper.GetSheetsName(_excelConnection);

            foreach (var shtName in sheetsName)
            {
                // 如果工作表的名称能够匹配出相对的测点类型，则开始搜索字段
                if (ExcelMapping.MultiPointsInSheet(shtName.Substring(0, shtName.Length - 1)))
                {
                    var ptNames = GetPointNames(_excelConnection, shtName);
                    foreach (var ptname in ptNames)
                    {
                        points.Add(new MonitorEntityExcel(
                            sheetName: shtName, 
                            fieldName: ptname,
                            storedInField: true,
                            possibleMatches: _selectedInstrum));
                    }
                }
                else
                {
                    // 对于测斜管的工作表，其工作表名与测点名称相同
                    points.Add(new MonitorEntityExcel(
                        sheetName: shtName,
                        fieldName: shtName.Substring(0, shtName.Length - 1),
                        storedInField: false,
                        possibleMatches: _selectedInstrum));
                }
            }

            // 最后检查无误，开始刷新界面
            _BindedExcelPoints.Clear();
            _BindedExcelPoints.AddRange(points);
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
            var indexTime = fieldNames.IndexOf(Constants.ExcelDatabasePrimaryKeyName);
            if (indexTime < 0)
            {
                return new List<string>();
            }

            // 检查工作表中“时间”字段的数据类型是否为时间类型（对应的数值为7，另外，整数或者小数对应的为5，字符对应130）
            int dataTypeIndex;
            int.TryParse(dt.Rows[indexTime]["DATA_TYPE"].ToString(), out dataTypeIndex);
            if (dataTypeIndex != 7)
            {
                Debug.Print(@"Excel 工作表中“时间”字段的数据类型不是DateTime。");
                // 但是由于 Excel 中并无严格的数据类型的概念，很容易出现异常的情况，所以这里就不报错了。
                // return new List<string>();
            }

            // 一切检查通过，开始提取字段名
            fieldNames.RemoveAt(indexTime);

            return fieldNames.Select(r => r.ToString()).ToList();
        }

        #endregion

        #region ---   DataGridView 界面UI操作与集合数据更新

        /// <summary> 选择多个 </summary>
        private void buttonCheckMultiple_Click(object sender, EventArgs e)
        {
            if (dataGridViewExcel != null && dataGridViewExcel.DataSource != null)
            {
                // 如果没有选择任何行，则将所有行进行勾选
                if (dataGridViewExcel.SelectedRows.Count == 0)
                {
                    foreach (DataGridViewRow r in dataGridViewExcel.Rows)
                    {
                        var rData = (MonitorEntityExcel)r.DataBoundItem;
                        rData.Transport = true;
                    }
                }
                else
                {
                    foreach (DataGridViewRow r in dataGridViewExcel.SelectedRows)
                    {
                        var rData = (MonitorEntityExcel)r.DataBoundItem;
                        rData.Transport = true;
                    }
                }
                // 刷新界面
                dataGridViewExcel.Refresh();
            }
        }

        /// <summary> 取消选择多个 </summary>
        private void buttonUnCheckMultiple_Click(object sender, EventArgs e)
        {
            // 如果没有选择任何行，则将所有行进行勾选
            if (dataGridViewExcel.SelectedRows.Count == 0)
            {
                foreach (DataGridViewRow r in dataGridViewExcel.Rows)
                {
                    var rData = (MonitorEntityExcel)r.DataBoundItem;
                    rData.Transport = false;
                }
            }
            else
            {
                foreach (DataGridViewRow r in dataGridViewExcel.SelectedRows)
                {
                    var rData = (MonitorEntityExcel)r.DataBoundItem;
                    rData.Transport = false;
                }
            }
            // 刷新界面
            dataGridViewExcel.Refresh();

        }

        private void DataGridViewExcelOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            string columnName = dataGridViewExcel.Columns[e.ColumnIndex].Name;
            // 绘制指定监测点的曲线图
            if (e.RowIndex >= 0 && columnName == "ColumnDrawData")
            {
                var rData = (MonitorEntityExcel)dataGridViewExcel.Rows[e.RowIndex].DataBoundItem;

                if (rData.StoredInField)
                {
                    DrawMonitorData(_excelConnection, rData.SheetName, rData.FieldName);
                }
                else
                {
                    // 说明不能绘图
                    DataGridViewButtonCell btn = (DataGridViewButtonCell)dataGridViewExcel.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    btn.UseColumnTextForButtonValue = false;
                }
            }
        }

        /// <summary> 修改 Checkbox 是否要导入 </summary>
        private void DataGridViewExcelOnCurrentCellDirtyStateChanged(object sender, EventArgs eventArgs)
        {
            if (dataGridViewExcel.CurrentCell.GetType() == typeof(DataGridViewCheckBoxCell)
                && dataGridViewExcel.IsCurrentCellDirty)
            {
                // 将 DataGridViewCheckBoxCell 的最新结果进行提交
                dataGridViewExcel.CommitEdit(DataGridViewDataErrorContexts.Commit);
                dataGridViewExcel.Refresh();
            }
        }

        private void DataGridViewExcelOnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                MessageBox.Show($"Excel监测数据导入界面中的Datagridview控件出错: \n\r ", @"DataError。", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        /// <summary>
        /// 临时绘制监测曲线图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sheetName"></param>
        /// <param name="fieldName"></param>
        private void DrawMonitorData(OleDbConnection conn, string sheetName, string fieldName)
        {
            DataTable dt = ExcelDbHelper.GetFieldDataFromExcel(conn, sheetName, Constants.ExcelDatabasePrimaryKeyName, fieldName);
            var f = new Chart_MonitorData(InstrumentationType.其他点测点);
            Series s = f.AddLineSeries(fieldName);
            s.Points.DataBindXY(DataTableHelper.GetValue(dt, Constants.ExcelDatabasePrimaryKeyName), DataTableHelper.GetValue(dt, fieldName));
            f.ShowDialog();
        }

        #endregion

        #region ---   监测数据的导入

        /// <summary> 事务：将Excel中的监测数据导入Revit中的测点单元 </summary>
        Transaction _tranImport;

        private void ButtonImport_Click(object sender, EventArgs e)
        {
            if (dataGridViewExcel != null && dataGridViewExcel.DataSource != null)
            {      // 首先检测有没有出现Revit中的某一个
                int errorRow;
                if (CheckDuplicated(dataGridViewExcel.DataSource as BindingList<MonitorEntityExcel>, out errorRow))
                {
                    MessageBox.Show($"除了“墙顶位移”外，其余的测点都只能对应一个字段。\n 第{errorRow + 1}行出错。");
                }
                else
                {
                    var datasource = dataGridViewExcel.DataSource as BindingList<MonitorEntityExcel>;
                    if (datasource != null && datasource.Count > 0)
                    {
                        if (!_backgroundWorker.IsBusy)
                        {
                            // 开始导入监测数据
                            // 在Revit上下文的线程中开启Revit事务
                            _tranImport = new Transaction(_document.Document, "将Excel中的监测数据导入Revit中的测点单元");
                            _tranImport.Start();

                            // 界面初始化
                            labelProgress.Visible = true;
                            labelProgress.Text = "";

                            // Start the asynchronous operation.
                            _backgroundWorker.RunWorkerAsync(argument: datasource);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查列表框中有没有出现Revit中的一个元素（除了“墙顶位移”外）对应的Excel中的多个数据字段的情况。
        /// </summary>
        /// <param name="datasource"></param>
        /// <param name="row"> 在执行到第row行时退出了循环 </param>
        /// <returns></returns>
        private bool CheckDuplicated(BindingList<MonitorEntityExcel> datasource, out int row)
        {
            row = -1;
            // 首先检测有没有出现Revit中的某一个
            if (datasource != null)
            {
                Dictionary<Instrum_WallTop, byte> wallTop = new Dictionary<Instrum_WallTop, byte>();
                List<Instrumentation> linkedInstrumentations = new List<Instrumentation>();

                for (row = 0; row < datasource.Count; row++)
                {
                    var mappedItem = datasource[row].MappedItem;
                    if (mappedItem != null)
                    {
                        if (linkedInstrumentations.Contains(mappedItem))
                        {
                            // 对于墙顶位移进行特殊处理，因为它在Revit中是线测点，而在Excel中是以两个点测点来存储的。
                            if ((mappedItem is Instrum_WallTop))
                            {
                                // 此墙顶位移测点肯定已经被添加到wallTop集合中了。
                                if (wallTop[(Instrum_WallTop)mappedItem] == 2)
                                {
                                    return true;
                                }
                                else
                                {
                                    wallTop[(Instrum_WallTop)mappedItem] += 1;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }

                        else  // 说明此项第一次出现
                        {
                            linkedInstrumentations.Add(mappedItem);
                            // 对于墙顶位移进行特殊处理，因为它在Revit中是线测点，而在Excel中是以两个点测点来存储的。
                            if ((mappedItem is Instrum_WallTop))
                            {
                                wallTop.Add((Instrum_WallTop)mappedItem, 1);
                            }
                        }
                    }
                } // 下一个测点
            }
            return false;
        }


        /// <summary>
        /// 开始在后台线程中实际执行测点数据从Excel导入Revit的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"> 输入前请确保 e.Argument所对应的集合的Count > 0 </param>
        private void BackgroundWorkerOnDoWork_ImportFromExcel(object sender, DoWorkEventArgs e)
        {
            // 
            BindingList<MonitorEntityExcel> datasource = e.Argument as BindingList<MonitorEntityExcel>;
            int count = datasource.Count;
            // 
            MonitorEntityExcel monitorEntity;  // 每一个要导入的数据库字段与Revit测点信息
            Instrumentation ins;
            int row = 0;
            try
            {
                // tran.Start();
                for (row = 0; row < count; row++)
                {
                    monitorEntity = datasource[row];
                    if (monitorEntity.Transport && monitorEntity.MappedItem != null)  // 进行此字段的数据导入的必要条件
                    {
                        // 执行监测数据的导入操作
                        ins = monitorEntity.MappedItem;
                        ins.ImportFromExcel(
                            tran: _tranImport,
                            conn: _excelConnection,
                            sheetName: monitorEntity.SheetName,
                            fieldName: monitorEntity.FieldName);
                    }
                    _backgroundWorker.ReportProgress((int)((float)row / count * 100));
                }
                // _tranImport.Commit();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                // _tranImport.RollBack();
                DebugUtils.ShowDebugCatch(ex, $"将Excel中的监测数据导入Revit中的测点单元出错，出错行：{row + 1} 。");
            }
        }

        private void BackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelProgress.Text = e.ProgressPercentage + @"%";
            progressBar1.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            { // 不报错，则提交
                _tranImport.Commit();
                // 刷新界面
                Thread.Sleep(10);
                labelProgress.Text = @"100%";
                progressBar1.Value = 100;


                // 下次再按Enter就执行导入操作
                AcceptButton = buttonMapping;
            }
            else
            { // 报错则回滚
                _tranImport.RollBack();
            }
        }

        #endregion

        #region ---   一般事件处理

        /// <summary> 关闭窗口事件 </summary>
        private void DataImport_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_excelConnection != null && _excelConnection.State == ConnectionState.Open)
            {
                _excelConnection.Close();
            }
            //
            _backgroundWorker.Dispose();
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonChooseFile_Click(object sender, EventArgs e)
        {
            string filePath = WindowsUtil.ChooseOpenExcel("选择Excel数据库");
            if (!string.IsNullOrEmpty(filePath))
            {
                textBoxFilePath.Text = filePath;
            }
        }
        #endregion


    }
}
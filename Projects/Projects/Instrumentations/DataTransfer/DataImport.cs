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

        /// <summary> �����������ݹ�����ʱ������ѡ��Ĳ�� </summary>
        private InstrumCollector _selectedInstrum;

        /// <summary> ��DataGridView��󶨵�����Դ���� </summary>
        private BindingList<MonitorEntityExcel> _BindedExcelPoints;

        /// <summary> Excel���ݿ������ </summary>
        OleDbConnection _excelConnection;

        /// <summary> ִ�����ݵ�������ĺ�̨�߳� </summary>
        readonly BackgroundWorker _backgroundWorker;
        #endregion

        /// <summary> ���캯�� </summary>
        /// <param name="eleIdCollection">����Ҫ���д���Ĳ��Ԫ�ص�Id����</param>
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

            // BackgroundWorker ������
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;

            // �¼�����
            _backgroundWorker.DoWork += BackgroundWorkerOnDoWork_ImportFromExcel;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;
            _backgroundWorker.ProgressChanged += BackgroundWorkerOnProgressChanged;
        }

        #region ---   DataGridView�Ĵ������¼�����

        private void ConstructdataGridViewExcel()
        {
            //-------------------- ��������Դ�ļ��� -------------------------------------
            _BindedExcelPoints.AllowNew = false;
            dataGridViewExcel.AllowUserToAddRows = false;

            // AutoGenerateColumns����Ҫ��ΪDataSource��ֵ֮ǰ�������ã�
            // ���AutoGenerateColumns ΪFalse����ô������DataSource ���û�����Ҫ�ֶ�Ϊָ��������ֵ��������С��ο�������ƪ2.1.6��AutoGenerateColumns���Զ����������С�
            dataGridViewExcel.AutoGenerateColumns = false;
            dataGridViewExcel.DataSource = _BindedExcelPoints;


            //-------- �����е�����Դ�����е�ÿһ��Ԫ�صĲ�ͬ�����ڲ�ͬ��������ʾ���� -------
            //
            // ColumnImport
            // 
            var ColumnImport = new DataGridViewCheckBoxColumn();
            ColumnImport.Name = "ColumnImport";
            ColumnImport.DataPropertyName = "Transport";
            ColumnImport.HeaderText = "����";
            ColumnImport.Width = 50;
            ColumnImport.Resizable = DataGridViewTriState.False;
            // 
            // ColumnDrawData
            // 
            DataGridViewButtonColumn ColumnDrawData;
            ColumnDrawData = new DataGridViewButtonColumn();
            ColumnDrawData.Name = "ColumnDrawData";
            ColumnDrawData.HeaderText = "�鿴";
            ColumnDrawData.Text = "�鿴";
            ColumnDrawData.UseColumnTextForButtonValue = true;
            ColumnDrawData.Width = 50;
            ColumnDrawData.Resizable = DataGridViewTriState.False;
            // 
            // ColumnSheet
            // 
            DataGridViewTextBoxColumn ColumnSheet = new DataGridViewTextBoxColumn();
            ColumnSheet.DataPropertyName = "SheetName";
            ColumnSheet.HeaderText = "������";
            ColumnSheet.Name = "ColumnSheet";
            ColumnSheet.ReadOnly = true;
            // 
            // ColumnField
            // 
            DataGridViewTextBoxColumn ColumnField = new DataGridViewTextBoxColumn();
            ColumnField.DataPropertyName = "FieldName";
            ColumnField.HeaderText = "���";
            ColumnField.Name = "ColumnField";
            ColumnField.ReadOnly = true;
            // 
            // ColumnMappedItem
            // 
            DataGridViewComboBoxColumn ColumnMappedItem = new DataGridViewComboBoxColumn();
            ColumnMappedItem.Name = "ColumnMappedItem";
            ColumnMappedItem.DataPropertyName = "MappedItem";
            ColumnMappedItem.HeaderText = "Ŀ��";
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

            // --------------- �ؼ���������  --------------- 
            dataGridViewExcel.AutoSize = false;
            dataGridViewExcel.AllowUserToResizeRows = false;
            dataGridViewExcel.EditMode = DataGridViewEditMode.EditOnEnter; // Ϊ�˽��DataGridViewComboBoxCellҪ������βų�������������⡣

            // --------------- �¼�����  --------------- 
            dataGridViewExcel.DataError += DataGridViewExcelOnDataError;
            dataGridViewExcel.CellContentClick += DataGridViewExcelOnCellContentClick;
            dataGridViewExcel.CurrentCellDirtyStateChanged += DataGridViewExcelOnCurrentCellDirtyStateChanged;
        }

        #endregion

        #region ---   Mapping ���ݿ��ֶ������ƥ��ӳ��

        /// <summary> ׼����ѡ��Ĺ������еļ����������Datagridview�ؼ���������ˢ�½��� </summary>
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

                    // ˢ�½���
                    ButtonImport.Enabled = true;
                    buttonCheckMultiple.Visible = true;
                    buttonUnCheckMultiple.Visible = true;

                    // ˢ�½�����
                    labelProgress.Text = "";
                    progressBar1.Value = 0;

                    // �´��ٰ�Enter��ִ�е������
                    AcceptButton = ButtonImport;
                }
                else
                {
                    MessageBox.Show(@"��ѡ����ʵ�Excel��������������ӳ�䡣", @"�ļ�����׺��ƥ��", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(@"��ѡ����ʵ�Excel��������������ӳ�䡣", @"�ļ�δ�ҵ�", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary> ��ѡ��Ĺ������еļ����������Datagridview�ؼ���������ˢ�½��� </summary>
        /// <param name="WorkbookName"> ������ݹ������ľ���·�� </param>
        private void Mapping(string WorkbookName)
        {
            // �ȹر�ǰ������ݿ�����
            if (_excelConnection != null && _excelConnection.State == ConnectionState.Open)
            {
                _excelConnection.Close();
            }
            // ���µ����ݿ�����
            _excelConnection = ExcelDbHelper.ConnectToExcel(WorkbookName, 1);
            _excelConnection.Open();

            //

            List<MonitorEntityExcel> points = new List<MonitorEntityExcel>();

            // �����������й����������
            var sheetsName = ExcelDbHelper.GetSheetsName(_excelConnection);

            foreach (var shtName in sheetsName)
            {
                // ���������������ܹ�ƥ�����ԵĲ�����ͣ���ʼ�����ֶ�
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
                    // ���ڲ�б�ܵĹ������乤����������������ͬ
                    points.Add(new MonitorEntityExcel(
                        sheetName: shtName,
                        fieldName: shtName.Substring(0, shtName.Length - 1),
                        storedInField: false,
                        possibleMatches: _selectedInstrum));
                }
            }

            // ��������󣬿�ʼˢ�½���
            _BindedExcelPoints.Clear();
            _BindedExcelPoints.AddRange(points);
        }

        /// <summary>
        /// ��鹤�����е��ֶ��Ƿ����Ҫ�󣬲���ȡ�������г���һ���ֶΡ�ʱ�䡱����������ֶε�����
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"> Ҫ����һ������������ȡ�ֶ���Ϣ�������ĸ�ʽΪ��Sheet1$��</param>
        /// <remarks></remarks>
        private IList<string> GetPointNames(OleDbConnection conn, string tableName)
        {
            DataTable dt = conn.GetSchema("Columns", new string[] { null, null, tableName });

            // MessageBox.Show( DataTableHelper.PrintTableOrView(dt, "tableName").ToString());

            // ��鹤�������Ƿ�����Ч���ֶ�
            if (dt.Rows.Count <= 0)
            {
                return new List<string>();
            }

            // ��鹤�������Ƿ��С�ʱ�䡱�ֶ�
            var fieldNames = DataTableHelper.GetValue(dt, "COLUMN_NAME");
            var indexTime = fieldNames.IndexOf(Constants.ExcelDatabasePrimaryKeyName);
            if (indexTime < 0)
            {
                return new List<string>();
            }

            // ��鹤�����С�ʱ�䡱�ֶε����������Ƿ�Ϊʱ�����ͣ���Ӧ����ֵΪ7�����⣬��������С����Ӧ��Ϊ5���ַ���Ӧ130��
            int dataTypeIndex;
            int.TryParse(dt.Rows[indexTime]["DATA_TYPE"].ToString(), out dataTypeIndex);
            if (dataTypeIndex != 7)
            {
                Debug.Print(@"Excel �������С�ʱ�䡱�ֶε��������Ͳ���DateTime��");
                // �������� Excel �в����ϸ���������͵ĸ�������׳����쳣���������������Ͳ������ˡ�
                // return new List<string>();
            }

            // һ�м��ͨ������ʼ��ȡ�ֶ���
            fieldNames.RemoveAt(indexTime);

            return fieldNames.Select(r => r.ToString()).ToList();
        }

        #endregion

        #region ---   DataGridView ����UI�����뼯�����ݸ���

        /// <summary> ѡ���� </summary>
        private void buttonCheckMultiple_Click(object sender, EventArgs e)
        {
            if (dataGridViewExcel != null && dataGridViewExcel.DataSource != null)
            {
                // ���û��ѡ���κ��У��������н��й�ѡ
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
                // ˢ�½���
                dataGridViewExcel.Refresh();
            }
        }

        /// <summary> ȡ��ѡ���� </summary>
        private void buttonUnCheckMultiple_Click(object sender, EventArgs e)
        {
            // ���û��ѡ���κ��У��������н��й�ѡ
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
            // ˢ�½���
            dataGridViewExcel.Refresh();

        }

        private void DataGridViewExcelOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            string columnName = dataGridViewExcel.Columns[e.ColumnIndex].Name;
            // ����ָ�����������ͼ
            if (e.RowIndex >= 0 && columnName == "ColumnDrawData")
            {
                var rData = (MonitorEntityExcel)dataGridViewExcel.Rows[e.RowIndex].DataBoundItem;

                if (rData.StoredInField)
                {
                    DrawMonitorData(_excelConnection, rData.SheetName, rData.FieldName);
                }
                else
                {
                    // ˵�����ܻ�ͼ
                    DataGridViewButtonCell btn = (DataGridViewButtonCell)dataGridViewExcel.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    btn.UseColumnTextForButtonValue = false;
                }
            }
        }

        /// <summary> �޸� Checkbox �Ƿ�Ҫ���� </summary>
        private void DataGridViewExcelOnCurrentCellDirtyStateChanged(object sender, EventArgs eventArgs)
        {
            if (dataGridViewExcel.CurrentCell.GetType() == typeof(DataGridViewCheckBoxCell)
                && dataGridViewExcel.IsCurrentCellDirty)
            {
                // �� DataGridViewCheckBoxCell �����½�������ύ
                dataGridViewExcel.CommitEdit(DataGridViewDataErrorContexts.Commit);
                dataGridViewExcel.Refresh();
            }
        }

        private void DataGridViewExcelOnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                MessageBox.Show($"Excel������ݵ�������е�Datagridview�ؼ�����: \n\r ", @"DataError��", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        /// <summary>
        /// ��ʱ���Ƽ������ͼ
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sheetName"></param>
        /// <param name="fieldName"></param>
        private void DrawMonitorData(OleDbConnection conn, string sheetName, string fieldName)
        {
            DataTable dt = ExcelDbHelper.GetFieldDataFromExcel(conn, sheetName, Constants.ExcelDatabasePrimaryKeyName, fieldName);
            var f = new Chart_MonitorData(InstrumentationType.��������);
            Series s = f.AddLineSeries(fieldName);
            s.Points.DataBindXY(DataTableHelper.GetValue(dt, Constants.ExcelDatabasePrimaryKeyName), DataTableHelper.GetValue(dt, fieldName));
            f.ShowDialog();
        }

        #endregion

        #region ---   ������ݵĵ���

        /// <summary> ���񣺽�Excel�еļ�����ݵ���Revit�еĲ�㵥Ԫ </summary>
        Transaction _tranImport;

        private void ButtonImport_Click(object sender, EventArgs e)
        {
            if (dataGridViewExcel != null && dataGridViewExcel.DataSource != null)
            {      // ���ȼ����û�г���Revit�е�ĳһ��
                int errorRow;
                if (CheckDuplicated(dataGridViewExcel.DataSource as BindingList<MonitorEntityExcel>, out errorRow))
                {
                    MessageBox.Show($"���ˡ�ǽ��λ�ơ��⣬����Ĳ�㶼ֻ�ܶ�Ӧһ���ֶΡ�\n ��{errorRow + 1}�г���");
                }
                else
                {
                    var datasource = dataGridViewExcel.DataSource as BindingList<MonitorEntityExcel>;
                    if (datasource != null && datasource.Count > 0)
                    {
                        if (!_backgroundWorker.IsBusy)
                        {
                            // ��ʼ����������
                            // ��Revit�����ĵ��߳��п���Revit����
                            _tranImport = new Transaction(_document.Document, "��Excel�еļ�����ݵ���Revit�еĲ�㵥Ԫ");
                            _tranImport.Start();

                            // �����ʼ��
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
        /// ����б������û�г���Revit�е�һ��Ԫ�أ����ˡ�ǽ��λ�ơ��⣩��Ӧ��Excel�еĶ�������ֶε������
        /// </summary>
        /// <param name="datasource"></param>
        /// <param name="row"> ��ִ�е���row��ʱ�˳���ѭ�� </param>
        /// <returns></returns>
        private bool CheckDuplicated(BindingList<MonitorEntityExcel> datasource, out int row)
        {
            row = -1;
            // ���ȼ����û�г���Revit�е�ĳһ��
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
                            // ����ǽ��λ�ƽ������⴦����Ϊ����Revit�����߲�㣬����Excel�����������������洢�ġ�
                            if ((mappedItem is Instrum_WallTop))
                            {
                                // ��ǽ��λ�Ʋ��϶��Ѿ�����ӵ�wallTop�������ˡ�
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

                        else  // ˵�������һ�γ���
                        {
                            linkedInstrumentations.Add(mappedItem);
                            // ����ǽ��λ�ƽ������⴦����Ϊ����Revit�����߲�㣬����Excel�����������������洢�ġ�
                            if ((mappedItem is Instrum_WallTop))
                            {
                                wallTop.Add((Instrum_WallTop)mappedItem, 1);
                            }
                        }
                    }
                } // ��һ�����
            }
            return false;
        }


        /// <summary>
        /// ��ʼ�ں�̨�߳���ʵ��ִ�в�����ݴ�Excel����Revit�Ĳ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"> ����ǰ��ȷ�� e.Argument����Ӧ�ļ��ϵ�Count > 0 </param>
        private void BackgroundWorkerOnDoWork_ImportFromExcel(object sender, DoWorkEventArgs e)
        {
            // 
            BindingList<MonitorEntityExcel> datasource = e.Argument as BindingList<MonitorEntityExcel>;
            int count = datasource.Count;
            // 
            MonitorEntityExcel monitorEntity;  // ÿһ��Ҫ��������ݿ��ֶ���Revit�����Ϣ
            Instrumentation ins;
            int row = 0;
            try
            {
                // tran.Start();
                for (row = 0; row < count; row++)
                {
                    monitorEntity = datasource[row];
                    if (monitorEntity.Transport && monitorEntity.MappedItem != null)  // ���д��ֶε����ݵ���ı�Ҫ����
                    {
                        // ִ�м�����ݵĵ������
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
                DebugUtils.ShowDebugCatch(ex, $"��Excel�еļ�����ݵ���Revit�еĲ�㵥Ԫ���������У�{row + 1} ��");
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
            { // ���������ύ
                _tranImport.Commit();
                // ˢ�½���
                Thread.Sleep(10);
                labelProgress.Text = @"100%";
                progressBar1.Value = 100;


                // �´��ٰ�Enter��ִ�е������
                AcceptButton = buttonMapping;
            }
            else
            { // ������ع�
                _tranImport.RollBack();
            }
        }

        #endregion

        #region ---   һ���¼�����

        /// <summary> �رմ����¼� </summary>
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
            string filePath = WindowsUtil.ChooseOpenExcel("ѡ��Excel���ݿ�");
            if (!string.IsNullOrEmpty(filePath))
            {
                textBoxFilePath.Text = filePath;
            }
        }
        #endregion


    }
}
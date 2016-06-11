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

        /// <summary> �����������ݹ�����ʱ������ѡ��Ĳ�� </summary>
        private InstrumDoc.InstrumCollector selectedInstrum;

        /// <summary> ��DataGridView��󶨵�����Դ���� </summary>
        private BindingList<MonitorEntityExcel> BindedExcelPoints;

        /// <summary> Excel���ݿ������ </summary>
        OleDbConnection excelConnection;

        /// <summary>  Excel���ݿ��У�ÿһ��������ĵ�һ���ֶ����Ʊ����ǡ�ʱ�䡱 </summary>
        private const string PrimaryKeyName = "ʱ��";

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
            selectedInstrum = new InstrumDoc.InstrumCollector(eleIdCollection);

            //
            BindedExcelPoints = new BindingList<MonitorEntityExcel>();
            ConstructdataGridViewExcel();
        }

        #region ---   DataGridView�Ĵ������¼�����

        private void ConstructdataGridViewExcel()
        {
            //-------------------- ��������Դ�ļ��� -------------------------------------
            BindedExcelPoints.AllowNew = false;
            dataGridViewExcel.AllowUserToAddRows = false;

            // AutoGenerateColumns����Ҫ��ΪDataSource��ֵ֮ǰ�������ã�
            // ���AutoGenerateColumns ΪFalse����ô������DataSource ���û�����Ҫ�ֶ�Ϊָ��������ֵ��������С��ο�������ƪ2.1.6��AutoGenerateColumns���Զ����������С�
            dataGridViewExcel.AutoGenerateColumns = false;
            dataGridViewExcel.AutoSize = false;
            dataGridViewExcel.DataSource = BindedExcelPoints;


            //-------- �����е�����Դ�����е�ÿһ��Ԫ�صĲ�ͬ�����ڲ�ͬ��������ʾ���� -------
            //
            // ColumnImport
            // 
            var ColumnImport = new DataGridViewCheckBoxColumn();
            ColumnImport.Name = "ColumnImport";
            ColumnImport.DataPropertyName = "Transport";
            ColumnImport.HeaderText = "����";
            ColumnImport.Width = 50;
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
            ColumnMappedItem.DataPropertyName = "MappedItem";
            ColumnMappedItem.Name = "ColumnMappedItem";
            ColumnMappedItem.HeaderText = "Ŀ��";

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

            // --------------- �¼�����  --------------- 
            dataGridViewExcel.DataError += DataGridViewExcelOnDataError;
            dataGridViewExcel.CellContentClick += DataGridViewExcelOnCellContentClick;
            dataGridViewExcel.CellValueChanged += DataGridViewExcelOnCellValueChanged;
            dataGridViewExcel.CurrentCellDirtyStateChanged += DataGridViewExcelOnCurrentCellDirtyStateChanged;
        }


        /// <summary>
        /// DataGridViewExcel������Ӧ��ʵ����
        /// </summary>
        internal class MonitorEntityExcel
        {
            /// <summary> ���ݿ��еĴ˲���Ƿ�Ҫ���д��䣬��������ߵ��� </summary>
            public bool Transport { get; set; }

            /// <summary> �˲����Excel����һ���������� </summary>
            public string SheetName { get; }

            /// <summary> �˲����Excel�������ڶ�Ӧ��һ���ֶ� </summary>
            public string FieldName { get; }

            /// <summary> �˲����Revit�а󶨵Ķ�Ӧ��㡣Ҳ�Ǻ���Ҫ����ʵ�ʵ����ݵ���Ķ�Ӧ��� </summary>
            public Instrumentation MappedItem { get; set; }

            /// <summary>  ����˲��������Ǳ�����һ���Ź������У������б���ݣ�����Ϊtrue��
            /// ������˲��������Ǳ�����һ���������ĳ�ֶ��£���Ϊfalse�� </summary>
            [Browsable(false)]
            public bool IsSheetField { get; }

            /// <summary> �˲������� </summary>
            [Browsable(false)]
            public InstrumentationType MonitorType { get; }

            /// <summary>
            /// ���캯��
            /// </summary>
            /// <param name="sheetName">�˲����Excel����һ����������</param>
            /// <param name="fieldName">�˲����Excel�������ڶ�Ӧ��һ���ֶ�</param>
            /// <param name="isSheetField"> ����˲��������Ǳ�����һ���Ź������У������б���ݣ�����Ϊtrue��
            /// ������˲��������Ǳ�����һ���������ĳ�ֶ��£���Ϊfalse�� </param>
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
            /// �������ݿ������Revit���ҵ�һ�����ܵ�ƥ���㵥Ԫ
            /// </summary>
            /// <param name="instrumsToMatch"> ��Revit��Ҫ����ƥ��Ĳ�㼯�� </param>
            /// <returns></returns>
            public void MatchInstrum(InstrumDoc.InstrumCollector instrumsToMatch)
            {

                // _listInstrum[2].Value
                MappedItem= instrumsToMatch.AllInstrumentations[3];
            }

        }


        #endregion

        #region ---   ���ݿ��ֶ������ƥ��ӳ��

        /// <summary> ׼����ѡ��Ĺ������еļ����������Datagridview�ؼ���������ˢ�½��� </summary>
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
            if (excelConnection != null && excelConnection.State == ConnectionState.Open)
            {
                excelConnection.Close();
            }
            // ���µ����ݿ�����
            excelConnection = ExcelDbHelper.ConnectToExcel(WorkbookName, 1);
            excelConnection.Open();

            //

            List<MonitorEntityExcel> points = new List<MonitorEntityExcel>();

            // �����������й����������
            var sheetsName = ExcelDbHelper.GetSheetsName(excelConnection);

            foreach (var shtName in sheetsName)
            {
                // ���������������ܹ�ƥ�����ԵĲ�����ͣ���ʼ�����ֶ�
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
                    // ���ڲ�б�ܵĹ������乤����������������ͬ
                    points.Add(new MonitorEntityExcel(
                        sheetName: shtName,
                        fieldName: shtName.Remove(shtName.Length - 1, 1),
                        isSheetField: true));
                }
            }

            // ��������󣬿�ʼˢ�½���
            BindedExcelPoints.Clear();
            BindedExcelPoints.AddRange(points);
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
            var indexTime = fieldNames.IndexOf(PrimaryKeyName);
            if (indexTime < 0)
            {
                return new List<string>();
            }

            // ��鹤�����С�ʱ�䡱�ֶε����������Ƿ�Ϊʱ�����ͣ���Ӧ����ֵΪ5��
            int dataTypeIndex;
            int.TryParse(dt.Rows[indexTime]["DATA_TYPE"].ToString(), out dataTypeIndex);
            if (dataTypeIndex != 7)
            {
                return new List<string>();
            }

            // һ�м��ͨ������ʼ��ȡ�ֶ���
            fieldNames.RemoveAt(indexTime);

            return fieldNames.Select(r => r.ToString()).ToList();
        }

        #endregion

        #region ---   DataGridView ����UI�����뼯�����ݸ���

        private void DataGridViewExcelOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string columnName = dataGridViewExcel.Columns[e.ColumnIndex].Name;

            // ����ָ�����������ͼ
            if (e.RowIndex >= 0 && columnName == "ColumnDrawData")
            {
                var rData = (MonitorEntityExcel)dataGridViewExcel.Rows[e.RowIndex].DataBoundItem;

                if (rData.IsSheetField)
                {
                    // ˵�����ܻ�ͼ
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

            // ���޸��Ƿ�Ҫ���˲�㵼�뵽Revit��ʱ���ı��Ӧ�Ĳ��ƥ����
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
                    // Ҳ����ֱ��ͨ�� cmb.Value = null; ��ʵ�֣���Ϊ MappedItem �����������б���ѡ�����ǰ󶨵�
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
                MessageBox.Show($"����ת��Ϊָ�����͵�����: \n\r ", @"���ݸ�ʽת������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        private void DrawMonitorData(OleDbConnection conn, string sheetName, string fieldName)
        {
            DataTable dt = ExcelDbHelper.GetDataFromExcel(conn, sheetName, PrimaryKeyName, fieldName);
            var f = new Chart_MonitorData(InstrumentationType.����);
            Series s = f.AddLineSeries(fieldName);
            s.Points.DataBindXY(DataTableHelper.GetValue(dt, PrimaryKeyName), DataTableHelper.GetValue(dt, fieldName));
            f.ShowDialog();
        }

        #endregion

        #region ---   һ���¼�����

        /// <summary> �رմ����¼� </summary>
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
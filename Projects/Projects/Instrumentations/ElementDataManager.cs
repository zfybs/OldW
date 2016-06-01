using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using OldW.Instrumentations;
using std_ez;

namespace OldW.DataManager
{
    /// <summary>
    /// 模型中的测点的监测数据的添加，删除，导入导出等
    /// </summary>
    /// <remarks></remarks>
    public partial class ElementDataManager
    {
        #region   ---  Fields

        private Document doc;

        /// <summary> 当前活动的测点对象 </summary>
        private Instrumentation ActiveInstru;

        /// <summary>
        /// 与表格数据进行绑定的点测点监测数据集合
        /// </summary>
        private BindingList<DgvBindingPointData> monitorData = new BindingList<DgvBindingPointData>();

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eleIdCollection">所有要进行处理的测点元素的Id集合</param>
        /// <param name="document"></param>
        /// <remarks></remarks>
        public ElementDataManager(ICollection<Instrumentation> eleIdCollection, Document document)
        {
            InitializeComponent();
            // --------------------------------
            this.doc = document;
            this.cmbx_elements.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
            this.cmbx_elements.ValueMember = LstbxValue<Instrumentation>.ValueMember;
            if (eleIdCollection.Count > 0)
            {
                this.cmbx_elements.DataSource = fillCombobox(eleIdCollection);
            }
            ConstructDataGridView();
        }

        #region    ---   表格控件 DataGridView

        /// <summary>
        /// 创建 DataGridView 为点测点监测数据类型
        /// </summary>
        private void ConstructDataGridView()
        {
            //-------------------- 设置数据源的集合 -------------------------------------
            monitorData.AllowNew = true;
            monitorData.AddingNew += monitorData_AddingNew;

            //-------------------- 设置数据源的集合 -------------------------------------

            // 如果AutoGenerateColumns 为False，那么当设置DataSource 后，用户必须要手动为指定的属性值添加数据列。
            DataGrid_pointMonitor.AutoGenerateColumns = false;
            DataGrid_pointMonitor.AutoSize = false;
            DataGrid_pointMonitor.AllowUserToAddRows = true;


            DataGrid_pointMonitor.DataSource = monitorData;

            //-------- 将已有的数据源集合中的每一个元素的不同属性在不同的列中显示出来 -------


            // Initialize and add a text box column.
            // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Date";
            // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.Name = "日期";
            DataGrid_pointMonitor.Columns.Add(column);

            // Initialize and add a text box column.
            // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Value";
            // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.Name = "数据";
            DataGrid_pointMonitor.Columns.Add(column);
        }

        #endregion

        #region    ---   组合列表框 ComboBox

        /// <summary>
        /// 将指定的测点集合对象添加到组合列表框中
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <returns></returns>
        private LstbxValue<Instrumentation>[] fillCombobox(ICollection<Instrumentation> elementCollection)
        {
            int c = elementCollection.Count;
            LstbxValue<Instrumentation>[] arr = new LstbxValue<Instrumentation>[c - 1 + 1];
            int i = 0;
            foreach (Instrumentation eleid in elementCollection)
            {
                arr[i] =
                    new LstbxValue<Instrumentation>(
                        eleid.Monitor.Name + "( " + eleid.getMonitorName() + " ):" +
                        eleid.Monitor.Id.IntegerValue.ToString(), eleid);
                i++;
            }
            return arr;
        }

        /// <summary>
        /// 在组合框中改变了选择的测点后，去更新DataGridView中的数据为指定测点的监测数据。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Instrumentation ele = (Instrumentation)cmbx_elements.SelectedValue;
            this.ActiveInstru = ele;
            FillTableWithElementData(ele, this.DataGrid_pointMonitor);
        }

        #endregion

        #region    ---   DataGridView中的数据与监测参数值之间的交互

        /// <summary>
        /// 将表格中的数据保存到Element的对应参数中。
        /// </summary>
        /// <remarks></remarks>
        public void SaveTableToElement(object sender, EventArgs e)
        {
            // 将表格数据构造为监测数据类，并将其保存到测点对象中
            if (ActiveInstru is Instrum_Point)
            {
                Instrum_Point pt = (Instrum_Point)ActiveInstru;

                using (Transaction tran = new Transaction(doc, "保存表格中的数据到Element的参数中"))
                {
                    tran.Start();
                    try
                    {
                        pt.SetMonitorData(tran, ConvertBindingList(monitorData));
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowDebugCatch(ex, "无法保存监测数据到对象参数中。");
                        tran.RollBack();
                    }
                }
            }
        }

        /// <summary>
        /// 将元素的数据从Revit中提取出来并写入表格
        /// </summary>
        /// <param name="ele">要进行数据提取的监测单元</param>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private void FillTableWithElementData(Instrumentation ele, DataGridView table)
        {
            //   table.Rows.Clear();  // 先将列表中的数据清空
            if (ele is Instrum_Point)
            {
                Instrum_Point pt = ele as Instrum_Point;
                List<MonitorData_Point> ptData;
                ptData = pt.GetMonitorData() ?? new List<MonitorData_Point>();
                FillBindingList(ptData, ref monitorData);
            }
        }

        /// <summary> 将 DataGridView 控件中绑定的数据集合转换为实体类 <see cref="MonitorData_Point"/> 的集合。 </summary>
        /// <remarks> BindingList 中的元素如果有测点的监测日期为null，则进行剔除。</remarks>
        private List<MonitorData_Point> ConvertBindingList(BindingList<DgvBindingPointData> monitorDataSet)
        {
            return (from variable in monitorDataSet
                    where variable.Date != null
                    select new MonitorData_Point(variable.Date.Value, variable.Value)).ToList();
        }

        /// <summary> 将 实体类 <see cref="MonitorData_Point"/> 的集合中的数据填充到 BindingList 集合中。</summary>
        private void FillBindingList(List<MonitorData_Point> monitorDataSet, ref BindingList<DgvBindingPointData> pts)
        {
            pts.Clear();
            foreach (var variable in monitorDataSet)
            {
                pts.Add(new DgvBindingPointData(variable.Date, variable.Value));
            }
        }

        /// <summary> 用来绑定到 DataGridView 的DataSource属性的监测数据类，表示点测点中的每一天的监测数据 </summary>
        /// <remarks>如果属性中包含有[System.ComponentModel.Browsable(false)]，则不计入表格项 </remarks>
        public class DgvBindingPointData
        {
            /// <summary>
            /// 监测日期
            /// </summary>
            public DateTime? Date { get; set; }

            /// <summary>
            /// 监测数据，如果当天没有数据，则为null
            /// </summary>
            public Single? Value { get; set; }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="Date"></param>
            /// <param name="Value"></param>
            public DgvBindingPointData(DateTime? Date, Single? Value)
            {
                this.Date = Date;
                this.Value = Value;
            }
        }

        #endregion

        #region    ---   绘制监测曲线图

        /// <summary>
        /// 绘制图表
        /// </summary>
        /// <param name="data"></param>
        private Chart_MonitorData DrawData(BindingList<DgvBindingPointData> data)
        {
            Chart_MonitorData Chart1 = new Chart_MonitorData(InstrumentationType.地表隆沉);

            var pts = ConvertBindingList(data);

            Chart1.Series.Points.DataBind(pts, "Date", "Value", "");

            Chart1.Show();
            return Chart1;
        }

        #endregion

        #region    ---   事件处理

        /// <summary>
        /// 表格中输入的数据不能进行正常转换时的异常处理
        /// </summary>
        public void MyDataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                Type tp = DataGrid_pointMonitor.Columns[e.ColumnIndex].ValueType;
                MessageBox.Show("输入的数据不能转换为指定类型的数据: \n\r " + tp.Name, 
                    "数据格式转换出错。", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        /// <summary>
        ///  绘制监测曲线图
        /// </summary>
        public void btnDraw_Click(object sender, EventArgs e)
        {
            DrawData(monitorData);
        }

        /// <summary>
        ///  在表格框宽度发生变化时，自动调整窗口的宽度
        /// </summary>
        private void DataGrid_pointMonitor_Resize(object sender, EventArgs e)
        {
            this.Size = new Size(DataGrid_pointMonitor.Width + 120, Size.Height);
        }

        private void monitorData_AddingNew(object sender, AddingNewEventArgs e)
        {
            e.NewObject = new DgvBindingPointData(null, null);
        }

        #endregion
    }
}
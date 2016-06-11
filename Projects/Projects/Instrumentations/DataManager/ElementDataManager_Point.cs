using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Autodesk.Revit.DB;
using OldW.DataManager;
using OldW.Instrumentations;
using stdOldW;
using stdOldW.WinFormHelper;

namespace OldW.DataManager
{
    public partial class ElementDataManager
    {
        internal class DgvPoint
        {
            #region    ---   Fields
            private readonly Document doc;

            private eZDataGridViewPaste dataGridViewPoint;

            #endregion

            #region    ---   Properties
            /// <summary>
            /// 表格中当前处理的那个测点。在为此属性赋值的过程中，会执行相关的刷新操作
            /// </summary>
            private Instrum_Point activeInstru;

            #endregion

            #region    ---   构造函数 与 控件初始化

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="doc"></param>
            /// <param name="dataGridViewLine"></param>
            public DgvPoint(Document doc, eZDataGridViewPaste dataGridViewPoint)
            {
                this.doc = doc;
                this.dataGridViewPoint = dataGridViewPoint;

                //
                ConstructDataGridView();

                // 事件绑定
                dataGridViewPoint.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.MyDataGridView1_DataError);
            }

            /// <summary>
            /// 创建 DataGridView 为点测点监测数据类型
            /// </summary>
            private void ConstructDataGridView()
            {
                //-------------------- 设置数据源的集合 -------------------------------------
                bindedTableData.AllowNew = true;
                bindedTableData.AddingNew += bindedTableData_AddingNew;

                //-------------------- 设置数据源的集合 -------------------------------------

                // 如果AutoGenerateColumns 为False，那么当设置DataSource 后，用户必须要手动为指定的属性值添加数据列。
                dataGridViewPoint.AutoGenerateColumns = false;
                dataGridViewPoint.AutoSize = false;
                dataGridViewPoint.AllowUserToAddRows = true;


                dataGridViewPoint.DataSource = bindedTableData;

                //-------- 将已有的数据源集合中的每一个元素的不同属性在不同的列中显示出来 -------


                // Initialize and add a text box column.
                // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Date";
                // 此列所对应的数据源中的元素中的哪一个属性的名称
                column.Name = "日期";
                dataGridViewPoint.Columns.Add(column);

                // Initialize and add a text box column.
                // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
                column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Value";
                // 此列所对应的数据源中的元素中的哪一个属性的名称
                column.Name = "数据";
                dataGridViewPoint.Columns.Add(column);
            }
            #endregion

            #region    ---   DataGridView中的数据与监测参数值之间的交互

            /// <summary>
            /// 将表格中的数据保存到Element的对应参数中。
            /// </summary>
            /// <remarks></remarks>
            public void SaveTableToElement()
            {
                // 将表格数据构造为监测数据类，并将其保存到测点对象中
                using (Transaction tran = new Transaction(doc, "保存表格中的数据到Element的参数中"))
                {
                    tran.Start();
                    try
                    {
                        activeInstru.SetMonitorData(tran, ConvertBindingList(bindedTableData));
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowDebugCatch(ex, "无法保存监测数据到对象参数中。");
                        tran.RollBack();
                    }
                }
            }

            /// <summary>
            /// 将元素的数据从Revit中提取出来并写入表格
            /// </summary>
            /// <param name="ele">要进行数据提取的监测单元</param>
            public void ShiftToNewElement(Instrum_Point ele)
            {
                if ((activeInstru == null) || (activeInstru.Monitor.Id != ele.Monitor.Id))
                {
                    Instrum_Point pt = ele as Instrum_Point;
                    List<MonitorData_Point> ptData = pt.GetMonitorData() ?? new List<MonitorData_Point>();

                    FillBindingList(ptData, ref bindedTableData);
                    activeInstru = ele;
                }
            }



            #endregion

            #region    ---   绘制监测曲线图

            /// <summary>
            /// 绘制图表
            /// </summary>
            /// <param name="data"></param>
            public Chart_MonitorData DrawData()
            {
                Chart_MonitorData Chart1 = new Chart_MonitorData(InstrumentationType.地表隆沉);

                var pts = ConvertBindingList(bindedTableData);

                Series s1= Chart1.AddLineSeries("Series1");
                s1.Points.DataBind(pts, "Date", "Value", "");

                Chart1.Show();
                return Chart1;
            }

            #endregion

            #region    ---   监测数据

            /// <summary> 与表格数据进行绑定的点测点监测数据集合 </summary>
            private BindingList<DgvBindingPointData> bindedTableData = new BindingList<DgvBindingPointData>();

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
            private class DgvBindingPointData
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

            #region    ---   事件处理

            /// <summary>
            /// 表格中输入的数据不能进行正常转换时的异常处理
            /// </summary>
            public void MyDataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
            {
                if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
                {
                    Type tp = dataGridViewPoint.Columns[e.ColumnIndex].ValueType;
                    MessageBox.Show("输入的数据不能转换为指定类型的数据: \n\r " + tp.Name,
                        "数据格式转换出错。", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.ThrowException = false;
                }
            }

            public void bindedTableData_AddingNew(object sender, AddingNewEventArgs e)
            {
                e.NewObject = new DgvBindingPointData(null, null);
            }

            #endregion
        }
    }
}

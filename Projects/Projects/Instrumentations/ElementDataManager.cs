using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
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

        /// <summary> 当前活动的图元中所保存的监测数据 </summary>
        private Instrum_Point.MonitorData_Point ActiveMonitorData;

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
            this.DataGrid_pointMonitor.Columns[0].ValueType = typeof(DateTime);
            this.DataGrid_pointMonitor.Columns[1].ValueType = typeof(object);
        }
        
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
                arr[i] = new LstbxValue<Instrumentation>(eleid.Monitor.Name + "( " + eleid.getMonitorName() + " ):" + eleid.Monitor.Id.IntegerValue.ToString(), eleid);
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
            string strData = "";
            int RowsCount = this.DataGrid_pointMonitor.Rows.Count;
            int ColsCount = this.DataGrid_pointMonitor.Columns.Count;
            DateTime[] arrDate = new DateTime[RowsCount - 2 + 1]; // 默认不读取最行一行，因为一般情况下，DataGridView中的最后一行是一行空数据
            Single?[] arrValue = new Single?[RowsCount - 2 + 1];
            //
            DataGridViewRow row = default(DataGridViewRow);
            for (var r = 0; r <= RowsCount - 2; r++)
            {
                row = this.DataGrid_pointMonitor.Rows[Convert.ToInt32(r)];
                for (var c = 0; c <= ColsCount - 1; c++)
                {
                    if (!DateTime.TryParse(Convert.ToString(row.Cells[0].Value), out arrDate[(int)r]))
                    {
                        TaskDialog.Show("Error", "第" + (r + 1) + "行数据不能正确地转换为DateTime。");
                        return;
                    }
                    arrValue[(int)r] = (Single?)row.Cells[1].Value;
                }
            }

            // 将表格数据构造为监测数据类，并将其保存到测点对象中
            if (ActiveInstru is Instrum_Point)
            {
                Instrum_Point pt = (Instrum_Point)ActiveInstru;
                Instrum_Point.MonitorData_Point moniData = new Instrum_Point.MonitorData_Point(arrDate, arrValue);

                using (Transaction tran = new Transaction(doc, "保存表格中的数据到Element的参数中"))
                {
                    tran.Start();
                    try
                    {
                        pt.SetMonitorData(tran, moniData);
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
        /// 将元素的数据写入表格
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool FillTableWithElementData(Instrumentation ele, DataGridView table)
        {
            bool blnSucceed = true;
            table.Rows.Clear();  // 先将列表中的数据清空

            if (ele is Instrum_Point)
            {
                Instrum_Point pt = ele as Instrum_Point;
                Instrum_Point.MonitorData_Point ptData;
                ptData = pt.GetMonitorData();

                if (ptData != null) // 说明成功提取到监测数据
                {
                    // 将此测点的监测数据写入表格
                    DateTime[] arrDate = ptData.arrDate;
                    Single?[] arrValue = ptData.arrValue;
                    int DataCount = Convert.ToInt32(ptData.arrDate.Length);
                    //
                    if (DataCount > 0)
                    {
                        int originalRowsCount = table.Rows.Count;

                        if (DataCount >= originalRowsCount) // 添加不够的行
                        {
                            table.Rows.Add(DataCount - originalRowsCount + 1);
                        }
                        else // 删除多余行
                        {
                            for (int i = originalRowsCount; i >= DataCount + 2; i--)
                            {
                                table.Rows.RemoveAt(0); // 不能直接移除最后那一行，因为那一行是未提交的
                            }
                        }

                        // 将数据写入表格
                        for (int r = 0; r <= DataCount - 1; r++)
                        {
                            table.Rows[r].SetValues(new object[] { arrDate[r], arrValue[r] });
                        }
                        // 将最后一行的数据清空，即使某个单元格的ValueType为DateTime，也可以设置其值为Nothing，此时这个单元格中会显示为空。
                        table.Rows[this.DataGrid_pointMonitor.Rows.Count - 1].SetValues(new object[] { null, null });
                    }
                }
            }
            return blnSucceed;
        }

        #endregion

        #region    ---   绘制监测曲线图
        /// <summary>
        /// 绘制图表
        /// </summary>
        /// <param name="Data"></param>
        private Chart_MonitorData DrawData(Instrum_Point.MonitorData_Point Data)
        {
            Chart_MonitorData Chart1 = new Chart_MonitorData(InstrumentationType.地表隆沉);
            Chart1.Series.Points.DataBindXY(Data.arrDate, Data.arrValue);
            Chart1.Show();
            return Chart1;
        }

        #endregion

        #region    ---   事件处理

        public void MyDataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                MessageBox.Show("输入的数据不能转换为日期数据。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        public void btnDraw_Click(object sender, EventArgs e)
        {
            DrawData(ActiveMonitorData);
        }

        #endregion
    }
}
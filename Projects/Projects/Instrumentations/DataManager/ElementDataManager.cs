using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using OldW.Instrumentations;
using stdOldW;
using stdOldW.WinFormHelper;

namespace OldW.DataManager
{
    /// <summary>
    /// 模型中的测点的监测数据的添加，删除，导入导出等
    /// </summary>
    /// <remarks></remarks>
    public partial class ElementDataManager
    {
        #region   ---  Types

        /// <summary> 当前操作的测点模式，比如是线测点（测斜管）、点测点等 </summary>
        private enum MonitorMode
        {
            /// <summary> 点测点，比如地表沉降、立柱垂直位移测点等 </summary>
            MonitorPoint,

            /// <summary> 线测点，比如测斜管 </summary>
            MonitorLine
        }

        #endregion

        #region   ---  Properties

        private readonly DgvPoint Dgv_Point;
        private readonly DgvLine Dgv_Line;

        #endregion

        #region   ---  Fields

        private readonly InstrumDoc _document;

        /// <summary> 当前活动的测点对象 </summary>
        private Instrumentation _activeInstru;

        /// <summary> 进入测点监测数据管理窗口时，所有选择的测点 </summary>
        private InstrumDoc.InstrumCollector selectedInstrum;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="eleIdCollection">所有要进行处理的测点元素的Id集合</param>
        /// <param name="document"></param>
        /// <remarks></remarks>
        public ElementDataManager(ICollection<Instrumentation> eleIdCollection, InstrumDoc document)
        {
            InitializeComponent();
            // --------------------------------
            this._document = document;

            // 属性绑定
            btnSetNodes.DataBindings.Add("Enabled", checkBox_incline, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);

            // 事件绑定
            checkBox_incline.CheckedChanged += new EventHandler(this.checkBox_CheckedChanged);
            checkBox_columnheave.CheckedChanged += new EventHandler(this.checkBox_CheckedChanged);
            checkBox_groundsettlement.CheckedChanged += new EventHandler(this.checkBox_CheckedChanged);
            checkBox_strutaxisforce.CheckedChanged += new EventHandler(this.checkBox_CheckedChanged);

            //
            Dgv_Point = new DgvPoint(document.Document, DataGridView_pointMonitor);
            Dgv_Line = new DgvLine(document.Document, DataGridView_LineMonitor);
            // --------------------------------
            // 根据不同的选择情况来进行不同的初始化
            selectedInstrum = new InstrumDoc.InstrumCollector(eleIdCollection);

            InitializeUI(selectedInstrum);
        }

        /// <summary>
        /// 根据不同的选择情况来进行不同的初始化
        ///  </summary>
        /// <param name="AllselectedInstrum"></param>
        private void InitializeUI(InstrumDoc.InstrumCollector AllselectedInstrum)
        {
            // 复选框的启用与否
            checkBox_columnheave.Enabled = AllselectedInstrum.ColumnHeave.Count != 0;
            checkBox_groundsettlement.Enabled = AllselectedInstrum.GroundSettlement.Count != 0;
            checkBox_incline.Enabled = AllselectedInstrum.Incline.Count != 0;
            checkBox_strutaxisforce.Enabled = AllselectedInstrum.StrutAxialForce.Count != 0;

            //
            btnActivateDatagridview.Enabled = false;
        }

        private void ShiftToPoint()
        {
            DataGridView_LineMonitor.Visible = false;
            DataGridView_pointMonitor.Visible = true;

            // 调整窗口大小
            Width = DataGridView_pointMonitor.Width + DataGridView_pointMonitor.Left + 29;
        }

        private void ShiftToLine()
        {
            DataGridView_LineMonitor.Visible = true;
            DataGridView_pointMonitor.Visible = false;

            // 调整窗口大小

            DataGridView_LineMonitor.Width = 600;
            Width = DataGridView_LineMonitor.Width + DataGridView_LineMonitor.Left + 29;
        }

        #region    ---   CheckBox 与 Datagridview 控件的激活

        /// <summary> 当前操作的测点模式，比如是线测点（测斜管）、点测点等 </summary>
        private MonitorMode activeMonitorMode;

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;
            if (!cbx.Checked)
            {
                return;
            }
            btnActivateDatagridview.Enabled = true;

            GroupBox gbx = (GroupBox)cbx.Parent;
            // 线测点模式
            if (gbx.Name == groupBoxLines.Name)
            {
                activeMonitorMode = MonitorMode.MonitorLine;
                // 禁用点测点的复选框
                foreach (var VARIABLE in groupBoxPoints.Controls)
                {
                    if (VARIABLE is CheckBox)
                    {
                        ((CheckBox)VARIABLE).Checked = false;
                    }
                }
            }
            // 点测点模式
            else if (gbx.Name == groupBoxPoints.Name)
            {
                activeMonitorMode = MonitorMode.MonitorPoint;

                // 禁用线测点的复选框
                foreach (var VARIABLE in groupBoxLines.Controls)
                {
                    if (VARIABLE is CheckBox)
                    {
                        ((CheckBox)VARIABLE).Checked = false;
                    }
                }
            }
        }

        /// <summary>
        /// 激活 Datagridview 控件
        /// </summary>
        public void btnActivateDatagridview_Click(object sender, EventArgs e)
        {
            switch (activeMonitorMode)
            {
                case MonitorMode.MonitorPoint:
                    {
                        List<Instrum_Point> points;
                        points = selectedInstrum.GetPointMonitors(checkBox_columnheave.Checked,
                            checkBox_groundsettlement.Checked,
                            checkBox_strutaxisforce.Checked);

                        // 切换 Datagridview
                        ShiftToPoint();

                        _document.FillCombobox(points, cmbx_elements);
                        break;
                    }
                case MonitorMode.MonitorLine:
                    {
                        List<Instrum_Line> lines;
                        lines = selectedInstrum.GetLineMonitors(checkBox_incline.Checked);

                        // 切换 Datagridview
                        ShiftToLine();

                        // 填充
                        _document.FillCombobox(lines, cmbx_elements);
                        break;
                    }
            }
            btnActivateDatagridview.Enabled = false;
        }

        #endregion

        #region    ---   组合列表框 ComboBox

        /// <summary>
        /// 在组合框中改变了选择的测点后，去更新DataGridView中的数据为指定测点的监测数据。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Instrumentation ele = (Instrumentation)cmbx_elements.SelectedValue;
            btnRename.Enabled = true;
            try
            {
                this._activeInstru = ele;
                switch (activeMonitorMode)
                {
                    case MonitorMode.MonitorPoint:
                        {
                            Dgv_Point.ShiftToNewElement((Instrum_Point)ele);
                            //  FillTableWithElementData(ele, this.DataGridView_pointMonitor);
                            break;
                        }
                    case MonitorMode.MonitorLine:
                        {
                            Dgv_Line.ShiftToNewElement((Instrum_Line)ele);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowDebugCatch(ex, "SelectedIndexChanged时出错。");
            }
        }

        #endregion

        /// <summary>
        /// 将表格中的数据保存到Element的对应参数中。
        /// </summary>
        /// <remarks></remarks>
        private void SaveTableToElement(object sender, EventArgs e)
        {
            switch (activeMonitorMode)
            {
                case MonitorMode.MonitorPoint:
                    {
                        Dgv_Point.SaveTableToElement();
                        break;
                    }
                case MonitorMode.MonitorLine:
                    {
                        Dgv_Line.SaveTableToElement();
                        break;
                    }
            }
        }

        #region    ---   事件处理

        /// <summary>
        ///  绘制监测曲线图
        /// </summary>
        public void btnDraw_Click(object sender, EventArgs e)
        {
            switch (activeMonitorMode)
            {
                case MonitorMode.MonitorPoint:
                    {
                        Dgv_Point.DrawData();
                        //  FillTableWithElementData(ele, this.DataGridView_pointMonitor);
                        break;
                    }
                case MonitorMode.MonitorLine:
                    {
                        Dgv_Line.DrawData();
                        break;
                    }
            }
        }

        /// <summary> 设置线测点的节点信息 </summary>
        public void btnSetNodes_Click(object sender, EventArgs e)
        {
            Dgv_Line.ChangeNodes();
        }

        /// <summary>
        ///  在表格框宽度发生变化时，自动调整窗口的宽度
        /// </summary>
        private void DataGrid_pointMonitor_Resize(object sender, EventArgs e)
        {
            this.Size = new Size(DataGridView_pointMonitor.Width + 120, Size.Height);
        }

        #endregion

        #region 测点的重命名

        /// <summary>
        /// 测点单元的重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenameElement(object sender, EventArgs e)
        {
            Instrumentation ele = (Instrumentation)cmbx_elements.SelectedValue;
            string oldName = ele.getMonitorName();
            FormReNameElement ff = new FormReNameElement(oldName);
            ff.ShowDialog();

            if (!ff.NameChanged)
            {
                return; // 如果测点编号没有修改，就不进行后面的操作了
            }
            // 测点重命名
            using (Transaction tt = new Transaction(_document.Document, "设置测点名称"))
            {
                tt.Start();
                ele.setMonitorName(tt, ff.MonitorName);
                tt.Commit();
            }

            // cmbx_elements 刷新界面
            string NewText = ele.Monitor.Name + "( " + ff.MonitorName + " ):" +
                             ele.Monitor.Id.IntegerValue;

            // 在设置DataSource时会立即触发 SelectedIndexChanged 事件，所以这里要先取消事件关联
            this.cmbx_elements.SelectedIndexChanged -= new EventHandler(this.ComboBox1_SelectedIndexChanged);

            // 提取刷新前的数据
            LstbxValue<Instrumentation>[] arr = (LstbxValue<Instrumentation>[])cmbx_elements.DataSource;

            // 先修改集合中的值，然后再将修改后的集合赋值给DataSource
            arr[cmbx_elements.SelectedIndex].DisplayedText = NewText;

            // 1. 先将数据源清空，这一操作是必须的。
            cmbx_elements.DataSource = null; // 这一清空操作是必须的

            // 2. 设置 DisplayMember 与 ValueMember。
            this.cmbx_elements.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
            this.cmbx_elements.ValueMember = LstbxValue<Instrumentation>.ValueMember;

            // 3. 重新设置数据源
            cmbx_elements.DataSource = arr;

            // 4. 最后重新进行事件关联
            this.cmbx_elements.SelectedIndexChanged += new EventHandler(this.ComboBox1_SelectedIndexChanged);
        }

        #endregion
    }
}
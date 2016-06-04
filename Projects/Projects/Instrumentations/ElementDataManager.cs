using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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

        private readonly Document doc;

        /// <summary> 当前活动的测点对象 </summary>
        private Instrumentation _activeInstru;

        private readonly List<Instrum_ColumnHeave> instColumnHeave = new List<Instrum_ColumnHeave>();
        private readonly List<Instrum_GroundSettlement> instGroundSettlement = new List<Instrum_GroundSettlement>();
        private readonly List<Instrum_Incline> instIncline = new List<Instrum_Incline>();
        private readonly List<Instrum_StrutAxialForce> instStrutAxialForce = new List<Instrum_StrutAxialForce>();

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

            // 属性绑定
            btnSetNodes.DataBindings.Add("Enabled", checkBox_incline, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);

            // 事件绑定
            checkBox_incline.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            checkBox_columnheave.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            checkBox_groundsettlement.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            checkBox_strutaxisforce.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);

            //
            Dgv_Point=new DgvPoint(document,DataGridView_pointMonitor);
            Dgv_Line=new DgvLine(document,DataGridView_LineMonitor);
            // --------------------------------
            // 根据不同的选择情况来进行不同的初始化
            InitializeUI(eleIdCollection);
        }

        /// <summary>
        /// 根据不同的选择情况来进行不同的初始化
        ///  </summary>
        /// <param name="eleIdCollection"></param>
        private void InitializeUI(ICollection<Instrumentation> eleIdCollection)
        {
            foreach (Instrumentation inst in eleIdCollection)
            {
                if (inst is Instrum_ColumnHeave)
                {
                    instColumnHeave.Add((Instrum_ColumnHeave)inst);
                }
                else if (inst is Instrum_GroundSettlement)
                {
                    instGroundSettlement.Add((Instrum_GroundSettlement)inst);
                }
                else if (inst is Instrum_Incline)
                {
                    instIncline.Add((Instrum_Incline)inst);
                }
                else if (inst is Instrum_StrutAxialForce)
                {
                    instStrutAxialForce.Add((Instrum_StrutAxialForce)inst);
                }
            }
            // 复选框的启用与否
            checkBox_columnheave.Enabled = (instColumnHeave.Count != 0);
            checkBox_groundsettlement.Enabled = (instGroundSettlement.Count != 0);
            checkBox_incline.Enabled = (instIncline.Count != 0);
            checkBox_strutaxisforce.Enabled = (instStrutAxialForce.Count != 0);

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
            List<Instrumentation> inst = new List<Instrumentation>();
            switch (activeMonitorMode)
            {
                case MonitorMode.MonitorPoint:
                    {
                        if (checkBox_columnheave.Checked)
                        {
                            inst.AddRange(instColumnHeave);
                        }
                        if (checkBox_groundsettlement.Checked)
                        {
                            inst.AddRange(instGroundSettlement);
                        }
                        if (checkBox_strutaxisforce.Checked)
                        {
                            inst.AddRange(instStrutAxialForce);
                        }

                        // 切换 Datagridview
                        ShiftToPoint();
                        break;
                    }
                case MonitorMode.MonitorLine:
                    {
                        if (checkBox_incline.Checked)
                        {
                            inst.AddRange(instIncline);
                        }

                        // 切换 Datagridview
                        ShiftToLine();
                        break;
                    }
            }
            btnActivateDatagridview.Enabled = false;
            // 填充
            FillCombobox(inst);
        }

        #endregion

        #region    ---   组合列表框 ComboBox

        /// <summary>
        /// 将指定的测点集合对象添加到组合列表框中
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <returns></returns>
        private void FillCombobox(ICollection<Instrumentation> elementCollection)
        {
            if (elementCollection.Count > 0)
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

                // ComboBox设置
                this.cmbx_elements.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
                this.cmbx_elements.ValueMember = LstbxValue<Instrumentation>.ValueMember;

                this.cmbx_elements.DataSource = arr;
            }
        }

        /// <summary>
        /// 在组合框中改变了选择的测点后，去更新DataGridView中的数据为指定测点的监测数据。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Instrumentation ele = (Instrumentation)cmbx_elements.SelectedValue;
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
        public void SaveTableToElement(object sender, EventArgs e)
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
    }
}
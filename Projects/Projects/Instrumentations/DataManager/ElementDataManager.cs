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

        #region   ---  Fields

        private readonly DgvPoint Dgv_Point;
        private readonly DgvLine Dgv_Line;


        private readonly InstrumDoc _document;

        /// <summary> 当前活动的测点对象 </summary>
        private Instrumentation _activeInstru;

        /// <summary> 进入测点监测数据管理窗口时，所有选择的测点 </summary>
        private InstrumCollector _selectedInstrum;

        /// <summary> 复选框集合 </summary>
        private readonly List<CheckBox> _checkBoxex;
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

            _checkBoxex = new List<CheckBox>();
            // Tag绑定
            checkBox_WallIncline.Tag = InstrumentationType.墙体测斜;
            checkBox_SoilIncline.Tag = InstrumentationType.土体测斜;
            checkBox_OtherLine.Tag = InstrumentationType.其他线测点;
            checkBox_OtherPoint.Tag = InstrumentationType.其他点测点;
            checkBox_WallTop.Tag = InstrumentationType.墙顶位移;
            checkBox_WaterTable.Tag = InstrumentationType.水位;
            checkBox_columnheave.Tag = InstrumentationType.立柱隆沉;
            checkBox_groundsettlement.Tag = InstrumentationType.地表隆沉;
            checkBox_strutaxisforce.Tag = InstrumentationType.支撑轴力;

            // 添加到集合中
            _checkBoxex.AddRange(new CheckBox[]
            {
                checkBox_WallIncline,
                checkBox_SoilIncline,
                checkBox_OtherLine,
                checkBox_OtherPoint,
                checkBox_WallTop,
                checkBox_WaterTable,
                checkBox_columnheave,
                checkBox_groundsettlement,
                checkBox_strutaxisforce,
            });


            // 事件绑定
            foreach (var checkBox in _checkBoxex)
            {
                checkBox.CheckedChanged += new EventHandler(this.checkBox_CheckedChanged);
            }

            //
            Dgv_Point = new DgvPoint(document.Document, DataGridView_pointMonitor);
            Dgv_Line = new DgvLine(document.Document, DataGridView_LineMonitor);

            // ----------------------------------------------------------------------------------------------
            // 根据不同的选择情况来进行不同的初始化
            _selectedInstrum = new InstrumCollector(eleIdCollection);

            // 复选框的启用与否
            foreach (var checkBox in _checkBoxex)
            {
                checkBox.Enabled = _selectedInstrum.GetMonitors((InstrumentationType)checkBox.Tag).Count != 0;
            }

            //
            btnActivateDatagridview.Enabled = false;
            btnSetNodes.Location = new System.Drawing.Point(x: 321, y: 33);

        }

        private void ShiftToPoint()
        {
            DataGridView_LineMonitor.Visible = false;
            DataGridView_pointMonitor.Visible = true;

            // 调整窗口大小
            Width = DataGridView_pointMonitor.Width + DataGridView_pointMonitor.Left + 29;

            // 调整“节点”按钮的UI界面
            btnSetNodes.Visible = false;

        }

        private void ShiftToLine()
        {
            DataGridView_LineMonitor.Visible = true;
            DataGridView_pointMonitor.Visible = false;

            // 调整窗口大小

            DataGridView_LineMonitor.Width = 600;
            Width = DataGridView_LineMonitor.Width + DataGridView_LineMonitor.Left + 29;

            // 调整“节点”按钮的UI界面
            btnSetNodes.Visible = true;

        }

        #region    ---   CheckBox 与 Datagridview 控件的激活

        /// <summary> 当前操作的测点模式，比如是线测点（测斜管）、点测点等 </summary>
        private MonitorMode _activeMonitorMode;

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;

            // 如果最终有勾选项，则激活“激活”按钮
            btnActivateDatagridview.Enabled = true;

            if (!cbx.Checked) { return; }

            GroupBox gbx = (GroupBox)cbx.Parent;
            // 线测点模式
            if (gbx.Name == groupBoxLines.Name)
            {
                _activeMonitorMode = MonitorMode.MonitorLine;
                // 禁用点测点的复选框
                foreach (var control in groupBoxPoints.Controls)
                {
                    if (control is CheckBox)
                    {
                        ((CheckBox)control).Checked = false;
                    }
                }
            }
            // 点测点模式
            else if (gbx.Name == groupBoxPoints.Name)
            {
                _activeMonitorMode = MonitorMode.MonitorPoint;

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
            switch (_activeMonitorMode)
            {
                case MonitorMode.MonitorPoint:
                    {
                        // 选择要进行提取的监测类型
                        InstrumentationType tp = InstrumentationType.未指定;

                        foreach (CheckBox chk in _checkBoxex)
                        {
                            if (chk.Checked)
                            {
                                InstrumentationType tp1 = (InstrumentationType)chk.Tag;
                                if ((tp1 & InstrumentationType.点测点集合) > 0)
                                {
                                    tp = tp | tp1;
                                }
                            }
                        }
                        //
                        var points = _selectedInstrum.GetPointMonitors(tp);
                        // 切换 Datagridview
                        ShiftToPoint();
                        // 填充
                        _document.FillCombobox(points, cmbx_elements);
                        break;
                    }
                case MonitorMode.MonitorLine:
                    {
                        // 选择要进行提取的监测类型
                        InstrumentationType tp = InstrumentationType.未指定;

                        foreach (CheckBox chk in _checkBoxex)
                        {
                            if (chk.Checked)
                            {
                                InstrumentationType tp1 = (InstrumentationType)chk.Tag;
                                if ((tp1 & InstrumentationType.线测点集合) > 0)
                                {
                                    tp = tp | tp1;
                                }
                            }
                        }

                        //
                        List<Instrum_Line> lines = _selectedInstrum.GetLineMonitors(tp);

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
        public void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Instrumentation ele = (Instrumentation)cmbx_elements.SelectedValue;
            if (ele != null)
            {
                btnRename.Enabled = true;
                try
                {
                    this._activeInstru = ele;
                    switch (_activeMonitorMode)
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

                                // 设置是否启用节点设置的UI界面
                                btnSetNodes.Enabled = ((Instrum_Line)ele).NodesDigital;
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowDebugCatch(ex, "SelectedIndexChanged时出错。");
                }
            }
        }

        #endregion

        /// <summary>
        /// 将表格中的数据保存到Element的对应参数中。
        /// </summary>
        /// <remarks></remarks>
        private void SaveTableToElement(object sender, EventArgs e)
        {
            switch (_activeMonitorMode)
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

        /// <summary>
        ///  绘制监测曲线图
        /// </summary>
        public void btnDraw_Click(object sender, EventArgs e)
        {
            switch (_activeMonitorMode)
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

        #region    ---   事件处理

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
            string oldName = ele.GetMonitorName();
            ElementInitialize ff = new ElementInitialize(oldName);
            ff.ShowDialog();

            if (!ff.NameChanged)
            {
                return; // 如果测点编号没有修改，就不进行后面的操作了
            }
            // 测点重命名
            using (Transaction tt = new Transaction(_document.Document, "设置测点名称"))
            {
                tt.Start();
                ele.SetMonitorName(tt, ff.MonitorName);
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Color = System.Drawing.Color;
using Form = System.Windows.Forms.Form;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 测点过滤器
    /// </summary>
    public partial class PickInstrums : Form
    {

        /// <summary> 最终决定要选择的测点单元 </summary>
        public readonly List<ElementId> FilterdInstrumentations;


        /// <summary> 构造函数 </summary>
        /// <param name="eleIdCollection">所有要进行处理的测点元素的Id集合</param>
        /// <param name="document"></param>
        /// <remarks></remarks>
        public PickInstrums(ICollection<Instrumentation> eleIdCollection, InstrumDoc document)
        {
            InitializeComponent();

            //
            FilterdInstrumentations = new List<ElementId>();

            // TreeViewIns 属性设置
            TreeViewIns.BackColor = Color.FromArgb(255, 204, 232, 207); // 背景色
            TreeViewIns.CheckBoxes = true;  // 是否显示复选框

            ConstructTreeView(eleIdCollection);  // 添加节点

            TreeViewIns.ExpandAll();       // 展开节点
            buttonSelectAll_Click(TreeViewIns, new EventArgs());  // 全部选择
        }

        /// <summary> 构造树形控件。一共分两级，第一级是测点类型，第二级是每种类型下的测点集合 </summary>
        /// <param name="allInstrums"></param>
        private void ConstructTreeView(ICollection<Instrumentation> eleIdCollection)
        {
            // TreeViewIns

            SortedSet<InstrumentationType> insType = new SortedSet<InstrumentationType>();
            foreach (Instrumentation ins in eleIdCollection)
            {
                if (!insType.Contains(ins.Type))
                {
                    // 创建一个新的测点类型节点，并设置属性
                    TreeNode ndType = new TreeNode();
                    ndType.Name = ins.Type.ToString();
                    ndType.Tag = ins.Type;
                    ndType.Text = ndType.Name;
                    TreeViewIns.Nodes.Add(ndType);
                    //
                    insType.Add(ins.Type);
                }

                // 在对应的测点类型节点下添加此监测测点单元，并设置属性
                TreeNode ndIns = new TreeNode();
                ndIns.Name = ins.Monitor.Id.ToString();
                ndIns.Text = ins.IdName;
                ndIns.Tag = ins;

                TreeViewIns.Nodes[ins.Type.ToString()].Nodes.Add(ndIns);
            }
        }

        #region   ---  节点选择的相关操作


        private void buttonExpand_Click(object sender, EventArgs e)
        {
            TreeViewIns.ExpandAll();
        }
        private void buttonShrink_Click(object sender, EventArgs e)
        {
            TreeViewIns.CollapseAll();
        }
        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode ndType in TreeViewIns.Nodes)
            {
                ndType.Checked = true;
            }
        }
        private void buttonDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode ndType in TreeViewIns.Nodes)
            {
                ndType.Checked = false;
            }
        }

        /// <summary>
        /// 在选择或者取消选择父节点时，对其子节点进行相同的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewIns_AfterCheck(object sender, TreeViewEventArgs e)
        {
            var nd = e.Node;
            if (nd.Level == 0)
            {
                foreach (TreeNode ndIns in nd.Nodes)
                {
                    ndIns.Checked = nd.Checked;

                }
            }
        }

        #endregion

        #region   ---  最终结果输出

        private void buttonOk_Click(object sender, EventArgs e)
        {
            // 收集最终的过滤信息
            foreach (TreeNode ndType in TreeViewIns.Nodes)
            {
                foreach (TreeNode ndIns in ndType.Nodes)
                {
                    if (ndIns.Checked)
                    {
                        Instrumentation ins = ndIns.Tag as Instrumentation;
                        if (ins != null)
                        {
                            FilterdInstrumentations.Add(ins.Monitor.Id);

                        }
                    }
                }
            }

            //
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion
    }
}

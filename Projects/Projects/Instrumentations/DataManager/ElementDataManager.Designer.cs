using System.Windows.Forms;

namespace OldW.DataManager
{
    public partial class ElementDataManager : System.Windows.Forms.Form
    {

        //Form overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnSaveChange = new System.Windows.Forms.Button();
            this.cmbx_elements = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.btnDraw = new System.Windows.Forms.Button();
            this.DataGridView_pointMonitor = new stdOldW.WinFormHelper.eZDataGridViewPaste();
            this.groupBoxPoints = new System.Windows.Forms.GroupBox();
            this.checkBox_strutaxisforce = new System.Windows.Forms.CheckBox();
            this.checkBox_columnheave = new System.Windows.Forms.CheckBox();
            this.checkBox_groundsettlement = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxLines = new System.Windows.Forms.GroupBox();
            this.checkBox_incline = new System.Windows.Forms.CheckBox();
            this.btnSetNodes = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.DataGridView_LineMonitor = new stdOldW.WinFormHelper.eZDataGridViewPaste();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnActivateDatagridview = new System.Windows.Forms.Button();
            this.btnRename = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_pointMonitor)).BeginInit();
            this.groupBoxPoints.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBoxLines.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_LineMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSaveChange
            // 
            this.btnSaveChange.Location = new System.Drawing.Point(12, 473);
            this.btnSaveChange.Name = "btnSaveChange";
            this.btnSaveChange.Size = new System.Drawing.Size(75, 23);
            this.btnSaveChange.TabIndex = 1;
            this.btnSaveChange.Text = "保存";
            this.btnSaveChange.UseVisualStyleBackColor = true;
            this.btnSaveChange.Click += new System.EventHandler(this.SaveTableToElement);
            // 
            // cmbx_elements
            // 
            this.cmbx_elements.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_elements.FormattingEnabled = true;
            this.cmbx_elements.Location = new System.Drawing.Point(116, 33);
            this.cmbx_elements.Name = "cmbx_elements";
            this.cmbx_elements.Size = new System.Drawing.Size(190, 20);
            this.cmbx_elements.TabIndex = 2;
            this.cmbx_elements.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(114, 9);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(65, 12);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "操作的测点";
            // 
            // btnDraw
            // 
            this.btnDraw.Location = new System.Drawing.Point(12, 444);
            this.btnDraw.Name = "btnDraw";
            this.btnDraw.Size = new System.Drawing.Size(75, 23);
            this.btnDraw.TabIndex = 1;
            this.btnDraw.Text = "绘图";
            this.btnDraw.UseVisualStyleBackColor = true;
            this.btnDraw.Click += new System.EventHandler(this.btnDraw_Click);
            // 
            // DataGridView_pointMonitor
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.DataGridView_pointMonitor.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.DataGridView_pointMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_pointMonitor.Location = new System.Drawing.Point(116, 68);
            this.DataGridView_pointMonitor.Name = "DataGridView_pointMonitor";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridView_pointMonitor.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.DataGridView_pointMonitor.RowHeadersWidth = 52;
            this.DataGridView_pointMonitor.RowTemplate.Height = 23;
            this.DataGridView_pointMonitor.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DataGridView_pointMonitor.Size = new System.Drawing.Size(280, 428);
            this.DataGridView_pointMonitor.TabIndex = 0;
            this.DataGridView_pointMonitor.Resize += new System.EventHandler(this.DataGrid_pointMonitor_Resize);
            // 
            // groupBoxPoints
            // 
            this.groupBoxPoints.Controls.Add(this.checkBox_strutaxisforce);
            this.groupBoxPoints.Controls.Add(this.checkBox_columnheave);
            this.groupBoxPoints.Controls.Add(this.checkBox_groundsettlement);
            this.groupBoxPoints.Location = new System.Drawing.Point(3, 94);
            this.groupBoxPoints.Name = "groupBoxPoints";
            this.groupBoxPoints.Size = new System.Drawing.Size(83, 89);
            this.groupBoxPoints.TabIndex = 4;
            this.groupBoxPoints.TabStop = false;
            this.groupBoxPoints.Text = "点测点";
            // 
            // checkBox_strutaxisforce
            // 
            this.checkBox_strutaxisforce.AutoSize = true;
            this.checkBox_strutaxisforce.Location = new System.Drawing.Point(6, 64);
            this.checkBox_strutaxisforce.Name = "checkBox_strutaxisforce";
            this.checkBox_strutaxisforce.Size = new System.Drawing.Size(72, 16);
            this.checkBox_strutaxisforce.TabIndex = 0;
            this.checkBox_strutaxisforce.Text = "支撑轴力";
            this.checkBox_strutaxisforce.UseVisualStyleBackColor = true;
            // 
            // checkBox_columnheave
            // 
            this.checkBox_columnheave.AutoSize = true;
            this.checkBox_columnheave.Location = new System.Drawing.Point(6, 42);
            this.checkBox_columnheave.Name = "checkBox_columnheave";
            this.checkBox_columnheave.Size = new System.Drawing.Size(72, 16);
            this.checkBox_columnheave.TabIndex = 0;
            this.checkBox_columnheave.Text = "立柱隆起";
            this.checkBox_columnheave.UseVisualStyleBackColor = true;
            // 
            // checkBox_groundsettlement
            // 
            this.checkBox_groundsettlement.AutoSize = true;
            this.checkBox_groundsettlement.Location = new System.Drawing.Point(6, 20);
            this.checkBox_groundsettlement.Name = "checkBox_groundsettlement";
            this.checkBox_groundsettlement.Size = new System.Drawing.Size(72, 16);
            this.checkBox_groundsettlement.TabIndex = 0;
            this.checkBox_groundsettlement.Text = "地表沉降";
            this.checkBox_groundsettlement.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBoxLines);
            this.panel1.Controls.Add(this.groupBoxPoints);
            this.panel1.Location = new System.Drawing.Point(12, 36);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(92, 190);
            this.panel1.TabIndex = 5;
            // 
            // groupBoxLines
            // 
            this.groupBoxLines.Controls.Add(this.checkBox_incline);
            this.groupBoxLines.Controls.Add(this.btnSetNodes);
            this.groupBoxLines.Location = new System.Drawing.Point(3, 3);
            this.groupBoxLines.Name = "groupBoxLines";
            this.groupBoxLines.Size = new System.Drawing.Size(83, 85);
            this.groupBoxLines.TabIndex = 4;
            this.groupBoxLines.TabStop = false;
            this.groupBoxLines.Text = "线测点";
            // 
            // checkBox_incline
            // 
            this.checkBox_incline.AutoSize = true;
            this.checkBox_incline.Location = new System.Drawing.Point(7, 21);
            this.checkBox_incline.Name = "checkBox_incline";
            this.checkBox_incline.Size = new System.Drawing.Size(48, 16);
            this.checkBox_incline.TabIndex = 0;
            this.checkBox_incline.Text = "测斜";
            this.checkBox_incline.UseVisualStyleBackColor = true;
            // 
            // btnSetNodes
            // 
            this.btnSetNodes.Location = new System.Drawing.Point(3, 47);
            this.btnSetNodes.Name = "btnSetNodes";
            this.btnSetNodes.Size = new System.Drawing.Size(75, 23);
            this.btnSetNodes.TabIndex = 1;
            this.btnSetNodes.Text = "节点";
            this.toolTip1.SetToolTip(this.btnSetNodes, "设置线测点中每一个节点距离测点起点的距离。");
            this.btnSetNodes.UseVisualStyleBackColor = true;
            this.btnSetNodes.Click += new System.EventHandler(this.btnSetNodes_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "测点类型";
            // 
            // DataGridView_LineMonitor
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.DataGridView_LineMonitor.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.DataGridView_LineMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_LineMonitor.Location = new System.Drawing.Point(116, 68);
            this.DataGridView_LineMonitor.Name = "DataGridView_LineMonitor";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridView_LineMonitor.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.DataGridView_LineMonitor.RowHeadersWidth = 52;
            this.DataGridView_LineMonitor.RowTemplate.Height = 23;
            this.DataGridView_LineMonitor.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DataGridView_LineMonitor.Size = new System.Drawing.Size(280, 428);
            this.DataGridView_LineMonitor.TabIndex = 0;
            this.DataGridView_LineMonitor.Resize += new System.EventHandler(this.DataGrid_pointMonitor_Resize);
            // 
            // btnActivateDatagridview
            // 
            this.btnActivateDatagridview.Location = new System.Drawing.Point(12, 232);
            this.btnActivateDatagridview.Name = "btnActivateDatagridview";
            this.btnActivateDatagridview.Size = new System.Drawing.Size(75, 23);
            this.btnActivateDatagridview.TabIndex = 1;
            this.btnActivateDatagridview.Text = "激活";
            this.btnActivateDatagridview.UseVisualStyleBackColor = true;
            this.btnActivateDatagridview.Click += new System.EventHandler(this.btnActivateDatagridview_Click);
            // 
            // btnRename
            // 
            this.btnRename.Enabled = false;
            this.btnRename.Location = new System.Drawing.Point(321, 33);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(75, 23);
            this.btnRename.TabIndex = 1;
            this.btnRename.Text = "重命名";
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new System.EventHandler(this.RenameElement);
            // 
            // ElementDataManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 507);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.cmbx_elements);
            this.Controls.Add(this.btnActivateDatagridview);
            this.Controls.Add(this.btnDraw);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnSaveChange);
            this.Controls.Add(this.DataGridView_LineMonitor);
            this.Controls.Add(this.DataGridView_pointMonitor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ElementDataManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "测点数据编辑";
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_pointMonitor)).EndInit();
            this.groupBoxPoints.ResumeLayout(false);
            this.groupBoxPoints.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.groupBoxLines.ResumeLayout(false);
            this.groupBoxLines.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_LineMonitor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        internal stdOldW.WinFormHelper.eZDataGridViewPaste DataGridView_pointMonitor;
        internal System.Windows.Forms.Button btnSaveChange;
        internal System.Windows.Forms.ComboBox cmbx_elements;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button btnDraw;
        private GroupBox groupBoxPoints;
        private CheckBox checkBox_columnheave;
        private CheckBox checkBox_groundsettlement;
        private Panel panel1;
        private GroupBox groupBoxLines;
        private CheckBox checkBox_incline;
        internal Label label2;
        internal stdOldW.WinFormHelper.eZDataGridViewPaste DataGridView_LineMonitor;
        internal Button btnSetNodes;
        private ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private CheckBox checkBox_strutaxisforce;
        internal Button btnActivateDatagridview;
        internal Button btnRename;
    }
}

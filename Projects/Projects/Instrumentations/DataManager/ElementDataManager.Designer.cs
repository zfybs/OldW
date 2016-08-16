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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnSaveChange = new System.Windows.Forms.Button();
            this.cmbx_elements = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.btnDraw = new System.Windows.Forms.Button();
            this.DataGridView_pointMonitor = new stdOldW.WinFormHelper.eZDataGridViewPaste();
            this.groupBoxPoints = new System.Windows.Forms.GroupBox();
            this.checkBox_OtherPoint = new System.Windows.Forms.CheckBox();
            this.checkBox_strutaxisforce = new System.Windows.Forms.CheckBox();
            this.checkBox_WaterTable = new System.Windows.Forms.CheckBox();
            this.checkBox_columnheave = new System.Windows.Forms.CheckBox();
            this.checkBox_groundsettlement = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxLines = new System.Windows.Forms.GroupBox();
            this.checkBox_OtherLine = new System.Windows.Forms.CheckBox();
            this.checkBox_WallTop = new System.Windows.Forms.CheckBox();
            this.checkBox_SoilIncline = new System.Windows.Forms.CheckBox();
            this.checkBox_WallIncline = new System.Windows.Forms.CheckBox();
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
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.DataGridView_pointMonitor.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle17;
            this.DataGridView_pointMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_pointMonitor.Location = new System.Drawing.Point(116, 68);
            this.DataGridView_pointMonitor.Name = "DataGridView_pointMonitor";
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridView_pointMonitor.RowHeadersDefaultCellStyle = dataGridViewCellStyle18;
            this.DataGridView_pointMonitor.RowHeadersWidth = 52;
            this.DataGridView_pointMonitor.RowTemplate.Height = 23;
            this.DataGridView_pointMonitor.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DataGridView_pointMonitor.Size = new System.Drawing.Size(280, 428);
            this.DataGridView_pointMonitor.TabIndex = 0;
            this.DataGridView_pointMonitor.Resize += new System.EventHandler(this.DataGrid_pointMonitor_Resize);
            // 
            // groupBoxPoints
            // 
            this.groupBoxPoints.Controls.Add(this.checkBox_OtherPoint);
            this.groupBoxPoints.Controls.Add(this.checkBox_strutaxisforce);
            this.groupBoxPoints.Controls.Add(this.checkBox_WaterTable);
            this.groupBoxPoints.Controls.Add(this.checkBox_columnheave);
            this.groupBoxPoints.Controls.Add(this.checkBox_groundsettlement);
            this.groupBoxPoints.Location = new System.Drawing.Point(3, 120);
            this.groupBoxPoints.Name = "groupBoxPoints";
            this.groupBoxPoints.Size = new System.Drawing.Size(92, 128);
            this.groupBoxPoints.TabIndex = 4;
            this.groupBoxPoints.TabStop = false;
            this.groupBoxPoints.Text = "点测点";
            // 
            // checkBox_OtherPoint
            // 
            this.checkBox_OtherPoint.AutoSize = true;
            this.checkBox_OtherPoint.Location = new System.Drawing.Point(6, 109);
            this.checkBox_OtherPoint.Name = "checkBox_OtherPoint";
            this.checkBox_OtherPoint.Size = new System.Drawing.Size(84, 16);
            this.checkBox_OtherPoint.TabIndex = 0;
            this.checkBox_OtherPoint.Text = "其他点测点";
            this.checkBox_OtherPoint.UseVisualStyleBackColor = true;
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
            // checkBox_WaterTable
            // 
            this.checkBox_WaterTable.AutoSize = true;
            this.checkBox_WaterTable.Location = new System.Drawing.Point(6, 87);
            this.checkBox_WaterTable.Name = "checkBox_WaterTable";
            this.checkBox_WaterTable.Size = new System.Drawing.Size(48, 16);
            this.checkBox_WaterTable.TabIndex = 0;
            this.checkBox_WaterTable.Text = "水位";
            this.checkBox_WaterTable.UseVisualStyleBackColor = true;
            // 
            // checkBox_columnheave
            // 
            this.checkBox_columnheave.AutoSize = true;
            this.checkBox_columnheave.Location = new System.Drawing.Point(6, 42);
            this.checkBox_columnheave.Name = "checkBox_columnheave";
            this.checkBox_columnheave.Size = new System.Drawing.Size(72, 16);
            this.checkBox_columnheave.TabIndex = 0;
            this.checkBox_columnheave.Text = "立柱隆沉";
            this.checkBox_columnheave.UseVisualStyleBackColor = true;
            // 
            // checkBox_groundsettlement
            // 
            this.checkBox_groundsettlement.AutoSize = true;
            this.checkBox_groundsettlement.Location = new System.Drawing.Point(6, 20);
            this.checkBox_groundsettlement.Name = "checkBox_groundsettlement";
            this.checkBox_groundsettlement.Size = new System.Drawing.Size(72, 16);
            this.checkBox_groundsettlement.TabIndex = 0;
            this.checkBox_groundsettlement.Text = "地表隆沉";
            this.checkBox_groundsettlement.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBoxLines);
            this.panel1.Controls.Add(this.groupBoxPoints);
            this.panel1.Location = new System.Drawing.Point(12, 36);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(98, 255);
            this.panel1.TabIndex = 5;
            // 
            // groupBoxLines
            // 
            this.groupBoxLines.Controls.Add(this.checkBox_OtherLine);
            this.groupBoxLines.Controls.Add(this.checkBox_WallTop);
            this.groupBoxLines.Controls.Add(this.checkBox_SoilIncline);
            this.groupBoxLines.Controls.Add(this.checkBox_WallIncline);
            this.groupBoxLines.Location = new System.Drawing.Point(3, 3);
            this.groupBoxLines.Name = "groupBoxLines";
            this.groupBoxLines.Size = new System.Drawing.Size(92, 111);
            this.groupBoxLines.TabIndex = 4;
            this.groupBoxLines.TabStop = false;
            this.groupBoxLines.Text = "线测点";
            // 
            // checkBox_OtherLine
            // 
            this.checkBox_OtherLine.AutoSize = true;
            this.checkBox_OtherLine.Location = new System.Drawing.Point(4, 87);
            this.checkBox_OtherLine.Name = "checkBox_OtherLine";
            this.checkBox_OtherLine.Size = new System.Drawing.Size(84, 16);
            this.checkBox_OtherLine.TabIndex = 0;
            this.checkBox_OtherLine.Text = "其他线测点";
            this.checkBox_OtherLine.UseVisualStyleBackColor = true;
            // 
            // checkBox_WallTop
            // 
            this.checkBox_WallTop.AutoSize = true;
            this.checkBox_WallTop.Location = new System.Drawing.Point(4, 65);
            this.checkBox_WallTop.Name = "checkBox_WallTop";
            this.checkBox_WallTop.Size = new System.Drawing.Size(72, 16);
            this.checkBox_WallTop.TabIndex = 0;
            this.checkBox_WallTop.Text = "墙顶位移";
            this.checkBox_WallTop.UseVisualStyleBackColor = true;
            // 
            // checkBox_SoilIncline
            // 
            this.checkBox_SoilIncline.AutoSize = true;
            this.checkBox_SoilIncline.Location = new System.Drawing.Point(5, 42);
            this.checkBox_SoilIncline.Name = "checkBox_SoilIncline";
            this.checkBox_SoilIncline.Size = new System.Drawing.Size(72, 16);
            this.checkBox_SoilIncline.TabIndex = 0;
            this.checkBox_SoilIncline.Text = "土体测斜";
            this.checkBox_SoilIncline.UseVisualStyleBackColor = true;
            // 
            // checkBox_WallIncline
            // 
            this.checkBox_WallIncline.AutoSize = true;
            this.checkBox_WallIncline.Location = new System.Drawing.Point(5, 20);
            this.checkBox_WallIncline.Name = "checkBox_WallIncline";
            this.checkBox_WallIncline.Size = new System.Drawing.Size(72, 16);
            this.checkBox_WallIncline.TabIndex = 0;
            this.checkBox_WallIncline.Text = "墙体测斜";
            this.checkBox_WallIncline.UseVisualStyleBackColor = true;
            // 
            // btnSetNodes
            // 
            this.btnSetNodes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetNodes.Location = new System.Drawing.Point(321, 9);
            this.btnSetNodes.Name = "btnSetNodes";
            this.btnSetNodes.Size = new System.Drawing.Size(75, 23);
            this.btnSetNodes.TabIndex = 1;
            this.btnSetNodes.Text = "节点";
            this.toolTip1.SetToolTip(this.btnSetNodes, "设置线测点中每一个节点距离测点起点的距离。");
            this.btnSetNodes.UseVisualStyleBackColor = true;
            this.btnSetNodes.Visible = false;
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
            dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle19.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.DataGridView_LineMonitor.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle19;
            this.DataGridView_LineMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_LineMonitor.Location = new System.Drawing.Point(116, 68);
            this.DataGridView_LineMonitor.Name = "DataGridView_LineMonitor";
            dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle20.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle20.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle20.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle20.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle20.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridView_LineMonitor.RowHeadersDefaultCellStyle = dataGridViewCellStyle20;
            this.DataGridView_LineMonitor.RowHeadersWidth = 52;
            this.DataGridView_LineMonitor.RowTemplate.Height = 23;
            this.DataGridView_LineMonitor.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DataGridView_LineMonitor.Size = new System.Drawing.Size(280, 428);
            this.DataGridView_LineMonitor.TabIndex = 0;
            this.DataGridView_LineMonitor.Resize += new System.EventHandler(this.DataGrid_pointMonitor_Resize);
            // 
            // btnActivateDatagridview
            // 
            this.btnActivateDatagridview.Location = new System.Drawing.Point(12, 297);
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
            this.Controls.Add(this.btnSetNodes);
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
        private CheckBox checkBox_WallIncline;
        internal Label label2;
        internal stdOldW.WinFormHelper.eZDataGridViewPaste DataGridView_LineMonitor;
        internal Button btnSetNodes;
        private ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private CheckBox checkBox_strutaxisforce;
        internal Button btnActivateDatagridview;
        internal Button btnRename;
        private CheckBox checkBox_SoilIncline;
        private CheckBox checkBox_OtherPoint;
        private CheckBox checkBox_WaterTable;
        private CheckBox checkBox_OtherLine;
        private CheckBox checkBox_WallTop;
    }
}

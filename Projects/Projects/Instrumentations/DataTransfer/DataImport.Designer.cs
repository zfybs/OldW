using System;
using System.Windows.Forms;

namespace OldW.DataManager
{
    public partial class DataImport : System.Windows.Forms.Form
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
            this.Label2 = new System.Windows.Forms.Label();
            this.ButtonImport = new System.Windows.Forms.Button();
            this.dataGridViewExcel = new System.Windows.Forms.DataGridView();
            this.buttonChooseFile = new System.Windows.Forms.Button();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.buttonMapping = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonCheckMultiple = new System.Windows.Forms.Button();
            this.buttonUnCheckMultiple = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.labelProgress = new System.Windows.Forms.Label();
            this.buttonExit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExcel)).BeginInit();
            this.SuspendLayout();
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(10, 20);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(95, 12);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "选择Excel数据源";
            // 
            // ButtonImport
            // 
            this.ButtonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonImport.Enabled = false;
            this.ButtonImport.Location = new System.Drawing.Point(554, 45);
            this.ButtonImport.Name = "ButtonImport";
            this.ButtonImport.Size = new System.Drawing.Size(75, 23);
            this.ButtonImport.TabIndex = 3;
            this.ButtonImport.Text = "导入 -->";
            this.ButtonImport.UseVisualStyleBackColor = true;
            this.ButtonImport.Click += new System.EventHandler(this.ButtonImport_Click);
            // 
            // dataGridViewExcel
            // 
            this.dataGridViewExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewExcel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewExcel.Location = new System.Drawing.Point(12, 74);
            this.dataGridViewExcel.Name = "dataGridViewExcel";
            this.dataGridViewExcel.RowTemplate.Height = 23;
            this.dataGridViewExcel.Size = new System.Drawing.Size(625, 402);
            this.dataGridViewExcel.TabIndex = 5;
            // 
            // buttonChooseFile
            // 
            this.buttonChooseFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChooseFile.Location = new System.Drawing.Point(554, 15);
            this.buttonChooseFile.Name = "buttonChooseFile";
            this.buttonChooseFile.Size = new System.Drawing.Size(75, 23);
            this.buttonChooseFile.TabIndex = 3;
            this.buttonChooseFile.Text = "选择";
            this.buttonChooseFile.UseVisualStyleBackColor = true;
            this.buttonChooseFile.Click += new System.EventHandler(this.buttonChooseFile_Click);
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilePath.Location = new System.Drawing.Point(132, 17);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.Size = new System.Drawing.Size(416, 21);
            this.textBoxFilePath.TabIndex = 6;
            this.textBoxFilePath.Text = "F:\\Software\\Revit\\RevitDevelop\\OldW\\bin\\SQL墙体测斜.xlsx";
            // 
            // buttonMapping
            // 
            this.buttonMapping.Location = new System.Drawing.Point(12, 45);
            this.buttonMapping.Name = "buttonMapping";
            this.buttonMapping.Size = new System.Drawing.Size(75, 23);
            this.buttonMapping.TabIndex = 3;
            this.buttonMapping.Text = "映射";
            this.buttonMapping.UseVisualStyleBackColor = true;
            this.buttonMapping.Click += new System.EventHandler(this.buttonMapping_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 516);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(649, 10);
            this.progressBar1.TabIndex = 7;
            // 
            // buttonCheckMultiple
            // 
            this.buttonCheckMultiple.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCheckMultiple.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.buttonCheckMultiple.Location = new System.Drawing.Point(57, 487);
            this.buttonCheckMultiple.Name = "buttonCheckMultiple";
            this.buttonCheckMultiple.Size = new System.Drawing.Size(20, 23);
            this.buttonCheckMultiple.TabIndex = 3;
            this.buttonCheckMultiple.Text = "√";
            this.toolTip1.SetToolTip(this.buttonCheckMultiple, "勾选所有表格中选中的数据行");
            this.buttonCheckMultiple.UseVisualStyleBackColor = false;
            this.buttonCheckMultiple.Visible = false;
            this.buttonCheckMultiple.Click += new System.EventHandler(this.buttonCheckMultiple_Click);
            // 
            // buttonUnCheckMultiple
            // 
            this.buttonUnCheckMultiple.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUnCheckMultiple.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.buttonUnCheckMultiple.Location = new System.Drawing.Point(77, 487);
            this.buttonUnCheckMultiple.Name = "buttonUnCheckMultiple";
            this.buttonUnCheckMultiple.Size = new System.Drawing.Size(20, 23);
            this.buttonUnCheckMultiple.TabIndex = 3;
            this.buttonUnCheckMultiple.Text = "×";
            this.toolTip1.SetToolTip(this.buttonUnCheckMultiple, "取消勾选所有表格中选中的数据行");
            this.buttonUnCheckMultiple.UseVisualStyleBackColor = false;
            this.buttonUnCheckMultiple.Visible = false;
            this.buttonUnCheckMultiple.Click += new System.EventHandler(this.buttonUnCheckMultiple_Click);
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(300, 495);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(29, 12);
            this.labelProgress.TabIndex = 8;
            this.labelProgress.Text = "100%";
            this.labelProgress.Visible = false;
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Location = new System.Drawing.Point(554, 487);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(75, 23);
            this.buttonExit.TabIndex = 3;
            this.buttonExit.Text = "退出";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.ButtonExit_Click);
            // 
            // DataImport
            // 
            this.AcceptButton = this.buttonMapping;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 526);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.textBoxFilePath);
            this.Controls.Add(this.dataGridViewExcel);
            this.Controls.Add(this.buttonMapping);
            this.Controls.Add(this.buttonChooseFile);
            this.Controls.Add(this.buttonUnCheckMultiple);
            this.Controls.Add(this.buttonCheckMultiple);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.ButtonImport);
            this.Controls.Add(this.Label2);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(665, 565);
            this.Name = "DataImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据导入";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DataImport_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewExcel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal Label Label2;
        internal Button ButtonImport;
        private DataGridView dataGridViewExcel;
        internal Button buttonChooseFile;
        private TextBox textBoxFilePath;
        internal Button buttonMapping;
        private ProgressBar progressBar1;
        internal Button buttonCheckMultiple;
        internal Button buttonUnCheckMultiple;
        private ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private Label labelProgress;
        internal Button buttonExit;
    }

}
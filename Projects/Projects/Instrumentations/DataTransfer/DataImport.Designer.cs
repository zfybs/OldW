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

        //Required by the Windows Form Designer
        private System.ComponentModel.Container components = null;

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Label2 = new System.Windows.Forms.Label();
            this.ButtonImport = new System.Windows.Forms.Button();
            this.dataGridViewExcel = new System.Windows.Forms.DataGridView();
            this.buttonChooseFile = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonMapping = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
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
            this.ButtonImport.Location = new System.Drawing.Point(562, 487);
            this.ButtonImport.Name = "ButtonImport";
            this.ButtonImport.Size = new System.Drawing.Size(75, 23);
            this.ButtonImport.TabIndex = 3;
            this.ButtonImport.Text = "导入 -->";
            this.ButtonImport.UseVisualStyleBackColor = true;
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
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(132, 17);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(416, 21);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "F:\\Software\\Revit\\RevitDevelop\\OldW\\bin\\SQL监测数据.xlsx";
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
            // DataImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 526);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.dataGridViewExcel);
            this.Controls.Add(this.buttonMapping);
            this.Controls.Add(this.buttonChooseFile);
            this.Controls.Add(this.ButtonImport);
            this.Controls.Add(this.Label2);
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
        private TextBox textBox1;
        internal Button buttonMapping;
        private ProgressBar progressBar1;

    }

}
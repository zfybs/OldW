// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
//using Autodesk.Revit.DB;
	using System.Xml.Linq;
//using Autodesk.Revit.UI;
	using Microsoft.VisualBasic;
using System.Collections;
using System.Data;
// End of VB project level imports


namespace OldW.DataManager
{public partial class ElementDataManager : System.Windows.Forms.Form
	{
		
		//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
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
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnSaveChange = new System.Windows.Forms.Button();
            this.cmbx_elements = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.DataGrid_pointMonitor = new std_ez.eZDataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnDraw = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid_pointMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSaveChange
            // 
            this.btnSaveChange.Location = new System.Drawing.Point(298, 68);
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
            this.cmbx_elements.Location = new System.Drawing.Point(12, 33);
            this.cmbx_elements.Name = "cmbx_elements";
            this.cmbx_elements.Size = new System.Drawing.Size(165, 20);
            this.cmbx_elements.TabIndex = 2;
            this.cmbx_elements.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 9);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(65, 12);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "操作的测点";
            // 
            // DataGrid_pointMonitor
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.DataGrid_pointMonitor.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.DataGrid_pointMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGrid_pointMonitor.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.DataGrid_pointMonitor.Location = new System.Drawing.Point(12, 68);
            this.DataGrid_pointMonitor.Name = "DataGrid_pointMonitor";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGrid_pointMonitor.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.DataGrid_pointMonitor.RowHeadersWidth = 52;
            this.DataGrid_pointMonitor.RowTemplate.Height = 23;
            this.DataGrid_pointMonitor.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DataGrid_pointMonitor.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DataGrid_pointMonitor.Size = new System.Drawing.Size(280, 359);
            this.DataGrid_pointMonitor.TabIndex = 0;
            this.DataGrid_pointMonitor.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.MyDataGridView1_DataError);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "日期";
            this.Column1.Name = "Column1";
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "数据";
            this.Column2.Name = "Column2";
            this.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // btnDraw
            // 
            this.btnDraw.Location = new System.Drawing.Point(298, 97);
            this.btnDraw.Name = "btnDraw";
            this.btnDraw.Size = new System.Drawing.Size(75, 23);
            this.btnDraw.TabIndex = 1;
            this.btnDraw.Text = "绘图";
            this.btnDraw.UseVisualStyleBackColor = true;
            this.btnDraw.Click += new System.EventHandler(this.btnDraw_Click);
            // 
            // ElementDataManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 434);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.cmbx_elements);
            this.Controls.Add(this.btnDraw);
            this.Controls.Add(this.btnSaveChange);
            this.Controls.Add(this.DataGrid_pointMonitor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ElementDataManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "测点数据编辑";
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid_pointMonitor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		internal std_ez.eZDataGridView DataGrid_pointMonitor;
		internal System.Windows.Forms.Button btnSaveChange;
		internal System.Windows.Forms.DataGridViewTextBoxColumn Column1;
		internal System.Windows.Forms.DataGridViewTextBoxColumn Column2;
		internal System.Windows.Forms.ComboBox cmbx_elements;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.Button btnDraw;
	}
}

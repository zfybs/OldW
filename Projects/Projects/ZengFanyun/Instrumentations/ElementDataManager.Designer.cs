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
			System.Windows.Forms.DataGridViewCellStyle DataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle DataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.btnSaveChange = new System.Windows.Forms.Button();
			this.btnSaveChange.Click += new System.EventHandler(this.SaveTableToElement);
			this.cmbx_elements = new System.Windows.Forms.ComboBox();
			this.cmbx_elements.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
			this.Label1 = new System.Windows.Forms.Label();
			this.eZDataGridView1 = new std_ez.eZDataGridView();
			this.eZDataGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.MyDataGridView1_DataError);
			this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.btnDraw = new System.Windows.Forms.Button();
			this.btnDraw.Click += new System.EventHandler(this.btnDraw_Click);
			((System.ComponentModel.ISupportInitialize) this.eZDataGridView1).BeginInit();
			this.SuspendLayout();
			//
			//btnSaveChange
			//
			this.btnSaveChange.Location = new System.Drawing.Point(298, 68);
			this.btnSaveChange.Name = "btnSaveChange";
			this.btnSaveChange.Size = new System.Drawing.Size(75, 23);
			this.btnSaveChange.TabIndex = 1;
			this.btnSaveChange.Text = "保存";
			this.btnSaveChange.UseVisualStyleBackColor = true;
			//
			//cmbx_elements
			//
			this.cmbx_elements.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbx_elements.FormattingEnabled = true;
			this.cmbx_elements.Location = new System.Drawing.Point(12, 33);
			this.cmbx_elements.Name = "cmbx_elements";
			this.cmbx_elements.Size = new System.Drawing.Size(165, 20);
			this.cmbx_elements.TabIndex = 2;
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(12, 9);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(65, 12);
			this.Label1.TabIndex = 3;
			this.Label1.Text = "操作的测点";
			//
			//eZDataGridView1
			//
			DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			DataGridViewCellStyle1.Font = new System.Drawing.Font("SimSun", (float) (9.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(134));
			DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			this.eZDataGridView1.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1;
			this.eZDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.eZDataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.Column1, this.Column2});
			this.eZDataGridView1.Location = new System.Drawing.Point(12, 68);
			this.eZDataGridView1.Name = "eZDataGridView1";
			DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
			DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			DataGridViewCellStyle2.Font = new System.Drawing.Font("SimSun", (float) (9.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(134));
			DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.eZDataGridView1.RowHeadersDefaultCellStyle = DataGridViewCellStyle2;
			this.eZDataGridView1.RowHeadersWidth = 52;
			this.eZDataGridView1.RowTemplate.Height = 23;
			this.eZDataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.eZDataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.eZDataGridView1.Size = new System.Drawing.Size(280, 359);
			this.eZDataGridView1.TabIndex = 0;
			//
			//Column1
			//
			this.Column1.HeaderText = "日期";
			this.Column1.Name = "Column1";
			this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			//
			//Column2
			//
			this.Column2.HeaderText = "数据";
			this.Column2.Name = "Column2";
			this.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			//
			//btnDraw
			//
			this.btnDraw.Location = new System.Drawing.Point(298, 97);
			this.btnDraw.Name = "btnDraw";
			this.btnDraw.Size = new System.Drawing.Size(75, 23);
			this.btnDraw.TabIndex = 1;
			this.btnDraw.Text = "绘图";
			this.btnDraw.UseVisualStyleBackColor = true;
			//
			//ElementDataManager
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (12.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(382, 434);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.cmbx_elements);
			this.Controls.Add(this.btnDraw);
			this.Controls.Add(this.btnSaveChange);
			this.Controls.Add(this.eZDataGridView1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ElementDataManager";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "测点数据编辑";
			((System.ComponentModel.ISupportInitialize) this.eZDataGridView1).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		internal std_ez.eZDataGridView eZDataGridView1;
		internal System.Windows.Forms.Button btnSaveChange;
		internal System.Windows.Forms.DataGridViewTextBoxColumn Column1;
		internal System.Windows.Forms.DataGridViewTextBoxColumn Column2;
		internal System.Windows.Forms.ComboBox cmbx_elements;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.Button btnDraw;
	}
}

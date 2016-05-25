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


namespace OldW.Excavation
{
public 
	partial class frm_ExcavationInfo : System.Windows.Forms.Form
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_ExcavationInfo));
			this.DataGridView1 = new System.Windows.Forms.DataGridView();
			this.DataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellContentClick);
			this.DataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellValueChanged);
			this.DataGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView1_DataError);
			this.btn_SyncMultiple = new System.Windows.Forms.Button();
			this.btn_SyncMultiple.Click += new System.EventHandler(this.btn_SyncMultiple_Click);
			this.Label1 = new System.Windows.Forms.Label();
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.btnGetExcavInfo = new System.Windows.Forms.Button();
			this.btnGetExcavInfo.Click += new System.EventHandler(this.btnGetExcavInfo_Click);
			this.BtnClearEmpty = new System.Windows.Forms.Button();
			this.BtnClearEmpty.Click += new System.EventHandler(this.BtnClearEmpty_Click);
			this.CheckBox1 = new System.Windows.Forms.CheckBox();
			this.CheckBox1.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
			this.CheckBox_MultiVisible = new System.Windows.Forms.CheckBox();
			this.CheckBox_MultiVisible.CheckedChanged += new System.EventHandler(this.CheckBox_MultiVisible_CheckedChanged);
			((System.ComponentModel.ISupportInitialize) this.DataGridView1).BeginInit();
			this.SuspendLayout();
			//
			//DataGridView1
			//
			this.DataGridView1.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DataGridView1.Location = new System.Drawing.Point(12, 41);
			this.DataGridView1.Name = "DataGridView1";
			this.DataGridView1.RowTemplate.Height = 23;
			this.DataGridView1.Size = new System.Drawing.Size(536, 459);
			this.DataGridView1.TabIndex = 1;
			//
			//btn_SyncMultiple
			//
			this.btn_SyncMultiple.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btn_SyncMultiple.Location = new System.Drawing.Point(562, 477);
			this.btn_SyncMultiple.Name = "btn_SyncMultiple";
			this.btn_SyncMultiple.Size = new System.Drawing.Size(75, 23);
			this.btn_SyncMultiple.TabIndex = 0;
			this.btn_SyncMultiple.Text = "同步";
			this.btn_SyncMultiple.UseVisualStyleBackColor = true;
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(12, 16);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(137, 12);
			this.Label1.TabIndex = 2;
			this.Label1.Text = "当前模型中的开挖土体：";
			//
			//btnGetExcavInfo
			//
			this.btnGetExcavInfo.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnGetExcavInfo.Location = new System.Drawing.Point(562, 41);
			this.btnGetExcavInfo.Name = "btnGetExcavInfo";
			this.btnGetExcavInfo.Size = new System.Drawing.Size(75, 23);
			this.btnGetExcavInfo.TabIndex = 0;
			this.btnGetExcavInfo.Text = "开挖信息";
			this.ToolTip1.SetToolTip(this.btnGetExcavInfo, "提取模型中的开挖土体的信息，并显示在列表中。");
			this.btnGetExcavInfo.UseVisualStyleBackColor = true;
			//
			//BtnClearEmpty
			//
			this.BtnClearEmpty.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.BtnClearEmpty.Location = new System.Drawing.Point(562, 417);
			this.BtnClearEmpty.Name = "BtnClearEmpty";
			this.BtnClearEmpty.Size = new System.Drawing.Size(75, 23);
			this.BtnClearEmpty.TabIndex = 4;
			this.BtnClearEmpty.Text = "清理";
			this.ToolTip1.SetToolTip(this.BtnClearEmpty, "清理模型中，没有实例对象的开挖土体族。");
			this.BtnClearEmpty.UseVisualStyleBackColor = true;
			//
			//CheckBox1
			//
			this.CheckBox1.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CheckBox1.AutoSize = true;
			this.CheckBox1.Location = new System.Drawing.Point(562, 455);
			this.CheckBox1.Name = "CheckBox1";
			this.CheckBox1.Size = new System.Drawing.Size(48, 16);
			this.CheckBox1.TabIndex = 3;
			this.CheckBox1.Text = "全选";
			this.CheckBox1.UseVisualStyleBackColor = true;
			//
			//CheckBox_MultiVisible
			//
			this.CheckBox_MultiVisible.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CheckBox_MultiVisible.AutoSize = true;
			this.CheckBox_MultiVisible.Checked = true;
			this.CheckBox_MultiVisible.CheckState = System.Windows.Forms.CheckState.Indeterminate;
			this.CheckBox_MultiVisible.Location = new System.Drawing.Point(562, 379);
			this.CheckBox_MultiVisible.Name = "CheckBox_MultiVisible";
			this.CheckBox_MultiVisible.Size = new System.Drawing.Size(48, 16);
			this.CheckBox_MultiVisible.TabIndex = 5;
			this.CheckBox_MultiVisible.Text = "可见";
			this.CheckBox_MultiVisible.ThreeState = true;
			this.CheckBox_MultiVisible.UseVisualStyleBackColor = true;
			//
			//frm_ExcavationInfo
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (12.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(649, 512);
			this.Controls.Add(this.CheckBox_MultiVisible);
			this.Controls.Add(this.BtnClearEmpty);
			this.Controls.Add(this.CheckBox1);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.DataGridView1);
			this.Controls.Add(this.btnGetExcavInfo);
			this.Controls.Add(this.btn_SyncMultiple);
			this.Icon = (System.Drawing.Icon) (resources.GetObject("$this.Icon"));
			this.Name = "frm_ExcavationInfo";
			this.Text = "土体开挖模型";
			((System.ComponentModel.ISupportInitialize) this.DataGridView1).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		internal System.Windows.Forms.DataGridView DataGridView1;
		internal System.Windows.Forms.Button btn_SyncMultiple;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.ToolTip ToolTip1;
		internal System.Windows.Forms.Button btnGetExcavInfo;
		internal System.Windows.Forms.CheckBox CheckBox1;
		internal System.Windows.Forms.Button BtnClearEmpty;
		internal CheckBox CheckBox_MultiVisible;
	}
}

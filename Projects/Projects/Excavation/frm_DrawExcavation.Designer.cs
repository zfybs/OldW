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
    public partial class frm_DrawExcavation : System.Windows.Forms.Form
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
			this.Closed += new System.EventHandler(frm_DrawExcavation_Closed);
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_DrawExcavation));
			this.GroupBox1 = new System.Windows.Forms.GroupBox();
			this.CheckBox_DrawSucceeded = new System.Windows.Forms.CheckBox();
			this.btn_DrawCurves = new System.Windows.Forms.Button();
			this.btn_DrawCurves.Click += new System.EventHandler(this.btn_DrawCurves_Click);
			this.LabelSides = new System.Windows.Forms.Label();
			this.ComboBox_sides = new System.Windows.Forms.ComboBox();
			this.RadioBtn_Polygon = new System.Windows.Forms.RadioButton();
			this.RadioBtn_Draw = new System.Windows.Forms.RadioButton();
			this.RadioBtn_PickShape = new System.Windows.Forms.RadioButton();
			this.Label2 = new System.Windows.Forms.Label();
			this.TextBox_Depth = new System.Windows.Forms.TextBox();
			this.TextBox_Depth.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
			this.BtnModeling = new System.Windows.Forms.Button();
			this.BtnModeling.Click += new System.EventHandler(this.BtnModeling_Click);
			this.GroupBox2 = new System.Windows.Forms.GroupBox();
			this.TextBox_StartedDate = new System.Windows.Forms.TextBox();
			this.TextBox_SoilName = new System.Windows.Forms.TextBox();
			this.TextBox_SoilName.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
			this.TextBox_CompletedDate = new System.Windows.Forms.TextBox();
			this.Label4 = new System.Windows.Forms.Label();
			this.btn__DateCalendar = new System.Windows.Forms.Button();
			this.Label3 = new System.Windows.Forms.Label();
			this.Label1 = new System.Windows.Forms.Label();
			this.LabelCompletedDate = new System.Windows.Forms.Label();
			this.RadioBtn_ExcavSoil = new System.Windows.Forms.RadioButton();
			this.RadioBtn_ModelSoil = new System.Windows.Forms.RadioButton();
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.Btn_ClearCurves = new System.Windows.Forms.Button();
			this.Btn_ClearCurves.Click += new System.EventHandler(this.Btn_ClearCurves_Click);
			this.GroupBox1.SuspendLayout();
			this.GroupBox2.SuspendLayout();
			this.SuspendLayout();
			//
			//GroupBox1
			//
			this.GroupBox1.Controls.Add(this.CheckBox_DrawSucceeded);
			this.GroupBox1.Controls.Add(this.Btn_ClearCurves);
			this.GroupBox1.Controls.Add(this.btn_DrawCurves);
			this.GroupBox1.Controls.Add(this.LabelSides);
			this.GroupBox1.Controls.Add(this.ComboBox_sides);
			this.GroupBox1.Controls.Add(this.RadioBtn_Polygon);
			this.GroupBox1.Controls.Add(this.RadioBtn_Draw);
			this.GroupBox1.Controls.Add(this.RadioBtn_PickShape);
			this.GroupBox1.Location = new System.Drawing.Point(195, 12);
			this.GroupBox1.Name = "GroupBox1";
			this.GroupBox1.Size = new System.Drawing.Size(214, 155);
			this.GroupBox1.TabIndex = 0;
			this.GroupBox1.TabStop = false;
			this.GroupBox1.Text = "轮廓形状";
			//
			//CheckBox_DrawSucceeded
			//
			this.CheckBox_DrawSucceeded.AutoSize = true;
			this.CheckBox_DrawSucceeded.Enabled = false;
			this.CheckBox_DrawSucceeded.Location = new System.Drawing.Point(87, 122);
			this.CheckBox_DrawSucceeded.Name = "CheckBox_DrawSucceeded";
			this.CheckBox_DrawSucceeded.Size = new System.Drawing.Size(72, 16);
			this.CheckBox_DrawSucceeded.TabIndex = 5;
			this.CheckBox_DrawSucceeded.Text = "绘制成功";
			this.CheckBox_DrawSucceeded.UseVisualStyleBackColor = true;
			//
			//btn_DrawCurves
			//
			this.btn_DrawCurves.Location = new System.Drawing.Point(84, 90);
			this.btn_DrawCurves.Name = "btn_DrawCurves";
			this.btn_DrawCurves.Size = new System.Drawing.Size(59, 23);
			this.btn_DrawCurves.TabIndex = 4;
			this.btn_DrawCurves.Text = "绘制";
			this.btn_DrawCurves.UseVisualStyleBackColor = true;
			//
			//LabelSides
			//
			this.LabelSides.AutoSize = true;
			this.LabelSides.Location = new System.Drawing.Point(94, 27);
			this.LabelSides.Name = "LabelSides";
			this.LabelSides.Size = new System.Drawing.Size(29, 12);
			this.LabelSides.TabIndex = 3;
			this.LabelSides.Text = "边数";
			//
			//ComboBox_sides
			//
			this.ComboBox_sides.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBox_sides.FormattingEnabled = true;
			this.ComboBox_sides.Items.AddRange(new object[] {"3", "4", "5", "6"});
			this.ComboBox_sides.Location = new System.Drawing.Point(129, 24);
			this.ComboBox_sides.Name = "ComboBox_sides";
			this.ComboBox_sides.Size = new System.Drawing.Size(59, 20);
			this.ComboBox_sides.TabIndex = 2;
			//
			//RadioBtn_Polygon
			//
			this.RadioBtn_Polygon.AutoSize = true;
			this.RadioBtn_Polygon.Location = new System.Drawing.Point(6, 28);
			this.RadioBtn_Polygon.Name = "RadioBtn_Polygon";
			this.RadioBtn_Polygon.Size = new System.Drawing.Size(59, 16);
			this.RadioBtn_Polygon.TabIndex = 1;
			this.RadioBtn_Polygon.Text = "多边形";
			this.RadioBtn_Polygon.UseVisualStyleBackColor = true;
			//
			//RadioBtn_Draw
			//
			this.RadioBtn_Draw.AutoSize = true;
			this.RadioBtn_Draw.Checked = true;
			this.RadioBtn_Draw.Location = new System.Drawing.Point(6, 90);
			this.RadioBtn_Draw.Name = "RadioBtn_Draw";
			this.RadioBtn_Draw.Size = new System.Drawing.Size(71, 16);
			this.RadioBtn_Draw.TabIndex = 0;
			this.RadioBtn_Draw.TabStop = true;
			this.RadioBtn_Draw.Text = "绘制轮廓";
			this.RadioBtn_Draw.UseVisualStyleBackColor = true;
			//
			//RadioBtn_PickShape
			//
			this.RadioBtn_PickShape.AutoSize = true;
			this.RadioBtn_PickShape.Location = new System.Drawing.Point(6, 59);
			this.RadioBtn_PickShape.Name = "RadioBtn_PickShape";
			this.RadioBtn_PickShape.Size = new System.Drawing.Size(71, 16);
			this.RadioBtn_PickShape.TabIndex = 0;
			this.RadioBtn_PickShape.Text = "选择轮廓";
			this.RadioBtn_PickShape.UseVisualStyleBackColor = true;
			//
			//Label2
			//
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(228, 188);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(53, 12);
			this.Label2.TabIndex = 1;
			this.Label2.Text = "深度 (m)";
			//
			//TextBox_Depth
			//
			this.TextBox_Depth.Location = new System.Drawing.Point(299, 185);
			this.TextBox_Depth.Name = "TextBox_Depth";
			this.TextBox_Depth.Size = new System.Drawing.Size(100, 21);
			this.TextBox_Depth.TabIndex = 2;
			this.TextBox_Depth.Text = "2";
			//
			//BtnModeling
			//
			this.BtnModeling.Location = new System.Drawing.Point(324, 218);
			this.BtnModeling.Name = "BtnModeling";
			this.BtnModeling.Size = new System.Drawing.Size(75, 23);
			this.BtnModeling.TabIndex = 3;
			this.BtnModeling.Text = "建模";
			this.BtnModeling.UseVisualStyleBackColor = true;
			//
			//GroupBox2
			//
			this.GroupBox2.Controls.Add(this.TextBox_StartedDate);
			this.GroupBox2.Controls.Add(this.TextBox_SoilName);
			this.GroupBox2.Controls.Add(this.TextBox_CompletedDate);
			this.GroupBox2.Controls.Add(this.Label4);
			this.GroupBox2.Controls.Add(this.btn__DateCalendar);
			this.GroupBox2.Controls.Add(this.Label3);
			this.GroupBox2.Controls.Add(this.Label1);
			this.GroupBox2.Controls.Add(this.LabelCompletedDate);
			this.GroupBox2.Controls.Add(this.RadioBtn_ExcavSoil);
			this.GroupBox2.Controls.Add(this.RadioBtn_ModelSoil);
			this.GroupBox2.Location = new System.Drawing.Point(12, 12);
			this.GroupBox2.Name = "GroupBox2";
			this.GroupBox2.Size = new System.Drawing.Size(177, 192);
			this.GroupBox2.TabIndex = 1;
			this.GroupBox2.TabStop = false;
			this.GroupBox2.Text = "土体类型";
			//
			//TextBox_StartedDate
			//
			this.TextBox_StartedDate.Location = new System.Drawing.Point(45, 86);
			this.TextBox_StartedDate.Name = "TextBox_StartedDate";
			this.TextBox_StartedDate.Size = new System.Drawing.Size(87, 21);
			this.TextBox_StartedDate.TabIndex = 5;
			//
			//TextBox_SoilName
			//
			this.TextBox_SoilName.Location = new System.Drawing.Point(45, 149);
			this.TextBox_SoilName.Name = "TextBox_SoilName";
			this.TextBox_SoilName.Size = new System.Drawing.Size(87, 21);
			this.TextBox_SoilName.TabIndex = 2;
			this.ToolTip1.SetToolTip(this.TextBox_SoilName, "名称可以不指定,此时程序会以开挖完成日期作为默认名称.");
			//
			//TextBox_CompletedDate
			//
			this.TextBox_CompletedDate.Location = new System.Drawing.Point(45, 113);
			this.TextBox_CompletedDate.Name = "TextBox_CompletedDate";
			this.TextBox_CompletedDate.Size = new System.Drawing.Size(87, 21);
			this.TextBox_CompletedDate.TabIndex = 6;
			this.ToolTip1.SetToolTip(this.TextBox_CompletedDate, "精确到分钟，推荐的格式为\"2016/04/04 16:30\"");
			//
			//Label4
			//
			this.Label4.AutoSize = true;
			this.Label4.Location = new System.Drawing.Point(4, 152);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(29, 12);
			this.Label4.TabIndex = 1;
			this.Label4.Text = "名称";
			//
			//btn__DateCalendar
			//
			this.btn__DateCalendar.Location = new System.Drawing.Point(138, 98);
			this.btn__DateCalendar.Name = "btn__DateCalendar";
			this.btn__DateCalendar.Size = new System.Drawing.Size(31, 23);
			this.btn__DateCalendar.TabIndex = 4;
			this.btn__DateCalendar.Text = "...";
			this.btn__DateCalendar.UseVisualStyleBackColor = true;
			//
			//Label3
			//
			this.Label3.AutoSize = true;
			this.Label3.Location = new System.Drawing.Point(4, 116);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(17, 12);
			this.Label3.TabIndex = 3;
			this.Label3.Text = "To";
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(4, 89);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(29, 12);
			this.Label1.TabIndex = 3;
			this.Label1.Text = "From";
			//
			//LabelCompletedDate
			//
			this.LabelCompletedDate.AutoSize = true;
			this.LabelCompletedDate.Location = new System.Drawing.Point(4, 60);
			this.LabelCompletedDate.Name = "LabelCompletedDate";
			this.LabelCompletedDate.Size = new System.Drawing.Size(53, 12);
			this.LabelCompletedDate.TabIndex = 3;
			this.LabelCompletedDate.Text = "开挖日期";
			//
			//RadioBtn_ExcavSoil
			//
			this.RadioBtn_ExcavSoil.AutoSize = true;
			this.RadioBtn_ExcavSoil.Checked = true;
			this.RadioBtn_ExcavSoil.Location = new System.Drawing.Point(6, 26);
			this.RadioBtn_ExcavSoil.Name = "RadioBtn_ExcavSoil";
			this.RadioBtn_ExcavSoil.Size = new System.Drawing.Size(71, 16);
			this.RadioBtn_ExcavSoil.TabIndex = 1;
			this.RadioBtn_ExcavSoil.TabStop = true;
			this.RadioBtn_ExcavSoil.Text = "开挖土体";
			this.RadioBtn_ExcavSoil.UseVisualStyleBackColor = true;
			//
			//RadioBtn_ModelSoil
			//
			this.RadioBtn_ModelSoil.AutoSize = true;
			this.RadioBtn_ModelSoil.Location = new System.Drawing.Point(100, 28);
			this.RadioBtn_ModelSoil.Name = "RadioBtn_ModelSoil";
			this.RadioBtn_ModelSoil.Size = new System.Drawing.Size(71, 16);
			this.RadioBtn_ModelSoil.TabIndex = 2;
			this.RadioBtn_ModelSoil.Text = "模型土体";
			this.RadioBtn_ModelSoil.UseVisualStyleBackColor = true;
			//
			//Btn_ClearCurves
			//
			this.Btn_ClearCurves.Location = new System.Drawing.Point(149, 90);
			this.Btn_ClearCurves.Name = "Btn_ClearCurves";
			this.Btn_ClearCurves.Size = new System.Drawing.Size(59, 23);
			this.Btn_ClearCurves.TabIndex = 4;
			this.Btn_ClearCurves.Text = "删除";
			this.Btn_ClearCurves.UseVisualStyleBackColor = true;
			//
			//frm_DrawExcavation
			//
			this.AcceptButton = this.BtnModeling;
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (12.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(421, 253);
			this.Controls.Add(this.BtnModeling);
			this.Controls.Add(this.TextBox_Depth);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.GroupBox2);
			this.Controls.Add(this.GroupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = (System.Drawing.Icon) (resources.GetObject("$this.Icon"));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frm_DrawExcavation";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "绘制土体";
			this.GroupBox1.ResumeLayout(false);
			this.GroupBox1.PerformLayout();
			this.GroupBox2.ResumeLayout(false);
			this.GroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		internal System.Windows.Forms.GroupBox GroupBox1;
		internal System.Windows.Forms.Label LabelSides;
		internal System.Windows.Forms.ComboBox ComboBox_sides;
		internal System.Windows.Forms.RadioButton RadioBtn_Polygon;
		internal System.Windows.Forms.RadioButton RadioBtn_PickShape;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.TextBox TextBox_Depth;
		internal System.Windows.Forms.Button BtnModeling;
		internal System.Windows.Forms.GroupBox GroupBox2;
		internal System.Windows.Forms.TextBox TextBox_CompletedDate;
		internal System.Windows.Forms.Button btn__DateCalendar;
		internal System.Windows.Forms.Label LabelCompletedDate;
		internal System.Windows.Forms.RadioButton RadioBtn_ExcavSoil;
		internal System.Windows.Forms.RadioButton RadioBtn_ModelSoil;
		internal System.Windows.Forms.ToolTip ToolTip1;
		internal System.Windows.Forms.TextBox TextBox_StartedDate;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.TextBox TextBox_SoilName;
		internal System.Windows.Forms.Label Label4;
		internal RadioButton RadioBtn_Draw;
		internal Button btn_DrawCurves;
		internal CheckBox CheckBox_DrawSucceeded;
		internal Button Btn_ClearCurves;
	}
}

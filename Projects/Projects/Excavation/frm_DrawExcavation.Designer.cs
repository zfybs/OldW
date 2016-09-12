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
using eZstd.UserControls;

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
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.Btn_PickModelCurves = new System.Windows.Forms.Button();
            this.Btn_GetEdgeFromFace = new System.Windows.Forms.Button();
            this.Btn_CancelDrawCurves = new System.Windows.Forms.Button();
            this.Btn_DrawCurves = new System.Windows.Forms.Button();
            this.LabelSides = new System.Windows.Forms.Label();
            this.ComboBox_sides = new System.Windows.Forms.ComboBox();
            this.RadioBtn_Polygon = new System.Windows.Forms.RadioButton();
            this.RadioBtn_UserDrawShape = new System.Windows.Forms.RadioButton();
            this.CheckBox_DrawSucceeded = new System.Windows.Forms.CheckBox();
            this.Btn_ClearCurves = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.Btn_Modeling = new System.Windows.Forms.Button();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.TextBox_StartedDate = new System.Windows.Forms.TextBox();
            this.TextBox_SoilName = new System.Windows.Forms.TextBox();
            this.TextBox_CompletedDate = new System.Windows.Forms.TextBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.btn__DateCalendar = new System.Windows.Forms.Button();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.RadioBtn_ExcavSoil = new System.Windows.Forms.RadioButton();
            this.RadioBtn_ModelSoil = new System.Windows.Forms.RadioButton();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.TextBox_Depth = new eZstd.UserControls.TextBoxNum();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.Btn_PickModelCurves);
            this.GroupBox1.Controls.Add(this.Btn_GetEdgeFromFace);
            this.GroupBox1.Controls.Add(this.Btn_CancelDrawCurves);
            this.GroupBox1.Controls.Add(this.Btn_DrawCurves);
            this.GroupBox1.Controls.Add(this.LabelSides);
            this.GroupBox1.Controls.Add(this.ComboBox_sides);
            this.GroupBox1.Controls.Add(this.RadioBtn_Polygon);
            this.GroupBox1.Controls.Add(this.RadioBtn_UserDrawShape);
            this.GroupBox1.Location = new System.Drawing.Point(195, 12);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(214, 192);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "轮廓形状";
            // 
            // Btn_PickModelCurves
            // 
            this.Btn_PickModelCurves.Location = new System.Drawing.Point(11, 118);
            this.Btn_PickModelCurves.Name = "Btn_PickModelCurves";
            this.Btn_PickModelCurves.Size = new System.Drawing.Size(75, 23);
            this.Btn_PickModelCurves.TabIndex = 8;
            this.Btn_PickModelCurves.Text = "选择线条";
            this.Btn_PickModelCurves.UseVisualStyleBackColor = true;
            this.Btn_PickModelCurves.Click += new System.EventHandler(this.Btn_PickModelCurves_Click);
            // 
            // Btn_GetEdgeFromFace
            // 
            this.Btn_GetEdgeFromFace.Location = new System.Drawing.Point(11, 84);
            this.Btn_GetEdgeFromFace.Name = "Btn_GetEdgeFromFace";
            this.Btn_GetEdgeFromFace.Size = new System.Drawing.Size(75, 23);
            this.Btn_GetEdgeFromFace.TabIndex = 7;
            this.Btn_GetEdgeFromFace.Text = "提取表面";
            this.ToolTip1.SetToolTip(this.Btn_GetEdgeFromFace, "选择一个面，并提取此面上的轮廓线");
            this.Btn_GetEdgeFromFace.UseVisualStyleBackColor = true;
            this.Btn_GetEdgeFromFace.Click += new System.EventHandler(this.Btn_GetEdgeFromFace_Click);
            // 
            // Btn_CancelDrawCurves
            // 
            this.Btn_CancelDrawCurves.Enabled = false;
            this.Btn_CancelDrawCurves.Location = new System.Drawing.Point(96, 152);
            this.Btn_CancelDrawCurves.Name = "Btn_CancelDrawCurves";
            this.Btn_CancelDrawCurves.Size = new System.Drawing.Size(59, 23);
            this.Btn_CancelDrawCurves.TabIndex = 6;
            this.Btn_CancelDrawCurves.Text = "取消";
            this.ToolTip1.SetToolTip(this.Btn_CancelDrawCurves, "取消模型线的绘制操作");
            this.Btn_CancelDrawCurves.UseVisualStyleBackColor = true;
            this.Btn_CancelDrawCurves.Click += new System.EventHandler(this.Btn_CancelDraw_Click);
            // 
            // Btn_DrawCurves
            // 
            this.Btn_DrawCurves.Location = new System.Drawing.Point(11, 152);
            this.Btn_DrawCurves.Name = "Btn_DrawCurves";
            this.Btn_DrawCurves.Size = new System.Drawing.Size(75, 23);
            this.Btn_DrawCurves.TabIndex = 4;
            this.Btn_DrawCurves.Text = "绘制轮廓";
            this.Btn_DrawCurves.UseVisualStyleBackColor = true;
            this.Btn_DrawCurves.Click += new System.EventHandler(this.btn_DrawCurves_Click);
            // 
            // LabelSides
            // 
            this.LabelSides.AutoSize = true;
            this.LabelSides.Location = new System.Drawing.Point(94, 27);
            this.LabelSides.Name = "LabelSides";
            this.LabelSides.Size = new System.Drawing.Size(29, 12);
            this.LabelSides.TabIndex = 3;
            this.LabelSides.Text = "边数";
            // 
            // ComboBox_sides
            // 
            this.ComboBox_sides.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBox_sides.FormattingEnabled = true;
            this.ComboBox_sides.Items.AddRange(new object[] {
            "3",
            "4",
            "5",
            "6"});
            this.ComboBox_sides.Location = new System.Drawing.Point(129, 24);
            this.ComboBox_sides.Name = "ComboBox_sides";
            this.ComboBox_sides.Size = new System.Drawing.Size(59, 20);
            this.ComboBox_sides.TabIndex = 2;
            // 
            // RadioBtn_Polygon
            // 
            this.RadioBtn_Polygon.AutoSize = true;
            this.RadioBtn_Polygon.Location = new System.Drawing.Point(6, 28);
            this.RadioBtn_Polygon.Name = "RadioBtn_Polygon";
            this.RadioBtn_Polygon.Size = new System.Drawing.Size(59, 16);
            this.RadioBtn_Polygon.TabIndex = 1;
            this.RadioBtn_Polygon.Text = "多边形";
            this.RadioBtn_Polygon.UseVisualStyleBackColor = true;
            this.RadioBtn_Polygon.CheckedChanged += new System.EventHandler(this.RadioBtn_Polygon_CheckedChanged);
            // 
            // RadioBtn_UserDrawShape
            // 
            this.RadioBtn_UserDrawShape.AutoSize = true;
            this.RadioBtn_UserDrawShape.Checked = true;
            this.RadioBtn_UserDrawShape.Location = new System.Drawing.Point(6, 59);
            this.RadioBtn_UserDrawShape.Name = "RadioBtn_UserDrawShape";
            this.RadioBtn_UserDrawShape.Size = new System.Drawing.Size(71, 16);
            this.RadioBtn_UserDrawShape.TabIndex = 0;
            this.RadioBtn_UserDrawShape.TabStop = true;
            this.RadioBtn_UserDrawShape.Text = "自定轮廓";
            this.RadioBtn_UserDrawShape.UseVisualStyleBackColor = true;
            // 
            // CheckBox_DrawSucceeded
            // 
            this.CheckBox_DrawSucceeded.AutoSize = true;
            this.CheckBox_DrawSucceeded.Enabled = false;
            this.CheckBox_DrawSucceeded.Location = new System.Drawing.Point(280, 217);
            this.CheckBox_DrawSucceeded.Name = "CheckBox_DrawSucceeded";
            this.CheckBox_DrawSucceeded.Size = new System.Drawing.Size(48, 16);
            this.CheckBox_DrawSucceeded.TabIndex = 5;
            this.CheckBox_DrawSucceeded.Text = "曲线";
            this.CheckBox_DrawSucceeded.UseVisualStyleBackColor = true;
            // 
            // Btn_ClearCurves
            // 
            this.Btn_ClearCurves.Enabled = false;
            this.Btn_ClearCurves.Location = new System.Drawing.Point(215, 213);
            this.Btn_ClearCurves.Name = "Btn_ClearCurves";
            this.Btn_ClearCurves.Size = new System.Drawing.Size(59, 23);
            this.Btn_ClearCurves.TabIndex = 4;
            this.Btn_ClearCurves.Text = "删除";
            this.Btn_ClearCurves.UseVisualStyleBackColor = true;
            this.Btn_ClearCurves.Click += new System.EventHandler(this.Btn_ClearCurves_Click);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(17, 218);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(53, 12);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "深度 (m)";
            // 
            // Btn_Modeling
            // 
            this.Btn_Modeling.Enabled = false;
            this.Btn_Modeling.Location = new System.Drawing.Point(334, 213);
            this.Btn_Modeling.Name = "Btn_Modeling";
            this.Btn_Modeling.Size = new System.Drawing.Size(75, 23);
            this.Btn_Modeling.TabIndex = 3;
            this.Btn_Modeling.Text = "建模";
            this.Btn_Modeling.UseVisualStyleBackColor = true;
            this.Btn_Modeling.Click += new System.EventHandler(this.BtnModeling_Click);
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.RadioBtn_ExcavSoil);
            this.GroupBox2.Controls.Add(this.RadioBtn_ModelSoil);
            this.GroupBox2.Location = new System.Drawing.Point(12, 12);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(177, 53);
            this.GroupBox2.TabIndex = 1;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "土体类型";
            // 
            // TextBox_StartedDate
            // 
            this.TextBox_StartedDate.Location = new System.Drawing.Point(47, 22);
            this.TextBox_StartedDate.Name = "TextBox_StartedDate";
            this.TextBox_StartedDate.Size = new System.Drawing.Size(87, 21);
            this.TextBox_StartedDate.TabIndex = 5;
            // 
            // TextBox_SoilName
            // 
            this.TextBox_SoilName.Location = new System.Drawing.Point(76, 188);
            this.TextBox_SoilName.Name = "TextBox_SoilName";
            this.TextBox_SoilName.Size = new System.Drawing.Size(92, 21);
            this.TextBox_SoilName.TabIndex = 2;
            this.ToolTip1.SetToolTip(this.TextBox_SoilName, "名称可以不指定,此时程序会以开挖完成日期作为默认名称.");
            // 
            // TextBox_CompletedDate
            // 
            this.TextBox_CompletedDate.Location = new System.Drawing.Point(47, 49);
            this.TextBox_CompletedDate.Name = "TextBox_CompletedDate";
            this.TextBox_CompletedDate.Size = new System.Drawing.Size(87, 21);
            this.TextBox_CompletedDate.TabIndex = 6;
            this.ToolTip1.SetToolTip(this.TextBox_CompletedDate, "精确到分钟，推荐的格式为\"2016/04/04 16:30\"");
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(16, 191);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(29, 12);
            this.Label4.TabIndex = 1;
            this.Label4.Text = "名称";
            // 
            // btn__DateCalendar
            // 
            this.btn__DateCalendar.Location = new System.Drawing.Point(140, 34);
            this.btn__DateCalendar.Name = "btn__DateCalendar";
            this.btn__DateCalendar.Size = new System.Drawing.Size(31, 23);
            this.btn__DateCalendar.TabIndex = 4;
            this.btn__DateCalendar.Text = "...";
            this.btn__DateCalendar.UseVisualStyleBackColor = true;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(6, 52);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(17, 12);
            this.Label3.TabIndex = 3;
            this.Label3.Text = "To";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(6, 25);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(29, 12);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "From";
            // 
            // RadioBtn_ExcavSoil
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
            // RadioBtn_ModelSoil
            // 
            this.RadioBtn_ModelSoil.AutoSize = true;
            this.RadioBtn_ModelSoil.Location = new System.Drawing.Point(100, 28);
            this.RadioBtn_ModelSoil.Name = "RadioBtn_ModelSoil";
            this.RadioBtn_ModelSoil.Size = new System.Drawing.Size(71, 16);
            this.RadioBtn_ModelSoil.TabIndex = 2;
            this.RadioBtn_ModelSoil.Text = "模型土体";
            this.RadioBtn_ModelSoil.UseVisualStyleBackColor = true;
            // 
            // TextBox_Depth
            // 
            this.TextBox_Depth.Location = new System.Drawing.Point(76, 215);
            this.TextBox_Depth.Name = "TextBox_Depth";
            this.TextBox_Depth.PositiveOnly = true;
            this.TextBox_Depth.Size = new System.Drawing.Size(92, 21);
            this.TextBox_Depth.TabIndex = 2;
            this.TextBox_Depth.Text = "3";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.TextBox_StartedDate);
            this.groupBox3.Controls.Add(this.Label1);
            this.groupBox3.Controls.Add(this.TextBox_CompletedDate);
            this.groupBox3.Controls.Add(this.Label3);
            this.groupBox3.Controls.Add(this.btn__DateCalendar);
            this.groupBox3.Location = new System.Drawing.Point(12, 71);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(177, 82);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "开挖日期";
            // 
            // frm_DrawExcavation
            // 
            this.AcceptButton = this.Btn_Modeling;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 250);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.Btn_Modeling);
            this.Controls.Add(this.TextBox_SoilName);
            this.Controls.Add(this.TextBox_Depth);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Btn_ClearCurves);
            this.Controls.Add(this.CheckBox_DrawSucceeded);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.GroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frm_DrawExcavation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "绘制土体";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frm_DrawExcavation_FormClosed);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		internal System.Windows.Forms.GroupBox GroupBox1;
		internal System.Windows.Forms.Label LabelSides;
		internal System.Windows.Forms.ComboBox ComboBox_sides;
		internal System.Windows.Forms.RadioButton RadioBtn_Polygon;
		internal System.Windows.Forms.RadioButton RadioBtn_UserDrawShape;
		internal System.Windows.Forms.Label Label2;
		internal TextBoxNum TextBox_Depth;
		internal System.Windows.Forms.Button Btn_Modeling;
		internal System.Windows.Forms.GroupBox GroupBox2;
		internal System.Windows.Forms.TextBox TextBox_CompletedDate;
		internal System.Windows.Forms.Button btn__DateCalendar;
		internal System.Windows.Forms.RadioButton RadioBtn_ExcavSoil;
		internal System.Windows.Forms.RadioButton RadioBtn_ModelSoil;
		internal System.Windows.Forms.ToolTip ToolTip1;
		internal System.Windows.Forms.TextBox TextBox_StartedDate;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.TextBox TextBox_SoilName;
		internal System.Windows.Forms.Label Label4;
		internal Button Btn_DrawCurves;
		internal CheckBox CheckBox_DrawSucceeded;
		internal Button Btn_ClearCurves;
        private System.ComponentModel.IContainer components;
        private Button Btn_CancelDrawCurves;
        private Button Btn_GetEdgeFromFace;
        private Button Btn_PickModelCurves;
        private GroupBox groupBox3;
    }
}

Namespace OldW.Excavation
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class frm_DrawExcavation
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frm_DrawExcavation))
            Me.GroupBox1 = New System.Windows.Forms.GroupBox()
            Me.CheckBox_DrawSucceeded = New System.Windows.Forms.CheckBox()
            Me.btn_DrawCurves = New System.Windows.Forms.Button()
            Me.LabelSides = New System.Windows.Forms.Label()
            Me.ComboBox_sides = New System.Windows.Forms.ComboBox()
            Me.RadioBtn_Polygon = New System.Windows.Forms.RadioButton()
            Me.RadioBtn_Draw = New System.Windows.Forms.RadioButton()
            Me.RadioBtn_PickShape = New System.Windows.Forms.RadioButton()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.TextBox_Depth = New System.Windows.Forms.TextBox()
            Me.BtnModeling = New System.Windows.Forms.Button()
            Me.GroupBox2 = New System.Windows.Forms.GroupBox()
            Me.TextBox_StartedDate = New System.Windows.Forms.TextBox()
            Me.TextBox_SoilName = New System.Windows.Forms.TextBox()
            Me.TextBox_CompletedDate = New System.Windows.Forms.TextBox()
            Me.Label4 = New System.Windows.Forms.Label()
            Me.btn__DateCalendar = New System.Windows.Forms.Button()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.LabelCompletedDate = New System.Windows.Forms.Label()
            Me.RadioBtn_ExcavSoil = New System.Windows.Forms.RadioButton()
            Me.RadioBtn_ModelSoil = New System.Windows.Forms.RadioButton()
            Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
            Me.Btn_ClearCurves = New System.Windows.Forms.Button()
            Me.GroupBox1.SuspendLayout()
            Me.GroupBox2.SuspendLayout()
            Me.SuspendLayout()
            '
            'GroupBox1
            '
            Me.GroupBox1.Controls.Add(Me.CheckBox_DrawSucceeded)
            Me.GroupBox1.Controls.Add(Me.Btn_ClearCurves)
            Me.GroupBox1.Controls.Add(Me.btn_DrawCurves)
            Me.GroupBox1.Controls.Add(Me.LabelSides)
            Me.GroupBox1.Controls.Add(Me.ComboBox_sides)
            Me.GroupBox1.Controls.Add(Me.RadioBtn_Polygon)
            Me.GroupBox1.Controls.Add(Me.RadioBtn_Draw)
            Me.GroupBox1.Controls.Add(Me.RadioBtn_PickShape)
            Me.GroupBox1.Location = New System.Drawing.Point(195, 12)
            Me.GroupBox1.Name = "GroupBox1"
            Me.GroupBox1.Size = New System.Drawing.Size(214, 155)
            Me.GroupBox1.TabIndex = 0
            Me.GroupBox1.TabStop = False
            Me.GroupBox1.Text = "轮廓形状"
            '
            'CheckBox_DrawSucceeded
            '
            Me.CheckBox_DrawSucceeded.AutoSize = True
            Me.CheckBox_DrawSucceeded.Enabled = False
            Me.CheckBox_DrawSucceeded.Location = New System.Drawing.Point(87, 122)
            Me.CheckBox_DrawSucceeded.Name = "CheckBox_DrawSucceeded"
            Me.CheckBox_DrawSucceeded.Size = New System.Drawing.Size(72, 16)
            Me.CheckBox_DrawSucceeded.TabIndex = 5
            Me.CheckBox_DrawSucceeded.Text = "绘制成功"
            Me.CheckBox_DrawSucceeded.UseVisualStyleBackColor = True
            '
            'btn_DrawCurves
            '
            Me.btn_DrawCurves.Location = New System.Drawing.Point(84, 90)
            Me.btn_DrawCurves.Name = "btn_DrawCurves"
            Me.btn_DrawCurves.Size = New System.Drawing.Size(59, 23)
            Me.btn_DrawCurves.TabIndex = 4
            Me.btn_DrawCurves.Text = "绘制"
            Me.btn_DrawCurves.UseVisualStyleBackColor = True
            '
            'LabelSides
            '
            Me.LabelSides.AutoSize = True
            Me.LabelSides.Location = New System.Drawing.Point(94, 27)
            Me.LabelSides.Name = "LabelSides"
            Me.LabelSides.Size = New System.Drawing.Size(29, 12)
            Me.LabelSides.TabIndex = 3
            Me.LabelSides.Text = "边数"
            '
            'ComboBox_sides
            '
            Me.ComboBox_sides.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.ComboBox_sides.FormattingEnabled = True
            Me.ComboBox_sides.Items.AddRange(New Object() {"3", "4", "5", "6"})
            Me.ComboBox_sides.Location = New System.Drawing.Point(129, 24)
            Me.ComboBox_sides.Name = "ComboBox_sides"
            Me.ComboBox_sides.Size = New System.Drawing.Size(59, 20)
            Me.ComboBox_sides.TabIndex = 2
            '
            'RadioBtn_Polygon
            '
            Me.RadioBtn_Polygon.AutoSize = True
            Me.RadioBtn_Polygon.Location = New System.Drawing.Point(6, 28)
            Me.RadioBtn_Polygon.Name = "RadioBtn_Polygon"
            Me.RadioBtn_Polygon.Size = New System.Drawing.Size(59, 16)
            Me.RadioBtn_Polygon.TabIndex = 1
            Me.RadioBtn_Polygon.Text = "多边形"
            Me.RadioBtn_Polygon.UseVisualStyleBackColor = True
            '
            'RadioBtn_Draw
            '
            Me.RadioBtn_Draw.AutoSize = True
            Me.RadioBtn_Draw.Checked = True
            Me.RadioBtn_Draw.Location = New System.Drawing.Point(6, 90)
            Me.RadioBtn_Draw.Name = "RadioBtn_Draw"
            Me.RadioBtn_Draw.Size = New System.Drawing.Size(71, 16)
            Me.RadioBtn_Draw.TabIndex = 0
            Me.RadioBtn_Draw.TabStop = True
            Me.RadioBtn_Draw.Text = "绘制轮廓"
            Me.RadioBtn_Draw.UseVisualStyleBackColor = True
            '
            'RadioBtn_PickShape
            '
            Me.RadioBtn_PickShape.AutoSize = True
            Me.RadioBtn_PickShape.Location = New System.Drawing.Point(6, 59)
            Me.RadioBtn_PickShape.Name = "RadioBtn_PickShape"
            Me.RadioBtn_PickShape.Size = New System.Drawing.Size(71, 16)
            Me.RadioBtn_PickShape.TabIndex = 0
            Me.RadioBtn_PickShape.Text = "选择轮廓"
            Me.RadioBtn_PickShape.UseVisualStyleBackColor = True
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Location = New System.Drawing.Point(228, 188)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(53, 12)
            Me.Label2.TabIndex = 1
            Me.Label2.Text = "深度 (m)"
            '
            'TextBox_Depth
            '
            Me.TextBox_Depth.Location = New System.Drawing.Point(299, 185)
            Me.TextBox_Depth.Name = "TextBox_Depth"
            Me.TextBox_Depth.Size = New System.Drawing.Size(100, 21)
            Me.TextBox_Depth.TabIndex = 2
            Me.TextBox_Depth.Text = "2"
            '
            'BtnModeling
            '
            Me.BtnModeling.Location = New System.Drawing.Point(324, 218)
            Me.BtnModeling.Name = "BtnModeling"
            Me.BtnModeling.Size = New System.Drawing.Size(75, 23)
            Me.BtnModeling.TabIndex = 3
            Me.BtnModeling.Text = "建模"
            Me.BtnModeling.UseVisualStyleBackColor = True
            '
            'GroupBox2
            '
            Me.GroupBox2.Controls.Add(Me.TextBox_StartedDate)
            Me.GroupBox2.Controls.Add(Me.TextBox_SoilName)
            Me.GroupBox2.Controls.Add(Me.TextBox_CompletedDate)
            Me.GroupBox2.Controls.Add(Me.Label4)
            Me.GroupBox2.Controls.Add(Me.btn__DateCalendar)
            Me.GroupBox2.Controls.Add(Me.Label3)
            Me.GroupBox2.Controls.Add(Me.Label1)
            Me.GroupBox2.Controls.Add(Me.LabelCompletedDate)
            Me.GroupBox2.Controls.Add(Me.RadioBtn_ExcavSoil)
            Me.GroupBox2.Controls.Add(Me.RadioBtn_ModelSoil)
            Me.GroupBox2.Location = New System.Drawing.Point(12, 12)
            Me.GroupBox2.Name = "GroupBox2"
            Me.GroupBox2.Size = New System.Drawing.Size(177, 192)
            Me.GroupBox2.TabIndex = 1
            Me.GroupBox2.TabStop = False
            Me.GroupBox2.Text = "土体类型"
            '
            'TextBox_StartedDate
            '
            Me.TextBox_StartedDate.Location = New System.Drawing.Point(45, 86)
            Me.TextBox_StartedDate.Name = "TextBox_StartedDate"
            Me.TextBox_StartedDate.Size = New System.Drawing.Size(87, 21)
            Me.TextBox_StartedDate.TabIndex = 5
            '
            'TextBox_SoilName
            '
            Me.TextBox_SoilName.Location = New System.Drawing.Point(45, 149)
            Me.TextBox_SoilName.Name = "TextBox_SoilName"
            Me.TextBox_SoilName.Size = New System.Drawing.Size(87, 21)
            Me.TextBox_SoilName.TabIndex = 2
            Me.ToolTip1.SetToolTip(Me.TextBox_SoilName, "名称可以不指定,此时程序会以开挖完成日期作为默认名称.")
            '
            'TextBox_CompletedDate
            '
            Me.TextBox_CompletedDate.Location = New System.Drawing.Point(45, 113)
            Me.TextBox_CompletedDate.Name = "TextBox_CompletedDate"
            Me.TextBox_CompletedDate.Size = New System.Drawing.Size(87, 21)
            Me.TextBox_CompletedDate.TabIndex = 6
            Me.ToolTip1.SetToolTip(Me.TextBox_CompletedDate, "精确到分钟，推荐的格式为""2016/04/04 16:30""")
            '
            'Label4
            '
            Me.Label4.AutoSize = True
            Me.Label4.Location = New System.Drawing.Point(4, 152)
            Me.Label4.Name = "Label4"
            Me.Label4.Size = New System.Drawing.Size(29, 12)
            Me.Label4.TabIndex = 1
            Me.Label4.Text = "名称"
            '
            'btn__DateCalendar
            '
            Me.btn__DateCalendar.Location = New System.Drawing.Point(138, 98)
            Me.btn__DateCalendar.Name = "btn__DateCalendar"
            Me.btn__DateCalendar.Size = New System.Drawing.Size(31, 23)
            Me.btn__DateCalendar.TabIndex = 4
            Me.btn__DateCalendar.Text = "..."
            Me.btn__DateCalendar.UseVisualStyleBackColor = True
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(4, 116)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(17, 12)
            Me.Label3.TabIndex = 3
            Me.Label3.Text = "To"
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(4, 89)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(29, 12)
            Me.Label1.TabIndex = 3
            Me.Label1.Text = "From"
            '
            'LabelCompletedDate
            '
            Me.LabelCompletedDate.AutoSize = True
            Me.LabelCompletedDate.Location = New System.Drawing.Point(4, 60)
            Me.LabelCompletedDate.Name = "LabelCompletedDate"
            Me.LabelCompletedDate.Size = New System.Drawing.Size(53, 12)
            Me.LabelCompletedDate.TabIndex = 3
            Me.LabelCompletedDate.Text = "开挖日期"
            '
            'RadioBtn_ExcavSoil
            '
            Me.RadioBtn_ExcavSoil.AutoSize = True
            Me.RadioBtn_ExcavSoil.Checked = True
            Me.RadioBtn_ExcavSoil.Location = New System.Drawing.Point(6, 26)
            Me.RadioBtn_ExcavSoil.Name = "RadioBtn_ExcavSoil"
            Me.RadioBtn_ExcavSoil.Size = New System.Drawing.Size(71, 16)
            Me.RadioBtn_ExcavSoil.TabIndex = 1
            Me.RadioBtn_ExcavSoil.TabStop = True
            Me.RadioBtn_ExcavSoil.Text = "开挖土体"
            Me.RadioBtn_ExcavSoil.UseVisualStyleBackColor = True
            '
            'RadioBtn_ModelSoil
            '
            Me.RadioBtn_ModelSoil.AutoSize = True
            Me.RadioBtn_ModelSoil.Location = New System.Drawing.Point(100, 28)
            Me.RadioBtn_ModelSoil.Name = "RadioBtn_ModelSoil"
            Me.RadioBtn_ModelSoil.Size = New System.Drawing.Size(71, 16)
            Me.RadioBtn_ModelSoil.TabIndex = 2
            Me.RadioBtn_ModelSoil.Text = "模型土体"
            Me.RadioBtn_ModelSoil.UseVisualStyleBackColor = True
            '
            'Btn_ClearCurves
            '
            Me.Btn_ClearCurves.Location = New System.Drawing.Point(149, 90)
            Me.Btn_ClearCurves.Name = "Btn_ClearCurves"
            Me.Btn_ClearCurves.Size = New System.Drawing.Size(59, 23)
            Me.Btn_ClearCurves.TabIndex = 4
            Me.Btn_ClearCurves.Text = "删除"
            Me.Btn_ClearCurves.UseVisualStyleBackColor = True
            '
            'frm_DrawExcavation
            '
            Me.AcceptButton = Me.BtnModeling
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(421, 253)
            Me.Controls.Add(Me.BtnModeling)
            Me.Controls.Add(Me.TextBox_Depth)
            Me.Controls.Add(Me.Label2)
            Me.Controls.Add(Me.GroupBox2)
            Me.Controls.Add(Me.GroupBox1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "frm_DrawExcavation"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "绘制土体"
            Me.GroupBox1.ResumeLayout(False)
            Me.GroupBox1.PerformLayout()
            Me.GroupBox2.ResumeLayout(False)
            Me.GroupBox2.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
        Friend WithEvents LabelSides As System.Windows.Forms.Label
        Friend WithEvents ComboBox_sides As System.Windows.Forms.ComboBox
        Friend WithEvents RadioBtn_Polygon As System.Windows.Forms.RadioButton
        Friend WithEvents RadioBtn_PickShape As System.Windows.Forms.RadioButton
        Friend WithEvents Label2 As System.Windows.Forms.Label
        Friend WithEvents TextBox_Depth As System.Windows.Forms.TextBox
        Friend WithEvents BtnModeling As System.Windows.Forms.Button
        Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
        Friend WithEvents TextBox_CompletedDate As System.Windows.Forms.TextBox
        Friend WithEvents btn__DateCalendar As System.Windows.Forms.Button
        Friend WithEvents LabelCompletedDate As System.Windows.Forms.Label
        Friend WithEvents RadioBtn_ExcavSoil As System.Windows.Forms.RadioButton
        Friend WithEvents RadioBtn_ModelSoil As System.Windows.Forms.RadioButton
        Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
        Friend WithEvents TextBox_StartedDate As System.Windows.Forms.TextBox
        Friend WithEvents Label3 As System.Windows.Forms.Label
        Friend WithEvents Label1 As System.Windows.Forms.Label
        Friend WithEvents TextBox_SoilName As System.Windows.Forms.TextBox
        Friend WithEvents Label4 As System.Windows.Forms.Label
        Friend WithEvents RadioBtn_Draw As RadioButton
        Friend WithEvents btn_DrawCurves As Button
        Friend WithEvents CheckBox_DrawSucceeded As CheckBox
        Friend WithEvents Btn_ClearCurves As Button
    End Class
End Namespace
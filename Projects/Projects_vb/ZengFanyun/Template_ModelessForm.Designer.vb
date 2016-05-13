
Namespace rvtTools_ez.Test
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ModelessForm
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.BtnPick = New System.Windows.Forms.Button()
            Me.ListBox1 = New System.Windows.Forms.ListBox()
            Me.Button1 = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'BtnPick
            '
            Me.BtnPick.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.BtnPick.Location = New System.Drawing.Point(190, 12)
            Me.BtnPick.Name = "BtnPick"
            Me.BtnPick.Size = New System.Drawing.Size(75, 23)
            Me.BtnPick.TabIndex = 0
            Me.BtnPick.Text = "绘制"
            Me.BtnPick.UseVisualStyleBackColor = True
            '
            'ListBox1
            '
            Me.ListBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.ListBox1.FormattingEnabled = True
            Me.ListBox1.ItemHeight = 12
            Me.ListBox1.Location = New System.Drawing.Point(12, 12)
            Me.ListBox1.Name = "ListBox1"
            Me.ListBox1.Size = New System.Drawing.Size(172, 184)
            Me.ListBox1.TabIndex = 1
            '
            'Button1
            '
            Me.Button1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.Button1.Location = New System.Drawing.Point(190, 47)
            Me.Button1.Name = "Button1"
            Me.Button1.Size = New System.Drawing.Size(75, 23)
            Me.Button1.TabIndex = 2
            Me.Button1.Text = "删除"
            Me.Button1.UseVisualStyleBackColor = True
            '
            'ModelessForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(277, 204)
            Me.Controls.Add(Me.Button1)
            Me.Controls.Add(Me.ListBox1)
            Me.Controls.Add(Me.BtnPick)
            Me.Name = "ModelessForm"
            Me.Text = "RevitForm"
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents BtnPick As System.Windows.Forms.Button
        Friend WithEvents ListBox1 As ListBox
        Friend WithEvents Button1 As Button
    End Class
End Namespace
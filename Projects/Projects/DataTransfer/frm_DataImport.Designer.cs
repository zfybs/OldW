using System;
using System.Windows.Forms;

namespace OldW.DataManager
{
    public partial class frm_DataImport : System.Windows.Forms.Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_DataImport));
            this.ComboBox1 = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.ListBox1 = new System.Windows.Forms.ListBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.Button1 = new System.Windows.Forms.Button();
            this.Button2 = new System.Windows.Forms.Button();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.Button3 = new System.Windows.Forms.Button();
            this.Button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            //ComboBox1
            //
            this.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBox1.FormattingEnabled = true;
            this.ComboBox1.Items.AddRange(new object[] { "立柱隆沉" });
            this.ComboBox1.Location = new System.Drawing.Point(71, 45);
            this.ComboBox1.Name = "ComboBox1";
            this.ComboBox1.Size = new System.Drawing.Size(143, 20);
            this.ComboBox1.TabIndex = 0;
            //
            //Label1
            //
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(10, 48);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(53, 12);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "监测类型";
            //
            //ListBox1
            //
            this.ListBox1.FormattingEnabled = true;
            this.ListBox1.ItemHeight = 12;
            this.ListBox1.Items.AddRange(new object[] { "LZ01", "LZ02", "LZ03", "LZ04", "LZ05", "LZ06", "LZ07" });
            this.ListBox1.Location = new System.Drawing.Point(12, 104);
            this.ListBox1.Name = "ListBox1";
            this.ListBox1.Size = new System.Drawing.Size(237, 172);
            this.ListBox1.TabIndex = 2;
            //
            //Label2
            //
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(12, 79);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(53, 12);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "测点编号";
            //
            //Button1
            //
            this.Button1.Location = new System.Drawing.Point(255, 104);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(75, 23);
            this.Button1.TabIndex = 3;
            this.Button1.Text = "查看";
            this.Button1.UseVisualStyleBackColor = true;
            //
            //Button2
            //
            this.Button2.Location = new System.Drawing.Point(259, 253);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(75, 23);
            this.Button2.TabIndex = 3;
            this.Button2.Text = "导入";
            this.Button2.UseVisualStyleBackColor = true;
            //
            //TextBox1
            //
            this.TextBox1.Location = new System.Drawing.Point(69, 16);
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.Size = new System.Drawing.Size(180, 21);
            this.TextBox1.TabIndex = 4;
            //
            //Label3
            //
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(10, 19);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(53, 12);
            this.Label3.TabIndex = 1;
            this.Label3.Text = "监测数据";
            //
            //Button3
            //
            this.Button3.Location = new System.Drawing.Point(255, 16);
            this.Button3.Name = "Button3";
            this.Button3.Size = new System.Drawing.Size(75, 23);
            this.Button3.TabIndex = 3;
            this.Button3.Text = "选择";
            this.Button3.UseVisualStyleBackColor = true;
            //
            //Button4
            //
            this.Button4.Location = new System.Drawing.Point(255, 45);
            this.Button4.Name = "Button4";
            this.Button4.Size = new System.Drawing.Size(75, 23);
            this.Button4.TabIndex = 3;
            this.Button4.Text = "打开";
            this.Button4.UseVisualStyleBackColor = true;
            //
            //frm_DataImport
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF((float)(6.0F), (float)(12.0F));
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 288);
            this.Controls.Add(this.TextBox1);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Button4);
            this.Controls.Add(this.Button3);
            this.Controls.Add(this.Button1);
            this.Controls.Add(this.ListBox1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.ComboBox1);
            this.Icon = (System.Drawing.Icon)(resources.GetObject("$this.Icon"));
            this.Name = "frm_DataImport";
            this.Text = "数据导入";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal System.Windows.Forms.ComboBox ComboBox1;
        internal Label Label1;
        internal ListBox ListBox1;
        internal Label Label2;
        internal Button Button1;
        internal Button Button2;
        internal System.Windows.Forms.TextBox TextBox1;
        internal Label Label3;
        internal Button Button3;
        internal Button Button4;
    }

}
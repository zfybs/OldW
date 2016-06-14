using System.Windows.Forms;

namespace OldW.DataManager
{
    public partial class frm_DataExport : System.Windows.Forms.Form
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
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(frm_DataExport));
            this.ComboBox1 = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.ListBox1 = new System.Windows.Forms.ListBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.Button1 = new System.Windows.Forms.Button();
            this.Button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            //ComboBox1
            //
            this.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBox1.FormattingEnabled = true;
            this.ComboBox1.Items.AddRange(new object[] { "立柱隆沉" });
            this.ComboBox1.Location = new System.Drawing.Point(12, 33);
            this.ComboBox1.Name = "ComboBox1";
            this.ComboBox1.Size = new System.Drawing.Size(143, 20);
            this.ComboBox1.TabIndex = 0;
            //
            //Label1
            //
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 9);
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
            this.ListBox1.Location = new System.Drawing.Point(12, 90);
            this.ListBox1.Name = "ListBox1";
            this.ListBox1.Size = new System.Drawing.Size(143, 172);
            this.ListBox1.TabIndex = 2;
            //
            //Label2
            //
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(10, 66);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(53, 12);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "测点编号";
            //
            //Button1
            //
            this.Button1.Location = new System.Drawing.Point(174, 33);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(75, 23);
            this.Button1.TabIndex = 3;
            this.Button1.Text = "查看";
            this.Button1.UseVisualStyleBackColor = true;
            //
            //Button2
            //
            this.Button2.Location = new System.Drawing.Point(174, 66);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(75, 23);
            this.Button2.TabIndex = 3;
            this.Button2.Text = "导出";
            this.Button2.UseVisualStyleBackColor = true;
            //
            //DataTransfer
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF((float)(6.0F), (float)(12.0F));
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 279);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Button1);
            this.Controls.Add(this.ListBox1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.ComboBox1);
            this.Icon = (System.Drawing.Icon)(resources.GetObject("$this.Icon"));
            this.Name = "DataTransfer";
            this.Text = "数据导出";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal System.Windows.Forms.ComboBox ComboBox1;
        internal Label Label1;
        internal ListBox ListBox1;
        internal Label Label2;
        internal Button Button1;
        internal Button Button2;
    }

}
using System.Windows.Forms;
using eZstd.UserControls;

namespace OldW.DynamicStages
{
    internal partial class ViewStageManually : System.Windows.Forms.Form
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
            this.monthCalendar1 = new System.Windows.Forms.MonthCalendar();
            this.labelConstructionTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numberChanging1 = new eZstd.UserControls.NumberChanging();
            this.SuspendLayout();
            // 
            // monthCalendar1
            // 
            this.monthCalendar1.Location = new System.Drawing.Point(12, 83);
            this.monthCalendar1.Name = "monthCalendar1";
            this.monthCalendar1.TabIndex = 3;
            this.monthCalendar1.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateSelected);
            // 
            // labelConstructionTime
            // 
            this.labelConstructionTime.AutoSize = true;
            this.labelConstructionTime.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Bold);
            this.labelConstructionTime.Location = new System.Drawing.Point(81, 17);
            this.labelConstructionTime.Name = "labelConstructionTime";
            this.labelConstructionTime.Size = new System.Drawing.Size(152, 16);
            this.labelConstructionTime.TabIndex = 4;
            this.labelConstructionTime.Text = "2013/10/10 11:20";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "施工日期：";
            // 
            // numberChanging1
            // 
            this.numberChanging1.BackColor = System.Drawing.Color.Transparent;
            this.numberChanging1.Location = new System.Drawing.Point(12, 50);
            this.numberChanging1.MaximumSize = new System.Drawing.Size(0, 21);
            this.numberChanging1.MinimumSize = new System.Drawing.Size(200, 21);
            this.numberChanging1.Name = "numberChanging1";
            this.numberChanging1.Size = new System.Drawing.Size(200, 21);
            this.numberChanging1.TabIndex = 0;
            this.numberChanging1.Unit = TimeSpan2.TimeSpanUnit.Days;
            this.numberChanging1.ValueAdded += new System.Action(this.numberChanging1_ValueAdded);
            this.numberChanging1.ValueMinused += new System.Action(this.numberChanging1_ValueMinused);
            // 
            // ViewStageManually
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 258);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelConstructionTime);
            this.Controls.Add(this.monthCalendar1);
            this.Controls.Add(this.numberChanging1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ViewStageManually";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "工况查看";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private eZstd.UserControls.NumberChanging numberChanging1;
        private MonthCalendar monthCalendar1;
        private Label labelConstructionTime;
        private Label label1;
    }
}


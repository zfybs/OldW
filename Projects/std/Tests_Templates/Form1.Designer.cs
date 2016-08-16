namespace stdOldW.Tests_Templates
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.numberChanging1 = new stdOldW.UserControls.NumberChanging();
            this.SuspendLayout();
            // 
            // numberChanging1
            // 
            this.numberChanging1.BackColor = System.Drawing.Color.Transparent;
            this.numberChanging1.IntegerOnly = false;
            this.numberChanging1.Location = new System.Drawing.Point(12, 12);
            this.numberChanging1.MaximumSize = new System.Drawing.Size(0, 21);
            this.numberChanging1.MinimumSize = new System.Drawing.Size(200, 21);
            this.numberChanging1.Name = "numberChanging1";
            this.numberChanging1.Size = new System.Drawing.Size(200, 21);
            this.numberChanging1.TabIndex = 2;
            this.numberChanging1.Unit = stdOldW.UserControls.TimeSpanUnit.Days;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.numberChanging1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private UserControls.NumberChanging numberChanging1;
    }
}
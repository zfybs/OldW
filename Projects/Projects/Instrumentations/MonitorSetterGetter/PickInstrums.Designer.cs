namespace OldW.Instrumentations.MonitorSetterGetter
{
    partial class PickInstrums
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
            this.TreeViewIns = new System.Windows.Forms.TreeView();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonDeselectAll = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonExpand = new System.Windows.Forms.Button();
            this.buttonShrink = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TreeViewIns
            // 
            this.TreeViewIns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TreeViewIns.Location = new System.Drawing.Point(111, 0);
            this.TreeViewIns.Name = "TreeViewIns";
            this.TreeViewIns.Size = new System.Drawing.Size(323, 441);
            this.TreeViewIns.TabIndex = 0;
            this.TreeViewIns.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewIns_AfterCheck);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Location = new System.Drawing.Point(12, 12);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 1;
            this.buttonSelectAll.Text = "全选";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonDeselectAll
            // 
            this.buttonDeselectAll.Location = new System.Drawing.Point(11, 41);
            this.buttonDeselectAll.Name = "buttonDeselectAll";
            this.buttonDeselectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonDeselectAll.TabIndex = 1;
            this.buttonDeselectAll.Text = "全部取消";
            this.buttonDeselectAll.UseVisualStyleBackColor = true;
            this.buttonDeselectAll.Click += new System.EventHandler(this.buttonDeselectAll_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOk.Location = new System.Drawing.Point(11, 377);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "确定";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.Location = new System.Drawing.Point(11, 406);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonExpand
            // 
            this.buttonExpand.Location = new System.Drawing.Point(12, 122);
            this.buttonExpand.Name = "buttonExpand";
            this.buttonExpand.Size = new System.Drawing.Size(75, 23);
            this.buttonExpand.TabIndex = 1;
            this.buttonExpand.Text = "展开";
            this.buttonExpand.UseVisualStyleBackColor = true;
            this.buttonExpand.Click += new System.EventHandler(this.buttonExpand_Click);
            // 
            // buttonShrink
            // 
            this.buttonShrink.Location = new System.Drawing.Point(12, 151);
            this.buttonShrink.Name = "buttonShrink";
            this.buttonShrink.Size = new System.Drawing.Size(75, 23);
            this.buttonShrink.TabIndex = 1;
            this.buttonShrink.Text = "收缩";
            this.buttonShrink.UseVisualStyleBackColor = true;
            this.buttonShrink.Click += new System.EventHandler(this.buttonShrink_Click);
            // 
            // PickInstrums
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 441);
            this.Controls.Add(this.buttonDeselectAll);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonShrink);
            this.Controls.Add(this.buttonExpand);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.TreeViewIns);
            this.MinimumSize = new System.Drawing.Size(450, 480);
            this.Name = "PickInstrums";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "测点过滤";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView TreeViewIns;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonDeselectAll;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonExpand;
        private System.Windows.Forms.Button buttonShrink;
    }
}
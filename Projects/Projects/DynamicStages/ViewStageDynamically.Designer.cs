using System;

namespace OldW.DynamicStages
{
    partial class ViewStageDynamically
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
            this.dateTimePicker_Start = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_End = new System.Windows.Forms.DateTimePicker();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.labelEndTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxNumInterval = new stdOldW.UserControls.TextBoxNum();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxNumSpan = new stdOldW.UserControls.TextBoxNum();
            this.comboBoxSpanUnit = new System.Windows.Forms.ComboBox();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.checkBoxLoopPlay = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBoxBackPlay = new System.Windows.Forms.CheckBox();
            this.labelCurrentTime = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dateTimePicker_Start
            // 
            this.dateTimePicker_Start.Location = new System.Drawing.Point(67, 8);
            this.dateTimePicker_Start.Name = "dateTimePicker_Start";
            this.dateTimePicker_Start.Size = new System.Drawing.Size(132, 21);
            this.dateTimePicker_Start.TabIndex = 0;
            this.dateTimePicker_Start.ValueChanged += new System.EventHandler(this.dateTimePicker_Start_ValueChanged);
            // 
            // dateTimePicker_End
            // 
            this.dateTimePicker_End.Location = new System.Drawing.Point(67, 35);
            this.dateTimePicker_End.Name = "dateTimePicker_End";
            this.dateTimePicker_End.Size = new System.Drawing.Size(132, 21);
            this.dateTimePicker_End.TabIndex = 0;
            this.dateTimePicker_End.ValueChanged += new System.EventHandler(this.dateTimePicker_End_ValueChanged);
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(8, 14);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(53, 12);
            this.labelStartTime.TabIndex = 1;
            this.labelStartTime.Text = "开始时间";
            // 
            // labelEndTime
            // 
            this.labelEndTime.AutoSize = true;
            this.labelEndTime.Location = new System.Drawing.Point(7, 41);
            this.labelEndTime.Name = "labelEndTime";
            this.labelEndTime.Size = new System.Drawing.Size(53, 12);
            this.labelEndTime.TabIndex = 1;
            this.labelEndTime.Text = "结束时间";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "播放速度(秒/帧)";
            // 
            // textBoxNumInterval
            // 
            this.textBoxNumInterval.Location = new System.Drawing.Point(123, 92);
            this.textBoxNumInterval.Name = "textBoxNumInterval";
            this.textBoxNumInterval.Size = new System.Drawing.Size(76, 21);
            this.textBoxNumInterval.TabIndex = 2;
            this.textBoxNumInterval.ValueNumberChanged += new System.EventHandler<double>(this.TextBoxNumIntervalOnValueNumberChanged);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 179);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(391, 17);
            this.hScrollBar1.TabIndex = 3;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // buttonPlay
            // 
            this.buttonPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlay.Location = new System.Drawing.Point(304, 12);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(75, 23);
            this.buttonPlay.TabIndex = 4;
            this.buttonPlay.Text = "播放";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "每帧跨度";
            // 
            // textBoxNumSpan
            // 
            this.textBoxNumSpan.Location = new System.Drawing.Point(69, 65);
            this.textBoxNumSpan.Name = "textBoxNumSpan";
            this.textBoxNumSpan.Size = new System.Drawing.Size(48, 21);
            this.textBoxNumSpan.TabIndex = 2;
            this.textBoxNumSpan.ValueNumberChanged += new System.EventHandler<double>(this.textBoxNumSpan_ValueNumberChanged);
            // 
            // comboBoxSpanUnit
            // 
            this.comboBoxSpanUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpanUnit.FormattingEnabled = true;
            this.comboBoxSpanUnit.Location = new System.Drawing.Point(123, 65);
            this.comboBoxSpanUnit.Name = "comboBoxSpanUnit";
            this.comboBoxSpanUnit.Size = new System.Drawing.Size(76, 20);
            this.comboBoxSpanUnit.TabIndex = 5;
            this.comboBoxSpanUnit.SelectedIndexChanged += new System.EventHandler(this.comboBoxSpanUnit_SelectedIndexChanged);
            // 
            // buttonPause
            // 
            this.buttonPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPause.Location = new System.Drawing.Point(304, 37);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(75, 23);
            this.buttonPause.TabIndex = 4;
            this.buttonPause.Text = "暂停";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStop.Location = new System.Drawing.Point(304, 65);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 4;
            this.buttonStop.Text = "停止";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // checkBoxLoopPlay
            // 
            this.checkBoxLoopPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxLoopPlay.AutoSize = true;
            this.checkBoxLoopPlay.Location = new System.Drawing.Point(250, 16);
            this.checkBoxLoopPlay.Name = "checkBoxLoopPlay";
            this.checkBoxLoopPlay.Size = new System.Drawing.Size(48, 16);
            this.checkBoxLoopPlay.TabIndex = 6;
            this.checkBoxLoopPlay.Text = "循环";
            this.checkBoxLoopPlay.UseVisualStyleBackColor = true;
            this.checkBoxLoopPlay.CheckedChanged += new System.EventHandler(this.checkBoxLoopPlay_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "播放进度 :";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.labelStartTime);
            this.panel1.Controls.Add(this.dateTimePicker_Start);
            this.panel1.Controls.Add(this.comboBoxSpanUnit);
            this.panel1.Controls.Add(this.dateTimePicker_End);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.labelEndTime);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBoxNumInterval);
            this.panel1.Controls.Add(this.textBoxNumSpan);
            this.panel1.Location = new System.Drawing.Point(12, 11);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(214, 124);
            this.panel1.TabIndex = 7;
            // 
            // checkBoxBackPlay
            // 
            this.checkBoxBackPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxBackPlay.AutoSize = true;
            this.checkBoxBackPlay.Location = new System.Drawing.Point(250, 41);
            this.checkBoxBackPlay.Name = "checkBoxBackPlay";
            this.checkBoxBackPlay.Size = new System.Drawing.Size(48, 16);
            this.checkBoxBackPlay.TabIndex = 6;
            this.checkBoxBackPlay.Text = "倒退";
            this.checkBoxBackPlay.UseVisualStyleBackColor = true;
            this.checkBoxBackPlay.CheckedChanged += new System.EventHandler(this.checkBoxBackPlay_CheckedChanged);
            // 
            // labelCurrentTime
            // 
            this.labelCurrentTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCurrentTime.AutoSize = true;
            this.labelCurrentTime.Location = new System.Drawing.Point(89, 157);
            this.labelCurrentTime.Name = "labelCurrentTime";
            this.labelCurrentTime.Size = new System.Drawing.Size(89, 12);
            this.labelCurrentTime.TabIndex = 8;
            this.labelCurrentTime.Text = "2016/6/6 12:20";
            // 
            // ViewStageDynamically
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 196);
            this.Controls.Add(this.labelCurrentTime);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.checkBoxBackPlay);
            this.Controls.Add(this.checkBoxLoopPlay);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.label3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ViewStageDynamically";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "开挖工况动态展示";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ViewStageDynamically_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker_Start;
        private System.Windows.Forms.DateTimePicker dateTimePicker_End;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.Label labelEndTime;
        private System.Windows.Forms.Label label1;
        private stdOldW.UserControls.TextBoxNum textBoxNumInterval;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Label label2;
        private stdOldW.UserControls.TextBoxNum textBoxNumSpan;
        private System.Windows.Forms.ComboBox comboBoxSpanUnit;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.CheckBox checkBoxLoopPlay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBoxBackPlay;
        private System.Windows.Forms.Label labelCurrentTime;
    }
}
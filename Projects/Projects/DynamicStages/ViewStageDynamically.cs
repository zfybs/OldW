using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using RevitStd;
using Timer = System.Timers.Timer;
using eZstd.UserControls;
using OldW.ProjectInfo;

namespace OldW.DynamicStages
{
    internal partial class ViewStageDynamically : Form
    {
        #region ---   Properties

        #endregion

        #region ---   Fields

        /// <summary>
        /// 正常情况下，这个值应该是和 hScrollBar1.Maximum 相等的，但是的 Win 10 系统下，
        /// 即使设置Maximum = 100，而不论滚动条的UI尺寸大小如何，在界面上可能滚动出来的最大值却竟然都是只有91。
        /// </summary>
        public const int ScrollBarMaximumValue = 91;

        private UIApplication _uiApplication;
        private OldWDocument _oldWDoc;
        private ViewStageDynamicallyHandler _VSDHandler;

        #endregion

        #region ---   构造函数

        /// <summary> 构造函数 </summary>
        public ViewStageDynamically(OldWDocument oldWDoc)
        {
            InitializeComponent();
            //
            _oldWDoc = oldWDoc;
            _uiApplication = new UIApplication(oldWDoc.Document.Application);
            _VSDHandler = new ViewStageDynamicallyHandler(_uiApplication);  // 这个值的指定一定要在靠前，因为后面的界面中的参数修改要同步到此对象中。
                                                                            //
            InitializeUI();

            // _VSDHandler 事件绑定
            _VSDHandler.CurrentTimeChanged += VsdHandlerOnCurrentTimeChanged;  // 事件绑定
            _VSDHandler.PlayerStopped += VsdHandlerOnPlayerStopped;
        }

        /// <summary> 在构造完成 _VSDHandler 之后进行其他的界面设置 </summary>
        private void InitializeUI()
        {
            textBoxNumInterval.PositiveOnly = true;
            textBoxNumInterval.Text = 0.5.ToString(CultureInfo.InvariantCulture); // 初始设置每秒2帧的播放速度
            //
            textBoxNumSpan.IntegerOnly = true;
            textBoxNumSpan.PositiveOnly = true;
            textBoxNumSpan.Text = @"2";

            // 设置开挖的起止时间
            OldWProjectInfo pi = _oldWDoc.GetProjectInfo();
            dateTimePicker_End.Value = pi.ExcavFinish;
            dateTimePicker_Start.Value = pi.ExcavStart;
            //
            comboBoxSpanUnit.DataSource = Enum.GetNames(typeof(TimeSpan2.TimeSpanUnit));
            comboBoxSpanUnit.SelectedItem = Enum.GetName(typeof(TimeSpan2.TimeSpanUnit), TimeSpan2.TimeSpanUnit.Days);
            //
            checkBoxLoopPlay.Checked = false;
            checkBoxBackPlay.Checked = false;
            //
        }

        private void ViewStageDynamically_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_VSDHandler != null)
            {
                _VSDHandler.Dispose();
            }
        }

        #endregion

        #region ---   为 _VSDHandler 赋值

        private DateTime _invalidStartTime;
        private void dateTimePicker_Start_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                _VSDHandler.StartTime = dateTimePicker_Start.Value;
                _invalidStartTime = dateTimePicker_Start.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"开始时间必须要早于结束时间", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dateTimePicker_Start.Value = _invalidStartTime;
            }
        }
        private DateTime _invalidEndTime;

        private void dateTimePicker_End_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                _VSDHandler.EndTime = dateTimePicker_End.Value;
                _invalidEndTime = dateTimePicker_End.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"开始时间必须要早于结束时间", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dateTimePicker_End.Value = _invalidEndTime;
            }
        }

        //   _VSDHandler.SpanValue
        private void textBoxNumSpan_ValueNumberChanged(object sender, double e)
        {
            _VSDHandler.SpanValue = e;
        }
        private void comboBoxSpanUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            _VSDHandler.SpanUnit = (TimeSpan2.TimeSpanUnit)Enum.Parse(typeof(TimeSpan2.TimeSpanUnit), comboBoxSpanUnit.SelectedItem.ToString());
        }

        // _VSDHandler.Intervals
        private void TextBoxNumIntervalOnValueNumberChanged(object sender, double d)
        {
            int interval = (int)(d * 1000);
            if (interval > 0)
            {
                _VSDHandler.Intervals = interval;
            }

        }

        private void checkBoxLoopPlay_CheckedChanged(object sender, EventArgs e)
        {
            _VSDHandler.LoopPlay = checkBoxLoopPlay.Checked;
        }

        private void checkBoxBackPlay_CheckedChanged(object sender, EventArgs e)
        {
            _VSDHandler.BackPlay = checkBoxBackPlay.Checked;
        }
        #endregion

        #region ---   响应 _VSDHandler 事件

        private delegate void ProgressChangedHander(DateTime dateTime);
        private void UpdateUI(DateTime dateTime)
        {
            if (this.hScrollBar1.InvokeRequired)
            {
                //非UI线程，再次封送该方法到UI线程
                this.BeginInvoke(new ProgressChangedHander(this.UpdateUI), new object[] { dateTime });
                // 一定要注意：Control类上的异步调用BeginInvoke并没有开辟新的线程完成委托任务，而是让界面控件的所属线程完成委托任务的。
            }
            else
            {
                //UI线程，进度更新
                hScrollBar1.Value = _VSDHandler.GetRatioBasedOnTime(dateTime);
                labelCurrentTime.Text = RvtTools.FormatTimeToMinite(dateTime);
            }
        }

        private void VsdHandlerOnCurrentTimeChanged(object sender, DateTime dateTime)
        {
            // UpdateUI(dateTime); return;
            // _scrollValueChangedOnUiOperation = false;
            hScrollBar1.Value = _VSDHandler.GetRatioBasedOnTime(dateTime);
            labelCurrentTime.Text = RvtTools.FormatTimeToMinite(dateTime);
        }

        #endregion

        #region ---   动画播放的事件处理

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            _VSDHandler.Play();
            buttonPlay.Enabled = false;
            this.buttonPlay.UseVisualStyleBackColor = true; // 用自定义的色彩显示
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            _VSDHandler.Pause();
            buttonPlay.Enabled = true;
            this.buttonPlay.UseVisualStyleBackColor = false; // 还原为平常的灰显
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _VSDHandler.Stop();
        }

        private void VsdHandlerOnPlayerStopped(object sender, EventArgs eventArgs)
        {
            buttonPlay.Enabled = true;
            this.buttonPlay.UseVisualStyleBackColor = false; // 还原为平常的灰显
        }

        #region ---   滚动条滚动操作

        ///// <summary> 滚动条的 ValueChanged 事件是否是由用户通过UI界面操作而触发的 </summary>
        //private bool _scrollValueChangedOnUiOperation;
        //private int _scrollBarValue;

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // 在松开鼠标左键以停止滚动时才触发视图刷新事件
            if (e.Type == ScrollEventType.EndScroll)
            {
                _VSDHandler.Pause();
                var playTime = _VSDHandler.GetTimeBasedOnRatio(e.NewValue);
                //
                // MessageBox.Show(playTime.ToString());
                _VSDHandler.StopFromScroll(playTime);
            }
            else
            {
                // 在移动滚动条时显示对应的日期
                var playTime = _VSDHandler.GetTimeBasedOnRatio(e.NewValue);
                labelCurrentTime.Text = RvtTools.FormatTimeToMinite(playTime);
            }
        }

        #endregion

        #endregion 
    }
}

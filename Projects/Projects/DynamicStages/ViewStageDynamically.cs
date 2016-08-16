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
using rvtTools;
using Timer = System.Timers.Timer;
using stdOldW.UserControls;

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

        private ViewStageDynamicallyHandler _VSDHandler;

        #endregion

        #region ---   构造函数

        /// <summary> 构造函数 </summary>
        public ViewStageDynamically(UIApplication uiApp)
        {
            InitializeComponent();
            //
            _uiApplication = uiApp;
            _VSDHandler = new ViewStageDynamicallyHandler(uiApp);  // 这个值的指定一定要在靠前，因为后面的界面中的参数修改要同步到此对象中。
                                                                   //
            InitializeUI();

            // _VSDHandler 
            _VSDHandler.CurrentTimeChanged += VsdHandlerOnCurrentTimeChanged;  // 事件绑定
        }

        /// <summary> 在构造完成 _VSDHandler 之后进行其他的界面设置 </summary>
        private void InitializeUI()
        {
            textBoxNumInterval.PositiveOnly = true;
            textBoxNumInterval.Text = 0.1.ToString(CultureInfo.InvariantCulture);
            //
            textBoxNumSpan.IntegerOnly = true;
            textBoxNumSpan.PositiveOnly = true;
            textBoxNumSpan.Text = 2.ToString();
            //
            dateTimePicker_End.Value = DateTime.Today;
            dateTimePicker_Start.Value = DateTime.Today.AddYears(-3);
            //
            comboBoxSpanUnit.DataSource = Enum.GetNames(typeof(TimeSpanUnit));
            comboBoxSpanUnit.SelectedItem = Enum.GetName(typeof(TimeSpanUnit), TimeSpanUnit.Days);
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

        private void dateTimePicker_Start_ValueChanged(object sender, EventArgs e)
        {
            _VSDHandler.StartTime = dateTimePicker_Start.Value;
        }

        private void dateTimePicker_End_ValueChanged(object sender, EventArgs e)
        {
            _VSDHandler.EndTime = dateTimePicker_End.Value;
        }

        //   _VSDHandler.SpanValue
        private void textBoxNumSpan_ValueNumberChanged(object sender, double e)
        {
            _VSDHandler.SpanValue = e;
        }
        private void comboBoxSpanUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            _VSDHandler.SpanUnit = (TimeSpanUnit)Enum.Parse(typeof(TimeSpanUnit), comboBoxSpanUnit.SelectedItem.ToString());
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
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            _VSDHandler.Pause();

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _VSDHandler.Stop();
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
                // MessageBox.Show(e.NewValue.ToString());
                _VSDHandler.Pause();
                var playTime = _VSDHandler.GetTimeBasedOnRatio(e.NewValue);
                _VSDHandler.ExternalRefreshView(playTime);
            }
            else
            {
                var playTime = _VSDHandler.GetTimeBasedOnRatio(e.NewValue);
                labelCurrentTime.Text = RvtTools.FormatTimeToMinite(playTime);
            }
        }

        #endregion

        #endregion

    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using eZstd;
using eZstd.Miscellaneous;
using eZstd.UserControls;
using Timer = System.Timers.Timer;
using View = Autodesk.Revit.DB.View;

namespace OldW.DynamicStages
{
    internal class ViewStageDynamicallyHandler : IDisposable, IExternalEventHandler
    {
        #region ---   Events

        /// <summary> 当前播放和进度 CurrentTime 属性改变时触发 </summary>
        public event EventHandler<DateTime> CurrentTimeChanged = delegate (object sender, DateTime time) { };

        /// <summary> 播放器停止播放 </summary>
        public event EventHandler PlayerStopped = delegate (object sender, EventArgs args) { };

        #endregion

        #region ---   Properties

        private DateTime _startTime;

        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                if (value >= _endTime)
                {
                    throw new ArgumentException("工况动画的开始时间必须早于结束时间");
                }
                _currentTime = value;
                _startTime = value;
            }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                if (_startTime >= value)
                {
                    throw new ArgumentException("工况动画的开始时间必须早于结束时间");
                }
                _endTime = value;
            }
        }

        /// <summary> 每一帧动画所对应的施工日期跨度 </summary>
        public double SpanValue { get; set; }
        public TimeSpan2.TimeSpanUnit SpanUnit { get; set; }

        /// <summary> 每一帧动画之间的时间间隔，单位为毫秒 </summary>
        private int _intervals;
        /// <summary> 每一帧动画之间的时间间隔，单位为毫秒 </summary>
        public int Intervals
        {
            get
            {
                return _intervals;
            }
            set
            {
                _intervals = value;
            }
        }

        private DateTime _currentTime;
        /// <summary> 当前动画正在展示的时间 </summary>
        public DateTime CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                // 触发事件
                CurrentTimeChanged(this, value);
            }
        }

        /// <summary> 循环播放，在播放到结尾时，再转到开始时间继续播放 </summary>
        public bool LoopPlay { get; set; }

        private bool _backPlay;
        /// <summary> 倒退播放，从最后面的时间向最开始的时间播放 </summary>
        public bool BackPlay
        {
            get { return _backPlay; }
            set
            {
                // 如果要进行回话，则将 SpanValue 设置为负值即可。
                SpanValue = value ? -Math.Abs(SpanValue) : Math.Abs(SpanValue);

                //
                _backPlay = value;
            }
        }

        #endregion

        #region ---   Fields

        private UIApplication _uiApp;

        private readonly ReviewDoc _reviewDoc;

        private View _view;

        /// <summary> 用来在Revit中进行施工工况的动画刷新的外部事件 </summary>
        private ExternalEvent _exEvent;

        private PlayOption _playOption;

        #endregion

        #region ---   构造函数 与 关闭

        /// <summary> 构造函数 </summary>
        public ViewStageDynamicallyHandler(UIApplication uiApp)
        {
            //
            _uiApp = uiApp;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //
            PlayOptionsInitialize();
            // 
            _reviewDoc = ReviewDoc.CreateFromActiveDocument(uiApp, doc);
            // 事件绑定

            //' ------ 将所有的初始化工作完成后，执行外部事件的绑定 ----------------
            this._exEvent = ExternalEvent.Create(this);
        }

        /// <summary> 对播放器的播放属性进行初始化 </summary>
        private void PlayOptionsInitialize()
        {
            Intervals = 500;
        }


        public string GetName()
        { return @"基坑群施工工况的动画展示"; }

        /// <summary> 析构函数 </summary>
        ~ViewStageDynamicallyHandler()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (_exEvent != null)
            {
                // 关闭事件关联
                _playOption = PlayOption.Stop;
                _exEvent.Raise();

                _exEvent.Dispose();
                _exEvent = null;
            }
        }

        #endregion

        #region ---   播放参数设置

        /// <summary> 根据指定的比例返回对应的施工日期 </summary>
        /// <param name="ratio">输入的值范围为[0,100]，0代表开始时间，100代表结束时间</param>
        public DateTime GetTimeBasedOnRatio(int ratio)
        {
            DateTime t = StartTime + TimeSpan.FromTicks((EndTime - StartTime).Ticks * ratio / ViewStageDynamically.ScrollBarMaximumValue);
            return t;
        }

        /// <summary> 根据指定的日期返回其在 StartTime 与 EndTime 之间的比例 </summary>
        /// <param name="time"> 某一个施工日期 </param>
        /// <returns>返回一个位于[0,100]的整数值</returns>
        public int GetRatioBasedOnTime(DateTime time)
        {
            if (time <= StartTime) return 0;
            if (time >= EndTime) return ViewStageDynamically.ScrollBarMaximumValue;
            //
            var t1 = (EndTime - StartTime).Ticks;
            var t2 = (time - StartTime).Ticks;
            return (int)((double)t2 / t1 * ViewStageDynamically.ScrollBarMaximumValue);
        }

        #endregion

        #region ---   动画的播放、暂停与停止 的控制

        /// <summary> 开始播放 </summary>
        public void Play()
        {
            _playOption = PlayOption.Play;
            _exEvent.Raise();
            //_timer.Start();
        }
        /// <summary> 暂停播放 </summary>
        public void Pause()
        {
            _playOption = PlayOption.Pause;
            _exEvent.Raise();
        }
        /// <summary> 停止播放 </summary>
        public void Stop()
        {
            _playOption = PlayOption.Stop;
            _exEvent.Raise();
        }

        private DateTime _scollTime;

        /// <summary>  </summary>
        /// <param name="currentTime"> 此 currentTime 必须是位于 StartTime 与 EndTime 之间的一个有效日期 </param>
        public void StopFromScroll(DateTime currentTime)
        {
            _scollTime = currentTime;
            //
            _playOption = PlayOption.PlayAndStop;
            _exEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            switch (_playOption)
            {
                case PlayOption.Play:
                    {
                        _view = _uiApp.ActiveUIDocument.ActiveGraphicalView;
                        _uiApp.Idling += UiAppOnIdling;
                        break;
                    }
                case PlayOption.Pause:
                    {
                        _uiApp.Idling -= UiAppOnIdling;
                        break;
                    }
                case PlayOption.Stop:
                    {
                        // 触发事件
                        PlayerStopped(this, new EventArgs());

                        // 取消事件关联
                        _uiApp.Idling -= UiAppOnIdling;
                        //
                        CurrentTime = StartTime;
                        break;
                    }
                case PlayOption.PlayAndStop:
                    {
                        _view = _uiApp.ActiveUIDocument.ActiveGraphicalView;
                        _uiApp.Idling -= UiAppOnIdling;
                        //
                        RefreshView(_scollTime);
                        break;
                    }
            }
        }

        #endregion

        #region ---   动画的播放 实现

        /// <summary> 当前正在执行Revit动画展示，此时不能再次进行触发。 </summary>
        private bool _isPlaying;

        /// <summary> 在每一次 Iding 事件时，进行动画的刷新。 </summary>
        /// <param name="sender"></param>
        /// <param name="idlingEventArgs"></param>
        private void UiAppOnIdling(object sender, IdlingEventArgs idlingEventArgs)
        {
            // 当Revit中的视图发生变化时，停止动画显示
            var v = (sender as UIApplication).ActiveUIDocument.ActiveGraphicalView;
            if (v.Id.IntegerValue != _view.Id.IntegerValue)
            {
                Stop();
            }

            //
            if (CheckIntervals())
            {
                var desiredTime = TimeSpan2.GetTimeFromTimeSpan(_currentTime, SpanValue, SpanUnit);
                DateTime playTime;
                if (CheckProgress(desiredTime, out playTime))
                {
                    // DebugUtils.ShowEnumerable(new object[]{ _currentTime,SpanValue,SpanUnit,desiredTime,playTime});
                    RefreshView(playTime);
                }
                else
                {
                    CurrentTime = playTime;
                    Stop();
                }
            }
            // 此语句是必须的，否则不会出现动画的刷新显示
            idlingEventArgs.SetRaiseWithoutDelay();

        }

        #region ---   有效性检查

        /// <summary>上一次成功发出Idling脉冲以及进行动画显示的时间 </summary>
        private DateTime _lastPulseTime;

        /// <summary>
        /// 检查两次 Idling 事件之间的时间间隔是否满足指定的速度Interval的要求。
        /// </summary>
        /// <returns></returns>
        private bool CheckIntervals()
        {
            DateTime dtNew = DateTime.Now;
            var interval = (dtNew - _lastPulseTime).TotalMilliseconds;
            if (interval > Intervals)
            {
                _lastPulseTime = dtNew;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> 检查当前播放进度，并按需要（是否超出指定的起止界限）调整实际播放的工况日期 </summary>
        /// <param name="desiredTime"> 想要播放的工况日期，但是此日期有可能超出了指定的起止界限 </param>
        /// <param name="playTime"> 实际要播放的工况日期 </param>
        /// <returns>如果返回true，则继续播放，如果返回false，则表示可以停止播放了。</returns>
        private bool CheckProgress(DateTime desiredTime, out DateTime playTime)
        {
            if (!LoopPlay)
            {
                if (desiredTime > EndTime)
                {
                    playTime = EndTime;
                    return false;
                }
                else if (desiredTime < StartTime)
                {
                    playTime = StartTime;
                    return false;
                }
                else
                {
                    playTime = desiredTime;
                    return true;
                }
            }
            else
            {
                if (desiredTime > EndTime)
                {
                    playTime = StartTime;
                    return true;
                }
                else if (desiredTime < StartTime)
                {  // 这种情况应该不会出现
                    playTime = EndTime;
                    return true;
                }
                else
                {
                    playTime = desiredTime;
                    return true;
                }
            }
        }

        #endregion

        /// <summary> 根据指定的施工日期来刷新 Revit 界面 </summary>
        /// <param name="currentTime"> 此 currentTime 必须是位于 StartTime 与 EndTime 之间的一个有效日期 </param>
        private void RefreshView(DateTime currentTime)
        {
            Debug.Print(_isPlaying.ToString() + "   ***   " + _exEvent.IsPending);
            if (!_isPlaying && !_exEvent.IsPending)
            {
                _isPlaying = true;

                CurrentTime = currentTime;

                // 播放动画
                using (Transaction tranDoc = new Transaction(_reviewDoc.Document, "施工工况动态展示"))
                {
                    try
                    {
                        tranDoc.Start();

                        // 根据施工日期刷新模型
                        _reviewDoc.ShowExcavation(tranDoc, currentTime, _view);
                        //
                        tranDoc.Commit();

                        // MessageBox.Show("完成  " + currentTime.ToString());
                    }
                    catch (Exception ex)
                    {
                        tranDoc.RollBack();
                        Stop();
                        DebugUtils.ShowDebugCatch(ex, "施工工况动态展示出错");
                    }
                }

                Debug.Print("执行 Raise");

                _isPlaying = false;
            }
        }

        #endregion
    }

    /// <summary> 控件播放器的播放选项 </summary>
    internal enum PlayOption
    {
        Play,
        Pause,
        Stop,
        /// <summary> 在指定的时间下显示一次，然后停止播放 </summary>
        PlayAndStop,
    }
}
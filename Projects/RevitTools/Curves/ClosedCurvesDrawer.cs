using System.Collections.Generic;
using System;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;



namespace rvtTools.Curves
{
    /// <summary>
    /// 绘制多重封闭的曲线
    /// </summary>
    public class ClosedCurvesDrawer
    {

        #region    ---   Events
        /// <summary>
        /// 在模型线绘制完成时，触发此事件。
        /// </summary>
        /// <param name="AddedCurves">添加的模型线</param>
        /// <param name="FinishedExternally">画笔是否是由外部程序强制关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为 True。</param>
        /// <param name="Succeeded">AddedCurves集合中的曲线集合是否满足指定的连续性条件</param>
        public delegate void DrawingCompletedEventHandler(List<List<ElementId>> AddedCurves, Boolean FinishedExternally, bool Succeeded);
        private DrawingCompletedEventHandler _closedDrawingCompletedEvent;

        /// <summary> 绘制封闭的模型线结束 </summary>
        public event DrawingCompletedEventHandler ClosedDrawingCompleted
        {
            add
            {
                _closedDrawingCompletedEvent = (DrawingCompletedEventHandler)System.Delegate.Combine(_closedDrawingCompletedEvent, value);
            }
            remove
            {
                _closedDrawingCompletedEvent = (DrawingCompletedEventHandler)System.Delegate.Remove(_closedDrawingCompletedEvent, value);
            }
        }


        #endregion

        #region    ---   Properties

        private bool checkInTime;
        /// <summary>
        /// 是否在每一步绘制时都检测所绘制的曲线是否符合指定的要求，如果为False，则在绘制操作退出后进行统一检测。
        /// </summary>
        /// <returns></returns>
        public bool CheckInTime
        {
            get
            {
                return checkInTime;
            }
        }

        #endregion

        #region    ---   Fields

        private UIApplication uiApp;
        private Autodesk.Revit.ApplicationServices.Application rvtApp;
        private Document doc;

        /// <summary>
        /// 用来绘制封闭的模型线
        /// </summary>
        private ModelCurvesDrawer ClosedCurveDrawer;

        /// <summary>
        /// 已经绘制的所有模型线
        /// </summary>
        private List<List<ElementId>> AddedModelCurvesId;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiApp">进行模型线绘制的Revit程序</param>
        /// <param name="CheckInTime">是否在每一步绘制时都检测所绘制的曲线是否符合指定的要求，如果为False，则在绘制操作退出后进行统一检测。</param>
        /// <param name="BaseCurves">
        /// 在新绘制之前，先指定一组基准曲线集合，而新绘制的曲线将与基准曲线一起来进行连续性条件的检测。
        /// </param>
        public ClosedCurvesDrawer(UIApplication uiApp, bool CheckInTime, List<ElementId> BaseCurves = null)
        {
            this.uiApp = uiApp;
            this.checkInTime = CheckInTime;
            AddedModelCurvesId = new List<List<ElementId>>();
        }

        /// <summary> 在UI界面中绘制模型线。此方法为异步操作，程序并不会等待 PostDraw 方法执行完成才继续向下执行。  </summary>
        public void PostDraw()
        {
            // 绘制轮廓
            this.ClosedCurveDrawer = new ModelCurvesDrawer(this.uiApp, CurveCheckMode.Closed, this.CheckInTime);
            ClosedCurveDrawer.DrawingCompleted += Drawer_DrawingCompleted;
            this.ClosedCurveDrawer.PostDraw();
        }

        public void Cancel()
        {
            if (ClosedCurveDrawer != null)
            {
                this.ClosedCurveDrawer.Cancel();
            }
        }

        private void Drawer_DrawingCompleted(List<ElementId> AddedCurves, bool FinishedExternally, bool Succeeded)
        {
            if (Succeeded)
            {

                // 将结果添加到集合中
                AddedModelCurvesId.Add(AddedCurves);

                // 询问是否还要添加
                DialogResult res = MessageBox.Show(@"封闭曲线绘制成功，是否还要继续绘制另一组封闭曲线？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    // Can not subscribe to an event during execution of that event. revit.exception.InvalidOperationException
                    this.ClosedCurveDrawer.PostDraw();
                }
                else
                {
                    // 取消与这个绘制器的关联
                    ClosedCurveDrawer.DrawingCompleted -= Drawer_DrawingCompleted;
                    ClosedCurveDrawer.Finish(FinishedExternally, Succeeded);
                    this.ClosedCurveDrawer = null;
                    //
                    if (_closedDrawingCompletedEvent != null)
                        _closedDrawingCompletedEvent(AddedModelCurvesId, FinishedExternally, true);
                } 
            }
            else
            {
                // 取消与这个绘制器的关联
                ClosedCurveDrawer.DrawingCompleted -= Drawer_DrawingCompleted;
                this.ClosedCurveDrawer.Finish(FinishedExternally, Succeeded);
                this.ClosedCurveDrawer = null;

                if (_closedDrawingCompletedEvent != null)
                    _closedDrawingCompletedEvent(AddedModelCurvesId, FinishedExternally, false);
            }

        }

    }
}

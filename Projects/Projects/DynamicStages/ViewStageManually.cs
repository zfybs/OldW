using System;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RevitStd;

namespace OldW.DynamicStages
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class ViewStageManually
    {
        #region ---   ExternalEvent

        /// <summary>用来触发外部事件（通过其Raise方法） </summary>
        /// <remarks>ExEvent属性是必须有的，它用来执行Raise方法以触发事件。</remarks>
        private ExternalEvent _exEvent;
        /// <summary> 用来响应外部事件中的不同需求（通过其Execute方法）。 </summary>
        /// <remarks>由于RequestHandler 对象是在构造ExternalEvent对象时就已经保存在ExternalEvent中的了，
        /// 所以，如果Execute方法中并不需要判断到底要实现哪一个需求，则此属性是不必要的。</remarks>
        private ViewStageManuallyHandler ReqHandler;
        #endregion

        #region ---   Properties

        private DateTime _activeTime;
        /// <summary>
        /// 当前的施工日期。通过指定此属性，即可以自动执行Revit中视图的刷新。
        /// </summary>
        private DateTime ActiveTime
        {
            get { return _activeTime; }
            set
            {
                string strActiveTime = RvtTools.FormatTimeToMinite(value);

                // 将时间的精度控制在分钟以内
                DateTime normalizeTime = DateTime.Parse(strActiveTime);

                if (_activeTime != normalizeTime)
                {
                    //
                    _activeTime = normalizeTime;

                    // 在Revit中刷新施工工况的显示
                    OnConstructionDateChanged(normalizeTime);
                    //
                    labelConstructionTime.Text = strActiveTime;
                }
            }
        }

        #endregion

        #region ---   Fields

        private readonly UIApplication _uiApplication;


        #endregion

        /// <summary> 构造函数 </summary>
        public ViewStageManually(UIApplication uiApp)
        {
            InitializeComponent();

            //
            _uiApplication = uiApp;
            _activeTime = DateTime.Now;
            labelConstructionTime.Text =RvtTools.FormatTimeToMinite(_activeTime) ;

            //
            monthCalendar1.MaxSelectionCount = 1;

            //
            numberChanging1.IntegerOnly = true;

            //' ------ 将所有的初始化工作完成后，执行外部事件的绑定 ----------------
            this.ReqHandler = new ViewStageManuallyHandler();
            // 新建一个外部事件实例
            this._exEvent = ExternalEvent.Create(this.ReqHandler);

            //
            ReqHandler.SetExcavSoilCollections(uiApp);
        }

        #region ---   根据施工日期刷新模型

        private void OnConstructionDateChanged(DateTime constructionTime)
        {
            ReqHandler.ExternalEventArgs = constructionTime;
            _exEvent.Raise();
        }

        #endregion

        #region ---   改变 ActiveTime 属性的事件

        private void numberChanging1_ValueAdded()
        {
            ActiveTime = numberChanging1.AddTimeSpan(_activeTime);
        }

        private void numberChanging1_ValueMinused()
        {
            ActiveTime = numberChanging1.MinusTimeSpan(_activeTime);
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            DateTime t = e.Start;
            ActiveTime = t;
        }
        #endregion
    }
}
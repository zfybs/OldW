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

        /// <summary>���������ⲿ�¼���ͨ����Raise������ </summary>
        /// <remarks>ExEvent�����Ǳ����еģ�������ִ��Raise�����Դ����¼���</remarks>
        private ExternalEvent _exEvent;
        /// <summary> ������Ӧ�ⲿ�¼��еĲ�ͬ����ͨ����Execute�������� </summary>
        /// <remarks>����RequestHandler �������ڹ���ExternalEvent����ʱ���Ѿ�������ExternalEvent�е��ˣ�
        /// ���ԣ����Execute�����в�����Ҫ�жϵ���Ҫʵ����һ��������������ǲ���Ҫ�ġ�</remarks>
        private ViewStageManuallyHandler ReqHandler;
        #endregion

        #region ---   Properties

        private DateTime _activeTime;
        /// <summary>
        /// ��ǰ��ʩ�����ڡ�ͨ��ָ�������ԣ��������Զ�ִ��Revit����ͼ��ˢ�¡�
        /// </summary>
        private DateTime ActiveTime
        {
            get { return _activeTime; }
            set
            {
                string strActiveTime = RvtTools.FormatTimeToMinite(value);

                // ��ʱ��ľ��ȿ����ڷ�������
                DateTime normalizeTime = DateTime.Parse(strActiveTime);

                if (_activeTime != normalizeTime)
                {
                    //
                    _activeTime = normalizeTime;

                    // ��Revit��ˢ��ʩ����������ʾ
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

        /// <summary> ���캯�� </summary>
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

            //' ------ �����еĳ�ʼ��������ɺ�ִ���ⲿ�¼��İ� ----------------
            this.ReqHandler = new ViewStageManuallyHandler();
            // �½�һ���ⲿ�¼�ʵ��
            this._exEvent = ExternalEvent.Create(this.ReqHandler);

            //
            ReqHandler.SetExcavSoilCollections(uiApp);
        }

        #region ---   ����ʩ������ˢ��ģ��

        private void OnConstructionDateChanged(DateTime constructionTime)
        {
            ReqHandler.ExternalEventArgs = constructionTime;
            _exEvent.Raise();
        }

        #endregion

        #region ---   �ı� ActiveTime ���Ե��¼�

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
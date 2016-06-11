using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OldW.Instrumentations;

namespace OldW.DataManager
{
    public partial class Chart_MonitorData
    {
        #region   ---  Properties

        #endregion

        #region   ---  Fields

        /// <summary> 数据图表 </summary>
        internal Chart Chart;


        #endregion

        #region   ---  构造函数与窗口的开启与关闭

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Type">窗口的标题：监测类型</param>
        /// <remarks></remarks>
        public Chart_MonitorData(InstrumentationType Type)
        {
            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            //
            this.KeyPreview = true;
            //
            this.Text = Type.ToString();
            SetupChart();

            // 设置窗口尺寸
            Chart.Size = new Size(650, 400);  //  一般情况下的窗口尺寸
            switch (Type) // 对于不同的监测数据类型，设置不同的图表格式
            {
                case InstrumentationType.测斜:
                    Chart.Size = new Size(400, 650);
                    break;
            }
        }

        /// <summary>
        /// 图表的初始化
        /// </summary>
        private void SetupChart()
        {

        }

        /// <summary>
        /// 添加一条监测曲线，并设置曲线的基本样式
        /// </summary>
        /// <param name="seriesName"></param>
        /// <param name="chartAreaName"></param>
        /// <returns></returns>
        public Series AddLineSeries(string seriesName, string chartAreaName = "ChartArea1")
        {
            Series series1 = new Series();
            series1.ChartArea = chartAreaName;
            series1.Name = seriesName;

            // 格式属性
            series1.ChartType = SeriesChartType.Line;
            series1.MarkerStyle = MarkerStyle.Circle;

            // 不显示空数据点的默认数值
            series1.EmptyPointStyle.IsValueShownAsLabel = false;
            this.Chart.Series.Add(series1);
            return series1;
        }

        #endregion

        #region   ---  事件

        public void Chart_MonitorData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        #endregion
    }
}
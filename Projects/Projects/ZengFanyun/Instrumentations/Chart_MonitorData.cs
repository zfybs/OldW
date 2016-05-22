// VBConversions Note: VB project level imports

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports

using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using OldW.GlobalSettings;


namespace OldW.DataManager
{
	public partial class Chart_MonitorData
	{
		
#region   ---  Properties
		
#endregion
		
#region   ---  Fields
		
		/// <summary> 数据图表 </summary>
		internal Chart Chart;
		internal Series Series;
		
#endregion
		
#region   ---  构造函数与窗口的开启与关闭
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Type">窗口的标题：监测类型</param>
		/// <remarks></remarks>
		public Chart_MonitorData(OldW.GlobalSettings.InstrumentationType Type)
		{
			
			// This call is required by the designer.
			InitializeComponent();
			
			// Add any initialization after the InitializeComponent() call.
			//
			this.KeyPreview = true;
			//
			this.Text = Type.ToString();
			SetupChart();
			
			switch (Type) // 对于不同的监测数据类型，设置不同的图表格式
			{
				case InstrumentationType.墙体测斜:
					Chart.Size = new Size(400, 650);
					break;
					
				case InstrumentationType.地表隆沉:
					Chart.Size = new Size(650, 400);
					break;
					
				case InstrumentationType.支撑轴力:
					Chart.Size = new Size(650, 400);
					break;
					
			}
			
		}
		
		/// <summary>
		/// 图表的初始化
		/// </summary>
		private void SetupChart()
		{
			this.Series = Chart.Series["Series1"];
			
			
			// 设置空数据点的格式
			DataPointCustomProperties pst = new DataPointCustomProperties();
			
			//  Me.Series.EmptyPointStyle = pst
			
			
		}
		
#endregion
		
#region   ---  事件
		
		public void Chart_MonitorData_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == System.Windows.Forms.Keys.Escape)
			{
				this.Close();
			}
		}
		
#endregion
		
	}
}

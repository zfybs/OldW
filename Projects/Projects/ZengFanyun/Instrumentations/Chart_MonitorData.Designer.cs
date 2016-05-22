using  System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OldW.DataManager
{
	public partial class Chart_MonitorData : System.Windows.Forms.Form
	{
		
		//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			System.Windows.Forms.DataVisualization.Charting.ChartArea ChartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Series Series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			this.Chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(Chart_MonitorData_KeyDown);
			((System.ComponentModel.ISupportInitialize) this.Chart).BeginInit();
			this.SuspendLayout();
			//
			//Chart
			//
			ChartArea1.Area3DStyle.WallWidth = 1;
			ChartArea1.Name = "ChartArea1";
			this.Chart.ChartAreas.Add(ChartArea1);
			this.Chart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Chart.Location = new System.Drawing.Point(0, 0);
			this.Chart.Name = "Chart";
			Series1.ChartArea = "ChartArea1";
			Series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			Series1.CustomProperties = "EmptyPointValue=Zero";
			Series1.EmptyPointStyle.IsValueShownAsLabel = true;
			Series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
			Series1.Name = "Series1";
			this.Chart.Series.Add(Series1);
			this.Chart.Size = new System.Drawing.Size(634, 362);
			this.Chart.TabIndex = 0;
			this.Chart.Text = "Chart1";
			//
			//Chart_MonitorData
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (12.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 362);
			this.Controls.Add(this.Chart);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Chart_MonitorData";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Chart_MonitorData";
			((System.ComponentModel.ISupportInitialize) this.Chart).EndInit();
			this.ResumeLayout(false);
			
		}
		
	}
}

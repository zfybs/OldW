// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Windows.Forms;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports

using std_ez;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

//using OldW.Instrumentation;
//using OldW.GlobalSettings.Constants;


namespace OldW.DataManager
{
	
	/// <summary>
	/// 模型中的测点的监测数据的添加，删除，导入导出等
	/// </summary>
	/// <remarks></remarks>
	public partial class ElementDataManager
	{
		
#region   ---  Properties
		
#endregion
		
#region   ---  Fields
		private Document doc;
		
		/// <summary> 当前活动的图元 </summary>
		private Element ActiveElement;
		/// <summary> 当前活动的图元中所保存的监测数据 </summary>
		private OldW.Instrumentation.MonitorData_Point ActiveMonitorData;
		
#endregion
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="eleidCollection">所有要进行处理的测点元素的Id集合</param>
		/// <param name="document"></param>
		/// <remarks></remarks>
		public ElementDataManager(ICollection<ElementId> eleIdCollection, Document document)
		{
			InitializeComponent();
			// --------------------------------
			this.doc = document;
			
			this.cmbx_elements.DisplayMember = LstbxDisplayAndItem.DisplayMember;
			this.cmbx_elements.ValueMember = LstbxDisplayAndItem.ValueMember;
			if (eleIdCollection.Count > 0)
			{
				this.cmbx_elements.DataSource = fillCombobox(eleIdCollection);
			}
			this.eZDataGridView1.Columns[0].ValueType = typeof(DateTime);
			this.eZDataGridView1.Columns[1].ValueType = typeof(object);
		}
		
		private LstbxDisplayAndItem[] fillCombobox(ICollection<ElementId> elementCollection)
		{
			int c = elementCollection.Count;
			LstbxDisplayAndItem[] arr = new LstbxDisplayAndItem[c - 1 + 1];
			int i = 0;
			Element ele = default(Element);
			foreach (ElementId eleid in elementCollection)
			{
				ele = doc.GetElement(eleid);
				arr[i] = new LstbxDisplayAndItem(ele.Name + ":" + ele.Id.IntegerValue.ToString(), ele);
				i++;
			}
			return arr;
		}
		
		public void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Element ele = (Element) cmbx_elements.SelectedValue;
			this.ActiveElement = ele;
			FillTableWithElementData(ele, this.eZDataGridView1);
		}
		
#region    ---   本地子方法
		
		/// <summary>
		/// 将表格中的数据保存到Element的对应参数中。
		/// </summary>
		/// <remarks></remarks>
		public void SaveTableToElement(object sender, EventArgs e)
		{
			string strData = "";
			int RowsCount = this.eZDataGridView1.Rows.Count;
			int ColsCount = this.eZDataGridView1.Columns.Count;
			DateTime[] arrDate = new DateTime[RowsCount - 2 + 1]; // 默认不读取最行一行，因为一般情况下，DataGridView中的最后一行是一行空数据
			object[] arrValue = new object[RowsCount - 2 + 1];
			//
			DataGridViewRow row = default(DataGridViewRow);
			for (var r = 0; r <= RowsCount - 2; r++)
			{
				row = this.eZDataGridView1.Rows[System.Convert.ToInt32(r)];
				for (var c = 0; c <= ColsCount - 1; c++)
				{
					if (!DateTime.TryParse(System.Convert.ToString(row.Cells[0].Value), out arrDate[(int) r]))
					{
						TaskDialog.Show("Error", "第" + (r + 1) + "行数据不能正确地转换为DateTime。");
						return;
					}
					arrValue[(int) r] = row.Cells[1].Value;
				}
			}
			OldW.Instrumentation.MonitorData_Point moniData = new OldW.Instrumentation.MonitorData_Point(arrDate, arrValue);
			strData = StringSerializer.Encode64(moniData);
			
			Parameter aa = ActiveElement.get_Parameter(OldW.GlobalSettings.Constants.SP_Monitor_Guid);
			
			Parameter para = ActiveElement.get_Parameter(OldW.GlobalSettings.Constants.SP_Monitor_Guid); // ActiveElement.Parameter_MonitorData
			using (Transaction Tran = new Transaction(doc, "保存表格中的数据到Element的参数中"))
			{
				Tran.Start();
				try
				{
					para.Set(strData);
					doc.Regenerate();
					Tran.Commit();
				}
				catch (Exception)
				{
					Tran.RollBack();
				}
			}
			
		}
		
		/// <summary>
		/// 将元素的数据写入表格
		/// </summary>
		/// <param name="ele"></param>
		/// <param name="Table"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		private bool FillTableWithElementData(Element ele, DataGridView Table)
		{
			bool blnSucceed = true;
			string strData = ele.get_Parameter(OldW.GlobalSettings.Constants.SP_Monitor_Guid).AsString();
			if (strData.Length > 0)
			{
				OldW.Instrumentation.MonitorData_Point Dt = default(OldW.Instrumentation.MonitorData_Point);
				try
				{
					Dt = (OldW.Instrumentation.MonitorData_Point) (StringSerializer.Decode64(strData));
					if ((Dt.arrDate == null) || (Dt.arrValue == null))
					{
						throw (new Exception());
					}
					this.ActiveMonitorData = Dt;
				}
				catch (Exception)
				{
					TaskDialog.Show("Error", "参数中的数据不能正确地提取。", TaskDialogCommonButtons.Ok, TaskDialogResult.Ok);
					return false;
				}
				// 将此测点的监测数据写入表格
				DateTime[] arrDate = Dt.arrDate;
				object[] arrValue = Dt.arrValue;
				int DataCount = System.Convert.ToInt32(Dt.arrDate.Length);
				//
				if (DataCount > 0)
				{
					int originalRowsCount = this.eZDataGridView1.Rows.Count;
					
					if (DataCount >= originalRowsCount) // 添加不够的行
					{
						this.eZDataGridView1.Rows.Add(DataCount - originalRowsCount + 1);
					}
					else // 删除多余行
					{
						for (int i = originalRowsCount; i >= DataCount + 2; i--)
						{
							this.eZDataGridView1.Rows.RemoveAt(0); // 不能直接移除最后那一行，因为那一行是未提交的
						}
					}
					
					// 将数据写入表格
					for (int r = 0; r <= DataCount - 1; r++)
					{
						this.eZDataGridView1.Rows[r].SetValues(new object[] {arrDate[r], arrValue[r]});
					}
					// 将最后一行的数据清空，即使某个单元格的ValueType为DateTime，也可以设置其值为Nothing，此时这个单元格中会显示为空。
					this.eZDataGridView1.Rows[this.eZDataGridView1.Rows.Count - 1].SetValues(new object[] {null, null});
				}
			}
			else
			{
				Table.Rows.Clear();
			}
			return blnSucceed;
		}
		
		/// <summary>
		/// 绘制图表
		/// </summary>
		/// <param name="Data"></param>
		private Chart_MonitorData DrawData(OldW.Instrumentation.MonitorData_Point Data)
		{
			Chart_MonitorData Chart1 = new Chart_MonitorData(GlobalSettings.InstrumentationType.地表隆沉);
			Chart1.Series.Points.DataBindXY(Data.arrDate, Data.arrValue);
			Chart1.Show();
			return Chart1;
		}
		
#endregion
		
#region    ---   事件处理
		
		public void MyDataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			if ((e.Context & DataGridViewDataErrorContexts.Parsing)!=0)
			{
				MessageBox.Show("输入的数据不能转换为日期数据。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				e.ThrowException = false;
			}
		}
		
		public void btnDraw_Click(object sender, EventArgs e)
		{
			DrawData(ActiveMonitorData);
		}
#endregion
		
	}
}

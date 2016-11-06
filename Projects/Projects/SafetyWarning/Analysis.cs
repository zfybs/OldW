using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.Instrumentations;
using RevitStd;

namespace OldW.SafetyWarning
{
	public class Analysis
	{
		
		private UIDocument uidoc;
		private Document doc;
		private ICollection<ElementId> SelectedElements;
		private List<ElementId> ViolateList;
		public Analysis(ICollection<ElementId> eleIds, UIDocument UIDocument)
		{
			
			// --------------------
			this.uidoc = UIDocument;
			this.doc = this.uidoc.Document;
			this.SelectedElements = eleIds;
		}
		
		public void CheckData()
		{
			ViolateList = new List<ElementId>();
			
			Family fami = doc.FindFamily(InstrumentationType.地表隆沉.ToString());
			//
			List<ElementId> Monitor = new List<ElementId>();
			if (fami != null)
			{
				Monitor = (List<ElementId>)fami.Instances().ToElementIds() ;
			}
			
			foreach (ElementId eleId in SelectedElements)
			{
				// 判断图形是否是测点
				if (Monitor.Contains(eleId))
				{
					
					//   If IsViolated(eleId) Then
					//       ViolateList.Add(eleId)
					//   End If
				}
				
			}
			// 选择单元
			dynamic with_1 = uidoc.Selection;
			with_1.SetElementIds(ViolateList);
			
			// 将结果显示在表格中
			System.Windows.Forms.Form f = new System.Windows.Forms.Form();
			DataGridView dgv = new DataGridView();
			f.Controls.Add(dgv);
			f.StartPosition = FormStartPosition.CenterScreen;
			DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
			dgv.AutoGenerateColumns = false;
			dgv.Columns.Clear();
			dgv.Dock = DockStyle.Fill;
			dgv.Columns.Add(col);
			dgv.DataSource = ViolateList;
			col.DataPropertyName = "IntegerValue";
			int a = 20;
			f.ShowDialog();
		}
		
		//      Private Function IsViolated(ByVal eleId As ElementId) As Boolean
		//          Dim blnIsViolated As Boolean = False
		//          '
		//          Dim ele As Element = doc.GetElement(eleId)
		//          Dim strData As String = ele.Parameter(Constants.SP_Monitor_Guid).AsString
		//          Dim Dt As MonitorData_Point = DirectCast(StringSerializer.Decode64(strData), MonitorData_Point)
		//          Dim v As Object
		//          With Dt
		//              For i As UInt32 = 0 To .arrDate.Length - 1
		//                  v = .arrValue(i)
		//                  Dim WV As WarningValue = GetWarningValue(GlobalSettings.ProjectPath.Path_WarningValueUsing)
		//                  '
		//                  If (v IsNot Nothing) AndAlso (v > WV.warningGSetle.sum) Then
		//                      Return True
		//                  End If
		//              Next
		//          End With
		//          Return blnIsViolated
		//      End Function
		
	}
}

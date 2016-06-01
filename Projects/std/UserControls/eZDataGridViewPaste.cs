using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using std_ez;

namespace std_ez
{
    /// <summary>
    /// 支持复制粘贴功能的表格控件
    /// </summary>
    public class eZDataGridViewPaste : std_ez.eZDataGridView
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public eZDataGridViewPaste() : base()
        {
            this.KeyDown += new KeyEventHandler(myDataGridView_KeyDown);
        }

        #region   ---  数据的复制与粘贴

        /// <summary>
        /// 如下按下Ctrl+V，则将表格中的数据粘贴到DataGridView控件中
        /// </summary>
        /// <remarks>DataGridView表格的索引：行号：表头为-1，第一行为0，列号：表示行编号的列为-1，第一个数据列的列号为0.
        /// DataGridView.Rows.Count与DataGridView.Columns.Count均只计算数据区域，而不包含表头与列头。</remarks>
        private void myDataGridView_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Control & e.KeyCode == Keys.V)
            {
                var a = this.SelectedCells;
                var count = a.Count;
                //Dim s As String = "行号" & vbTab & "列号" & vbCrLf
                //For i = 0 To count - 1
                //    s = s & a.Item(i).RowIndex & vbTab & a.Item(i).ColumnIndex & vbCrLf
                //Next
                //MessageBox.Show(s)
                if (count != 1)
                {
                    MessageBox.Show("请选择某一个单元格，来作为粘贴的起始位置。", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataGridViewCell startCell = this.SelectedCells[0];
                PasteFromTable(startCell.RowIndex, startCell.ColumnIndex);
            }
        }

        /// <summary> 将表格中的数据粘贴到DataGridView控件中 </summary>
        /// <param name="startRow">粘贴的起始单元格的行位置</param>
        /// <param name="startCol">粘贴的起始单元格的列位置</param>
        /// <remarks>DataGridView表格的索引：行号：表头为-1，第一行为0，列号：表示行编号的列为-1，第一个数据列的列号为0.
        /// DataGridView.Rows.Count与DataGridView.Columns.Count均只计算数据区域，而不包含表头与列头。总行数包括最后一行空数据行。</remarks>
        private void PasteFromTable(int startRow, int startCol)
        {
            string pastTest = Clipboard.GetText();

            if (string.IsNullOrEmpty(pastTest))
            { return; }

            // excel中是以"空格"和"换行"来当做字段和行，所以用"\r\n"来分隔，即"回车+换行"
            string[] lines = pastTest.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int writeRowsCount = lines.Length; //要写入多少行数据
            int writeColsCount = lines[0].Split('\t').Length; //要写入的每一行数据中有多少列
                                                              //
            int endRow = startRow + writeRowsCount - 1; // 要修改的最后一行的行号

            int rowsToAdd = endRow + 2 - this.Rows.Count;// 说明要额外添加这么多行才能放置要粘贴进来的数据

            if (rowsToAdd > 0)
            {
                if (this.DataSource is IBindingList)
                {
                    IBindingList ds = (IBindingList)this.DataSource;

                    // 对于 DataSource 绑定到 IBindingList 时，不能直接对DataGridView添加行，而是通过对于绑定的 IBindingList 进行添加来实现的。
                    // 因为 IBindingList 中每一个新添加的元素都要符合绑定的类的构造形式。
                    if (startRow == Rows.Count - 1)
                    {
                        // 当DataGridView的最后一行（AddNew的那一行）被选中时，执行BindingList.AddNew方法会出现报错。
                        // 所以这里进行判断，并且当其被选中时就先取消这一行的选择。
                        CurrentCell = null;
                    }
                    for (int i = 0; i < rowsToAdd; i++)
                    {
                        ds.AddNew();
                    }
                    CurrentCell = Rows[startRow].Cells[startCol];
                }
                else
                {  // 直接添加数据行
                    this.Rows.Add(rowsToAdd);
                }

            }
            int endCol = 0; // 要修改的最后面的那一列的列号
            endCol = startCol + writeColsCount <= this.Columns.Count
                ? startCol + writeColsCount - 1
                : this.Columns.Count - 1;
            //
            string strline = "";
            string[] strs = null;
            try
            {
                for (int r = startRow; r <= endRow; r++)
                {
                    strline = lines[r - startRow];
                    strs = strline.Split('\t'); //在每一行的单元格间，作为单元格的分隔的字符为"\t",即水平换行符

                    for (int c = startCol; c <= endCol; c++)
                    {
                        // 在修改单元格数据时，即使添加的数据不符合此列字段的数据格式，也不会报错，而是会直接取消对于此单元格的赋值，转而继续进行下一个单元格的赋值操作。
                        this.Rows[r].Cells[c].Value = strs[c - startCol];
                        SetSelectedCellCore(c, r, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowDebugCatch(ex, "粘贴数据出错");
            }
        }

        #endregion
    }
}

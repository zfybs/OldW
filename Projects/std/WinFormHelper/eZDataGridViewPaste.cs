using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using stdOldW.WinFormHelper;

namespace stdOldW.WinFormHelper
{
    /// <summary>
    /// 支持复制粘贴功能的表格控件
    /// </summary>
    public class eZDataGridViewPaste : eZDataGridView
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public eZDataGridViewPaste() : base()
        {
            this.KeyDown += new KeyEventHandler(myDataGridView_KeyDown);
        }

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

                if (count != 1)
                {
                    MessageBox.Show("请选择某一个单元格，来作为粘贴的起始位置。", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataGridViewCell startCell = this.SelectedCells[0];
                PasteFromTable(startCell.RowIndex, startCell.ColumnIndex);
            }
        }

        /// <summary> 将表格中的数据粘贴到DataGridView控件中（通过先添加全部行，再为添加的行赋值的方式） </summary>
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
                if (DataSource is IBindingList)
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
                        // BindingList.AddNew方法会触发其 AddingNew 事件，用户必须手动在此事件中定义要实例化的初始变量。
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


            //  每一列的要进行检测的数据类型
            Type tp;
            Type[] checkedTypes = new Type[endCol - startCol + 1];

            for (int c = startCol; c <= endCol; c++)
            {
                tp = Columns[c].ValueType;
                // 如果某列的ValueType为Nullable<>的话，则要对其所指定的泛型进行检测，
                // 因为在为Rows[r].Cells[c].Value赋值时，字符"1.2"不能转换为float，而会被转换为null，
                // 但实际上1.2是一个合法的float值。所以这里要通过Convert.ChangeType来进行显式检验。
                checkedTypes[c - startCol] = Utils.GetNullableGenericArgurment(tp) ?? tp;
            }

            // 当前操作的单元格的坐标
            int rowIndex = 0, colIndex = 0;
            object value;
            try
            {
                // 数据赋值与检测
                string strline = "";
                string[] strs = null;
                for (rowIndex = startRow; rowIndex <= endRow; rowIndex++)
                {
                    // 一条记录中的数据
                    strline = lines[rowIndex - startRow];
                    strs = strline.Split('\t'); //在每一行的单元格间，作为单元格的分隔的字符为"\t",即水平换行符

                    for (colIndex = startCol; colIndex <= endCol; colIndex++)
                    {
                        // Convert.ChangeType 用来检查字符所对应的值是否可以转换为对应字段列的数据类型，如果不能转换，则会报错。
                        value = !string.IsNullOrEmpty(strs[colIndex - startCol])
                            ? Convert.ChangeType(strs[colIndex - startCol], checkedTypes[colIndex - startCol])
                            : null;

                        // 在修改单元格数据时，即使添加的数据不符合此列字段的数据格式，也不会报错，而是会直接取消对于此单元格的赋值，转而继续进行下一个单元格的赋值操作。
                        this.Rows[rowIndex].Cells[colIndex].Value = value;

                        SetSelectedCellCore(colIndex, rowIndex, true);  // 选中此单元格
                    }
                }
            }
            catch (Exception ex)
            {
                DebugUtils.ShowDebugCatch(ex, $"粘贴数据出错,出错的单元格为第 {rowIndex + 1} 行,第 {colIndex + 1} 列）");
            }
        }
    }
}

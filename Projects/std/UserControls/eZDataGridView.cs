using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace std_ez
{
    /// <summary>
    /// 自定义控件：DataGridView，向其中增加了：插入行、删除行、显示行号等功能.
    /// 此控件不支持表格内容的复制粘贴，如果要用此功能，请用其派生类<see cref="std_ez.eZDataGridViewPaste"/> 
    /// </summary>
    /// <remarks></remarks>
    public class eZDataGridView : DataGridView
    {
        private Container components = null;
        private ToolStripMenuItem ToolStripMenuItemInsert;
        private ToolStripMenuItem ToolStripMenuItemRemove;
        private ContextMenuStrip CMS_DeleteRows;
        private ToolStripMenuItem ToolStripMenuItemRemoveRows;
        private ContextMenuStrip CMS_RowHeader;

        #region   ---  控件的初始属性

        /// <summary>
        /// 构造函数
        /// </summary>
        public eZDataGridView()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(myDataGridView_KeyDown);
        }

        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new Container();
            this.RowsAdded += new DataGridViewRowsAddedEventHandler(myDataGridView_RowsNumberChanged);
            this.RowsRemoved += new DataGridViewRowsRemovedEventHandler(myDataGridView_RowsNumberChanged);
            this.RowsAdded += new DataGridViewRowsAddedEventHandler(RowsResizable);
            this.RowStateChanged += new DataGridViewRowStateChangedEventHandler(myDataGridView_RowStateChanged);
            this.RowHeaderMouseClick += new DataGridViewCellMouseEventHandler(myDataGridView_RowHeaderMouseClick);
            this.CellMouseClick += new DataGridViewCellMouseEventHandler(myDataGridView_CellMouseClick);
            this.CMS_RowHeader = new ContextMenuStrip(this.components);
            this.ToolStripMenuItemInsert = new ToolStripMenuItem();
            this.ToolStripMenuItemInsert.Click += new EventHandler(this.InsertRow);
            this.ToolStripMenuItemRemove = new ToolStripMenuItem();
            this.ToolStripMenuItemRemove.Click += new EventHandler(this.RemoveRow);
            this.CMS_DeleteRows = new ContextMenuStrip(this.components);
            this.ToolStripMenuItemRemoveRows = new ToolStripMenuItem();
            this.ToolStripMenuItemRemoveRows.Click += new EventHandler(this.ToolStripMenuItemRemoveRows_Click);
            this.CMS_RowHeader.SuspendLayout();
            this.CMS_DeleteRows.SuspendLayout();
            ((ISupportInitialize)this).BeginInit();
            this.SuspendLayout();
            //
            //CMS_RowHeader
            //
            this.CMS_RowHeader.Items.AddRange(new ToolStripItem[]
            {this.ToolStripMenuItemInsert, this.ToolStripMenuItemRemove});
            this.CMS_RowHeader.Name = "CMS_RowHeader";
            this.CMS_RowHeader.ShowImageMargin = false;
            this.CMS_RowHeader.Size = new Size(76, 48);
            //
            //ToolStripMenuItemInsert
            //
            this.ToolStripMenuItemInsert.Name = "ToolStripMenuItemInsert";
            this.ToolStripMenuItemInsert.Size = new Size(75, 22);
            this.ToolStripMenuItemInsert.Text = "插入";
            //
            //ToolStripMenuItemRemove
            //
            this.ToolStripMenuItemRemove.Name = "ToolStripMenuItemRemove";
            this.ToolStripMenuItemRemove.Size = new Size(75, 22);
            this.ToolStripMenuItemRemove.Text = "移除";
            //
            //CMS_DeleteRows
            //
            this.CMS_DeleteRows.Items.AddRange(new ToolStripItem[] { this.ToolStripMenuItemRemoveRows });
            this.CMS_DeleteRows.Name = "CMS_RowHeader";
            this.CMS_DeleteRows.ShowImageMargin = false;
            this.CMS_DeleteRows.Size = new Size(112, 26);
            //
            //ToolStripMenuItemRemoveRows
            //
            this.ToolStripMenuItemRemoveRows.Name = "ToolStripMenuItemRemoveRows";
            this.ToolStripMenuItemRemoveRows.Size = new Size(111, 22);
            this.ToolStripMenuItemRemoveRows.Text = "删除所选行";
            //
            //myDataGridView
            //
            this.ColumnHeadersHeight = 25;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.RowTemplate.Height = 23;
            this.RowTemplate.Resizable = DataGridViewTriState.False;
            this.ScrollBars = ScrollBars.Vertical;
            this.Size = new Size(346, 110);
            this.CMS_RowHeader.ResumeLayout(false);
            this.CMS_DeleteRows.ResumeLayout(false);
            ((ISupportInitialize)this).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        #region   ---  显示行号

        /// <summary>
        /// 行数改变时的事件：显示行号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void myDataGridView_RowsNumberChanged(object sender, dynamic e)
        {
            Int32 longRow = 0;
            for (longRow = Convert.ToInt32(e.RowIndex + e.RowCount) - 1;
                longRow <= this.Rows.GetLastRow(DataGridViewElementStates.Displayed);
                longRow++)
            {
                this.Rows[longRow].HeaderCell.Value = (longRow + 1).ToString();
            }
        }

        /// <summary>
        /// 设置新添加的一行的Resizable属性为False
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void RowsResizable(object sender, DataGridViewRowsAddedEventArgs e)
        {
            this.Rows[e.RowIndex].Resizable = DataGridViewTriState.False;
        }

        private void myDataGridView_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = (e.Row.Index + 1).ToString();
        }

        #endregion

        #region   ---  右键菜单的关联与显示

        private void myDataGridView_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //如果是右击
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex != this.Rows.Count - 1)  //  在选择中最后一行（用来新建一条数据的那一行）时，不弹出菜单。
                {
                    //如果行数只有一行
                    if (this.Rows.Count <= 1)
                    {
                        this.ToolStripMenuItemRemove.Enabled = false;
                    }
                    else
                    {
                        this.ToolStripMenuItemRemove.Enabled = true;
                    }


                    //选择右击项的那一行
                    this.ClearSelection();
                    this.Rows[e.RowIndex].Selected = true;
                    //显示菜单栏
                    CMS_RowHeader.Show();
                    CMS_RowHeader.Left = MousePosition.X;
                    CMS_RowHeader.Top = MousePosition.Y;
                }
            }
        }

        private void myDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) // 如果是对单元格进行右击
            {
                int R = e.RowIndex;
                int C = e.ColumnIndex;
                if (R >= 0 & C >= 0)
                {
                    //显示菜单栏
                    if (this.SelectedRows.Count == 0 || this.Rows.Count < 2)
                    {
                        this.ToolStripMenuItemRemoveRows.Enabled = false;
                    }
                    else
                    {
                        this.ToolStripMenuItemRemoveRows.Enabled = true;
                    }
                    CMS_DeleteRows.Show();
                    CMS_DeleteRows.Left = MousePosition.X;
                    CMS_DeleteRows.Top = MousePosition.Y;
                }
            }
        }

        #endregion

        #region   ---  行的插入与删除

        /// <summary>
        /// 插入一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        protected void InsertRow(object sender, EventArgs e)
        {
            int SelectedIndex = this.SelectedRows[0].Index;

            // 对于 DataSource 绑定到 IBindingList 时，不能直接对DataGridView添加行，而是通过对于绑定的 IBindingList 进行添加来实现的。
            // 因为 IBindingList 中每一个新添加的元素都要符合绑定的类的构造形式。
            if (DataSource is IBindingList)
            {
                IBindingList ds = (IBindingList)this.DataSource;
                var n = ds.AddNew();  // 在末尾添加一个新的实例
                ds.Insert(SelectedIndex, n); // 将实例插入到集合指定位置
                ds.RemoveAt(ds.Count - 1);  // 将新添加到末尾的元素删除
            }
            else
            {
                this.Rows.Insert(SelectedIndex, 1);
            }
        }

        /// <summary>
        /// 移除一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void RemoveRow(object sender, EventArgs e)
        {
            var Row = this.SelectedRows[0];
            if (Row.Index < this.Rows.Count - 1)
            {
                //当删除最后一行（不带数据，自动添加的行）时会报错：无法删除未提交的新行。
                this.Rows.Remove(Row);
            }
        }

        /// <summary>
        /// 移除多行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void ToolStripMenuItemRemoveRows_Click(object sender, EventArgs e)
        {
            //下面的 For Each 是从下往上索引的，即前面的Row对象的index的值大于后面的Index的值
            foreach (DataGridViewRow Row in this.SelectedRows)
            {
                if (Row.Index < this.Rows.Count - 1)
                {
                    //当删除最后一行（不带数据，自动添加的行）时会报错：无法删除未提交的新行。
                    this.Rows.Remove(Row);
                }
            }
        }

        #endregion

        #region   ---  单元格数据的删除
        /// <summary>
        /// 如下按下 Delete ，则将表格中的选中的单元格中的数据清除 </summary>
        private void myDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                // 删除选择的单元格中的数据
                foreach (DataGridViewCell c in this.SelectedCells)
                {
                    c.Value = null;
                }
            }
        }
    }
    #endregion
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using OldW.DataManager;
using stdOldW;
using stdOldW.DAL;
using stdOldW.WinFormHelper;
using Color = System.Drawing.Color;
using Form = System.Windows.Forms.Form;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 测点过滤器
    /// </summary>
    public partial class DataExport : Form
    {
        /// <summary> 最终决定要导出到Excel的测点单元。字典中的键为每一类测点，值为此测点类型下要导出的测点单元 </summary>
        public Dictionary<InstrumentationType, List<Instrumentation>> InstrumentationsToBeExported;


        /// <summary> 构造函数 </summary>
        /// <param name="eleIdCollection">所有要进行处理的测点元素的Id集合</param>
        /// <param name="document"></param>
        /// <remarks></remarks>
        public DataExport(ICollection<Instrumentation> eleIdCollection, InstrumDoc document)
        {
            InitializeComponent();

            // TreeViewIns 属性设置
            TreeViewIns.BackColor = Color.FromArgb(255, 204, 232, 207); // 背景色
            TreeViewIns.CheckBoxes = true;  // 是否显示复选框

            ConstructTreeView(eleIdCollection);  // 添加节点

            TreeViewIns.ExpandAll();       // 展开节点
            buttonSelectAll_Click(TreeViewIns, new EventArgs());  // 全部选择

            // BackgroundWorker 的设置
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            // 事件关联
            _backgroundWorker.DoWork += BackgroundWorkerOnDoWork_ImportFromExcel;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;
            _backgroundWorker.ProgressChanged += BackgroundWorkerOnProgressChanged;
        }

        /// <summary> 构造树形控件。一共分两级，第一级是测点类型，第二级是每种类型下的测点集合 </summary>
        /// <param name="eleIdCollection"></param>
        private void ConstructTreeView(ICollection<Instrumentation> eleIdCollection)
        {
            // TreeViewIns

            SortedSet<InstrumentationType> insType = new SortedSet<InstrumentationType>();
            foreach (Instrumentation ins in eleIdCollection)
            {
                string typeName = Enum.GetName(typeof(InstrumentationType), value: ins.Type);

                if (!insType.Contains(ins.Type))
                {
                    // 创建一个新的测点类型节点，并设置属性
                    TreeNode ndType = new TreeNode
                    {
                        Name = typeName,
                        Tag = ins.Type,
                        Text = typeName,
                    };
                    
                    TreeViewIns.Nodes.Add(ndType);
                    //
                    insType.Add(ins.Type);
                }

                // 在对应的测点类型节点下添加此监测测点单元，并设置属性
                TreeNode ndIns = new TreeNode();
                ndIns.Name = ins.Monitor.Id.ToString();
                ndIns.Text = ins.IdName;
                ndIns.Tag = ins;

                TreeViewIns.Nodes[typeName].Nodes.Add(ndIns);
            }
        }

        #region   ---  节点选择的相关操作


        private void buttonExpand_Click(object sender, EventArgs e)
        {
            TreeViewIns.ExpandAll();
        }
        private void buttonShrink_Click(object sender, EventArgs e)
        {
            TreeViewIns.CollapseAll();
        }
        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode ndType in TreeViewIns.Nodes)
            {
                ndType.Checked = true;
            }
        }
        private void buttonDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode ndType in TreeViewIns.Nodes)
            {
                ndType.Checked = false;
            }
        }

        /// <summary>
        /// 在选择或者取消选择父节点时，对其子节点进行相同的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewIns_AfterCheck(object sender, TreeViewEventArgs e)
        {
            var nd = e.Node;
            if (nd.Level == 0)
            {
                foreach (TreeNode ndIns in nd.Nodes)
                {
                    ndIns.Checked = nd.Checked;

                }
            }
        }

        #endregion

        #region ---   一般事件处理


        private void buttonChooseFile_Click(object sender, EventArgs e)
        {
            string filePath = WindowsUtil.ChooseSaveExcel("选择要保存的位置");
            if (!string.IsNullOrEmpty(filePath))
            {
                textBoxFilePath.Text = filePath;
            }
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region ---   监测数据的导出

        /// <summary> 执行数据导入操作的后台线程 </summary>
        private readonly BackgroundWorker _backgroundWorker;
        /// <summary> 要导出数据的Excel工作簿 </summary>
        private OleDbConnection _conn;


        /// <summary> 点击“导出-->”按钮 </summary>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            //
            InstrumentationsToBeExported = new Dictionary<InstrumentationType, List<Instrumentation>>();

            // 收集最终的过滤信息
            foreach (TreeNode ndType in TreeViewIns.Nodes)
            {
                InstrumentationType tp = (InstrumentationType)ndType.Tag;
                List<Instrumentation> instrums = new List<Instrumentation>();
                //
                foreach (TreeNode ndIns in ndType.Nodes)
                {
                    if (!ndIns.Checked) continue;

                    var ins = ndIns.Tag as Instrumentation;
                    if (ins != null)
                    {
                        instrums.Add(ins);
                    }
                } // 下一个测点对象

                if (instrums.Count > 0)
                {
                    InstrumentationsToBeExported.Add(tp, instrums);
                }
            }  // 下一个测点类型
            ExportToExcel(InstrumentationsToBeExported);
        }

        private void ExportToExcel(Dictionary<InstrumentationType, List<Instrumentation>> elems)
        {
            if (elems.Count > 0)
            {
                // 要创建的工作簿路径，此时此工作簿还不存在
                string filePath = textBoxFilePath.Text;
                if (!ExcelDbHelper.IsExcelDataSource(filePath))
                {
                    MessageBox.Show(@"无效的Excel工作簿路径");
                    return;
                }
                if (File.Exists(filePath))  // 删除已经存在的工作簿
                {
                    File.Delete(filePath);
                }

                if (!_backgroundWorker.IsBusy)
                {
                    // 开始导入监测数据

                    _conn = ExcelDbHelper.ConnectToExcel(filePath, 0);
                    _conn.Open();

                    // 界面初始化
                    labelProgress.Visible = true;
                    labelProgress.Text = "";

                    // Start the asynchronous operation.
                    _backgroundWorker.RunWorkerAsync(argument: elems);
                }
            }
        }

        /// <summary>
        /// 开始在后台线程中实际执行测点数据从Excel导入Revit的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"> 输入前请确保 e.Argument所对应的集合的Count > 0 </param>
        private void BackgroundWorkerOnDoWork_ImportFromExcel(object sender, DoWorkEventArgs e)
        {
            // 
            Dictionary<InstrumentationType, List<Instrumentation>> elems = e.Argument as Dictionary<InstrumentationType, List<Instrumentation>>;
            if (elems == null) throw new NullReferenceException("没有选择任何测点。");
            //
            int row = 0;
            try
            {
                for (row = 0; row < elems.Count; row++)
                {
                    var elem = elems.ElementAt(row);

                    // 导出此监测类型中所有选择的测点的监测数据
                    ExportOneType(elem.Key, elem.Value);

                    _backgroundWorker.ReportProgress((int)((float)row / elems.Count * 100));
                }
            }
            catch (Exception ex)
            {
                Utils.ShowDebugCatch(ex, $"将Excel中的监测数据导入Revit中的测点单元出错，出错行：{row + 1} 。");
                e.Cancel = true;
                return;
            }

        }

        /// <summary>
        /// 导出一类测点到一个或者多个 Excel 工作表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="elems"></param>
        private void ExportOneType(InstrumentationType type, IEnumerable<Instrumentation> elems)
        {
            DataTable table;

            if ((type & InstrumentationType.点测点集合) > 0)
            {
                // 对于一般的点测点，直接将此测点类型所对应的集合中的所有测点导出到一个工作表中
                table = Instrum_Point.ConvertToDatatable(elems.Select(r => r as Instrum_Point));
                table.TableName = type.ToString();

                // 创建一个Excel工作表中
                ExcelDbHelper.CreateNewSheet(_conn, table);

                //  将DataTable中的数据导出到Excel工作表中
                ExcelDbHelper.InsertDataTable(_conn, table, table.TableName + "$");
            }
            else if ((type & InstrumentationType.数值线测点集合) > 0)
            {
                // 每一个测点导出为一个表
                foreach (Instrumentation ele in elems)
                {
                    Instrum_Line line = (Instrum_Line)ele;
                    table = line.ConvertToDatatable(_conn);

                    table.TableName = line.GetMonitorName();

                    // 创建一个Excel工作表中
                    ExcelDbHelper.CreateNewSheet(_conn, table);

                    DataTable metaTable = _conn.GetSchema("Tables");
                    DataTable names = metaTable.DefaultView.ToTable(false, new string[] { "TABLE_NAME" }); // 提取表格中的指定字段以构造新表
                    List<object> TableNameList = metaTable.AsEnumerable().Select(r => r["TABLE_NAME"]).ToList();

                    //  将DataTable中的数据导出到Excel工作表中
                    ExcelDbHelper.InsertDataTable(_conn, table, table.TableName + "$");
                }
            }
            else if ((type & InstrumentationType.非数值线测点集合) > 0)  // 此类测点的每一个子节点类型导出为一个表
            {
                // 每一类测点导出为多个表
                DataTable[] tables = Instrum_Line.ConvertToDatatables(elems.Select(r => r as Instrum_Line).ToList());

                for (int i = 0; i < tables.Length; i++)
                {
                    table = tables[i];

                    // 创建一个Excel工作表中
                    ExcelDbHelper.CreateNewSheet(_conn, table);

                    //  将DataTable中的数据导出到Excel工作表中
                    ExcelDbHelper.InsertDataTable(_conn, table, table.TableName + "$");
                }
            }
        }

        private void BackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelProgress.Text = e.ProgressPercentage + @"%";
            progressBar1.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 关联Excel数据库的连接
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
            if (!e.Cancelled)
            { // 不报错，则提交
                // 刷新界面
                Thread.Sleep(10);
                labelProgress.Text = @"100%";
                progressBar1.Value = 100;

                //
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            { // 报错则回滚
                DialogResult = DialogResult.Cancel;
            }
        }

        #endregion

    }
}

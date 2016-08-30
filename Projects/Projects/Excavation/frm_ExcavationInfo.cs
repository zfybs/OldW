using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using eZstd;
using Control = System.Windows.Forms.Control;
using View = Autodesk.Revit.DB.View;

namespace OldW.Excavation
{
    /// <summary>
    /// 提取并且操作基坑开挖的信息
    /// </summary>
    public partial class frm_ExcavationInfo : IExternalEventHandler
    {
        #region    ---   Declarations

        #region    ---   Types

        /// <summary>
        /// 每一个外部事件调用时所提出的需求
        /// </summary>
        private class RequestParameter
        {
            private object F_sender;

            /// <summary> 引发Form事件控件对象 </summary>
            public dynamic sender
            {
                get { return F_sender; }
            }

            private EventArgs F_e;

            /// <summary> Form中的事件所对应的事件参数 </summary>
            public EventArgs e
            {
                get { return F_e; }
            }

            private Request F_Id;

            /// <summary> 具体的需求 </summary>
            public Request Id
            {
                get { return F_Id; }
            }


            /// <summary>
            /// 定义事件需求与窗口中引发此事件的控件对象及对应的事件参数
            /// </summary>
            /// <param name="RequestId">具体的需求</param>
            /// <param name="e">Form中的事件所对应的事件参数</param>
            /// <param name="sender">引发Form事件控件对象</param>
            /// <remarks></remarks>
            public RequestParameter(Request RequestId, EventArgs e = null, object sender = null)
            {
                RequestParameter with_1 = this;
                with_1.F_sender = sender;
                with_1.F_e = e;
                with_1.F_Id = RequestId;
            }
        }

        /// <summary>
        /// ModelessForm的操作需求，用来从窗口向IExternalEventHandler对象传递需求。
        /// </summary>
        /// <remarks></remarks>
        private enum Request
        {
            /// <summary>
            /// 从模型中提取开挖土体的信息，并显示在列表中
            /// </summary>
            /// <remarks></remarks>
            GetExcavationInfo,

            /// <summary> 将列表中的某一个开挖土体的信息同步到Revit文档中的对应元素中去。 </summary>
            SynToElement,

            /// <summary> 将列表中选中的多个开挖土体的信息同步到Revit文档中的对应元素中去。 </summary>
            SynToMultipleElements,

            /// <summary> 设置选定图元在当前视图中的可见性 </summary>
            SetVisibility,

            /// <summary>
            /// 设置列表中选择的每一行所对应的开挖土体的可见性。
            /// </summary>
            SetMultiVisibility,

        }


        #endregion

        #region    ---   Properties

        private Soil_Model F_SoilModel;

        private Soil_Model SoilModel
        {
            get
            {
                if (F_SoilModel != null)
                {
                    return F_SoilModel;
                }
                else
                {
                    return ExcavDoc.FindSoilModel();
                }
            }
        }

        #endregion

        #region    ---   Fields

        /// <summary>用来触发外部事件（通过其Raise方法） </summary>
        /// <remarks>ExEvent属性是必须有的，它用来执行Raise方法以触发事件。</remarks>
        private ExternalEvent ExEvent;

        /// <summary> Execute方法所要执行的需求 </summary>
        /// <remarks>在Form中要执行某一个操作时，先将对应的操作需求信息赋值为一个RequestId枚举值，然后再执行ExternalEvent.Raise()方法。
        /// 然后Revit会在会在下个闲置时间（idling time cycle）到来时调用IExternalEventHandler.Excute方法，在这个Execute方法中，
        /// 再通过RequestId来提取对应的操作需求，</remarks>
        private RequestParameter RequestPara;

        public Document Document;
        public UIDocument UIDoc;
        public ExcavationDoc ExcavDoc;

        /// <summary> 用来与列表框进行交互的开挖土体信息集合 </summary>
        /// <remarks></remarks>
        private BindingList<ExcavSoilEntity> ExcavSoilInfos;

        #endregion

        #endregion

        #region    ---   构造函数与窗口的打开关闭

        /// <summary> 构造函数 </summary>
        /// <param name="ExcavDoc"></param>
        public frm_ExcavationInfo(ExcavationDoc ExcavDoc)
        {
            // This call is required by the designer.
            InitializeComponent();
            // Add any initialization after the InitializeComponent() call.
            //' ----------------------

            this.StartPosition = FormStartPosition.CenterScreen;

            //
            this.ExcavDoc = ExcavDoc;
            this.Document = ExcavDoc.Document;
            this.UIDoc = new UIDocument(Document);
            //
            SetupDatagridview();

            //' ------ 将所有的初始化工作完成后，执行外部事件的绑定 ----------------

            // 新建一个外部事件实例
            this.ExEvent = ExternalEvent.Create(this);
        }

        #endregion

        #region    ---   DataGridView控件的 构造

        /// <summary> 将列表框进行初始化 </summary>
        private void SetupDatagridview()
        {
            ExcavSoilInfos = new BindingList<ExcavSoilEntity>();

            this.DataGridView1.AutoGenerateColumns = false;
            this.DataGridView1.AllowUserToAddRows = false;
            this.DataGridView1.AllowUserToResizeRows = false;
            this.DataGridView1.AllowUserToResizeColumns = false;

            //-------- 将已有的数据源集合中的每一个元素的不同属性在不同的列中显示出来 -------

            this.DataGridView1.DataSource = ExcavSoilInfos;

            //-------- 为DataGridView控件中添加一列，此列与DataSource没有任何绑定关系 -------
            //Add an Unbound Column to a Data-Bound Windows Forms DataGridView Control
            // 注意，添加此列后，DataGridView.DataSource的值并不会发生改变。
            DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
            buttonColumn.HeaderText = "同步";
            buttonColumn.Name = "Sync";
            buttonColumn.Text = "同步";
            buttonColumn.Width = 50;
            // Use the Text property for the button text for all cells rather
            // than using each cell's Value as the text for its own button.
            buttonColumn.UseColumnTextForButtonValue = true;
            buttonColumn.ToolTipText = @"将此开挖土体单元的信息同步到Revit模型中。";
            DataGridView1.Columns.Add(buttonColumn);

            // Initialize and add a text box column.
            // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Id"; // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.HeaderText = "Id";
            column.Width = 60;
            column.ReadOnly = true;
            DataGridView1.Columns.Add(column);


            // Initialize and add a text box column.
            // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Name"; // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.HeaderText = @"名称";
            column.Resizable = DataGridViewTriState.True;
            column.Width = 100;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            DataGridView1.Columns.Add(column);

            // Initialize and add a text box column.
            // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Depth"; // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.HeaderText = @"深度(m)";
            column.Width = 100;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            DataGridView1.Columns.Add(column);

            // Initialize and add a text box column.
            // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "StartedDate"; // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.HeaderText = @"开始日期";
            DataGridView1.Columns.Add(column);

            // Initialize and add a text box column.
            // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "CompletedDate"; // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.HeaderText = @"完成日期";
            DataGridView1.Columns.Add(column);

            // Initialize and add a check box column.
            column = new DataGridViewCheckBoxColumn();
            column.Name = "Visible"; // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.DataPropertyName = "Visible"; // 此列所对应的数据源中的元素中的哪一个属性的名称
            column.HeaderText = @"可见";
            column.Width = 35;
            DataGridView1.Columns.Add(column);

            // 事件关联

            this.DataGridView1.CellContentClick += new DataGridViewCellEventHandler(this.DataGridView1_CellContentClick);
            DataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            this.DataGridView1.CellValueChanged += new DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.DataGridView1.DataError += new DataGridViewDataErrorEventHandler(this.DataGridView1_DataError);
        }

        /// <summary> Datagridview 中的每一行数据所对应的实体类 </summary>
        private class ExcavSoilEntity
        {
            /// <summary> 开挖土体对象 </summary>
            [Browsable(false)]
            public Soil_Excav Soil { get; set; }

            /// <summary> 土体单元的Id值 </summary>
            public ElementId Id { get; set; }

            /// <summary> 开挖土体的名称 </summary>
            public string Name { get; set; }

            /// <summary> 开挖土体的深度 </summary>
            public double Depth { get; set; }

            /// <summary> 开挖土体开挖完成的日期 </summary>
            public DateTime? StartedDate { get; set; }

            /// <summary> 开挖土体开挖完成的日期 </summary>
            public DateTime? CompletedDate { get; set; }

            /// <summary> 开挖土体在当前视图中是否可见 </summary>
            public bool Visible { get; set; }


            /// <summary>
            /// 将界面中设置的信息同步到Revit文档中
            /// </summary>
            /// <param name="tran"></param>
            /// <param name="View"></param>
            public void syncToDocument(Transaction tran, View View)
            {
                Soil.SetExcavatedDate(tran, true, StartedDate);
                Soil.SetExcavatedDate(tran, false, CompletedDate);
                Soil.SetName(tran, newName: Name);
                Soil.SetDepth(tran, depth: Depth);

            }

            /// <summary>
            /// 设置开挖土体的可见性
            /// </summary>
            /// <param name="tran"></param>
            /// <param name="show"> True代表设置其为可见 </param>
            /// <param name="view">当前视图对象</param>
            public void SetVisibility(Transaction tran, bool show, View view)
            {
                if (show)
                {
                    view.UnhideElements(new[] { Soil.Soil.Id });
                }
                else
                {
                    view.HideElements(new[] { Soil.Soil.Id });
                }
            }
        }

        #endregion

        #region    ---   DataGridView控件的 事件处理

        public void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = DataGridView1.Columns[e.ColumnIndex].Name;
            switch (colName)
            {
                case "Sync": // 将表格信息同步到Revit文档
                    if (e.RowIndex >= 0) // 说明不是点击的表头位置
                    {
                        this.RequestPara = new RequestParameter(Request.SynToElement, e, sender);
                        this.ExEvent.Raise();
                        //  Me.DozeOff()
                    }
                    break;
            }
        }

        public void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                //说明单元格中输入的数据类型不能转换为DataGridView中某列指定的ValueType类型。
                MessageBox.Show("数据类型转换出错！");
            }
            e.ThrowException = false;
        }

        #endregion

        #region    ---   触发 IExternalEventHandler.Execute 的操作

        #region    ---   通过复选框来设置开挖土体单元的显示与否

        void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // IsCurrentCellDirty属性：Gets a value indicating whether the current cell has uncommitted changes.
            if (DataGridView1.CurrentCell.GetType() == typeof(DataGridViewCheckBoxCell)
                && DataGridView1.IsCurrentCellDirty)
            {
                DataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // If a check box cell is clicked, this event handler disables   
        // or enables the button in the same row as the clicked cell. 
        public void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            string colName = DataGridView1.Columns[e.ColumnIndex].Name;
            switch (colName)
            {
                case "Visible": // 设置开挖土体的可见性
                    this.RequestPara = new RequestParameter(Request.SetVisibility, e, sender);
                    this.ExEvent.Raise();
                    // Me.DozeOff()
                    break;
            }
        }

        #endregion

        public void btnGetExcavInfo_Click(object sender, EventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.GetExcavationInfo, e, sender);
            this.ExEvent.Raise();
            // Me.DozeOff()
        }

        public void btn_SyncMultiple_Click(object sender, EventArgs e)
        {
            if (DataGridView1.SelectedRows.Count > 0)
            {
                this.RequestPara = new RequestParameter(Request.SynToMultipleElements, e, sender);
                this.ExEvent.Raise();
                this.DozeOff();
            }
        }


        public void CheckBox_MultiVisible_CheckedChanged(object sender, EventArgs e)
        {
            if (DataGridView1.SelectedRows.Count > 0)
            {
                this.RequestPara = new RequestParameter(Request.SetMultiVisibility, e, sender);
                this.ExEvent.Raise();
                //Me.DozeOff()
            }
        }

        #endregion

        #region    ---   执行操作 IExternalEventHandler.Execute

        /// <summary>
        /// 在执行ExternalEvent.Raise()方法之前，请先将操作需求信息赋值给其RequestHandler对象的RequestId属性。
        /// 当ExternalEvent.Raise后，Revit会在下个闲置时间（idling time cycle）到来时调用IExternalEventHandler.Execute方法的实现。
        /// </summary>
        /// <param name="app">此属性由Revit自动提供，其值不是Nothing，而是一个真实的UIApplication对象</param>
        /// <remarks>由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，
        /// 而是直接退出函数。所以要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。</remarks>
        public void Execute(UIApplication app)
        {
            try // 由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，而是直接退出函数。所以这里要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。
            {
                // 开始执行具体的操作
                switch (RequestPara.Id) // 判断具体要干什么
                {
                    case Request.GetExcavationInfo:
                        this.DataGridView1.DataSource = GetExcavSoilInfo();
                        break;

                    // ------------------------------------------------------------------------------------------------------------
                    case Request.SynToElement:
                        DataGridViewCellEventArgs e_1 = (DataGridViewCellEventArgs)RequestPara.e;
                        ExcavSoilEntity exsI_1 = (ExcavSoilEntity)DataGridView1.Rows[e_1.RowIndex].DataBoundItem;
                        using (Transaction t = new Transaction(Document, "将单个元素的信息从表格中同步到文档中"))
                        {
                            t.Start();
                            exsI_1.syncToDocument(t, UIDoc.ActiveView);
                            t.Commit();
                        }

                        break;


                    // ------------------------------------------------------------------------------------------------------------
                    case Request.SynToMultipleElements:
                        ExcavSoilEntity exsI_2 = default(ExcavSoilEntity);
                        using (Transaction t = new Transaction(Document, "将单个元素的信息从表格中同步到文档中"))
                        {
                            t.Start();
                            foreach (DataGridViewRow r in this.DataGridView1.SelectedRows)
                            {
                                exsI_2 = (ExcavSoilEntity)r.DataBoundItem;
                                exsI_2.syncToDocument(t, UIDoc.ActiveView);
                            }
                            t.Commit();
                        }

                        break;

                    // -------------------------------------------------------------------------------------------------------------
                    case Request.SetVisibility:
                        DataGridViewCellEventArgs e = (DataGridViewCellEventArgs)RequestPara.e;
                        ExcavSoilEntity exsI_3 = (ExcavSoilEntity)DataGridView1.Rows[e.RowIndex].DataBoundItem;

                        //
                        using (Transaction t = new Transaction(Document, "将设置单个元素的可见性"))
                        {
                            t.Start();
                            exsI_3.SetVisibility(tran: t, show: exsI_3.Visible, view: UIDoc.ActiveView);
                            t.Commit();
                        }

                        break;

                    // -------------------------------------------------------------------------------------------------------------
                    case Request.SetMultiVisibility:

                        using (Transaction tran = new Transaction(Document, "设置多个元素的可见性"))
                        {
                            tran.Start();

                            ExcavSoilEntity exsI = default(ExcavSoilEntity);

                            var v = this.CheckBox_MultiVisible.CheckState;
                            DataGridViewCheckBoxCell Ch;
                            foreach (DataGridViewRow r in this.DataGridView1.SelectedRows)
                            {
                                Ch = (DataGridViewCheckBoxCell)r.Cells["Visible"];
                                exsI = (ExcavSoilEntity)r.DataBoundItem;
                                if (v == CheckState.Checked)
                                {
                                    Ch.Value = true;
                                    exsI.SetVisibility(tran, true, UIDoc.ActiveView);
                                }
                                else if (v == CheckState.Unchecked)
                                {
                                    Ch.Value = false;
                                    exsI.SetVisibility(tran, false, UIDoc.ActiveView);
                                }
                            }
                            tran.Commit();
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("出错" + "\r\n" + ex.Message + "\r\n" + ex.TargetSite.Name + "\r\n" + ex.StackTrace,
                    "外部事件执行出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 刷新Form，将Form中的Controls的Enable属性设置为True
                this.WarmUp();
            }
        }

        /// <summary> 将模型中的开挖土体信息同步到列表中 </summary>
        private BindingList<ExcavSoilEntity> GetExcavSoilInfo()
        {
            BindingList<ExcavSoilEntity> ExcSoils = new BindingList<ExcavSoilEntity>();
            View V = new UIDocument(Document).ActiveView;
            if (V != null)
            {
                var Ses = this.ExcavDoc.FindExcavSoils(SoilModel);
                foreach (Soil_Excav es in Ses)
                {
                    ExcavSoilEntity EsI = new ExcavSoilEntity();
                    EsI.Soil = es;
                    EsI.Name = es.GetName();
                    EsI.Depth = es.GetDepth();
                    EsI.Id = es.Soil.Id;
                    EsI.StartedDate = es.GetExcavatedDate(true);
                    EsI.CompletedDate = es.GetExcavatedDate(false);
                    EsI.Visible = !es.Soil.IsHidden(V);
                    ExcSoils.Add(EsI);
                }
            }
            else
            {
                throw new NullReferenceException("当前视图不可用！");
            }
            return ExcSoils;
        }

        #endregion

        #region    ---   界面效果与 一般事件响应


        protected override void OnClosed(EventArgs e)
        {
            // 保存的实例需要进行释放
            this.ExEvent.Dispose();
            this.ExEvent = null;
            // 不关闭，只隐藏
            base.Hide();
        }

        public string GetName()
        {
            return "Revit External Event Example";
        }

        /// <summary> 在Revit执行相关操作时，禁用窗口中的控件 </summary>
        private void DozeOff()
        {
            foreach (Control c in this.Controls)
            {
                c.Enabled = false;
            }
        }

        /// <summary> 在外部事件RequestHandler中的Execute方法执行完成后，用来激活窗口中的控件 </summary>
        private void WarmUp()
        {
            foreach (Control c in this.Controls)
            {
                c.Enabled = true;
            }
        }

        public void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBox1.Checked)
            {
                this.DataGridView1.SelectAll();
            }
            else
            {
                this.DataGridView1.ClearSelection();
            }
        }

        #endregion

    }
}
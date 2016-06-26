using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using rvtTools;
using stdOldW;
using Control = System.Windows.Forms.Control;


namespace OldW.Excavation
{
    /// <summary>
    /// 无模态窗口的模板
    /// 此窗口可以直接通过Form.Show来进行调用
    /// </summary>
    /// <remarks></remarks>
    public partial class frm_DrawExcavation : IExternalEventHandler
    {
        #region    ---   Declarations

        #region    ---   Fields

        /// <summary>用来触发外部事件（通过其Raise方法） </summary>
        /// <remarks>ExEvent属性是必须有的，它用来执行Raise方法以触发事件。</remarks>
        private ExternalEvent ExEvent;

        /// <summary> Execute方法所要执行的需求 </summary>
        /// <remarks>在Form中要执行某一个操作时，先将对应的操作需求信息赋值为一个RequestId枚举值，然后再执行ExternalEvent.Raise()方法。
        /// 然后Revit会在会在下个闲置时间（idling time cycle）到来时调用IExternalEventHandler.Excute方法，在这个Execute方法中，
        /// 再通过RequestId来提取对应的操作需求，</remarks>
        private RequestParameter RequestPara;

        private Document Document;

        public ExcavationDoc ExcavDoc;

        /// <summary> 要绘制的模型的深度，单位为m </summary>
        private double Depth;

        /// <summary> 开挖土体开挖完成的日期 </summary>
        private Nullable<DateTime> CompletedDate;

        /// <summary> 开挖土体开始开挖的日期 </summary>
        private Nullable<DateTime> StartedDate;

        /// <summary>
        /// 为开挖土体或者模型墙体预设的名称
        /// </summary>
        private string DesiredName;

        #endregion

        #endregion

        #region    ---   构造函数与窗口的打开关闭

        public frm_DrawExcavation(ExcavationDoc ExcavDoc)
        {
            // This call is required by the designer.
            InitializeComponent();
            // Add any initialization after the InitializeComponent() call.
            //' ----------------------

            // Me.TopMost = True
            this.StartPosition = FormStartPosition.CenterScreen;

            // 参数绑定
            LabelCompletedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            btn__DateCalendar.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            TextBox_StartedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            TextBox_CompletedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            TextBox_SoilName.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            LabelSides.DataBindings.Add("Enabled", RadioBtn_Polygon, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            ComboBox_sides.DataBindings.Add("Enabled", RadioBtn_Polygon, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            btn_DrawCurves.DataBindings.Add("Enabled", RadioBtn_Draw, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            drawnCurveArrArr = null;
            //
            this.ExcavDoc = ExcavDoc;
            this.Document = ExcavDoc.Document;

            //' ------ 将所有的初始化工作完成后，执行外部事件的绑定 ----------------
            // 新建一个外部事件实例
            this.ExEvent = ExternalEvent.Create(this);
        }

        public void frm_DrawExcavation_Closed(object sender, EventArgs e)
        {
            // 保存的实例需要进行释放
            if (ModelCurvesDrawer.IsBeenUsed && this.ClosedCurveDrawer != null)
            {
                ClosedCurveDrawer.DrawingCompleted -= ClosedCurveDrawer_DrawingCompleted;
                // ClosedCurveDrawer.cancel()
                ClosedCurveDrawer = null;
                ClosedCurveDrawer.DrawingCompleted += ClosedCurveDrawer_DrawingCompleted;
            }
            //
            this.ExEvent.Dispose();
            this.ExEvent = null;
        }

        public string GetName()
        {
            return "绘制基坑的模型土体与开挖土体。";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        #endregion

        #region    ---   界面效果与事件响应

        /// <summary> 在Revit执行相关操作时，禁用窗口中的控件 </summary>
        private void DozeOff()
        {
            this.BtnModeling.Enabled = false;
        }

        /// <summary> 在外部事件RequestHandler中的Execute方法执行完成后，用来激活窗口中的控件 </summary>
        private void WarmUp()
        {
            foreach (Control c in this.Controls)
            {
                c.Enabled = true;
            }
        }

        public void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(TextBox_Depth.Text, out Depth))
            {
                TextBox_Depth.Text = "";
            }
        }

        #endregion

        #region    ---   执行操作 ExternalEvent.Raise 与 IExternalEventHandler.Execute

        // 绘制模型线
        public void btn_DrawCurves_Click(object sender, EventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.DrawCurves, e, sender);
            this.ExEvent.Raise();
            this.DozeOff();
        }

        //删除模型线
        public void Btn_ClearCurves_Click(object sender, EventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.DeleteCurves, e, sender);
            this.ExEvent.Raise();
            this.DozeOff();
        }

        // 建模
        public void BtnModeling_Click(object sender, EventArgs e)
        {
            bool blnDraw = CheckUI();
            if (blnDraw)
            {
                this.RequestPara = new RequestParameter(Request.StartModeling, e, sender);
                //
                this.ExEvent.Raise();
                this.DozeOff();
            }
            //
        }

        /// <summary>
        /// 对窗口中的数据进行检测，并判断是否可以进行绘制
        /// </summary>
        private bool CheckUI()
        {
            bool blnDraw = true;
            // 提取开挖深度
            if (this.Depth == 0)
            {
                MessageBox.Show("深度值不能为0。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string strDate = "";

            // 提取开始开挖的时间

            strDate = this.TextBox_StartedDate.Text;
            if (!string.IsNullOrEmpty(strDate))
            {
                if (DateTimeHelper.String2Date(strDate, ref this.StartedDate)) // 说明不能直接转化为日期
                {
                }
                else
                {
                    MessageBox.Show("请输入正确格式的开挖完成日期。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            strDate = this.TextBox_CompletedDate.Text;
            if (!string.IsNullOrEmpty(strDate))
            {
                if (DateTimeHelper.String2Date(strDate, ref this.CompletedDate)) // 说明不能直接转化为日期
                {
                    DesiredName = Convert.ToString(this.CompletedDate.Value.ToShortDateString());
                }
                else
                {
                    MessageBox.Show("请输入正确格式的开挖完成日期。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            // 是否有指定开挖土体的名称
            if (!string.IsNullOrEmpty(this.TextBox_SoilName.Text))
            {
                DesiredName = this.TextBox_SoilName.Text;
            }
            return blnDraw;
        }

        //'为每一项操作执行具体的实现
        /// <summary>
        /// 在执行ExternalEvent.Raise()方法之前，请先将操作需求信息赋值给其RequestHandler对象的RequestId属性。
        /// 当ExternalEvent.Raise后，Revit会在下个闲置时间（idling time cycle）到来时调用IExternalEventHandler.Execute方法的实现。
        /// </summary>
        /// <param name="app">此属性由Revit自动提供，其值不是Nothing，而是一个真实的UIApplication对象</param>
        /// <remarks>由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，
        /// 而是直接退出函数。所以要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。</remarks>
        public void Execute(UIApplication uiApp)
        {
            try // 由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，而是直接退出函数。所以这里要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。
            {
                UIDocument uiDoc = new UIDocument(Document);


                // 开始执行具体的操作
                switch (RequestPara.Id) // 判断具体要干什么
                {
                    case Request.DrawCurves:
                        // -------------------------------------------------------------------------------------------------------------------------

                        // 先确定模型土体或者开挖土体的轮廓曲线
                        drawnCurveArrArr = null;

                        // 绘制轮廓
                        this.ClosedCurveDrawer = new ClosedCurvesDrawer(uiApp, true, null);
                        this.ClosedCurveDrawer.DrawingCompleted += this.ClosedCurveDrawer_DrawingCompleted;
                        this.ClosedCurveDrawer.PostDraw();

                        // 由于 PostDraw 是异步操作，所以这里不能直接获得绘制好的模型线
                        // 而是要在 Drawer_DrawingCompleted 事件中来构造土体的轮廓线
                        return;

                    case Request.DeleteCurves:
                        if (drawnCurveArrArr != null)
                        {
                            // 删除模型线
                            using (Transaction t = new Transaction(Document, "删除绘制好的模型线。"))
                            {
                                List<ElementId> elemIds = new List<ElementId>();
                                t.Start();
                                try
                                {
                                    foreach (List<ElementId> ca in DrawnCurveIds)
                                    {
                                        foreach (ElementId cid in ca)
                                        {
                                            elemIds.Add(cid);
                                        }
                                    }
                                    Document.Delete(elemIds);
                                    t.Commit();
                                }
                                catch (Exception)
                                {
                                    t.RollBack();
                                }
                            }


                            // 清空数据
                            drawnCurveArrArr = null;
                        }
                        break;
                    case Request.StartModeling: // 通过 多边形的族样板 来直接放置土体模型

                        // 考虑不同的建模方式
                        // -------------------------------------------------------------------------------------------------------------------------
                        if (this.RadioBtn_Draw.Checked == true)
                        {
                            if (this.drawnCurveArrArr != null)
                            {
                                // 根据选择好的轮廓线来进行土体的建模
                                DrawSoilFromCurve(drawnCurveArrArr);
                            }
                            else
                            {
                                MessageBox.Show("请先绘制好要进行建模的封闭轮廓。");
                            }
                            // -------------------------------------------------------------------------------------------------------------------------
                        }
                        else if (this.RadioBtn_PickShape.Checked == true)
                        {
                            // 先确定模型土体或者开挖土体的轮廓曲线
                            CurveArrArray CurveArrArr = null;

                            // 选择轮廓
                            ClosedCurveSelector cs = new ClosedCurveSelector(uiDoc, true);
                            CurveArrArr = cs.SendSelect();

                            // 根据选择好的轮廓线来进行土体的建模
                            DrawSoilFromCurve(CurveArrArr);

                            // -------------------------------------------------------------------------------------------------------------------------
                        }
                        else if (this.RadioBtn_Polygon.Checked == true)
                        {
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowDebugCatch(ex, "外部事件执行出错", "出错");
            }
            finally
            {
                // 刷新Form，将Form中的Controls的Enable属性设置为True
                this.WarmUp();
            }
        }

        /// <summary>
        /// 根据绘制或者选择出来的土体轮廓模型线来进行模型土体或者开挖土体的建模
        /// </summary>
        /// <param name="CurveArrArr"></param>
        private void DrawSoilFromCurve(CurveArrArray CurveArrArr)
        {
            Soil_Model soil = default(Soil_Model);

            if (this.RadioBtn_ExcavSoil.Checked) // 绘制开挖土体
            {
                soil = this.ExcavDoc.FindSoilModel();
                Soil_Excav exc = this.ExcavDoc.CreateExcavationSoil(soil, this.Depth, CurveArrArr, DesiredName);

                // 设置开挖开始或者完成的时间

                using (Transaction tran = new Transaction(Document))
                {
                    try
                    {
                        tran.SetName("设置开挖土体的开始或者完成的时间");
                        tran.Start();

                        if ((this.StartedDate != null) || (this.CompletedDate != null))
                        {
                            if (this.StartedDate != null)
                            {
                                exc.SetExcavatedDate(tran, true, StartedDate.Value);
                            }
                            if (this.CompletedDate != null)
                            {
                                exc.SetExcavatedDate(tran, false, CompletedDate.Value);
                            }
                        }

                        // 将开挖土体在模型土体中隐藏起来
                        soil.RemoveSoil(tran, exc);

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.RollBack();
                        Utils.ShowDebugCatch(ex, $"事务“{tran.GetName()}”出错");
                    }
                }
            }
            else // 绘制模型土体
            {
                // 获得用来创建实体的模型线
                soil = this.ExcavDoc.CreateModelSoil(this.Depth, CurveArrArr);
            }
        }

        #endregion

        #region    ---   DrawCurves 绘制土体轮廓

        /// <summary>
        /// 用来绘制封闭的模型线
        /// </summary>
        private ClosedCurvesDrawer ClosedCurveDrawer;

        // 绘制好的模型线
        private List<List<ElementId>> DrawnCurveIds;
        private CurveArrArray F_drawnCurveArrArr;

        /// <summary>
        /// 通过界面绘制出来的模型线
        /// </summary>
        private CurveArrArray drawnCurveArrArr
        {
            get { return F_drawnCurveArrArr; }
            set
            {
                if (value == null)
                {
                    this.CheckBox_DrawSucceeded.Checked = false;
                    Btn_ClearCurves.Enabled = false;
                }
                else
                {
                    this.CheckBox_DrawSucceeded.Checked = true;
                    Btn_ClearCurves.Enabled = true;
                }
                F_drawnCurveArrArr = value;
            }
        }

        private void ClosedCurveDrawer_DrawingCompleted(List<List<ElementId>> AddedCurves, bool FinishedExternally,
            bool Succeeded)
        {
            if (Succeeded)
            {
                // 构造土体轮廓
                drawnCurveArrArr = new CurveArrArray();
                ModelCurve c = default(ModelCurve);
                //
                foreach (List<ElementId> cs in AddedCurves)
                {
                    CurveArray CurveArr = new CurveArray();
                    foreach (ElementId cid in cs)
                    {
                        c = (ModelCurve) this.Document.GetElement(cid);
                        CurveArr.Append(c.GeometryCurve);
                    }
                    // 必须将曲线集合进行重排以使其连续。
                    CurvesFormator.GetContiguousCurvesFromCurveArray(CurveArr, out CurveArr);
                    drawnCurveArrArr.Append(CurveArr);
                }
                DrawnCurveIds = AddedCurves;
            }
            else
            {
                drawnCurveArrArr = null;
                //
                this.WarmUp();
            }
        }

        #endregion
    }
}
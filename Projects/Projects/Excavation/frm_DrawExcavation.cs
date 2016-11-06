using System;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using eZstd;
using eZstd.Miscellaneous;
using RevitStd.Selector;
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
        private double _soilDepth;

        /// <summary> 开挖土体开挖完成的日期 </summary>
        private Nullable<DateTime> CompletedDate;

        /// <summary> 开挖土体开始开挖的日期 </summary>
        private Nullable<DateTime> StartedDate;

        /// <summary>
        /// 为开挖土体或者模型墙体预设的名称
        /// </summary>
        private string DesiredName;

        #endregion

        #region    ---   构造函数与窗口的打开关闭

        /// <summary> 构造函数 </summary>
        /// <param name="ExcavDoc"></param>
        public frm_DrawExcavation(ExcavationDoc ExcavDoc)
        {
            // This call is required by the designer.
            InitializeComponent();
            // Add any initialization after the InitializeComponent() call.
            //' ----------------------

            // Me.TopMost = True
            this.StartPosition = FormStartPosition.CenterScreen;

            // 用绘制开挖土体时 的控件参数绑定
            btn__DateCalendar.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            TextBox_StartedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            TextBox_CompletedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            TextBox_SoilName.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);

            // 用多边形模板作为土体轮廓时 的控件参数绑定
            LabelSides.DataBindings.Add("Enabled", RadioBtn_Polygon, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            ComboBox_sides.DataBindings.Add("Enabled", RadioBtn_Polygon, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);

            //
            this.ExcavDoc = ExcavDoc;
            this.Document = ExcavDoc.Document;

            //' ------ 将所有的初始化工作完成后，执行外部事件的绑定 ----------------
            // 新建一个外部事件实例
            this.ExEvent = ExternalEvent.Create(this);
        }


        public string GetName()
        {
            return "绘制基坑的模型土体与开挖土体。";
        }

        private void frm_DrawExcavation_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.CancelDraw, e, sender);
            this.ExEvent.Raise();

            //
            this.ExEvent.Dispose();
            this.ExEvent = null;
        }

        #region    ---   在绘制模型线的过程中禁止关闭窗口

        /// <summary> 禁止关闭窗口 </summary>
        private bool _forbiddenFormClosing;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_forbiddenFormClosing)
            {
                MessageBox.Show(@"请先结束当前界面操作。");
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
            base.OnClosing(e);
        }

        #endregion

        #endregion

        #region    ---   界面效果与事件响应

        /// <summary> 在Revit执行相关操作时，禁用窗口中的控件 </summary>
        /// <param name="req"> 当前正在执行的命令 </param>
        private void DozeOff(Request req)
        {
            // 通用性的禁用
            this.Btn_Modeling.Enabled = false; // 建模按钮
            DisableCurvesGenerators();

            // 
            switch (req)
            {
                case Request.DrawCurvesInUI:
                    {
                        Btn_CancelDrawCurves.Enabled = true;
                        break;
                    }
            }
        }

        private void DisableCurvesGenerators()
        {
            Btn_DrawCurves.Enabled = false;
            Btn_GetEdgeFromFace.Enabled = false;
            Btn_PickModelCurves.Enabled = false;
        }

        /// <summary> 在外部事件RequestHandler中的Execute方法执行完成后，用来激活窗口中的控件 </summary>
        private void WarmUp(Request req)
        {
            //foreach (Control c in this.Controls){c.Enabled = true;}
            // 
            Btn_DrawCurves.Enabled = true;
            Btn_PickModelCurves.Enabled = true;
            Btn_GetEdgeFromFace.Enabled = true;
            //
            switch (req)
            {
                case Request.DrawCurvesInUI:
                    {
                        Btn_CancelDrawCurves.Enabled = false;
                        break;
                    }
            }
            //
            _forbiddenFormClosing = false;
        }

        private void ChangeDrawingPermission(bool canDrawSoilNow)
        {

            this.CheckBox_DrawSucceeded.Checked = canDrawSoilNow;
            Btn_Modeling.Enabled = canDrawSoilNow;
            Btn_ClearCurves.Enabled = canDrawSoilNow;
        }

        private void RadioBtn_Polygon_CheckedChanged(object sender, EventArgs e)
        {
            Btn_Modeling.Enabled = RadioBtn_Polygon.Checked;
        }

        #endregion

        #region    ---   执行操作 ExternalEvent.Raise 的界面事件

        // 选择一个平面以绘制对应的平面轮廓，用来作为绘制土体的轮廓
        private void Btn_GetEdgeFromFace_Click(object sender, EventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.DrawCurvesFromFaceEdge, e, sender);
            this.ExEvent.Raise();
            this.DozeOff(Request.DrawCurvesFromFaceEdge);
        }

        private void Btn_PickModelCurves_Click(object sender, EventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.PickCurves, e, sender);
            this.ExEvent.Raise();
            this.DozeOff(Request.PickCurves);
        }

        // 绘制模型线
        public void btn_DrawCurves_Click(object sender, EventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.DrawCurvesInUI, e, sender);
            this.ExEvent.Raise();
            this.DozeOff(Request.DrawCurvesInUI);
        }

        //删除模型线
        public void Btn_ClearCurves_Click(object sender, EventArgs e)
        {
            this.RequestPara = new RequestParameter(Request.DeleteCurves, e, sender);
            this.ExEvent.Raise();
            this.DozeOff(Request.DeleteCurves);

        }

        // 建模
        public void BtnModeling_Click(object sender, EventArgs e)
        {
            var canDraw = CheckUiDataForDrawing();
            if (canDraw)
            {
                this.RequestPara = new RequestParameter(Request.StartModeling, e, sender);
                //
                this.ExEvent.Raise();
                this.DozeOff(Request.StartModeling);
            }
            //
        }

        // 取消模型线的绘制
        private void Btn_CancelDraw_Click(object sender, EventArgs e)
        {

            // MessageBox.Show(ExEvent.IsPending.ToString()); // 必定返回 false

            this.RequestPara = new RequestParameter(Request.CancelDraw, e, sender);
            //
            this.ExEvent.Raise();
            this.DozeOff(Request.CancelDraw);
        }


        /// <summary>
        /// 对窗口中的数据进行检测，并判断是否可以进行绘制
        /// </summary>
        private bool CheckUiDataForDrawing()
        {
            bool blnDraw = true;
            // 提取开挖深度
            _soilDepth = TextBox_Depth.ValueNumber;
            if (_soilDepth == 0)
            {
                MessageBox.Show("深度值不能为0。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string strDate = "";

            // 提取开始开挖的时间

            strDate = this.TextBox_StartedDate.Text;
            if (!string.IsNullOrEmpty(strDate))
            {
                StartedDate = DateTimeHelper.String2Date(strDate);
                if (StartedDate != null)
                {
                }
                else // 说明不能直接转化为日期
                {
                    MessageBox.Show("请输入正确格式的开挖完成日期。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            strDate = this.TextBox_CompletedDate.Text;
            if (!string.IsNullOrEmpty(strDate))
            {
                CompletedDate = DateTimeHelper.String2Date(strDate);
                if (CompletedDate != null)
                {
                    DesiredName = Convert.ToString(this.CompletedDate.Value.ToShortDateString());
                }
                else // 说明不能直接转化为日期
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

        #endregion

    }
}
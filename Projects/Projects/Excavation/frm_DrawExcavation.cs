using System;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using eZstd;
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
            Btn_DrawCurves.DataBindings.Add("Enabled", RadioBtn_Draw, "Checked", false,
                DataSourceUpdateMode.OnPropertyChanged);
            drawnCurveArrArr = null;
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
                MessageBox.Show(@"请先结束土体轮廓模型线的绘制");
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
        private void DozeOff()
        {
            this.BtnModeling.Enabled = false; // 建模按钮
            this.Btn_DrawCurves.Enabled = false; // 绘制模型线按钮
            this.Btn_ClearCurves.Enabled = false; // 删除模型线按钮
        }

        /// <summary> 在外部事件RequestHandler中的Execute方法执行完成后，用来激活窗口中的控件 </summary>
        private void WarmUp()
        {
            foreach (Control c in this.Controls)
            {
                c.Enabled = true;
            }
            // 
            Btn_DrawCurves.Enabled = true;
        }

        private void Btn_DrawCurves_EnabledChanged(object sender, EventArgs e)
        {
            Btn_CancelDraw.Enabled = !Btn_DrawCurves.Enabled;
        }
        #endregion

        #region    ---   执行操作 ExternalEvent.Raise 的界面事件

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

        // 取消模型线的绘制
        private void Btn_CancelDraw_Click(object sender, EventArgs e)
        {

            // MessageBox.Show(ExEvent.IsPending.ToString()); // 必定返回 false

            this.RequestPara = new RequestParameter(Request.CancelDraw, e, sender);
            //
            this.ExEvent.Raise();
            this.DozeOff();
        }

        /// <summary>
        /// 对窗口中的数据进行检测，并判断是否可以进行绘制
        /// </summary>
        private bool CheckUI()
        {
            bool blnDraw = true;
            // 提取开挖深度
            if (TextBox_Depth.ValueNumber == 0)
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
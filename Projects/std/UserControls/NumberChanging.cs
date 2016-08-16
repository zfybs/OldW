using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stdOldW.UserControls
{
    /// <summary> 时间跨度的单位 </summary>
    public enum TimeSpanUnit
    {
        Years = 0,
        Months = 1,
        Days = 2,
        Hours = 3,
        Minites = 4,
    }

    /// <summary>
    /// 用户控件，用来增加或减少指定的时间跨度值。
    /// </summary>
    /// <remarks></remarks>
    public partial class NumberChanging : UserControl
    {
        /// <summary> 构造函数 </summary>
        public NumberChanging()
        {
            InitializeComponent();
            //
            string[] names = Enum.GetNames(typeof(TimeSpanUnit));
            cbUnit.Items.Clear();
            cbUnit.Items.AddRange(names);
            cbUnit.SelectedIndex = 2;
            TextBoxNumber.Text = 1.ToString();

            // 
            TextBoxNumber.PositiveOnly = true;
        }

        #region   ---  Properties

        /// <summary> 控件中所对应的数值，即日期文本框上显示的用来进行日期值增减的跨度。
        /// 正值表示增加时间跨度，负值表示送去时间跨度。 </summary>
        [Browsable(true), DefaultValue(false), Category("数值"),
            Description("控件中所对应的数值，即日期文本框上显示的用来进行日期值增减的跨度")]
        public double ValueNumber
        {
            get
            {
                return TextBoxNumber.ValueNumber;
            }
        }

        private TimeSpanUnit _unit;
        /// <summary> 时间跨度的单位 </summary>
        [Browsable(false), DefaultValue(false), Category("数值"), Description("时间跨度的单位")]
        public TimeSpanUnit Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                cbUnit.SelectedText = Enum.GetName(Unit.GetType(), Unit);
                _unit = value;
            }
        }

        private bool _integerOnly;
        /// <summary> 是否只允许整数 </summary>
        [Browsable(true), DefaultValue(false), Category("数值"), Description("文本框中是否只允许输入整数")]
        public bool IntegerOnly
        {
            get { return _integerOnly; }
            set
            {
                TextBoxNumber.IntegerOnly = value;
                _integerOnly = value;
            }
        }

        #endregion

        #region   ---  events

        public event Action ValueAdded = delegate { };

        public event Action ValueMinused = delegate { };

        #endregion

        #region   ---  界面事件处理
        private void btnNext_Click_1(object sender, EventArgs e)
        {
            ValueAdded();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            ValueMinused();
        }

        private void cbUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._unit = (TimeSpanUnit)Enum.Parse(typeof(TimeSpanUnit), cbUnit.SelectedItem.ToString(), true);
        }
        #endregion

        #region   ---  时间跨度的增减

        /// <summary> 根据当前控件中所对应的时间跨度来进行时间的增减 </summary>
        public DateTime AddTimeSpan(DateTime originTime)
        {
            return ModifyTimeSpan(originTime, true);
        }
        /// <summary> 根据当前控件中所对应的时间跨度来进行时间的增减 </summary>
        public DateTime MinusTimeSpan(DateTime originTime)
        {
            return ModifyTimeSpan(originTime, false);
        }

        /// <summary> 根据当前控件中所对应的时间跨度来进行时间的增减 </summary>
        /// <param name="originTime"></param>
        /// <param name="add">如果为true，则增加时间跨度，如果为false，则减去时间跨度。</param>
        /// <returns></returns>
        private DateTime ModifyTimeSpan(DateTime originTime, bool add)
        {
            return Utils.GetTimeFromTimeSpan(originTime, (int)ValueNumber, Unit);
        }


        #endregion
        //
    }

}
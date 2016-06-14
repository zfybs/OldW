using System;
using System.Windows.Forms;

namespace OldW.DataManager
{
    /// <summary>
    /// 对测点的编号进行重命名
    /// </summary>
    public partial class FormReNameElement : Form
    {
        /// <summary>
        /// 测点的新名称
        /// </summary>
        public string MonitorName;

        public bool NameChanged = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oldName"></param>
        public FormReNameElement(string oldName)
        {
            InitializeComponent();
            KeyPreview = true;  // 优先于子控件而获得键盘事件
            textBoxName.Text = oldName;
            MonitorName = oldName;
            this.KeyDown += Form_KeyDown;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            MonitorName = textBoxName.Text;
            NameChanged = true;
            Close();
        }

        public void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                NameChanged = false;
                this.Close();
            }
        }
    }
}
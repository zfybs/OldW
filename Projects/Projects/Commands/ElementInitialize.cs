using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OldW.Commands
{
    public partial class ElementInitializer : Form
    {

        #region ---   测点单元的初始化属性

        /// <summary> 测点名称 </summary>
        public string MonitorName;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ElementInitializer(string name)
        {
            InitializeComponent();

            KeyPreview = true;
            KeyDown += OnKeyDown;

            //
            textBox_ElementName.Text = name;
        }

        #region ---   一般事件处理

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
              this.Close();
            }
        }

        private void ElementInitialize_Load(object sender, EventArgs e)
        {
            textBox_ElementName.Focus();
        }

        #endregion

        private void ElementInitializer_FormClosed(object sender, FormClosedEventArgs e)
        {
            MonitorName = textBox_ElementName.Text;
        }
    }
}

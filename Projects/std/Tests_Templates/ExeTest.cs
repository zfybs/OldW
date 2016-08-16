using System;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace stdOldW.Tests_Templates
{
    /// <summary> </summary>
    public class ExeTest
    {
        /// <summary> </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 f = new Form1();
            f.ShowDialog();
        }
    }
}

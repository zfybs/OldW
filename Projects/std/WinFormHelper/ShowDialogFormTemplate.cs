using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using stdOldW.WinFormHelper;

namespace stdOldW.WinFormHelper
{
    internal partial class ShowDialogFormTemplate : ShowDialogForm
    {
        public ShowDialogFormTemplate()
        {
            InitializeComponent();
            labelTime.Text = DateTime.Now.ToString("yyMMdd-hhmmss");
            //
        }

        #region ---   隐藏窗口并在 Revit 中选择任意单元

        private void button1_Click(object sender, EventArgs e)
        {

            base.HideAndOperate(new PickObjectProc(PickObject),
                new HideMethodReturnedProc(PickOperationReturned), null);

        }

        private delegate int PickObjectProc();

        private int index = 0;

        public int PickObject()
        {
            try
            {
                index += 1;
                //var refe = _uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "选择任意单元");
                //Element elem = _uiDoc.Document.GetElement(refe);

            }
            catch (Exception ex)
            {
            }
            return index;// elem.Id.IntegerValue;

        }

        private void PickOperationReturned(object e)
        {
            try
            {
              
                labelId.Text = e.ToString();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}

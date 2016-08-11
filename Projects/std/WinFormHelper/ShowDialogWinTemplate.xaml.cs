using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace stdOldW.WinFormHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class ShowDialogWinTemplate : ShowDialogWin
    {
        public ShowDialogWinTemplate()
        {
            InitializeComponent();
        }

        #region ---   隐藏窗口并在 Revit 中选择任意单元

        private void Button_Click_1(object sender, RoutedEventArgs e)
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
     
                labelId.Content = e.ToString();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}

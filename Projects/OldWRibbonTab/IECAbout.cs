// VBConversions Note: VB project level imports
using System.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.VisualBasic;
using System.Windows.Media;
using System.Collections;
using System;
using System.Linq;
// End of VB project level imports

using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

//using Autodesk.Revit;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using Autodesk.Revit.ApplicationServices;


namespace OldW
{

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class IECAbout : IExternalCommand
    {

        #region IExternalCommand Members

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string aboutString = "OldW 基坑施工工况与监测数据同步管理 \r\n" + "by 张威、曾凡云";
            TaskDialog.Show("About", aboutString);
            return Result.Succeeded;
        }

        #endregion
    }

}

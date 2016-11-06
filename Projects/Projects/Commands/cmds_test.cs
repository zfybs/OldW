using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitStd;
using eZstd;
using Application = Autodesk.Revit.ApplicationServices.Application;


namespace OldW.Commands
{

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_Test : IExternalCommand
    {
        private UIApplication uiApp;
        private UIDocument uiDoc;
        private Application App;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
       

            return Result.Succeeded;

        }

    }
}


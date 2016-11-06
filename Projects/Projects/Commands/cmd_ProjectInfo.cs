using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.Excavation;
using OldW.ProjectInfo;

namespace OldW.Commands
{
 /// <summary> 对项目信息进行设置与刷新 </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmd_ProjectInfo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            DllActivator.DllActivator_Projects dat = new DllActivator.DllActivator_Projects();
            dat.ActivateReferences();

            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            //
            OldWApplication WApp = OldWApplication.GetUniqueApplication(uiApp);
            OldWDocument WDoc = WApp.SearchOrCreateOldWDocument(doc); // OldWDocument.SearchOrCreate(WApp, doc);
           
            //
            frm_ProjectInfo frm = new frm_ProjectInfo(WDoc);
            frm.ShowDialog();
            
            return Result.Succeeded;
        }

    }

}

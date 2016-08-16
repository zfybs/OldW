using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace OldW.GlobalSettings
{
    public class Operations
    {
        /// <summary>
        /// 将程序插件中的所有面板禁用
        /// </summary>
        public static void DeActivateControls(UIApplication uiApp)
        {
            var Panels = uiApp.GetRibbonPanels(Constants.AppName);
            foreach (RibbonPanel p in Panels)
            {
                p.Enabled = false;
            }
        }

        /// <summary>
        /// 将程序插件中的所有面板激活
        /// </summary>
        public static void ActivateControls(UIApplication uiApp)
        {
            var Panels = uiApp.GetRibbonPanels(Constants.AppName);
            foreach (RibbonPanel p in Panels)
            {
                p.Enabled = true;
            }
        }


        /// <summary>
        /// 获取OldW插件中指定名称的按钮或者文本框等控件
        /// </summary>
        /// <param name="uiApp"></param>
        /// <param name="panelName"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public static RibbonItem GetRibbonItem(UIApplication uiApp, string panelName, string itemName)
        {
            var panels = uiApp.GetRibbonPanels("OldW");
            if (panels == null) return null;
            //
            var panel = panels.FirstOrDefault(r => r.Name == panelName);
            if (panel == null) return null;
            //
            RibbonItem item = panel.GetItems().FirstOrDefault(r => r.Name == itemName);

            return item;
        }
    }
}
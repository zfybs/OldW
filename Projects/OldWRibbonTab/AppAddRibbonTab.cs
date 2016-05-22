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

using System.IO;
using Autodesk.Revit.UI;

//using Autodesk.Revit;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using System.IO.Directory;

namespace OldW
{

    // -------------------------- OldWApplication.addin -----------------------------------------
    // <?xml version="1.0" encoding="utf-8"?>
    // <RevitAddIns>
    //   <AddIn Type="Application">
    //     <Name>ExternalApplication</Name>
    //     <Assembly>F:\Software\Revit\RevitDevelop\OldW\bin\OldWApplication.dll</Assembly>
    //     <ClientId>32df6eb7-7dbc-45ff-b86e-b9c8bf0f8185</ClientId>
    //     <FullClassName>OldW.AppAddRibbonTab</FullClassName>
    //     <VendorId>OldW</VendorId>
    //     <VendorDescription>http://naoce.sjtu.edu.cn/</VendorDescription>
    //   </AddIn>
    //
    // </RevitAddIns>
    // ------------------------------------------------------------------------------------------

    public class AppAddRibbonTab : IExternalApplication
    {
        public AppAddRibbonTab()
        {
            // VBConversions Note: Non-static class variable initialization is below.  Class variables cannot be initially assigned non-static values in C#.
            Path_Dlls = (new Microsoft.VisualBasic.ApplicationServices.ConsoleApplicationBase()).Info.DirectoryPath;
            Path_icons = Path.Combine(new DirectoryInfo(Path_Dlls).Parent.FullName, "Resources\\icons");
        }

        #region   ---  文件路径

        /// <summary> Application的Dll所对应的路径，也就是“bin”文件夹的目录。 </summary>
        private string Path_Dlls; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.
                                  /// <summary> 存放图标的文件夹 </summary>
        private string Path_icons; // VBConversions Note: Initial value cannot be assigned here since it is non-static.  Assignment has been moved to the class constructors.

        #endregion

        #region   ---  常数

        /// <summary> 整个程序的标志性名称 </summary>
        private const string AppName = "OldW";

        /// <summary> Projects 项目的Dll的名称 </summary>
        private const string Dll_Projects = "Projects.dll";

        /// <summary> 本程序集的Dll的名称 </summary>
        private const string Dll_RibbonTab = "OldWRibbonTab.dll";

        #endregion

        /// <summary> Ribbon界面设计 </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            //Create a custom ribbon tab
            string tabName = AppName;
            application.CreateRibbonTab(tabName);

            // 建模面板
            RibbonPanel ribbonPanelModeling = application.CreateRibbonPanel(tabName, "开挖");
            AddPushButtonExcavation(ribbonPanelModeling);
            AddPushButtonExcavationInfo(ribbonPanelModeling);

            // 监测数据面板
            RibbonPanel ribbonPanelData = application.CreateRibbonPanel(tabName, "监测");
            AddSplitButtonModeling(ribbonPanelData);
            AddPushButtonDataEdit(ribbonPanelData);

            // 查看面板
            RibbonPanel ribbonPanelView = application.CreateRibbonPanel(tabName, "查看");
            AddTextBoxCurrentDate(ribbonPanelView);
            AddPushButtViewStage(ribbonPanelView);

            // 分析面板
            RibbonPanel ribbonPanelAnalysis = application.CreateRibbonPanel(tabName, "分析");
            AddPushButtonSetWarning(ribbonPanelAnalysis);
            AddPushButtonAnalysis(ribbonPanelAnalysis);

            // 关于面板
            RibbonPanel ribbonPanelAbout = application.CreateRibbonPanel(tabName, "关于");
            AddPushButtonAbout(ribbonPanelAbout); //添加关于
                                                  //

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        #region   ---  添加按钮  （如果LargeImage所对应的图片不能在Ribbon中显示，请尝试先下载128*128的，然后通过画图工具将其大小调整为32*32.）

        /// <summary> 添加“关于”的下拉记忆按钮 </summary>
        private void AddPushButtonAbout(RibbonPanel panel)
        {
            // Create a new push button
            PushButton pushButton = panel.AddItem(new PushButtonData("About", "关于", Path.Combine(Path_Dlls, Dll_RibbonTab), "OldW.IECAbout")) as PushButton;
            // Set ToolTip
            pushButton.ToolTip = "关于信息";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pushButton.SetContextualHelp(contextHelp);
            // Set Icon
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "About-32.png")));
        }

        /// <summary> 添加“放置监测点”的下拉记忆按钮 </summary>
        /// <param name="panel"> 目标RibbonPanel </param>
        private void AddSplitButtonModeling(RibbonPanel panel)
        {
            // 创建一个SplitButton
            SplitButtonData splitButtonData = new SplitButtonData("ModelingMonitor", "监测建模");
            SplitButton splitButton = panel.AddItem(splitButtonData) as SplitButton;

            // 创建一个沉降pushButton加到SplitButton的下拉列表里
            PushButton pushButton = splitButton.AddPushButton(new PushButtonData("ModelingSettlement", "沉降", Path.Combine(Path_Dlls, Dll_Projects), "OldW.Commands.cmd_SetFamilySettlement"));
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "MonitorSet-32.png"))); //36*36的大小
            pushButton.Image = new BitmapImage(new Uri(Path.Combine(Path_icons, "MonitorSet-16.png")));
            pushButton.ToolTip = "放置沉降测点模型";

            // 创建一个轴力pushButton加到SplitButton的下拉列表里
            pushButton = splitButton.AddPushButton(new PushButtonData("ModelingForce", "轴力", Path.Combine(Path_Dlls, Dll_Projects), "OldW.Commands.cmd_SetFamilyForce"));
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "MonitorForce-32.png"))); //36*36的大小
            pushButton.Image = new BitmapImage(new Uri(Path.Combine(Path_icons, "MonitorForce-16.png")));
            pushButton.ToolTip = "放置轴力测点模型";

            // 创建一个测斜pushButton加到SplitButton的下拉列表里
            pushButton = splitButton.AddPushButton(new PushButtonData("ModelingIncli", "测斜", Path.Combine(Path_Dlls, Dll_Projects), "OldW.Commands.cmd_SetFamilyIncli"));
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "MonitorIncli-32.png"))); //36*36的大小
            pushButton.Image = new BitmapImage(new Uri(Path.Combine(Path_icons, "MonitorIncli-16.png")));
            pushButton.ToolTip = "放置测斜测点模型";
        }

        /// <summary> 添加“测点数据编辑”的下拉记忆按钮 </summary>
        private void AddPushButtonDataEdit(RibbonPanel panel)
        {
            // Create a new push button
            string str = Path.Combine(Path_Dlls, Dll_Projects);
            PushButton pushButton = panel.AddItem(new PushButtonData("DataEdit", "导入数据", str, "OldW.Commands.cmd_DataEdit")) as PushButton;
            // Set ToolTip
            pushButton.ToolTip = "导入到导出监测数据";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pushButton.SetContextualHelp(contextHelp);
            // Set Icon
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "DataEdit-32.png")));
        }

        /// <summary> 添加“警戒值设定”的下拉记忆按钮 </summary>
        private void AddPushButtonSetWarning(RibbonPanel panel)
        {
            // Create a new push button
            string str = Path.Combine(Path_Dlls, Dll_Projects);
            PushButton pushButton = panel.AddItem(new PushButtonData("SetWarning", "警戒值设定", str, "OldW.Commands.cmd_SetWarning")) as PushButton;
            // Set ToolTip
            pushButton.ToolTip = "警戒值设定";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pushButton.SetContextualHelp(contextHelp);
            // Set Icon
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "SetWarning-32.png")));
        }

        /// <summary> 添加“分析”的下拉记忆按钮 </summary>
        private void AddPushButtonAnalysis(RibbonPanel panel)
        {
            // Create a new push button
            PushButton pushButton = panel.AddItem(new PushButtonData("Analysis", "分析", Path.Combine(Path_Dlls, Dll_Projects), "OldW.Commands.cmd_Analyze")) as PushButton;
            // Set ToolTip
            pushButton.ToolTip = "警戒值分析";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pushButton.SetContextualHelp(contextHelp);
            // Set Icon
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "Analysis-32.png")));
        }

        /// <summary> 添加“开挖”的下拉记忆按钮 </summary>
        private void AddPushButtonExcavation(RibbonPanel panel)
        {
            // Create a new push button
            PushButton pushButton = panel.AddItem(new PushButtonData("Excavation", "开挖", Path.Combine(Path_Dlls, Dll_Projects), "OldW.Commands.cmd_Excavation")) as PushButton;
            // Set ToolTip
            pushButton.ToolTip = "基坑开挖与回筑";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pushButton.SetContextualHelp(contextHelp);
            // Set Icon
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "Excavation-32.png"))); // "Excavation-32.png"

        }


        /// <summary> 添加“开挖信息”的下拉记忆按钮 </summary>
        private void AddPushButtonExcavationInfo(RibbonPanel panel)
        {
            // Create a new push button
            PushButton pushButton = panel.AddItem(new PushButtonData("ExcavationInfo", "开挖信息", Path.Combine(Path_Dlls, Dll_Projects), "OldW.Commands.cmd_ExcavationInfo")) as PushButton;
            pushButton.ToolTip = "提取模型中的基坑开挖模型土体与开挖土体的信息。";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pushButton.SetContextualHelp(contextHelp);
            // Set Icon
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "ExcavationInfo-32.png"))); // "Excavation-32.png"

        }

        /// <summary> 添加“当前时间”的文本框 </summary>
        private void AddTextBoxCurrentDate(RibbonPanel panel)
        {
            // Create a new push button
            TextBox TextBox = panel.AddItem(new TextBoxData("CurrentDate")) as TextBox;

            // Set Icon
            TextBox.ToolTip = "当前时间，用来查看开挖情况与显示对应的监测数据。可以精确到分钟。";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            TextBox.SetContextualHelp(contextHelp);
            // Set Icon
            TextBox.Image = new BitmapImage(new Uri(Path.Combine(Path_icons, "ViewStage-16.png"))); // "Excavation-32.png"
            TextBox.Width = 100;
            TextBox.Value = DateTime.Today.ToShortDateString();
            TextBox.SelectTextOnFocus = true;
        }

        /// <summary> 添加“查看开挖工况”的下拉记忆按钮 </summary>
        private void AddPushButtViewStage(RibbonPanel panel)
        {
            // Create a new push button
            PushButton pushButton = panel.AddItem(new PushButtonData("ViewStage", "开挖工况", Path.Combine(Path_Dlls, Dll_Projects), "OldW.Commands.cmd_ViewStage")) as PushButton;
            pushButton.ToolTip = "查看指定日期的开挖工况。";
            // Set Contextual help
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pushButton.SetContextualHelp(contextHelp);
            // Set Icon
            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(Path_icons, "ViewStage-32.png"))); // "Excavation-32.png"
            
        }
        
        #endregion
        }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forms=System.Windows.Forms;
using Autodesk.Revit.DB;

namespace OldW.ProjectInfo
{
    /// <summary>
    /// 与整个基坑相关的信息设置
    /// </summary>
    public partial class frm_ProjectInfo : Forms.Form
    {
        #region    ---   Fields
        private readonly OldWDocument _oldWDoc;
        private readonly OldWProjectInfo _projectInfo;

        #endregion

        #region    ---   构造函数与窗口的加载关闭
        /// <summary> 构造函数 </summary>
        public frm_ProjectInfo(OldWDocument oldWDoc)
        {
            InitializeComponent();
            //
            _oldWDoc = oldWDoc;
            _projectInfo = oldWDoc.GetProjectInfo();
            //
            LoadProjectInfo(_projectInfo);

        }
        #endregion

        #region    ---   项目信息数据 projectInfo 与界面的交互

        /// <summary> 将Revit文档中保存的项目信息加载到UI界面中 </summary>
        private void LoadProjectInfo(OldWProjectInfo projectInfo)
        {
            dateTimePicker_excavStart.Value = projectInfo.ExcavStart;
            dateTimePicker_excavFinish.Value = projectInfo.ExcavFinish;
        }

        private void SaveProjectInfoToDoc()
        {
            OldWProjectInfo projectInfo = GetProjectInfoFromUI();
            // 将刷新后的项目信息保存到Revit文档中

            using (Transaction tran = new Transaction(_oldWDoc.Document, "将OldW项目信息保存到Document中"))
            {
                try
                {
                    tran.Start();
                    _oldWDoc.SetProjectInfo(tran, projectInfo);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.RollBack();
                }
            }
        }

        private OldWProjectInfo GetProjectInfoFromUI()
        {
            // 对 _projectInfo 中的某些项进行刷新
            _projectInfo.ExcavStart = dateTimePicker_excavStart.Value;
            _projectInfo.ExcavFinish = dateTimePicker_excavFinish.Value;

            //
            return _projectInfo;
        }

        #endregion

        #region    ---   界面操作

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveProjectInfoToDoc();
        }
        #endregion
    }
}

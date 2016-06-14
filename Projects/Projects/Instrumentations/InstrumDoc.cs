using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using rvtTools;
using stdOldW.WinFormHelper;
using Forms = System.Windows.Forms;

namespace OldW.Instrumentations
{
    /// <summary> 用来执行基坑中的测点布置，监测数据管理等操作 </summary>
    /// <remarks></remarks>
    public class InstrumDoc : OldWDocument
    {

        #region    ---   Types

        #endregion

        #region    ---   Fields

        private Application App;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="OldWDoc"></param>
        public InstrumDoc(OldWDocument OldWDoc) : base(OldWDoc.Document)
        {
            this.App = Document.Application;
        }

        /// <summary>
        /// 放置测点
        /// </summary>
        /// <param name="monitorType"> 要放置的测点类型 </param>
        /// <returns></returns>
        public void SetFamily(InstrumentationType monitorType)
        {
            UIDocument uidoc = new UIDocument(Document);
            ElementId foundId = null;

            //是否存在族
            Boolean found = FilterTools.existFamliyByName(uidoc.Document, monitorType.ToString(), out foundId);

            Family family = null;
            if (found == true)
            {
                //如果存在，获得文件该族
                family = uidoc.Document.GetElement(foundId) as Family;
            }
            else
            {
                {
                    //如果不存在，载入族
                    using (Transaction trans = new Transaction(uidoc.Document, "trans"))
                    {
                        try
                        {
                            trans.Start();
                            uidoc.Document.LoadFamily(Path.Combine(ProjectPath.Path_family,
                                monitorType.ToString() + ".rfa"), out family);
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
            }

            //获得该族的族类型，并且放置族实例
            FamilySymbol symbol = uidoc.Document.GetElement(family.GetFamilySymbolIds().ElementAt(0)) as FamilySymbol;
            uidoc.PostRequestForElementTypePlacement(symbol);
        }

        #region    ---   组合列表框 ComboBox 的操作

        /// <summary>
        /// 将指定的测点集合对象转换为组合列表框Combox控件的DataSource类型。
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <param name="comboboxControl"> ComboBox 控件对象或者 DataGridViewComboBoxCell 控件对象  </param>
        /// <returns> </returns>
        public void FillCombobox(IEnumerable<Instrumentation> elementCollection, object comboboxControl)
        {
            List<LstbxValue<Instrumentation>> arr = GetComboboxDatasource(elementCollection);

            if (comboboxControl.GetType() == typeof(Forms.ComboBox))
            {
                Forms.ComboBox cmb = (Forms.ComboBox)comboboxControl;
                // ComboBox设置
                cmb.DataSource = null;
                cmb.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
                cmb.ValueMember = LstbxValue<Instrumentation>.ValueMember;
                cmb.DataSource = arr;
            }

            if (comboboxControl.GetType() == typeof(Forms.DataGridViewComboBoxCell))
            {
                Forms.DataGridViewComboBoxCell cmb = (Forms.DataGridViewComboBoxCell)comboboxControl;

                // ComboBox设置
                cmb.DataSource = null;
                cmb.DisplayMember = LstbxValue<Instrumentation>.DisplayMember;
                cmb.ValueMember = LstbxValue<Instrumentation>.ValueMember;
                cmb.DataSource = arr;
            }

        }

        /// <summary>
        /// 将指定的测点集合对象转换为组合列表框Combox控件的DataSource类型。
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <returns> </returns>
        public List<LstbxValue<Instrumentation>> GetComboboxDatasource(IEnumerable<Instrumentation> elementCollection)
        {
            return elementCollection.Select(eleid =>
            new LstbxValue<Instrumentation>(DisplayedText: eleid.Monitor.Name + "( " + eleid.getMonitorName() + " ):" + eleid.Monitor.Id.IntegerValue,
            Value: eleid)).ToList();
        }

        #endregion

    }
}


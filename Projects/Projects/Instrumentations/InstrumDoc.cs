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

        /// <summary>
        /// 将不同的Excel工作表字段的名称映射到Revit对应的测点中去
        /// </summary>
        public struct InstrumTypeMapping
        {
            private const string SheetGroundHeave = "地表隆沉";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="excelSheetName"> Excel工作表的名称，不包含后缀$</param>
            /// <returns></returns>
            public static Instrumentation MapToType(string excelSheetName)
            {
                return null;
            }

            /// <summary>
            /// 根据给出的字段名匹配出对应的测点编号。比如由CX02
            /// </summary>
            /// <param name="type"></param>
            /// <param name="excelFieldName"></param>
            /// <returns></returns>
            public static int MapToNumber(InstrumentationType type, string excelFieldName)
            {
                return 1;
            }

            /// <summary>
            /// 给出的Excel工作表名称是代表一类测点（表中的每一个字段代表一个测点），
            /// 还是一个测点（比如测斜管，表中的每一个字段代表此测点的子节点）
            /// </summary>
            /// <param name="excelSheetName"></param>
            /// <returns> 如果此工作表名称代表一类测点，比如“地表隆沉”，则返回true；
            /// 如果此工作表就代表一个测斜测点，则返回false。 </returns>
            public static bool IsMonitorType(string excelSheetName)
            {
                if (excelSheetName.StartsWith("CX", StringComparison.OrdinalIgnoreCase) ||
                    excelSheetName.StartsWith("TX", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            }

        }

        /// <summary>
        /// 测点收集器，用来对测点集合进行分类管理
        /// </summary>
        public struct InstrumCollector
        {

            /// <summary> 所有的测点的集合 </summary>
            List<Instrumentation> _allInstrumentations;
            /// <summary> 所有的测点的集合 </summary>
            public List<Instrumentation> AllInstrumentations
            {
                get { return _allInstrumentations; }
            }

            #region ---   不同的测点集合 

            /// <summary> 立柱隆沉测点 </summary>
            public readonly List<Instrum_ColumnHeave> ColumnHeave;

            /// <summary> 地表隆沉测点 </summary>
            public readonly List<Instrum_GroundSettlement> GroundSettlement;

            /// <summary> 测斜点 </summary>
            public readonly List<Instrum_Incline> Incline;

            /// <summary> 支撑轴力点 </summary>
            public readonly List<Instrum_StrutAxialForce> StrutAxialForce;

            #endregion

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="instrums"> 要进行测点分类的测点集合 </param>
            public InstrumCollector(IEnumerable<Instrumentation> instrums)
            {
                ColumnHeave = new List<Instrum_ColumnHeave>();
                GroundSettlement = new List<Instrum_GroundSettlement>();
                Incline = new List<Instrum_Incline>();
                StrutAxialForce = new List<Instrum_StrutAxialForce>();
                //
                _allInstrumentations = new List<Instrumentation>();


                // 
                Truncate(instrums);
            }

            /// <summary>
            /// 清空原测点集合中的元素，并重新添加新的元素
            /// </summary>
            /// <param name="instrums"></param>
            public void Truncate(IEnumerable<Instrumentation> instrums)
            {
                ColumnHeave.Clear();
                GroundSettlement.Clear();
                Incline.Clear();
                StrutAxialForce.Clear();
                //
                _allInstrumentations.Clear();

                Append(instrums);
            }

            /// <summary>
            /// 直接向集合中附加新的测点
            /// </summary>
            /// <param name="instrums"></param>
            public void Append(IEnumerable<Instrumentation> instrums)
            {
                foreach (Instrumentation inst in instrums)
                {
                    if (inst is Instrum_ColumnHeave)
                    {
                        ColumnHeave.Add((Instrum_ColumnHeave)inst);
                    }
                    else if (inst is Instrum_GroundSettlement)
                    {
                        GroundSettlement.Add((Instrum_GroundSettlement)inst);
                    }
                    else if (inst is Instrum_Incline)
                    {
                        Incline.Add((Instrum_Incline)inst);
                    }
                    else if (inst is Instrum_StrutAxialForce)
                    {
                        StrutAxialForce.Add((Instrum_StrutAxialForce)inst);
                    }
                }
                _allInstrumentations.AddRange(instrums);

            }

            #region ---   过滤出线测点 

            /// <summary> 过滤出集合中所有的线测点 </summary>
            public List<Instrum_Line> GetLineMonitors()
            {
                List<Instrum_Line> lines = new List<Instrum_Line>();
                lines.AddRange(Incline);
                return lines;
            }


            /// <summary> 按指定的选项过滤出集合中所有的线测点 </summary>
            /// <param name="incline"> 是否包含立柱隆沉测点 </param>
            public List<Instrum_Line> GetLineMonitors(bool incline)
            {
                List<Instrum_Line> lines = new List<Instrum_Line>();
                if (incline)
                {
                    lines.AddRange(this.Incline);
                }
                return lines;
            }

            #endregion

            #region ---   过滤出点测点 

            /// <summary> 过滤出集合中所有的线测点 </summary>
            public List<Instrum_Point> GetPointMonitors()
            {
                List<Instrum_Point> points = new List<Instrum_Point>();

                points.AddRange(ColumnHeave);
                points.AddRange(GroundSettlement);
                points.AddRange(StrutAxialForce);
                return points;
            }

            /// <summary> 按指定的选项过滤出集合中所有的点测点 </summary>
            /// <param name="columnHeave"> 是否包含立柱隆沉测点 </param>
            /// <param name="groundSettlement"> 是否包含地表隆沉测点 </param>
            /// <param name="strutAxialForce"> 是否包含支持轴力测点 </param>
            public List<Instrum_Point> GetPointMonitors(bool columnHeave, bool groundSettlement, bool strutAxialForce)
            {
                List<Instrum_Point> points = new List<Instrum_Point>();
                if (columnHeave)
                {
                    points.AddRange(this.ColumnHeave);
                }
                if (groundSettlement)
                {
                    points.AddRange(this.GroundSettlement);
                }
                if (strutAxialForce)
                {
                    points.AddRange(this.StrutAxialForce);
                }
                return points;
            }

            #endregion

        }

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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using RevitStd;
using eZstd.UserControls;
using Form = System.Windows.Forms.Form;

namespace OldW.Instrumentations.MonitorSetterGetter
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ChooseFamilySymbol : Form
    {
        #region    ---   Properties
        private FamilySymbol _ChoosedSymbol;
        /// <summary> 用户在界面中选择的那一个族类型，如果未成功选择，则默认为集合中的第一个族类型。 </summary>
        public FamilySymbol Symbol { get { return _ChoosedSymbol; } }

        #endregion

        #region    ---   Fields

        private readonly List<FamilySymbol> _symbols;
        // private readonly Document _doc;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="doc"></param>
        /// <param name="symbolIds"></param>
        public ChooseFamilySymbol(Document doc, IEnumerable<ElementId> symbolIds)
        {
            InitializeComponent();
            // 
            // _doc = doc;

            // 族类型集合的初始化
            _symbols = new List<FamilySymbol>();
            List<ListControlValue<FamilySymbol>> symboLstbxValue = new List<ListControlValue<FamilySymbol>>();
            foreach (ElementId sId in symbolIds)
            {
                FamilySymbol fs = doc.GetElement(sId) as FamilySymbol;
                _symbols.Add(fs);
                symboLstbxValue.Add(new ListControlValue<FamilySymbol>(fs.Name, fs));
            }
            if (_symbols.Any())
            {
                _ChoosedSymbol = _symbols.First();
            }
            else
            {
                throw new NullReferenceException("集合中未包含任何族类型。");
            }

            //
            var monitorFamily = _ChoosedSymbol.Family;
            labelInfo.Text = @"    族 “ " + monitorFamily.Name + " ” 中有多个族类型，\n\r 请选择其中的一个以进行测点的放置";

            // 将集合中的元素添加到ComboBox中
            comboBoxSymbols.ValueMember = ListControlValue<FamilySymbol>.ValueMember;
            comboBoxSymbols.DisplayMember = ListControlValue<FamilySymbol>.DisplayMember;
            comboBoxSymbols.DataSource = symboLstbxValue;
        }

        private void comboBoxSymbols_SelectedIndexChanged(object sender, EventArgs e)
        {
            var s = comboBoxSymbols.SelectedValue as FamilySymbol;
            _ChoosedSymbol = s;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

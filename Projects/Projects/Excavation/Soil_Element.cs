using System;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using RevitStd;

namespace OldW.Excavation
{
    /// <summary>
    /// 土体单元对象。一个土体单元的族实例，必须满足的条件有：1. 族的名称限制；2. 实例类别为“场地”。
    /// </summary>
    /// <remarks></remarks>
    public abstract class Soil_Element
    {
        #region    ---   Properties

        /// <summary> 模型土体或者开挖土体单元所对应的族实例对象 </summary>
        public FamilyInstance Soil { get; }

        /// <summary> 基坑开挖文档 </summary>
        public ExcavationDoc ExcavDoc { get; }

        /// <summary> 土体单元所在的 Revit 文档 </summary>
        public Document Document { get; }

        #endregion

        #region    ---   构造函数，通过 OldWDocument.GetSoilModel，或者是Create静态方法

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="excavDoc"></param>
        /// <param name="soilElement"></param>
        protected Soil_Element(ExcavationDoc excavDoc, FamilyInstance soilElement)
        {
            if (soilElement != null && soilElement.IsValidObject)
            {
                this.ExcavDoc = excavDoc;

                this.Soil = soilElement;
                this.Document = excavDoc.Document;
            }
            else
            {
                throw new NullReferenceException("The specified element is not valid as soil.");
            }
        }

        #endregion

        /// <summary>
        /// 对于一个单元进行全面的检测，以判断其是否为一个模型土体单元或者开挖土体单元。
        /// </summary>
        /// <param name="doc">进行土体单元搜索的文档</param>
        /// <param name="SoilElementId">可能的土体单元的ElementId值</param>
        /// <param name="FailureMessage">如果不是，则返回不能转换的原因。</param>
        /// <returns>如果检查通过，则可以直接通过Create静态方法来创建对应的模型土体</returns>
        /// <remarks>一个土体单元的族实例，必须满足的条件有：1. 族的名称限制；2. 实例类别为“场地”。</remarks>
        protected static bool IsSoilElement(Document doc, ElementId SoilElementId, ref string FailureMessage)
        {
            bool blnSucceed = false;
            if (SoilElementId != ElementId.InvalidElementId) // 说明用户手动指定了土体单元的ElementId，此时需要检测此指定的土体单元是否是有效的土体单元
            {
                try
                {
                    FamilyInstance Soil = (FamilyInstance)SoilElementId.Element(doc);
                    // 进行细致的检测
                    //1. 族实例的族名称
                    if (String.Compare(Convert.ToString(Soil.Symbol.FamilyName), Constants.FamilyName_Soil, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        FailureMessage = $"指定的ElementId所对应的单元的族名称与全局的土体族的名称\"{Constants.FamilyName_Soil}\"不相同。";
                        return false;
                    }
                    //2. 族实例的类别
                    if (!Soil.Category.Id.Equals(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Site).Id))
                    {
                        FailureMessage = "指定的ElementId所对应的单元的族类别不是\"场地\"类别。";
                        return false;
                    }
                    blnSucceed = true;
                }
                catch (Exception ex)
                {
                    FailureMessage = ex.Message;
                    //MessageBox.Show(String.Format("指定的元素Id ({0})不是有效的土体单元。", SoilElementId) & vbCrLf &
                    //                ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    blnSucceed = false;
                }
            }
            //
            return blnSucceed;
        }


        #region    ---   模型土体或者开挖土体的深度参数的提取与设置

        /// <summary>
        /// 模型土体或者开挖土体的深度值，单位为米
        /// </summary>
        private double? _depth;

        /// <summary> 提取模型土体或者开挖土体的深度参数。此参数是通过API添加的。 </summary>
        /// <returns> 返回深度值，单位为米 </returns>
        public double GetDepth()
        {
            if (_depth != null) return _depth.Value;
            
            var dInch = Soil.get_Parameter(Constants.SP_SoilDepth_Guid).AsDouble();
            return UnitUtils.ConvertFromInternalUnits(dInch, DisplayUnitType.DUT_METERS); // 将英尺转换为米。
        }

        /// <summary> 设置模型土体或者开挖土体的深度参数。此参数是通过API添加的。 </summary>
        /// <param name="tran">  </param>
        /// <param name="depth"> 模型的新的深度，单位为米 </param>
        public void SetDepth(Transaction tran, double depth)
        {
            if (_depth == null || Math.Abs(depth - _depth.Value) > 0.000001)
            {
                Parameter para = Soil.get_Parameter(Constants.SP_SoilDepth_Guid);
                para.Set(UnitUtils.ConvertToInternalUnits(depth, DisplayUnitType.DUT_METERS)); // 将米转换为英尺。
            }

            // store its name in the private variable.
            this._depth = depth;
        }


        #endregion
    }
}
using System;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using rvtTools;

namespace OldW.Excavation
{
    /// <summary>
    /// 土体单元对象。一个土体单元的族实例，必须满足的条件有：1. 族的名称限制；2. 实例类别为“场地”。
    /// </summary>
    /// <remarks></remarks>
    public abstract class Soil_Element
    {
        #region    ---   Properties

        private FamilyInstance F_Soil;

        /// <summary> 土体单元所对应的族实例对象 </summary>
        public FamilyInstance Soil
        {
            get { return F_Soil; }
        }

        protected ExcavationDoc F_ExcavDoc;

        public ExcavationDoc ExcavDoc
        {
            get { return F_ExcavDoc; }
        }

        protected Document Doc;

        public Document Document
        {
            get { return Doc; }
        }

        #endregion

        #region    ---   Fields

        #endregion

        #region    ---   构造函数，通过 OldWDocument.GetSoilModel，或者是Create静态方法

        protected Soil_Element(ExcavationDoc ExcavDoc, FamilyInstance SoilElement)
        {
            if (SoilElement != null && SoilElement.IsValidObject)
            {
                this.F_ExcavDoc = ExcavDoc;
                this.F_Soil = SoilElement;
                this.Doc = ExcavDoc.Document;
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
        protected static bool IsSoildElement(Document doc, ElementId SoilElementId, ref string FailureMessage)
        {
            bool blnSucceed = false;
            if (SoilElementId != ElementId.InvalidElementId) // 说明用户手动指定了土体单元的ElementId，此时需要检测此指定的土体单元是否是有效的土体单元
            {
                try
                {
                    FamilyInstance Soil = (FamilyInstance) SoilElementId.Element(doc);
                    // 进行细致的检测
                    //1. 族实例的族名称
                    if (string.Compare(Convert.ToString(Soil.Symbol.FamilyName), Constants.FamilyName_Soil, true) != 0)
                    {
                        throw new TypeUnloadedException(string.Format("指定的ElementId所对应的单元的族名称与全局的土体族的名称\"{0}\"不相同。",
                            Constants.FamilyName_Soil));
                    }
                    //2. 族实例的类别
                    if (!Soil.Category.Id.Equals(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Site).Id))
                    {
                        throw new InvalidCastException("指定的ElementId所对应的单元的族类别不是\"场地\"类别。");
                    }
                    blnSucceed = true;
                }
                catch (Exception ex)
                {
                    FailureMessage = ex.Message;
                    //MessageBox.Show(String.Format("指定的元素Id ({0})不是有效的土体单元。", SoilElementId) & vbCrLf &
                    //                ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                }
            }
            //
            return blnSucceed;
        }
    }
}
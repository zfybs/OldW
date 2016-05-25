// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Linq;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports
using rvtTools;
using Autodesk.Revit.DB;

namespace OldW.Excavation
{

    /// <summary> 用来模拟分块开挖的土体元素。 </summary>
    /// <remarks></remarks>
    public class Soil_Excav : Soil_Element
    {

        #region    ---   Properties

        /// <summary> 开挖土体单元所附着的模型土体 </summary>
        private Soil_Model F_ModelSoil;
        /// <summary> 开挖土体单元所附着的模型土体 </summary>
        public Soil_Model ModelSoil
        {
            get
            {
                return F_ModelSoil;
            }
        }

        private Nullable<DateTime> F_CompletedDate;
        /// <summary>
        /// 每一个开挖土体都有一个开挖完成的时间。由于记录的不完整，这个时间可能暂时不知道，但是后期要可以指定。
        /// </summary>
        public Nullable<DateTime> CompletedDate
        {
            get
            {
                if (F_CompletedDate == null)
                {
                    F_CompletedDate = GetExcavatedDate(false);
                }
                return F_CompletedDate;
            }
        }

        private Nullable<DateTime> F_StartedDate;
        /// <summary>
        /// 每一个开挖土体都有一个开始开挖的时间。由于记录的不完整，这个时间可能暂时不知道，但是后期要可以指定。
        /// </summary>
        public Nullable<DateTime> StartedDate
        {
            get
            {
                if (F_StartedDate == null)
                {
                    F_StartedDate = GetExcavatedDate(true);
                }
                return F_StartedDate;
            }
        }

        #endregion

        #region    ---   构造函数，通过 OldWDocument.GetSoilModel，或者是Create静态方法

        /// <summary>
        /// 构造函数：用来模拟分块开挖的土体元素。
        /// </summary>
        /// <param name="SoilRemove">用来模拟土体开挖的土体Element</param>
        /// <param name="BindedModelSoil">开挖土体单元所附着的模型土体。</param>
        /// <remarks></remarks>
        private Soil_Excav(FamilyInstance SoilRemove, Soil_Model BindedModelSoil) : base(BindedModelSoil.ExcavDoc, SoilRemove)
        {
            this.F_ModelSoil = BindedModelSoil;
        }

        /// <summary>
        /// 对于一个单元进行全面的检测，以判断其是否为一个开挖土体单元。
        /// </summary>
        /// <param name="doc">进行土体单元搜索的文档</param>
        /// <param name="SoilElementId">可能的开挖土体单元的ElementId值</param>
        /// <param name="FailureMessage">如果不是，则返回不能转换的原因。</param>
        /// <returns>如果检查通过，则可以直接通过Create静态方法来创建对应的模型土体</returns>
        /// <remarks></remarks>
        public static bool IsExcavationModel(Document doc, ElementId SoilElementId, ref string FailureMessage)
        {
            bool blnSucceed = false;
            if (SoilElementId != ElementId.InvalidElementId) // 说明用户手动指定了土体单元的ElementId，此时需要检测此指定的土体单元是否是有效的土体单元
            {
                try
                {
                    FamilyInstance Soil = (FamilyInstance)(SoilElementId.Element(doc));
                    // 是否满足基本的土体单元的条件
                    if (!Soil_Element.IsSoildElement(doc, SoilElementId, ref FailureMessage))
                    {
                        throw (new InvalidCastException(FailureMessage));
                    }
                    // 进行细致的检测
                    Parameter pa = Soil.get_Parameter(GlobalSettings.Constants.SP_ExcavationCompleted_Guid);
                    if (pa != null)
                    {
                        throw (new InvalidCastException("族实例中没有参数：\"" + GlobalSettings.Constants.SP_ExcavationCompleted + "\""));
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

        /// <summary>
        /// 创建开挖土体。除非是在API中创建出来，否则。在创建之前，请先通过静态方法IsExcavationModel来判断此族实例是否可以转换为Soil_Model对象。
        /// 否则，在程序运行过程中可能会出现各种报错。
        /// </summary>
        /// <param name="SoilElement">开挖土体单元</param>
        /// <param name="BindedModelSoil">开挖土体单元所附着的模型土体。</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Soil_Excav Create(FamilyInstance SoilElement, Soil_Model BindedModelSoil)
        {
            return new Soil_Excav(SoilElement, BindedModelSoil);
        }

        #endregion

        #region    ---   为开挖土体设置与读取对应的开挖完成的时间

        /// <summary>
        /// 为多个开挖土体设置对应的开挖完成的时间
        /// </summary>
        /// <param name="Started">如果要设置土体开始开挖的时间，则设置为True，反之则是设置土体开挖完成的时间</param>
        /// <param name="Soil_Date">一个字典集合，其中包括要进行日期设置的所有开挖土体。土体开挖完成的时间，可以精确到分钟</param>
        /// <remarks></remarks>
        public static void SetExcavatedDate(Document doc, bool Started, Dictionary<Soil_Excav, DateTime> Soil_Date)
        {
            Soil_Excav Exs = default(Soil_Excav);
            using (Transaction t = new Transaction(doc, "设置土体开挖完成的时间"))
            {
                t.Start();
                for (int Ind = 0; Ind <= Soil_Date.Count; Ind++)
                {
                    Exs = Soil_Date.Keys.ToArray()[Ind];
                    Exs.SetExcavatedDate(t, Started, Soil_Date.Values.ToArray()[Ind]);
                }
                t.Commit();
            }
        }

        /// <summary>
        /// 为开挖土体设置对应的开挖完成的时间
        /// </summary>
        /// <param name="Tran">Revit事务对象，在此函数中此事务并不会Start或者Commit，所以在调用此函数时，请确保此事务对象已经Started了。</param>
        /// <param name="Started">如果要设置土体开始开挖的时间，则设置为True，反之则是设置土体开挖完成的时间</param>
        /// <param name="ResDate">土体开挖开始或者完成的时间，可以精确到分钟。如果要清空日期字符，则设置其为Nothing。</param>
        /// <remarks></remarks>
        public void SetExcavatedDate(Transaction Tran, bool Started, Nullable<DateTime> ResDate)
        {
            Parameter pa = default(Parameter);
            if (Started)
            {
                pa = Soil.get_Parameter(GlobalSettings.Constants.SP_ExcavationStarted_Guid);
            }
            else
            {
                pa = Soil.get_Parameter(GlobalSettings.Constants.SP_ExcavationCompleted_Guid);
            }

            if (pa != null)
            {
                if (ResDate == null)
                {
                    pa.Set("");
                }
                else
                {
                    if (ResDate.Value.Hour == 0)
                    {
                        pa.Set(ResDate.Value.ToString("yyyy/MM/dd"));
                    }
                    else
                    {
                        pa.Set(ResDate.Value.ToString("yyyy/MM/dd hh:mm"));
                    }
                }

                if (Started)
                {
                    this.F_StartedDate = ResDate;
                }
                else
                {
                    this.F_CompletedDate = ResDate;
                }
            }
            else
            {
                throw (new NullReferenceException(string.Format("土体单元中未找到指定的参数\"{0}\"", GlobalSettings.Constants.SP_ExcavationCompleted)));
            }
        }

        /// <summary>
        /// 从开挖土体的单元中读取开挖完成的日期
        /// </summary>
        /// <param name="Started">如果要提取土体开始开挖的时间，则设置为True，反之则是提取土体开挖完成的时间</param>
        private Nullable<DateTime> GetExcavatedDate(bool Started)
        {
            Parameter pa = default(Parameter);
            string paName = "";
            if (Started)
            {
                pa = Soil.get_Parameter(GlobalSettings.Constants.SP_ExcavationStarted_Guid);
                paName = GlobalSettings.Constants.SP_ExcavationStarted;
            }
            else
            {
                pa = Soil.get_Parameter(GlobalSettings.Constants.SP_ExcavationCompleted_Guid);
                paName = GlobalSettings.Constants.SP_ExcavationCompleted;
            }
            //
            string strDate = "";
            if (pa != null)
            {
                strDate = System.Convert.ToString(pa.AsString());
                DateTime dt = default(DateTime);
                if (DateTime.TryParse(strDate, out dt))
                {
                    return dt;
                }
                else // 说明此字符为空或者不能转换为日期
                {
                    //MessageBox.Show(String.Format("土体单元{0}中的参数""{1}""的值""{2}""不能正确地转换为日期。",
                    //                                          Me.Soil.Id, Constants.SP_ExcavationCompleted, strDate))
                    return default(Nullable<DateTime>);
                }
            }
            else
            {
                throw (new NullReferenceException(string.Format("土体单元中未找到指定的参数\"{0}\"", paName)));
            }

        }

        #endregion

        /// <summary>
        /// 得到开挖土体的顶面或者底面的在模型中的标高
        /// </summary>
        /// <param name="Top">If true, obtain the elevation of the top surface. If false, obtain the elevation of the bottom surface.  </param>
        /// <returns>指定表面的标高值（单位为m）。the elevation of the specified surface, in the unit of meter.</returns>
        /// <remarks>不用Element.Geometry（）方法，因为此方法包含大量的数据结构转换，太消耗CPU。而应使用GetBoundingBox与GetTransform等方法。</remarks>
        public double GetElevation(bool Top)
        {
            double Z = 0;
            if (Top)
            {
                Z = Soil.get_BoundingBox(Doc.ActiveView).Max.Z;
            }
            else
            {
                Z = System.Convert.ToDouble(Soil.get_BoundingBox(Doc.ActiveView).Min.Z);
            }
            return UnitUtils.ConvertFromInternalUnits(Z, DisplayUnitType.DUT_CUBIC_METERS);
        }

        /// <summary> 获取开挖土体的名称，这里取的是族实例所对应的族类型的名称 </summary>
        public string GetName()
        {
            return Soil.Symbol.Name;
        }
    }

}

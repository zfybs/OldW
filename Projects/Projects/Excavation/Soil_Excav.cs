using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using rvtTools;
using Color = System.Drawing.Color;

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
            get { return F_ModelSoil; }
        }


        #endregion

        #region    ---   构造函数，通过 OldWDocument.GetSoilModel，或者是Create静态方法

        /// <summary>
        /// 构造函数：用来模拟分块开挖的土体元素。
        /// </summary>
        /// <param name="SoilRemove">用来模拟土体开挖的土体Element</param>
        /// <param name="BindedModelSoil">开挖土体单元所附着的模型土体。</param>
        /// <remarks></remarks>
        private Soil_Excav(FamilyInstance SoilRemove, Soil_Model BindedModelSoil)
            : base(BindedModelSoil.ExcavDoc, SoilRemove)
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
                    FamilyInstance Soil = (FamilyInstance)SoilElementId.Element(doc);
                    // 是否满足基本的土体单元的条件
                    if (!IsSoilElement(doc, SoilElementId, ref FailureMessage))
                    {
                        throw new InvalidCastException(FailureMessage);
                    }
                    // 进行细致的检测
                    Parameter pa = Soil.get_Parameter(Constants.SP_ExcavationCompleted_Guid);
                    if (pa != null)
                    {
                        throw new InvalidCastException("族实例中没有参数：\"" + Constants.SP_ExcavationCompleted + "\"");
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

        /// <summary>
        /// 得到开挖土体的顶面或者底面的在模型中的标高
        /// </summary>
        /// <param name="Top">If true, obtain the elevation of the top surface. If false, obtain the elevation of the bottom surface.  </param>
        /// <returns>指定表面的标高值（单位为m）。the elevation of the specified surface, in the unit of meter.</returns>
        /// <remarks>不用Element.Geometry（）方法，因为此方法包含大量的数据结构转换，太消耗CPU。而应使用GetBoundingBox与GetTransform等方法。</remarks>
        public double GetElevation(bool Top)
        {
            double z = Top
                ? Soil.get_BoundingBox(Document.ActiveView).Max.Z
                : Convert.ToDouble(Soil.get_BoundingBox(Document.ActiveView).Min.Z);

            return UnitUtils.ConvertFromInternalUnits(z, DisplayUnitType.DUT_CUBIC_METERS);
        }

        #region    ---   为开挖土体设置与读取对应的开挖完成的时间


        /// <summary>
        /// 为多个开挖土体设置对应的开挖完成的时间
        /// </summary>
        /// <param name="Started">如果要设置土体开始开挖的时间，则设置为True，反之则是设置土体开挖完成的时间</param>
        /// <param name="Soil_Date">一个字典集合，其中包括要进行日期设置的所有开挖土体。土体开挖完成的时间，可以精确到分钟</param>
        /// <remarks></remarks>
        public static void SetExcavatedDate(Transaction tran, Document doc, bool Started, Dictionary<Soil_Excav, DateTime> Soil_Date)
        {
            Soil_Excav Exs = default(Soil_Excav);
            tran.SetName("设置土体开挖完成的时间");
            for (int Ind = 0; Ind <= Soil_Date.Count; Ind++)
            {
                Exs = Soil_Date.Keys.ToArray()[Ind];
                Exs.SetExcavatedDate(tran, Started, Soil_Date.Values.ToArray()[Ind]);
            }
        }

        /// <summary> 每一个开挖土体都有一个开挖完成的时间。由于记录的不完整，这个时间可能暂时不知道，但是后期要可以指定。 </summary>
        private DateTime? _completedDate;
        /// <summary> 开挖土体的开挖完成日期已经从Revit中刷新到内存中。 </summary>
        private bool _completedDateInMemory = false;

        /// <summary> 每一个开挖土体都有一个开始开挖的时间。由于记录的不完整，这个时间可能暂时不知道，但是后期要可以指定。 </summary>
        private DateTime? _startedDate;
        /// <summary> 开挖土体的开挖开始日期已经从Revit中刷新到内存中。 </summary>
        private bool _startedDateInMemory = false;

        /// <summary>
        /// 为开挖土体设置对应的开挖完成的时间
        /// </summary>
        /// <param name="tran">Revit事务对象，在此函数中此事务并不会Start或者Commit，所以在调用此函数时，请确保此事务对象已经Started了。</param>
        /// <param name="Started">如果要设置土体开始开挖的时间，则设置为True，反之则是设置土体开挖完成的时间</param>
        /// <param name="ResDate">土体开挖开始或者完成的时间，可以精确到分钟。如果要清空日期字符，则设置其为Nothing。</param>
        /// <remarks></remarks>
        public void SetExcavatedDate(Transaction tran, bool Started, Nullable<DateTime> ResDate)
        {
            Parameter pa = default(Parameter);
            if (Started)
            {
                pa = Soil.get_Parameter(Constants.SP_ExcavationStarted_Guid);
            }
            else
            {
                pa = Soil.get_Parameter(Constants.SP_ExcavationCompleted_Guid);
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
                    this._startedDate = ResDate;
                    this._startedDateInMemory = true;
                }
                else
                {
                    this._completedDate = ResDate;
                    this._completedDateInMemory = true;
                }
            }
            else
            {
                throw new NullReferenceException(string.Format("土体单元中未找到指定的参数\"{0}\"", Constants.SP_ExcavationCompleted));
            }
        }

        /// <summary>
        /// 从开挖土体的单元中读取开挖开始或者开挖完成的日期
        /// </summary>
        /// <param name="started">如果要提取土体开始开挖的时间，则设置为True，反之则是提取土体开挖完成的时间</param>
        public DateTime? GetExcavatedDate(bool started)
        {
            Parameter pa = default(Parameter);
            string paName = "";
            if (started)
            {
                if (this._startedDateInMemory)
                {
                    return _startedDate;
                }
                pa = Soil.get_Parameter(Constants.SP_ExcavationStarted_Guid);
                paName = Constants.SP_ExcavationStarted;
            }
            else
            {
                if (_completedDateInMemory)
                {
                    return _completedDate;
                }
                pa = Soil.get_Parameter(Constants.SP_ExcavationCompleted_Guid);
                paName = Constants.SP_ExcavationCompleted;
            }
            //
            DateTime? excavateDate = null;

            string strDate = "";
            if (pa != null)
            {
                strDate = Convert.ToString(pa.AsString());
                DateTime dt = default(DateTime);
                if (DateTime.TryParse(strDate, out dt))
                {
                    excavateDate = dt;
                }
                else // 说明此字符为空或者不能转换为日期
                {
                    //MessageBox.Show(String.Format("土体单元{0}中的参数""{1}""的值""{2}""不能正确地转换为日期。",
                    //                                          Me.Soil.Id, Constants.SP_ExcavationCompleted, strDate))
                    excavateDate = null;
                }
            }
            else
            {
                throw new NullReferenceException(string.Format("土体单元中未找到指定的参数\"{0}\"", paName));
            }

            // 将最新的结果刷新到内存中
            if (started)
            {
                _startedDate = excavateDate;
                _startedDateInMemory = true;
            }
            else
            {
                _completedDate = excavateDate;
                _completedDateInMemory = true;
            }
            return excavateDate;
        }

        #endregion

        #region   ---   开挖土体的名称的提取与设置

        /// <summary> 获取开挖土体的名称，这里取的是族实例所对应的族类型的名称 </summary>
        public string GetName()
        {
            return Soil.Symbol.Name;
        }

        /// <summary> 设置开挖土体的名称。默认情况下，一个开挖土体族中只有一个族类型，此族类型也只有一个实例单元。
        /// 族实例所对应的族类型的名称 </summary>
        /// <param name="tran">  </param>
        /// <param name="newName"> 这里会将开挖土体对应所对应的族类型，以及其族都设置为此名称。 </param>
        public void SetName(Transaction tran, string newName)
        {
            // 设置族类型的名称
            Soil.Symbol.Name = newName;
            // 设置族在Revit文档中的名称
            Soil.Symbol.Family.Name = newName;
        }

        #endregion

        /// <summary> 将自身从整个模型文档中删除 </summary>
        public void Delete(Transaction tranDoc)
        {
            tranDoc.SetName("将开挖土体从模型中删除");

            // 删除实例对象、实例所对应的族、以及族类型
            Document.Delete(new ElementId[] { Soil.Id, Soil.Symbol.Id, Soil.Symbol.Family.Id });
        }

        #region    ---   根据不同的工况状态来设置开挖土体的显示样式

        /// <summary>
        /// 根据开挖土体当前的开挖状态来设置其在Revit中的显示样式
        /// </summary>
        /// <param name="tranDoc"></param>
        /// <param name="view"></param>
        /// <param name="stage">开挖土体当前的开挖状态</param>
        public void RefreshByStage(Transaction tranDoc, View view, ExcavationStage stage)
        {

            switch (stage)
            {
                case ExcavationStage.UnStarted: SetStyle_UnStarted(tranDoc, view, new ElementId[] { Soil.Id }); break;
                case ExcavationStage.Excavating: SetStyle_Excavating(tranDoc, view, new ElementId[] { Soil.Id }); break;
                case ExcavationStage.Completed: SetStyle_Completed(tranDoc, view, new ElementId[] { Soil.Id }); break;
                case ExcavationStage.UnKown: SetStyle_UnKown(tranDoc, view, new ElementId[] { Soil.Id }); break;
            }
        }

        private void SetStyle_UnStarted(Transaction tranDoc, View view, ICollection<ElementId> excavSoils)
        {
            // 让其以一般土体的样式显示出来
            view.UnhideElements(excavSoils);

            // 设置其显示颜色
            System.Drawing.Color c1 = Color.DarkGray;
            Autodesk.Revit.DB.Color c;
            RvtTools.ConvertColor(c1, out c);
            SetColor(Document, view, c);
        }

        private void SetStyle_Excavating(Transaction tranDoc, View view, ICollection<ElementId> excavSoils)
        {
            // 让其以正在开挖的土体的样式显示出来
            view.UnhideElements(excavSoils);

            // 设置其显示颜色
            System.Drawing.Color c1 = Color.Red;
            Autodesk.Revit.DB.Color c;
            RvtTools.ConvertColor(c1, out c);
            SetColor(Document, view, c, 40);
        }

        private void SetStyle_Completed(Transaction tranDoc, View view, ICollection<ElementId> excavSoils)
        {
            // 让其隐藏
            view.HideElements(excavSoils);

        }

        private void SetStyle_UnKown(Transaction tranDoc, View view, ICollection<ElementId> excavSoils)
        {
            // 让其以其本来的材质样式显示出来
            view.UnhideElements(excavSoils);


            // 设置其显示颜色
            System.Drawing.Color c1 = Color.DarkGoldenrod;
            Autodesk.Revit.DB.Color c;
            RvtTools.ConvertColor(c1, out c);
            SetColor(Document, view, c, 25);

        }

        /// <summary>
        /// 设置土体的显示颜色、透明度
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="v"></param>
        /// <param name="fillColor">如果不指定颜色，则“按材质”显示</param>
        /// <param name="transparency"> 表面填充的透明度，0表示不透明，100表示全透明 </param>
        private void SetColor(Document doc, View v, Autodesk.Revit.DB.Color fillColor = null, int transparency = 0)
        {
            OverrideGraphicSettings gs = v.GetElementOverrides(Soil.Id);
            string fillPatternName;

            if (fillColor == null)
            {
                // 填充模式
                // fillPatternName ="" ;
                gs.SetProjectionFillPatternId(ElementId.InvalidElementId);  // 设置为“按材质”
                gs.SetCutFillColor(Autodesk.Revit.DB.Color.InvalidColorValue);
            }
            else
            {
                // 填充颜色
                gs.SetProjectionFillColor(fillColor);

                // 填充模式
                fillPatternName = Constants.SolidFillPattern;

                FillPatternElement fpe = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, fillPatternName);
                if (fpe == null)
                {
                    fpe = FillPatternElement.Create(doc,
                          new FillPattern(fillPatternName, FillPatternTarget.Drafting, FillPatternHostOrientation.ToHost));
                }
                gs.SetSurfaceTransparency(transparency);
                gs.SetProjectionFillPatternId(fpe.Id);
            }
            v.SetElementOverrides(Soil.Id, gs);

        }

        #endregion
    }

    /// <summary> 开挖土体当前的开挖状态 </summary>
    public enum ExcavationStage
    {
        /// <summary> 还未开始开挖 </summary>
        UnStarted,
        /// <summary> 当前正在开挖 </summary>
        Excavating,
        /// <summary> 已经开挖完成 </summary>
        Completed,
        /// <summary> 开挖状态未知 </summary>
        UnKown
    }
}
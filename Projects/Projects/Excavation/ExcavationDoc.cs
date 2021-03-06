using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using RevitStd;
using eZstd;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Document = Autodesk.Revit.DB.Document;
using View = Autodesk.Revit.DB.View;


namespace OldW.Excavation
{
    /// <summary>
    /// 用来执行基坑开挖模拟中的数据存储与绘制操作
    /// </summary>
    /// <remarks></remarks>
    public class ExcavationDoc : OldWDocument
    {
        #region    ---   Types

        /// <summary> 用来创建此开挖土体族样板的类型 </summary>
        private enum Type
        {
            /// <summary> 公制常规模型 </summary>
            GenericForm,

            /// <summary> 自适应常规模型 </summary>
            AutoAdapt
        }

        #endregion

        #region    ---   Fields

        private Application App;

        /// <summary>
        /// 文档中已经检索出来的模型土体，这个对象可能为空，也可能是一个无效的对象。
        /// </summary>
        public Soil_Model ModelSoil { get; set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="OldWDoc"></param>
        public ExcavationDoc(OldWDocument OldWDoc) : base(OldWDoc.Document)
        {
            this.App = Document.Application;
        }

        #region    ---   创建与设置土体

        #region    ---   创建模型土体与开挖土体

        /// <summary>
        /// 创建模型土体，此土体单元在模型中应该只有一个。
        /// </summary>
        /// <param name="CurveArrArr">要进行拉伸的平面轮廓（可以由多个封闭的曲线组成）</param>
        /// <param name="Depth">模型土体的深度，单位为m，数值为正表示向下的深度，反之表示向上的高度。</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Soil_Model CreateModelSoil(double Depth, CurveArrArray CurveArrArr)
        {
            if (CurveArrArr.IsEmpty) throw new NullReferenceException("用来绘制土体所用的轮廓为空。");

            Soil_Model SM = null;

            // 构造一个族文档
            Document famDoc = CreateFamilyFromProfile(Depth, CurveArrArr);

            // 将族加载到项目文档中
            Family fam = famDoc.LoadFamily(Document, UIDocument.GetRevitUIFamilyLoadOptions());
            famDoc.Close(false);
            // 获取一个族类型，以加载一个族实例到项目文档中
            FamilySymbol fs = fam.GetFamilySymbolIds().First().Element(Document) as FamilySymbol;

            using (Transaction tranDoc = new Transaction(Document, "将模型土体的族在项目文档中创建一个实例并放置到组中"))
            {
                try
                {
                    tranDoc.Start();

                    // 将模型土体放到Group中
                    List<ElementId> GroupMems = new List<ElementId>();
                    GroupType gptp =
                        (GroupType)Document.FindElement(typeof(GroupType), targetName: Constants.FamilyName_Soil);
                    // 组类型中包含有族实例，所以要找到对应的组类型，然后找到对应的组实例，再将组实例解组。
                    // 这是为了避免在删除“基坑土体”族及其实例时，UI界面中会出现“删除组实例中的最后一个成员”的警告。
                    Group gp = default(Group);
                    if (gptp != null)
                    {
                        gp =
                            (Group)Document.FindElement(typeof(Group), targetName: Constants.FamilyName_Soil);
                        if (gp != null)
                        {
                            GroupMems = gp.GetMemberIds() as List<ElementId>;
                            gp.UngroupMembers();
                        }
                        Document.Delete(gptp.Id);
                    }

                    //
                    Family f = Document.FindFamily(Constants.FamilyName_Soil, BuiltInCategory.OST_Site);
                    if (f != null)
                    {
                        // 将此模型土体从Group的集合中删除
                        foreach (ElementId e in f.Instances().ToElementIds())
                        {
                            if (GroupMems.Contains(e))
                            {
                                GroupMems.Remove(e);
                            }
                        }
                        // 删除模型土体族及对应的实例
                        Document.Delete(f.Id);
                    }

                    // 为族与族类型重命名
                    fam.Name = Constants.FamilyName_Soil;
                    fs.Name = Constants.FamilyName_Soil;

                    // 生成族实例
                    if (!fs.IsActive)
                    {
                        fs.Activate();
                    }
                    FamilyInstance fi = Document.Create.NewFamilyInstance(new XYZ(), fs, StructuralType.NonStructural);

                    // 重新构造Group
                    Document.Regenerate();
                    GroupMems.Add(fi.Id);
                    gp = Document.Create.NewGroup(GroupMems);
                    gp.GroupType.Name = Constants.FamilyName_Soil;

                    // 将模型组在Revit界面中显示出来
                    Document.ActiveView.UnhideElements(new ElementId[] { gp.Id });

                    //
                    SM = Soil_Model.Create(this, fi);

                    // 将此模型土体所在的组中的其他开挖土体来剪切此模型土体
                    SM.RemoveSoils(tranDoc);

                    //
                    tranDoc.Commit();
                }
                catch (Exception ex)
                {
                    // Utils.ShowDebugCatch(ex, $"事务“{tranDoc.GetName()}”出错");
                    tranDoc.RollBack();
                    throw new InvalidOperationException($"事务“{tranDoc.GetName()}”出错", ex);
                }
            }

            return SM;
        }

        /// <summary>
        /// 创建开挖土体，此土体单元在模型中可以有很多个。
        /// </summary>
        /// <param name="modelSoil">在创建开挖土体之前，请先确保已经创建好了模型土体。
        /// 在此方法中，模型土体对象并不起任何作用，只是用来确保模型土体对象已经创建。</param>
        /// <param name="curveArrArr"> 要进行拉伸的平面轮廓（可以由多个封闭的曲线组成） </param>
        /// <param name="depth">开挖土体的深度，单位为m，数值为正表示向下的深度，反之表示向上的高度。</param>
        /// <param name="desiredName">此开挖土体实例的名称（推荐以开挖完成的日期）。如果此名称已经被使用，则以默认的名称来命名。</param>
        /// <returns></returns>
        public Soil_Excav CreateExcavationSoil(Soil_Model modelSoil, double depth, CurveArrArray curveArrArr,
            string desiredName)
        {
            if (this.ModelSoil == null) throw new NullReferenceException("请先在文档中创建出对应的模型土体。");
            if (curveArrArr.IsEmpty) throw new NullReferenceException("用来绘制土体所用的轮廓为空。");

            // 
            Soil_Excav excavSoil = null;

            // 构造一个族文档
            Document famDoc = CreateFamilyFromProfile(depth, curveArrArr);

            // 对于开挖土体族，其与模型土体不同的地方在于，它还是
            using (Transaction tranFam = new Transaction(famDoc))
            {
                try
                {
                    tranFam.SetName("添加参数：开挖的开始与完成日期");
                    tranFam.Start();
                    // ---------------------------------------------------------------------------------------------------------

                    // 添加参数
                    FamilyManager FM = famDoc.FamilyManager;

                    // 开挖开始
                    DefinitionGroup defGroup = OldWDocument.GetOldWDefinitionGroup(App);
                    ExternalDefinition ExDef =
                        defGroup.Definitions.get_Item(Constants.SP_ExcavationStarted) as ExternalDefinition;

                    FM.AddParameter(ExDef, BuiltInParameterGroup.PG_DATA, true);
                    // 开挖完成
                    ExDef = defGroup.Definitions.get_Item(Constants.SP_ExcavationCompleted) as ExternalDefinition;

                    FM.AddParameter(ExDef, BuiltInParameterGroup.PG_DATA, true);
                    tranFam.Commit();
                }
                catch (Exception ex)
                {
                    // Utils.ShowDebugCatch(ex, $"事务“{tranFam.GetName()}” 出错！");
                    tranFam.RollBack();
                    throw new InvalidOperationException($"事务“{tranFam.GetName()}”出错", ex);
                }
            }

            // 将族加载到项目文档中
            Family fam = famDoc.LoadFamily(Document, UIDocument.GetRevitUIFamilyLoadOptions());
            famDoc.Close(false);

            // 获取一个族类型，以加载一个族实例到项目文档中
            FamilySymbol fs = (FamilySymbol)fam.GetFamilySymbolIds().First().Element(Document);

            using (Transaction tranDoc = new Transaction(Document, "将开挖土体的族在项目文档中创建一个实例并放置到组中"))
            {
                try
                {
                    tranDoc.Start();
                    // 族或族类型的重命名

                    string soilName = GetValidExcavationSoilName(Document, desiredName);
                    fs.Name = soilName;
                    fam.Name = soilName;

                    // 创建实例
                    if (!fs.IsActive)
                    {
                        fs.Activate();
                    }
                    FamilyInstance fi = Document.Create.NewFamilyInstance(new XYZ(), fs, StructuralType.NonStructural);
                    excavSoil = Soil_Excav.Create(fi, modelSoil);

                    tranDoc.Commit();
                }
                catch (Exception ex)
                {
                    tranDoc.RollBack();

                    // 删除前面加载到项目文档中的族
                    using (Transaction tranDelectFamaiy = new Transaction(Document, "删除前面加载到项目文档中的族"))
                    {
                        tranDelectFamaiy.Start();
                        Document.Delete(new ElementId[] { fam.Id });
                        tranDelectFamaiy.Commit();
                    }

                    throw new InvalidOperationException($"事务“{tranDoc.GetName()}”出错", ex);
                }
            }
            return excavSoil;
        }

        #endregion

        /// <summary>
        /// 根据输入的平面轮廓信息，以及对应的模型深度，创建出模型土体或者开挖土体族
        /// </summary>
        /// <param name="Depth"></param>
        /// <param name="CurveArrArr"></param>
        /// <returns></returns>
        private Document CreateFamilyFromProfile(double Depth, CurveArrArray CurveArrArr)
        {
            string TemplateName = Path.Combine(ProjectPath.Path_family, Constants.FamilyTemplateName_Soil);
            if (!File.Exists(TemplateName))
            {
                throw new FileNotFoundException("在创建土体时，族样板文件没有找到", TemplateName);
            }

            // 从族样板文件创建族文档
            Document famDoc = App.NewFamilyDocument(TemplateName);

            using (Transaction tranFam = new Transaction(famDoc))
            {
                try
                {
                    tranFam.SetName("设置族类别");
                    tranFam.Start();
                    // 设置族的族类别
                    famDoc.OwnerFamily.FamilyCategory = famDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Site);

                    // 绘制拉伸实体，并关联深度参数
                    ExtrusionAndBindingDimension(tranFam, famDoc, CurveArrArr, Depth);

                    tranFam.Commit();
                }
                catch (Exception ex)
                {
                    // Utils.ShowDebugCatch(ex, $" 事务 {tranFam.GetName()} 出错");
                    tranFam.RollBack();
                    throw new InvalidOperationException($" 事务 {tranFam.GetName()} 出错", ex);
                }
            }
            return famDoc;
        }

        /// <summary>
        /// 绘制拉伸实体，并将其深度值与具体参数关联起来。
        /// </summary>
        /// <param name="tranFam"></param>
        /// <param name="famDoc">实体所在的族文档，此文档当前已经处于打开状态。</param>
        /// <param name="curveArrArr">用来绘制实体的闭合轮廓</param>
        /// <param name="depth">模型土体的深度，单位为m，数值为正表示向下的深度，反之表示向上的高度。</param>
        /// <remarks></remarks>
        private void ExtrusionAndBindingDimension(Transaction tranFam, Document famDoc, CurveArrArray curveArrArr,
            double depth)
        {
            // 获取拉伸轮廓所在水平面的Z坐标值。
            Curve c = curveArrArr.get_Item(0).get_Item(0).Clone();
            c.MakeBound(0, 1);
            double TopZ = Convert.ToDouble(c.GetEndPoint(0).Z);
            c.Dispose();

            // 确定拉伸方向与拉伸长度
            SByte Direction = 0;
            if (depth > 0) // 说明是向下拉伸
            {
                Direction = -1;
            }
            else if (depth < 0) // 说明是向上拉伸
            {
                Direction = 1;
            }
            else
            {
                throw new ArgumentException("深度值不能为0！");
            }
            depth = Math.Abs(UnitUtils.ConvertToInternalUnits(depth, DisplayUnitType.DUT_METERS)); // 将米转换为英尺。


            // 定义参考平面
            Plane P_Top = new Plane(new XYZ(0, 0, Direction), new XYZ(0, 0, TopZ));
            ReferencePlane RefP_Top = default(ReferencePlane);

            // 在族文档中绘制实体

            tranFam.SetName("添加实体与参数关联");
            View V = famDoc.FindElement(typeof(View), BuiltInCategory.OST_Views, "前") as View;
            // 定义视图
            if (V == null)
            {
                throw new NullReferenceException("当前视图为空。");
            }

            RefP_Top = famDoc.FamilyCreate.NewReferencePlane(P_Top.Origin, P_Top.Origin + P_Top.XVec, P_Top.YVec, V);
            RefP_Top.Name = "SoilTop";

            FamilyItemFactory familyCreation = famDoc.FamilyCreate;

            // 创建拉伸实体
            SketchPlane sp = SketchPlane.Create(famDoc, P_Top); // P_Top 为坐标原点所在的水平面

            // ----------------------------------------------------------------------------------------------------------------------------------------

            if (curveArrArr.IsEmpty || sp == null || !sp.IsValidObject)
            {
              throw new InvalidOperationException(@"创建土体模型的轮廓出错:未指定封闭的曲线轮廓，或者对应的工作平面无效。"); 
            }

            Extrusion extru = familyCreation.NewExtrusion(
                isSolid: true,
                profile: curveArrArr,
                sketchPlane: sp,
                end: depth); // 创建拉伸实体

            // ----------------------------------------------------------------------------------------------------------------------------------------

            // 将拉伸实体的顶面与参数平面进行绑定  '  ElementTransformUtils.MoveElement(FamDoc, extru.Id, New XYZ(0, 0, 0))
            PlanarFace Ftop = GeoHelper.FindFace(extru, new XYZ(0, 0, 1));
            familyCreation.NewAlignment(V, Ftop.Reference, RefP_Top.GetReference());

            //' 添加深度参数
            FamilyManager FM = famDoc.FamilyManager;
            //’在进行参数读写之前，首先需要判断当前族类型是否存在，如果不存在，读写族参数都是不可行的
            if (FM.CurrentType == null)
            {
                FM.NewType("CurrentType"); // 随便取个名字即可，后期会将族中的第一个族类型名称统一进行修改。
            }

            // ExternalDefinition familyDefinition = null;
            DefinitionGroup defGroup = OldWDocument.GetOldWDefinitionGroup(App);
            ExternalDefinition ExDef = defGroup.Definitions.get_Item(Constants.SP_SoilDepth) as ExternalDefinition;
            FamilyParameter Para_Depth = FM.AddParameter(ExDef, BuiltInParameterGroup.PG_GEOMETRY, isInstance: true);

            //' give initial values
            FM.Set(Para_Depth, depth); // 这里不知为何为给出报错：InvalidOperationException:There is no current type.


            // 添加标注
            PlanarFace TopFace = GeoHelper.FindFace(extru, new XYZ(0, 0, 1));
            PlanarFace BotFace = GeoHelper.FindFace(extru, new XYZ(0, 0, -1));

            // make an array of references
            ReferenceArray refArray = new ReferenceArray();
            refArray.Append(TopFace.Reference);
            refArray.Append(BotFace.Reference);
            // define a demension line
            var a = GeoHelper.FindFace(extru, new XYZ(0, 0, 1)).Origin;
            Line DimLine = Line.CreateBound(TopFace.Origin, BotFace.Origin);
            // create a dimension
            Dimension DimDepth = familyCreation.NewDimension(V, DimLine, refArray);

            // 将深度参数与其拉伸实体的深度值关联起来
            DimDepth.FamilyLabel = Para_Depth;
        }

        // 开挖土体族的命名
        /// <summary>
        /// 在当前模型文档中，构造出一个有效的名称，来供开挖土体族使用。
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="DesiredName">此开挖土体实例的名称（推荐以开挖完成的日期）。如果此名称已经被使用，则以默认的名称来命名。</param>
        /// <returns></returns>
        /// <remarks>其基本格式为：“开挖-01”或者“开挖-命名-01”</remarks>
        private string GetValidExcavationSoilName(Document doc, string DesiredName)
        {
            string prefix = "开挖-";

            List<Family> Fams = Document.FindFamilies(BuiltInCategory.OST_Site);
            // 构造一个字典，其中包括的所有开挖土体族的名称中，每一个日期，以及对应的可能编号
            List<string> exFamis = new List<string>();
            string FamName = "";
            // 提取所有的开挖土体族名称
            foreach (Family f in Fams)
            {
                FamName = Convert.ToString(f.Name);
                if (FamName.StartsWith(prefix)) // 说明是开挖土体
                {
                    exFamis.Add(FamName);
                }
            }

            // 构造新名称
            int Num = 0;
            string Pre = "";
            string NewName = "";
            if (string.IsNullOrEmpty(DesiredName)) // 命名为“开挖-01”
            {
                int Max = 0;
                foreach (string n in exFamis)
                {
                    if (n.StartsWith(prefix) && int.TryParse(n.Substring(prefix.Length), out Num))
                    {
                        Max = Math.Max(Max, Num);
                    }
                }
                NewName = prefix + (Max + 1).ToString("00");
            }
            else
            {
                DesiredName = prefix + DesiredName;
                if (exFamis.Contains(DesiredName))
                {
                    if (HasSuffixNum(DesiredName, ref Num, ref Pre))
                    {
                        NewName = Pre + (Num + 1).ToString(); // 将同名的字符后面的序号加1”
                    }
                    else
                    {
                        // 要确保修改后的名称不包含在现有的名称集合中！！
                        // 即确保添加的后缀编号的唯一性。
                        int Max = 0;
                        foreach (string n in exFamis)
                        {
                            if (n.StartsWith(DesiredName + "-") &&
                                int.TryParse(n.Substring(Convert.ToInt32((DesiredName + "-").Length)), out Num))
                            {
                                Max = Math.Max(Max, Num);
                            }
                        }
                        NewName = DesiredName + "-" + (Max + 1).ToString("00");
                    }
                }
                else
                {
                    NewName = DesiredName;
                }
            }
            return NewName;
        }

        /// <summary>
        /// 检查一个字符串是否符合“字符-123456”的格式，如果符合，则将其分割为前缀Prefix与后面的数字两部分
        /// </summary>
        private bool HasSuffixNum(string Name, ref int Num, ref string Prefix)
        {
            string[] parts = Name.Split('-');
            var n = parts.Length;
            if (parts.Length >= 2 && int.TryParse(parts[n - 1], out Num)) // 说明此名称为“字符-123”的格式
            {
                Prefix = "";
                for (var i = 0; i <= n - 2; i++)
                {
                    Prefix = Prefix + parts[(int)i] + "-";
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region    ---    Document中的土体单元 搜索

        /// <summary>
        /// 模型中的土体单元
        /// </summary>
        /// <param name="SoilElementId">可能的土体单元的ElementId值，如果没有待选的，可以不指定，此时程序会在整个Document中进行搜索。</param>
        /// <returns>如果成功搜索到，则返回对应的土体单元，如果没有找到，则返回Nothing</returns>
        /// <remarks></remarks>
        public Soil_Model FindSoilModel(int SoilElementId = -1)
        {
            string FMessage = null;
            if (ModelSoil == null)
            {
                ModelSoil = GetModelSoil(Document, new ElementId(SoilElementId), ref FMessage);
            }
            else // 检查现有的这个模型土体单元是否还有效
            {
                if (!Soil_Model.IsSoildModel(Document, ModelSoil.Soil.Id, ref FMessage))
                {
                    ModelSoil = null;
                }
            }
            if (ModelSoil == null)
            {
                MessageBox.Show("在模型中未找到有效的模型土体单元。" + "\r\n" +
                                FMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return ModelSoil;
        }

        /// <summary>
        /// 找到模型中的开挖土体单元
        /// </summary>
        /// <param name="doc">进行土体单元搜索的文档</param>
        /// <param name="SoilElementId">可能的土体单元的ElementId值</param>
        /// <returns>如果找到有效的土体单元，则返回对应的Soil_Model，否则返回Nothing</returns>
        /// <remarks></remarks>
        private Soil_Model GetModelSoil(Document doc, ElementId SoilElementId, ref string FailureMessage)
        {
            FamilyInstance Soil = null;
            if (SoilElementId != ElementId.InvalidElementId) // 说明用户手动指定了土体单元的ElementId，此时需要检测此指定的土体单元是否是有效的土体单元
            {
                try
                {
                    string FMessage = null;
                    if (Soil_Model.IsSoildModel(doc, SoilElementId, ref FMessage))
                    {
                        Soil = (FamilyInstance)Document.GetElement(SoilElementId);
                    }
                    else
                    {
                        throw new InvalidCastException(FMessage);
                    }
                }
                catch (Exception ex)
                {
                    FailureMessage = string.Format("指定的元素Id ({0})不是有效的土体单元。", SoilElementId) + "\r\n" +
                                     ex.Message;
                    Soil = null;
                }
            }
            else // 说明用户根本没有指定任何可能的土体单元，此时需要在模型中按特定的方式来搜索出土体单元
            {
                // 在整个文档中进行搜索
                // 1. 族名称
                Family SoilFamily = Document.FindFamily(Constants.FamilyName_Soil);
                if (SoilFamily != null)
                {
                    // 实例的类别
                    List<ElementId> soils =
                        SoilFamily.Instances(BuiltInCategory.OST_Site).ToElementIds() as List<ElementId>;

                    // 整个模型中只能有一个模型土体对象
                    if (soils.Count == 0)
                    {
                        FailureMessage = "模型中没有土体单元";
                    }
                    else if (soils.Count > 1)
                    {
                        UIDocument UIDoc = new UIDocument(doc);
                        UIDoc.Selection.SetElementIds(soils);
                        FailureMessage = string.Format("模型中的土体单元数量多于一个，请删除多余的土体单元 ( 族\"{0}\"的实例对象 )。",
                            Constants.FamilyName_Soil);
                    }
                    else
                    {
                        Soil = (FamilyInstance)soils[0].Element(doc); // 找到有效且唯一的土体单元 ^_^
                    }
                }
            }
            Soil_Model SM = null;
            if (Soil != null)
            {
                SM = Soil_Model.Create(this, Soil);
            }
            return SM;
        }

        /// <summary> 搜索文档中与模型土体位于同一个Group中的所有的开挖土体。 </summary>
        /// <param name="soilM">文档中的模型土体单元，可以通过 ExcavationDoc.GetSoilModel 函数获得</param>
        /// <returns></returns>
        /// <remarks> 搜索的基本准则是开挖土体与模型土体位于同一个组中。但是开挖土体并不一定要剪切模型土体。 </remarks>
        public List<Soil_Excav> FindExcavSoils(Soil_Model soilM)
        {
            List<Soil_Excav> SoilEx = new List<Soil_Excav>();
            if (soilM == null) return SoilEx;

            //
            // 首先在模型Group中进行搜索
            FamilyInstance sm = soilM.Soil;
            List<ElementId> elemIds = soilM.Group.GetMemberIds() as List<ElementId>;
            if (elemIds != null && elemIds.Count > 0)
            {
                FilteredElementCollector col = new FilteredElementCollector(Document, elemIds);
                // 所有的模型土体与开挖土体集合
                IList<Element> Elems =
                    col.OfClass(typeof(FamilyInstance))
                        .OfCategoryId(new ElementId(BuiltInCategory.OST_Site))
                        .ToElements();

                // 排除模型土体并生成对应的开挖土体对象 
                ElementId smId = soilM.Soil.Id;
                SoilEx.AddRange(
                    from e in Elems
                    where e.Id != smId
                    // 说明是开挖土体
                    select Soil_Excav.Create((FamilyInstance)e, soilM));
            }
            return SoilEx;
        }


        /// <summary>
        /// 根据施工日期来判断土体的开挖状态
        /// </summary>
        /// <param name="excavSoils">要进行过滤的开挖土体集合</param>
        /// <param name="constructionTime"> 要考查的施工日期 </param>
        /// <returns> excavSoils 集合中每一个开挖土体所对应的开挖状态 </returns>
        public Dictionary<Soil_Excav, ExcavationStage> FilterExcavSoils(
            IEnumerable<Soil_Excav> excavSoils, DateTime constructionTime)
        {
            Dictionary<Soil_Excav, ExcavationStage> soils = new Dictionary<Soil_Excav, ExcavationStage>();
            //

            foreach (Soil_Excav es in excavSoils)
            {
                if (soils.ContainsKey(es)) continue;
                //
                var startTime = es.GetExcavatedDate(true);
                var endTime = es.GetExcavatedDate(false);
                //
                if (startTime == null && endTime == null)
                {
                    soils.Add(es, ExcavationStage.UnKown);
                }
                else if (startTime == null && endTime != null)  // 只知道结束日期
                {
                    if (constructionTime > endTime.Value)
                    {
                        soils.Add(es, ExcavationStage.Completed);
                    }
                    else
                    {
                        soils.Add(es, ExcavationStage.UnKown);
                    }
                }
                else if (startTime != null && endTime == null)  // 只知道开始日期
                {
                    if (constructionTime < startTime.Value)
                    {
                        soils.Add(es, ExcavationStage.UnStarted);
                    }
                    else
                    {
                        soils.Add(es, ExcavationStage.Excavating);
                    }
                }
                else if (startTime != null && endTime != null)
                {
                    if (constructionTime < startTime.Value)
                    {
                        soils.Add(es, ExcavationStage.UnStarted);
                    }
                    else if (constructionTime > endTime.Value)
                    {
                        soils.Add(es, ExcavationStage.Completed);
                    }
                    else
                    {
                        soils.Add(es, ExcavationStage.Excavating);
                    }
                }
            }
            return soils;
        }

        #endregion

        /// <summary>
        /// 在Revit的项目浏览器中，土体族位于“族>场地”之中，常规情况下，每一个族中只有一个族类型，
        /// 因为每一个模型土体或者开挖土体，都是通过唯一的曲线创建出来的（在后期的开发中，可能会将其修改为通过“自适应常规模型”来创建土体。）。
        /// 当模型使用很长一段时间后，出于各种原因，一个模型中可能有很多的开挖土体族都已经没有实例了，这时就需要将这些没有实例的开挖土体族删除。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public void DeleteEmptySoilFamily(Transaction tranDoc)
        {
            try
            {
                tranDoc.SetName("删除没有实例的土体族及对应的族类型");

                List<Family> fams = Document.FindFamilies(BuiltInCategory.OST_Site);
                List<ElementId> famsToDelete = new List<ElementId>();
                string deletedFamilyName = "";
                foreach (Family f in fams)
                {
                    if (!f.Instances(BuiltInCategory.OST_Site).Any())
                    {
                        famsToDelete.Add(f.Id);
                        deletedFamilyName = deletedFamilyName + f.Name + " ,";
                    }
                }

                Document.Delete(famsToDelete);

                MessageBox.Show(@"删除空的土体族对象成功。删除掉的族对象有：" + "\r\n（" + deletedFamilyName + @"）", @"成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("删除空的土体族对象失败", ex);

            }
        }
    }
}
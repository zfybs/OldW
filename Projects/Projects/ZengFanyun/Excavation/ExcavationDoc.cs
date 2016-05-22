
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Forms;
using OldW.GlobalSettings;
using System.IO;
using rvtTools_ez;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Constants = OldW.GlobalSettings.Constants;

namespace OldW.Excavation
{
	
	/// <summary>
	/// 用来执行基坑开挖模拟中的数据存储与绘制操作
	/// </summary>
	/// <remarks></remarks>
	public class ExcavationDoc : OldWDocument
	{
		
#region    ---   Fields
		
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
		
		private Autodesk.Revit.ApplicationServices.Application App;
		
		/// <summary>
		/// 文档中已经检索出来的模型土体，这个对象可能为空，也可能是一个无效的对象。
		/// </summary>
		public OldW.Soil.Soil_Model ModelSoil {get; set;}
		
#endregion
		
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
		public OldW.Soil.Soil_Model CreateModelSoil(double Depth, CurveArrArray CurveArrArr)
		{
			OldW.Soil.Soil_Model SM = null;
			
			if (!CurveArrArr.IsEmpty)
			{
				// 构造一个族文档
				Document FamilyDoc = CreateSoilFamily();
				ExtrusionAndBindingDimension(FamilyDoc, CurveArrArr, Depth);
				
				// 将族加载到项目文档中
				Family fam = FamilyDoc.LoadFamily(Doc, UIDocument.GetRevitUIFamilyLoadOptions());
				FamilyDoc.Close(false);
				// 获取一个族类型，以加载一个族实例到项目文档中
				FamilySymbol fs = fam.GetFamilySymbolIds().First().Element(Doc) as FamilySymbol;
				
				using (Transaction t = new Transaction(Doc, "添加族实例"))
				{
					t.Start();
					// 将模型土体放到Group中
					
					List<ElementId> GroupMems = new List<ElementId>();
					GroupType gptp = (GroupType)rvtTools.FindElement(Doc, typeof(GroupType), targetName: Constants.FamilyName_Soil);
					// 组类型中包含有族实例，所以要找到对应的组类型，然后找到对应的组实例，再将组实例解组。
					// 这是为了避免在删除“基坑土体”族及其实例时，UI界面中会出现“删除组实例中的最后一个成员”的警告。
					Group gp = default(Group);
					if (gptp != null)
					{
						gp =(Group) rvtTools.FindElement( Doc, typeof(Group),targetName: Constants.FamilyName_Soil);
						if (gp != null)
						{
							GroupMems = gp.GetMemberIds() as List<ElementId>;
							gp.UngroupMembers();
						}
						Doc.Delete(gptp.Id);
					}
					
					//
					Family f = Doc.FindFamily(Constants.FamilyName_Soil, BuiltInCategory.OST_Site);
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
						Doc.Delete(f.Id);
					}
					
					// 为族与族类型重命名
					fam.Name = Constants.FamilyName_Soil;
					fs.Name = Constants.FamilyName_Soil;
					
					// 生成族实例
					if (!fs.IsActive)
					{
						fs.Activate();
					}
					FamilyInstance fi = Doc.Create.NewFamilyInstance(new XYZ(), fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
					
					// 重新构造Group
					Doc.Regenerate();
					GroupMems.Add(fi.Id);
					gp = Doc.Create.NewGroup(GroupMems);
					gp.GroupType.Name = Constants.FamilyName_Soil;
					//
					SM = OldW.Soil.Soil_Model.Create(this, fi);
					t.Commit();
				}
				
			}
			return SM;
		}
		
		/// <summary>
		/// 创建开挖土体，此土体单元在模型中可以有很多个。
		/// </summary>
		/// <param name="p_ModelSoil">在创建开挖土体之前，请先确保已经创建好了模型土体。
		/// 在此方法中，模型土体对象并不起任何作用，只是用来确保模型土体对象已经创建。</param>
		/// <param name="CurveArrArr"> 要进行拉伸的平面轮廓（可以由多个封闭的曲线组成） </param>
		/// <param name="Depth">开挖土体的深度，单位为m，数值为正表示向下的深度，反之表示向上的高度。</param>
		/// <param name="DesiredName">此开挖土体实例的名称（推荐以开挖完成的日期）。如果此名称已经被使用，则以默认的名称来命名。</param>
		/// <returns></returns>
		public OldW.Soil.Soil_Excav CreateExcavationSoil(OldW.Soil.Soil_Model p_ModelSoil, double Depth, CurveArrArray CurveArrArr, string DesiredName)
			{
			
			OldW.Soil.Soil_Excav SE = null;
			if (this.ModelSoil != null)
			{
				
				// 获得用来创建实体的模型线
				
				if (!CurveArrArr.IsEmpty)
				{
					// 构造一个族文档
					Document FamDoc = CreateSoilFamily();
					ExtrusionAndBindingDimension(FamDoc, CurveArrArr, Depth);
					//
					using (Transaction t = new Transaction(FamDoc, "添加参数：开挖的开始与完成日期"))
					{
						try
						{
							t.Start();
							// 添加参数
							FamilyManager FM = FamDoc.FamilyManager;
							// 开挖开始
							ExternalDefinition ExDef = rvtTools.GetOldWDefinitionGroup(App).Definitions.get_Item(Constants.SP_ExcavationStarted) as ExternalDefinition;
							FM.AddParameter(ExDef, BuiltInParameterGroup.PG_DATA, true);
							// 开挖完成
							ExDef = rvtTools.GetOldWDefinitionGroup(App).Definitions.get_Item(Constants.SP_ExcavationCompleted) as ExternalDefinition;
							FM.AddParameter(ExDef, BuiltInParameterGroup.PG_DATA, true);
							t.Commit();
						}
						catch (Exception ex)
						{
							DialogResult res = MessageBox.Show("添加开挖的完成日期参数失败！" + "\r\n" + ex.Message, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
							t.RollBack();
						}
					}
					
					
					// 将族加载到项目文档中
					Family fam = FamDoc.LoadFamily(Doc, UIDocument.GetRevitUIFamilyLoadOptions());
					FamDoc.Close(false);
					// 获取一个族类型，以加载一个族实例到项目文档中
					FamilySymbol fs =(FamilySymbol) fam.GetFamilySymbolIds().First().Element(Doc);
					using (Transaction t = new Transaction(Doc, "添加族实例"))
					{
						t.Start();
						// 族或族类型的重命名
						
						string soilName = GetValidExcavationSoilName(Doc, DesiredName);
						fam.Name = soilName;
						fs.Name = soilName;
						
						// 创建实例
						if (!fs.IsActive)
						{
							fs.Activate();
						}
						FamilyInstance fi = Doc.Create.NewFamilyInstance(new XYZ(), fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
						SE = OldW.Soil.Soil_Excav.Create(fi, p_ModelSoil);
						t.Commit();
					}
					
				}
			}
			else
			{
				throw (new NullReferenceException("请先在文档中创建出对应的模型土体。"));
			}
			return SE;
		}
		
#endregion
		
		/// <summary>
		/// 创建一个模型土体（或者是开挖土体）的族文档，并将其打开。
		/// </summary>
		private Document CreateSoilFamily()
		{
			var app = Doc.Application;
			string TemplateName = Path.Combine(ProjectPath.Path_family, Constants.FamilyTemplateName_Soil);
			if (!File.Exists(TemplateName))
			{
				throw (new FileNotFoundException("在创建土体时，族样板文件没有找到", TemplateName));
			}
			else
			{
				// 创建族文档
				Document FamDoc = App.NewFamilyDocument(TemplateName);
				using (Transaction T = new Transaction(FamDoc, "设置族类别"))
				{
					T.Start();
					// 设置族的族类别
					FamDoc.OwnerFamily.FamilyCategory = FamDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Site);
					// 设置族的名称
					T.Commit();
				}
				
				return FamDoc;
			}
		}
		
		/// <summary>
		/// 绘制拉伸实体，并将其深度值与具体参数关联起来。
		/// </summary>
		/// <param name="FamDoc">实体所在的族文档，此文档当前已经处于打开状态。</param>
		/// <param name="CurveAA">用来绘制实体的闭合轮廓</param>
		/// <param name="Depth">模型土体的深度，单位为m，数值为正表示向下的深度，反之表示向上的高度。</param>
		/// <remarks></remarks>
		private void ExtrusionAndBindingDimension(Document FamDoc, CurveArrArray CurveAA, double Depth)
		{
			
			// 获取拉伸轮廓所在水平面的Z坐标值。
			Curve c = CurveAA.get_Item(0).get_Item(0).Clone();
			c.MakeBound(0, 1);
			double TopZ = System.Convert.ToDouble(c.GetEndPoint(0).Z);
			c.Dispose();
			
			// 确定拉伸方向与拉伸长度
			SByte Direction = 0;
			if (Depth > 0) // 说明是向下拉伸
			{
				Direction = -1;
			}
			else if (Depth < 0) // 说明是向上拉伸
			{
				Direction = 1;
			}
			else
			{
				throw (new ArgumentException("深度值不能为0！"));
			}
			Depth = Math.Abs(UnitUtils.ConvertToInternalUnits(Depth, DisplayUnitType.DUT_METERS)); // 将米转换为英尺。
			
			
			// 定义参考平面
			Plane P_Top = new Plane(new XYZ(0, 0, Direction), new XYZ(0, 0, TopZ));
			ReferencePlane RefP_Top = default(ReferencePlane);
			
			// 在族文档中绘制实体
			using (Transaction trans = new Transaction(FamDoc, "添加实体与参数关联"))
			{
				trans.Start();
				try
				{
                    Autodesk.Revit.DB.View V = rvtTools_ez.rvtTools.FindElement(FamDoc, typeof(Autodesk.Revit.DB.View), BuiltInCategory.OST_Views, "前") as Autodesk.Revit.DB.View; // 定义视图
					if (V == null)
					{
						throw (new NullReferenceException("当前视图为空。"));
					}
					
					RefP_Top = FamDoc.FamilyCreate.NewReferencePlane(P_Top.Origin, P_Top.Origin + P_Top.XVec, P_Top.YVec, V);
					RefP_Top.Name = "SoilTop";

                    Autodesk.Revit.Creation.FamilyItemFactory FamilyCreation = FamDoc.FamilyCreate;
					
					// 创建拉伸实体
					SketchPlane sp = SketchPlane.Create(FamDoc, P_Top); // P_Top 为坐标原点所在的水平面
					Extrusion extru = FamilyCreation.NewExtrusion(true, CurveAA, sp, Depth); // 创建拉伸实体
					
					// 将拉伸实体的顶面与参数平面进行绑定  '  ElementTransformUtils.MoveElement(FamDoc, extru.Id, New XYZ(0, 0, 0))
					PlanarFace Ftop = GeoHelper.FindFace(extru, new XYZ(0, 0, 1));
					FamilyCreation.NewAlignment(V, Ftop.Reference, RefP_Top.GetReference());
					
					//' 添加深度参数
					FamilyManager FM = FamDoc.FamilyManager;
					//’在进行参数读写之前，首先需要判断当前族类型是否存在，如果不存在，读写族参数都是不可行的
					if (FM.CurrentType == null)
					{
						FM.NewType("CurrentType"); // 随便取个名字即可，后期会将族中的第一个族类型名称统一进行修改。
					}
					FamilyParameter Para_Depth = FM.AddParameter("Depth", BuiltInParameterGroup.PG_GEOMETRY, ParameterType.Length, false);
					//' give initial values
					FM.Set(Para_Depth, Depth); // 这里不知为何为给出报错：InvalidOperationException:There is no current type.
					
					
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
					Dimension DimDepth = FamilyCreation.NewDimension(V, DimLine, refArray);
					
					// 将深度参数与其拉伸实体的深度值关联起来
					DimDepth.FamilyLabel = Para_Depth;
					trans.Commit();
				}
				catch (Exception ex)
				{
					MessageBox.Show("创建拉伸实体与参数关联出错。" + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					trans.RollBack();
				}
			}
			
			
		}
		
		// 开挖土体族的命名
		/// <summary>
		/// 在当前模型文档中，构造出一个有效的名称，来供开挖土体族使用。
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="ExcavationCompleteDate">日期的数据中请保证不包含小时或者更小的值，即请用“Date对象.Date”来进行赋值。</param>
		/// <returns></returns>
		/// <remarks>其基本格式为：开挖土体-2016/04/03-02</remarks>
		private string GetValidExcavationSoilNameFromDate(Document doc, DateTime ExcavationCompleteDate)
		{
			string prefix = "开挖土体-";
			List<Family> Fams = Doc.FindFamilies(BuiltInCategory.OST_Site);
			
			// 构造一个字典，其中包括的所有开挖土体族的名称中，每一个日期，以及对应的可能编号
			System.Collections.Generic.Dictionary<DateTime, List<UInt16>> Date_Count = new System.Collections.Generic.Dictionary<DateTime, List<UInt16>>();
			
			//
			DateTime dt = default(DateTime);
			ushort Id = 0;
			string FamName = "";
			//
			foreach (Family f in Fams)
			{
				FamName = System.Convert.ToString(f.Name);
				if (FamName.StartsWith(prefix)) // 说明可能是开挖土体
				{
					FamName = FamName.Substring(prefix.Length);
					string[] Cps = FamName.Split('-');
					if (Cps.Count() > 0)
					{
						if (DateTime.TryParse(Cps[0], out dt)) // 说明找到了一个开挖土体族，其对应的日期为dt
						{
							List<UInt16> Ids = default(List<UInt16>);
							
							if (Date_Count.Keys.Contains(dt)) // 说明此日期前面已经出现过了
							{
								Ids = Date_Count[dt]; // 返回日期所对应的值
								if (Ids == null) // 字典中的集合可能还未初始化
								{
									Ids = new List<UInt16>();
									Date_Count[dt] = Ids;
								}
								
							}
							else // 说明此日期第一次出现
							{
								Ids = new List<UInt16>();
								Date_Count.Add(dt, Ids);
							}
							
							if ((Cps.Count() == 2) && (UInt16.TryParse(Cps[1],out Id))) // 说明族名称中同时记录了日期与序号
							{
								Ids.Add(Id);
							}
							else // 说明族名称中只记录了日期
							{
								Ids.Add(0);
							}
						}
					}
				}
			}
			
			//
			string NewName = prefix + DateTime.Today.Date.ToString("yyyy/MM/dd"); // 先赋一个初值，避免出现问题
			
			if (!Date_Count.Keys.Contains(ExcavationCompleteDate))
			{
				NewName = prefix + ExcavationCompleteDate.ToString("yyyy/MM/dd");
			}
			else
			{
				UInt16 maxId = (ushort)(Date_Count[ExcavationCompleteDate].Max( ) + 1);
				NewName = prefix + ExcavationCompleteDate.ToString("yyyy/MM/dd") + "-" + maxId.ToString("00");
			}
			return NewName;
		}
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
			
			List<Family> Fams = Doc.FindFamilies(BuiltInCategory.OST_Site);
			// 构造一个字典，其中包括的所有开挖土体族的名称中，每一个日期，以及对应的可能编号
			List<string> FamNames = new List<string>();
			string FamName = "";
			
			// 提取所有的开挖土体族名称
			foreach (Family f in Fams)
			{
				FamName = System.Convert.ToString(f.Name);
				if (FamName.StartsWith(prefix)) // 说明是开挖土体
				{
					FamNames.Add(FamName);
				}
			}
			
			// 构造新名称
			int Num = 0;
			string Pre = "";
			string NewName = "";
			if (string.IsNullOrEmpty(DesiredName)) // 命名为“开挖-01”
			{
				int Max = 0;
				foreach (string n in FamNames)
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
				if (FamNames.Contains(DesiredName))
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
						foreach (string n in FamNames)
						{
							if (n.StartsWith(DesiredName + "-") && int.TryParse(n.Substring(System.Convert.ToInt32((DesiredName + "-").Length)), out Num))
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
					Prefix = Prefix + parts[(int) i] + "-";
				}
				return true;
			}
			else
			{
				return false;
			}
		}
		
#endregion
		
#region    ---    Document中的土体单元搜索
		
		/// <summary>
		/// 模型中的土体单元
		/// </summary>
		/// <param name="SoilElementId">可能的土体单元的ElementId值，如果没有待选的，可以不指定，此时程序会在整个Document中进行搜索。</param>
		/// <returns>如果成功搜索到，则返回对应的土体单元，如果没有找到，则返回Nothing</returns>
		/// <remarks></remarks>
		public OldW.Soil.Soil_Model FindSoilModel(int SoilElementId = -1)
		{
			string FMessage = null;
			if (ModelSoil == null)
			{
				
				ModelSoil = GetModelSoil(Doc, new ElementId(SoilElementId), ref FMessage);
				
			}
			else // 检查现有的这个模型土体单元是否还有效
			{
				if (!OldW.Soil.Soil_Model.IsSoildModel(Doc, ModelSoil.Soil.Id, ref FMessage))
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
		private OldW.Soil.Soil_Model GetModelSoil(Document doc, ElementId SoilElementId, ref string FailureMessage)
		{
			FamilyInstance Soil = null;
			if (SoilElementId != ElementId.InvalidElementId) // 说明用户手动指定了土体单元的ElementId，此时需要检测此指定的土体单元是否是有效的土体单元
			{
				try
				{
					string FMessage = null;
					if (OldW.Soil.Soil_Model.IsSoildModel(doc, SoilElementId, ref FMessage))
					{
						Soil = (FamilyInstance) (Doc.GetElement(SoilElementId));
					}
					else
					{
						throw (new InvalidCastException(FMessage));
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
				Family SoilFamily = Doc.FindFamily(Constants.FamilyName_Soil);
				if (SoilFamily != null)
				{
					// 实例的类别
					List<ElementId> soils = SoilFamily.Instances(BuiltInCategory.OST_Site).ToElementIds() as List<ElementId>;
					
					// 整个模型中只能有一个模型土体对象
					if (soils.Count == 0)
					{
						FailureMessage = "模型中没有土体单元";
					}
					else if (soils.Count > 1)
					{
						UIDocument UIDoc = new UIDocument(doc);
						UIDoc.Selection.SetElementIds(soils);
						FailureMessage = string.Format("模型中的土体单元数量多于一个，请删除多余的土体单元 ( 族\"{0}\"的实例对象 )。", Constants.FamilyName_Soil);
					}
					else
					{
						Soil = (FamilyInstance) (soils[0].Element(doc)); // 找到有效且唯一的土体单元 ^_^
					}
					
				}
			}
			OldW.Soil.Soil_Model SM = null;
			if (Soil != null)
			{
				SM = OldW.Soil.Soil_Model.Create(this, Soil);
			}
			return SM;
		}
		
		/// <summary>
		/// 搜索文档中与模型土体位于同一个Group中的所有的开挖土体。
		/// </summary>
		/// <param name="SoilM">文档中的模型土体单元，可以通过 ExcavationDoc.GetSoilModel 函数获得</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public List<OldW.Soil.Soil_Excav> FindExcavSoils(OldW.Soil.Soil_Model SoilM)
		{
			List<OldW.Soil.Soil_Excav> SoilEx = new List<OldW.Soil.Soil_Excav>();
			//
			// 首先在模型Group中进行搜索
			Group gp = SoilM.Group;
			FamilyInstance sm = SoilM.Soil;
			List<ElementId> ElemIds = gp.GetMemberIds() as List<ElementId>;
			if (ElemIds.Count > 0)
			{
				FilteredElementCollector col = new FilteredElementCollector(Doc, ElemIds);
				// 所有的模型土体与开挖土体集合
				List<Element> Elems = col.OfClass(typeof(FamilyInstance)).OfCategoryId(new ElementId(BuiltInCategory.OST_Site)).ToElements() as List<Element>;
				
				// 排队模型土体并生成对应的开挖土体对象
				ElementId smId = SoilM.Soil.Id;
				foreach (Element e in Elems)
				{
					if (e.Id != smId) // 说明是开挖土体
					{
						SoilEx.Add(OldW.Soil.Soil_Excav.Create((FamilyInstance)e, SoilM));
					}
				}
			}
			return SoilEx;
		}
		
#endregion
		
		/// <summary>
		/// 在Revit的项目浏览器中，土体族位于“族>场地”之中，常规情况下，每一个族中只有一个族类型，
		/// 因为每一个模型土体或者开挖土体，都是通过唯一的曲线创建出来的（在后期的开发中，可能会将其修改为通过“自适应常规模型”来创建土体。）。
		/// 当模型使用很长一段时间后，出于各种原因，一个模型中可能有很多的开挖土体族都已经没有实例了，这时就需要将这些没有实例的开挖土体族删除。
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		public bool DeleteEmptySoilFamily()
		{
			bool blnSuc = false;
			using (Transaction t = new Transaction(Doc, "删除文档中没有实例对象的土体族。"))
			{
				try
				{
					List<Family> Fams = Doc.FindFamilies(BuiltInCategory.OST_Site);
					List<ElementId> FamsToDelete = new List<ElementId>();
					string DeletedFamilyName = "";
					foreach (Family f in Fams)
					{
						if (f.Instances(BuiltInCategory.OST_Site).Count() == 0)
						{
							FamsToDelete.Add(f.Id);
							DeletedFamilyName = DeletedFamilyName + f.Name + "\r\n";
						}
					}
					t.Start();
					Doc.Delete(FamsToDelete);
					t.Commit();
					//  MessageBox.Show("删除空的土体族对象成功。删除掉的族对象有：" & vbCrLf & DeletedFamilyName, "恭喜", MessageBoxButtons.OK, MessageBoxIcon.None)
					blnSuc = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show("删除空的土体族对象失败" + "\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					t.RollBack();
					blnSuc = false;
				}
			}
			
			
			
			return blnSuc;
		}
		
	}
}

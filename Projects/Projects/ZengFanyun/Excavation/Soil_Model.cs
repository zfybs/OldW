using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Forms;
using rvtTools_ez;
using Autodesk.Revit.DB;

namespace OldW.Soil
{
	/// <summary>
	/// 基坑中的开挖土体，整个模型中，只有一个土体元素
	/// </summary>
	/// <remarks></remarks>
	public class Soil_Model : Soil_Element
	{
		
#region    ---   Properties
		
		private Group F_Group;
		/// <summary>
		/// 此模型土体所位于的组。
		/// 注意：所有的土体开挖模型都会位于此组中，如果将开挖土体从此组中移除，则有会被识别为开挖土体。
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
public Group Group
		{
			get
			{
				if (!F_Group.IsValidObject)
				{
					throw (new InvalidOperationException("请先将模型土体放置在一个组中。" + "\r\n" +
						"提示：所有的土体开挖模型都会位于此组中，如果将开挖土体从此组中移除，则有会被识别为开挖土体。"));
				}
				return F_Group;
			}
		}
		
#endregion
		
#region    ---   构造函数，通过 OldWDocument.GetSoilModel，或者是Create静态方法
		
		/// <summary>
		/// 构造函数：不要直接通过New Soil_Model来创建此对象，而应该用 OldWDocument.GetSoilModel，或者是Create静态方法来从模型中返回。
		/// </summary>
		/// <param name="ExcavDoc">模型土体单元所位于的文档</param>
		/// <param name="ModelSoil">模型中的开挖土体单元</param>
		/// <remarks></remarks>
		private Soil_Model(OldW.Excavation.ExcavationDoc ExcavDoc, FamilyInstance ModelSoil)
            : base(ExcavDoc, ModelSoil)
        {
            // 检查模型土体单元是否位于一个组中
            F_Group =(Group) ModelSoil.GroupId.Element(Doc);
			ExcavDoc.ModelSoil = this;
			if ((F_Group == null) || (!F_Group.IsValidObject))
			{
				throw (new InvalidOperationException("请先将模型土体放置在一个组中。" + "\r\n" +
					"提示：所有的土体开挖模型都会位于此组中，如果将开挖土体从此组中移除，则有会被识别为开挖土体。"));
			}
		}
		
		/// <summary>
		/// 对于一个单元进行全面的检测，以判断其是否为一个模型土体单元。
		/// </summary>
		/// <param name="doc">进行土体单元搜索的文档</param>
		/// <param name="SoilElementId">可能的土体单元的ElementId值</param>
		/// <param name="FailureMessage">如果不是，则返回不能转换的原因。</param>
		/// <returns>如果检查通过，则可以直接通过Create静态方法来创建对应的模型土体</returns>
		/// <remarks></remarks>
		public static bool IsSoildModel(Document doc, ElementId SoilElementId, ref string FailureMessage)
		{
			bool blnSucceed = false;
			if (SoilElementId != ElementId.InvalidElementId) // 说明用户手动指定了土体单元的ElementId，此时需要检测此指定的土体单元是否是有效的土体单元
			{
				try
				{
					FamilyInstance Soil = (FamilyInstance) (SoilElementId.Element(doc));
					// 是否满足基本的土体单元的条件
					if (!Soil_Element.IsSoildElement(doc, SoilElementId, ref FailureMessage))
					{
						throw (new InvalidCastException(FailureMessage));
					}
					// 进行细致的检测
					// 3. 是否在组中
					Group gp =(Group) Soil.GroupId.Element(doc);
					if (!gp.IsValidObject)
					{
						throw (new InvalidCastException("指定的ElementId所对应的单元并不在任何一个组内。"));
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
		/// 创建模型土体。除非是在API中创建出来，否则。在创建之前，请先通过静态方法IsSoildModel来判断此族实例是否可以转换为Soil_Model对象。否则，在程序运行过程中可能会出现各种报错。
		/// </summary>
		/// <param name="ExcavDoc"></param>
		/// <param name="SoilElement"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static Soil_Model Create(OldW.Excavation.ExcavationDoc ExcavDoc, FamilyInstance SoilElement)
		{
			return new Soil_Model(ExcavDoc, SoilElement);
		}
		
#endregion
		
		/// <summary>
		/// 从当前的开挖状态中，移除指定的一块土，用来模拟土体的开挖
		/// </summary>
		/// <param name="SoilToRemove"></param>
		/// <remarks></remarks>
		public bool RemoveSoil(Soil_Excav SoilToRemove)
		{
			bool blnSucceed = false;
			if (SoilToRemove != null)
			{
				FamilyInstance removedSoil = SoilToRemove.Soil;
				using (Transaction tran = new Transaction(Doc, "土体开挖"))
				{
					try
					{
						tran.Start();
						// 将要进行开挖的土体单元也添加进模型土体的组合中
						Group gp = this.Group;
						ICollection<ElementId> elems = gp.GetMemberIds();
						elems.Add(removedSoil.Id);
                       //
						gp.UngroupMembers(); // 将Group实例删除
						Doc.Delete(gp.GroupType.Id); // 删除组类型
						
						// 进行开挖土体对于模型土体的剪切操作
						CutFailureReason CFR = default(CutFailureReason);
						bool blnCanCut = System.Convert.ToBoolean(SolidSolidCutUtils.CanElementCutElement(removedSoil, this.Soil,out CFR));
						if (blnCanCut)
						{
							
							SolidSolidCutUtils.AddCutBetweenSolids(Doc, this.Soil, removedSoil);
                            // 将用来剪切的开挖土体在视图中进行隐藏
                            Autodesk.Revit.DB.View V = Doc.ActiveView;
							if (V == null)
							{
								throw (new NullReferenceException("未找到有效的视图对象。"));
							}
							V.HideElements(new[] {removedSoil.Id});
						}
						else
						{
							if (CFR == CutFailureReason.CutNotAppropriateForElements || CFR == CutFailureReason.OppositeCutExists)
							{
								throw (new InvalidOperationException("开挖土体不能对模型土体进行剪切，其原因为：" + "\r\n" +
									CFR.ToString()));
							}
						}
						
						// 重新构造Group，它将产生一个新的GroupType
						Doc.Regenerate();
						this.F_Group = Doc.Create.NewGroup(elems); // 	在通过NewGroup创建出组后，可以对组内的元素进行隐藏或移动等操作，但是最好不要再对组内的元素进行剪切，否则还是可能会在UI中出现“the group has changed outside group edit mode.”的警告。
						this.F_Group.GroupType.Name = "基坑土体";
						
						tran.Commit();
						blnSucceed = true;
					}
					catch (Exception ex)
					{
						MessageBox.Show("土体开挖失败！" + "\r\n" + ex.Message, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
						tran.RollBack();
					}
				}
				
			}
			return blnSucceed;
		}
		
		/// <summary>
		/// 从当前的开挖状态中，移除指定的一块土，用来模拟土体的开挖
		/// </summary>
		/// <param name="SoilsToRemove"></param>
		/// <remarks></remarks>
		public bool RemoveSoils(List<Soil_Excav> SoilsToRemove)
		{
			bool blnSucceed = false;
			
			if (SoilsToRemove.Count > 0)
			{
				
				using (Transaction tran = new Transaction(Doc, "土体开挖"))
				{
					tran.Start();
					try
					{

                        // 将用来剪切的开挖土体在视图中进行隐藏
                        Autodesk.Revit.DB.View V = Doc.ActiveView;
						if (V == null)
						{
							throw (new NullReferenceException("未找到有效的视图对象。"));
						}
						
						// 将要进行开挖的土体单元也添加进模型土体的组合中
						Group gp = this.Group;
						List<ElementId> GroupedElems = gp.UngroupMembers().ToList(); // 将Group实例删除
						
						List<ElementId> AddedSoil = new List<ElementId>();
						foreach (var ele in SoilsToRemove)
						{
							if (!GroupedElems.Contains(ele.Soil.Id))
							{
								AddedSoil.Add(ele.Soil.Id);
							}
						}
						GroupedElems.AddRange(AddedSoil);
						//
						Doc.Delete(gp.GroupType.Id); // 删除组类型
						
						// 进行开挖土体对于模型土体的剪切操作
						List<ElementId> RemovedSoilId = new List<ElementId>();
						CutFailureReason CFR = default(CutFailureReason);
						foreach (var SoilEx in SoilsToRemove)
						{
							RemovedSoilId.Add(SoilEx.Soil.Id);
							bool blnCanCut = System.Convert.ToBoolean(SolidSolidCutUtils.CanElementCutElement(SoilEx.Soil, this.Soil,out CFR));
							if (blnCanCut)
							{
								SolidSolidCutUtils.AddCutBetweenSolids(Doc, this.Soil, SoilEx.Soil);
							}
							else
							{
								if (CFR == CutFailureReason.CutNotAppropriateForElements || CFR == CutFailureReason.OppositeCutExists)
								{
									throw (new InvalidOperationException("开挖土体不能对模型土体进行剪切，其原因为：" + "\r\n" +
										CFR.ToString()));
								}
							}
						}
						
						V.HideElements(RemovedSoilId);
						
						// 重新构造Group，它将产生一个新的GroupType
						Doc.Regenerate();
						this.F_Group = Doc.Create.NewGroup(GroupedElems); // 	在通过NewGroup创建出组后，可以对组内的元素进行隐藏或移动等操作，但是最好不要再对组内的元素进行剪切，否则还是可能会在UI中出现“the group has changed outside group edit mode.”的警告。
						this.F_Group.GroupType.Name = "基坑土体";
						
						tran.Commit();
						blnSucceed = true;
					}
					catch (Exception ex)
					{
						MessageBox.Show("土体开挖失败！" + "\r\n" + ex.Message, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
						tran.RollBack();
					}
				}
				
			}
			return blnSucceed;
		}
		
		
		
		/// <summary>
		/// 从当前的开挖状态中，添加进指定的一块土，用来模拟土方的回填，或者反向回滚开挖状态
		/// </summary>
		/// <param name="SoilToRemove"></param>
		/// <remarks></remarks>
		public void FillSoil(Soil_Excav SoilToRemove)
		{
			
			
		}
		
	}
}

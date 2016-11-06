using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using RevitStd;

namespace OldW.Excavation
{
    /// <summary>
    /// 基坑中的开挖土体，整个模型中，只有一个土体元素
    /// </summary>
    /// <remarks></remarks>
    public class Soil_Model : Soil_Element
    {
        #region    ---   Properties

        private Group _group;
        /// <summary>
        /// 此模型土体所位于的组。
        /// 注意：所有的土体开挖模型都会位于此组中，如果将开挖土体从此组中移除，则不会被识别为开挖土体。
        /// </summary>
        /// <remarks></remarks>
        public Group Group
        {
            get
            {
                if (_isUnGrouped || !_group.IsValidObject)
                {
                    throw new InvalidOperationException(@"无法提取模型土体所在的组。");
                }
                return _group;
            }
        }


        /// <summary> 当前土体模型是否处于解组状态 </summary>
        private bool _isUnGrouped;

        #endregion

        #region    ---   RemoveSoil 剪切土体以模拟开挖

        /// <summary>
        /// 从当前的开挖状态中，移除指定的一块土，用来模拟土体的开挖
        /// </summary>
        /// <param name="tranDoc"></param>
        /// <param name="soilToRemove"> 此开挖土体应该与模型土体位于同一个 Group 内，
        /// 但是如果不在同一个组内，本方法也会将其转移到同一个组内。 </param>
        /// <remarks></remarks>
        public bool RemoveSoil(Transaction tranDoc, Soil_Excav soilToRemove)
        {
            bool blnSucceed = false;
            if (soilToRemove != null)
            {
                FamilyInstance removedSoil = soilToRemove.Soil;
                tranDoc.SetName("将开挖土体从模型土体中隐藏");
                try
                {
                    // 将要进行开挖的土体单元也添加进模型土体的组合中
                    Group gp = this.Group;
                    ICollection<ElementId> excavSoilIds = UnGroup(tranDoc);

                    if (!excavSoilIds.Contains(removedSoil.Id))
                    {
                        excavSoilIds.Add(removedSoil.Id);
                    }


                    // 进行开挖土体对于模型土体的剪切操作
                    CutFailureReason CFR = default(CutFailureReason);
                    bool blnCanCut =
                        Convert.ToBoolean(SolidSolidCutUtils.CanElementCutElement(removedSoil, this.Soil, out CFR));
                    if (blnCanCut)
                    {
                        SolidSolidCutUtils.AddCutBetweenSolids(Document, this.Soil, removedSoil);
                        // 将用来剪切的开挖土体在视图中进行隐藏
                        //View V = Document.ActiveView;
                        //if (V == null)
                        //{
                        //    throw new NullReferenceException("未找到有效的视图对象。");
                        //}
                        //V.HideElements(new[] { removedSoil.Id });
                    }
                    else
                    {
                        if (CFR == CutFailureReason.CutNotAppropriateForElements ||
                            CFR == CutFailureReason.OppositeCutExists)
                        {
                            throw new InvalidOperationException("开挖土体不能对模型土体进行剪切，其原因为：" + "\r\n" +
                                                                CFR.ToString());
                        }
                    }

                    // 重新构造Group
                    ReGroup(tranDoc, excavSoilIds);

                    blnSucceed = true;
                }
                catch (Exception ex)
                {
                    // 事务回滚后一定要重新设置_group，因为在事务commit之前，_group被指定给了一个组，但是这个组在回滚操作后被清除了。
                    this._group = (Group)Document.FindElement(typeof(Group), targetName: Constants.FamilyName_Soil);
                    //
                    throw new InvalidOperationException($"事务“{tranDoc.GetName()}”出错。土体开挖失败",ex);
                }
            }
            return blnSucceed;
        }

        /// <summary>
        /// 从当前的开挖状态中，移除指定的开挖土体集合，用来模拟土体的开挖
        /// </summary>
        /// <param name="tranDoc"></param>
        /// <param name="soilsToRemove"> 集合中的开挖土体应该与模型土体位于同一个 Group 内，
        /// 但是如果不在同一个组内，本方法也会将其转移到同一个组内。</param>
        /// <remarks></remarks>
        public void RemoveSoils(Transaction tranDoc, List<Soil_Excav> soilsToRemove)
        {
            if (soilsToRemove.Count <= 0) return;

            tranDoc.SetName("将多个开挖土体单元对模型土体进行剪切操作");
            try
            {
                // 将要进行开挖的土体单元也添加进模型土体的组合中
                //
                var excavSoils = UnGroup(tranDoc);

                //
                List<ElementId> AddedSoil = new List<ElementId>();
                foreach (var ele in soilsToRemove)
                {
                    if (!excavSoils.Contains(ele.Soil.Id))
                    {
                        AddedSoil.Add(ele.Soil.Id);
                    }
                }



                // 进行开挖土体对于模型土体的剪切操作
                CutFailureReason cfr = default(CutFailureReason);
                foreach (var soilEx in soilsToRemove)
                {
                    bool blnCanCut = SolidSolidCutUtils.CanElementCutElement(soilEx.Soil, this.Soil, out cfr);
                    if (blnCanCut)
                    {
                        SolidSolidCutUtils.AddCutBetweenSolids(Document, this.Soil, soilEx.Soil);
                    }
                    else
                    {
                        if (cfr == CutFailureReason.CutNotAppropriateForElements ||
                            cfr == CutFailureReason.OppositeCutExists)
                        {
                            throw new InvalidOperationException("开挖土体不能对模型土体进行剪切，其原因为：" + "\r\n" +
                                                                cfr);
                        }
                    }
                }

                //
                ReGroup(tranDoc, excavSoils);

                //// 重新构造Group，它将产生一个新的GroupType
                //Document.Regenerate();
                //this._group = Document.Create.NewGroup(GroupedElems);
                //// 	在通过NewGroup创建出组后，可以对组内的元素进行隐藏或移动等操作，但是最好不要再对组内的元素进行剪切，否则还是可能会在UI中出现“the group has changed outside group edit mode.”的警告。
                //this._group.GroupType.Name = Constants.SoilGroupName; // "基坑土体";
            }
            catch (Exception ex)
            {
                // 事务回滚后一定要重新设置_group，因为在事务commit之前，_group被指定给了一个组，但是这个组在回滚操作后被清除了。
                this._group = (Group)Document.FindElement(typeof(Group), targetName: Constants.SoilGroupName);

                //
                throw new InvalidOperationException($"事务“{tranDoc.GetName()}”出错", ex);
            }
        }


        /// <summary>
        /// 设置模型土体所在的 group 中所有的开挖土体集合与此模型土体的剪切关系
        /// </summary>
        /// <param name="tranDoc"></param>
        /// <remarks></remarks>
        public void RemoveSoils(Transaction tranDoc)
        {
            tranDoc.SetName("将多个开挖土体单元对模型土体进行剪切操作");
            try
            {
                // 将要进行开挖的土体单元也添加进模型土体的组合中
                Group gp = this.Group;
                List<ElementId> excavSoilIds = UnGroup(tranDoc); // 将Group实例删除
                if (excavSoilIds.Any())
                {
                    // 进行开挖土体对于模型土体的剪切操作
                    CutFailureReason cfr = default(CutFailureReason);
                    foreach (var excavSoilId in excavSoilIds)
                    {
                        Element excavSoil = Document.GetElement(excavSoilId);
                        bool blnCanCut = SolidSolidCutUtils.CanElementCutElement(cuttingElement: excavSoil,
                            cutElement: Soil, reason: out cfr);
                        if (blnCanCut)
                        {
                            SolidSolidCutUtils.AddCutBetweenSolids(Document, this.Soil, excavSoil);
                        }
                        else
                        {
                            if (cfr == CutFailureReason.CutNotAppropriateForElements ||
                                cfr == CutFailureReason.OppositeCutExists)
                            {
                                throw new InvalidOperationException("开挖土体不能对模型土体进行剪切，其原因为：" + "\r\n" +
                                                                    cfr);
                            }
                        }
                    }
                }
                // 
                ReGroup(tranDoc, excavSoilIds);
            }
            catch (Exception ex)
            {
                //
                throw new InvalidOperationException($"事务“{tranDoc.GetName()}”出错", ex);
            }
        }

        #endregion

        /// <summary>
        /// 从当前的开挖状态中，添加进指定的一块土，用来模拟土方的回填，或者反向回滚开挖状态
        /// </summary>
        /// <param name="SoilToRemove"></param>
        /// <remarks></remarks>
        public void FillSoil(Soil_Excav SoilToRemove)
        {
        }

        #region    ---   构造函数，通过 OldWDocument.GetSoilModel，或者是Create静态方法

        /// <summary>
        /// 构造函数：不要直接通过New Soil_Model来创建此对象，而应该用 OldWDocument.GetSoilModel，或者是Create静态方法来从模型中返回。
        /// </summary>
        /// <param name="excavDoc">模型土体单元所位于的文档</param>
        /// <param name="ModelSoil">模型中的开挖土体单元</param>
        /// <remarks></remarks>
        private Soil_Model(ExcavationDoc excavDoc, FamilyInstance ModelSoil)
            : base(excavDoc, ModelSoil)
        {
            // 检查模型土体单元是否位于一个组中
            _group = (Group)ModelSoil.GroupId.Element(Document);
            if ((_group == null) || !_group.IsValidObject)
            {
                throw new InvalidOperationException("无法构造模型土体：请先将模型土体放置在一个组中。" + "\r\n" +
                                                    "提示：所有的土体开挖模型都会位于此组中，如果将开挖土体从此组中移除，则不会被识别为开挖土体。");
            }
            if (_group.Name != Constants.SoilGroupName)
            {
                throw new InvalidOperationException($"模型土体并未在指定名称“ {Constants.SoilGroupName} ”的组中。");
            }
            excavDoc.ModelSoil = this;
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
                    FamilyInstance Soil = (FamilyInstance)SoilElementId.Element(doc);
                    // 是否满足基本的土体单元的条件
                    if (!IsSoilElement(doc, SoilElementId, ref FailureMessage))
                    {
                        throw new InvalidCastException(FailureMessage);
                    }
                    // 进行细致的检测
                    // 3. 是否在组中
                    Group gp = (Group)Soil.GroupId.Element(doc);
                    if (!gp.IsValidObject)
                    {
                        throw new InvalidCastException("指定的ElementId所对应的单元并不在任何一个组内。");
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
        public static Soil_Model Create(ExcavationDoc ExcavDoc, FamilyInstance SoilElement)
        {
            return new Soil_Model(ExcavDoc, SoilElement);
        }

        #endregion

        #region    ---   模型土体 group 的解组与重组操作

        /// <summary>
        /// 将模型土体所在的 group 进行解组操作，并返回其中的开挖土体。
        /// 注意在解组后务必还要通过 ReGroup 方法进行重组。
        /// </summary>
        /// <param name="tranDoc"></param>
        /// <returns> 此组合中所有的开挖土体单元 </returns>
        public List<ElementId> UnGroup(Transaction tranDoc)
        {
            if (!_isUnGrouped)
            {
                // 将要进行开挖的土体单元也添加进模型土体的组合中
                Group gp = this.Group;

                // 删除  group  的实例
                List<ElementId> excavSoilIds = this.GetExcavSoilsInGroup(true);

                // 删除 group 类型
                Document.Delete(gp.GroupType.Id);
                //
                _isUnGrouped = true;
                return excavSoilIds;
            }
            else
            {
                throw new InvalidOperationException("当前土体模型已经解组，不能再重复解组操作");
            }
        }

        /// <summary>
        /// 将通过 UnGroup 方法进行解组后的 group 重新组合起来
        /// </summary>
        /// <param name="tranDoc"></param>
        /// <param name="excavSoils"> 开挖土体单元的集合，集合中也可以包含模型土体单元。</param>
        public void ReGroup(Transaction tranDoc, ICollection<ElementId> excavSoils)
        {
            if (_isUnGrouped)
            {
                // 将模型土体添加到集合中
                if (!excavSoils.Contains(Soil.Id))
                {
                    excavSoils.Add(this.Soil.Id);
                }

                // 重新构造Group，它将产生一个新的GroupType
                Document.Regenerate();
                //
                this._group = Document.Create.NewGroup(excavSoils);
                // 	在通过NewGroup创建出组后，可以对组内的元素进行隐藏或移动等操作，但是最好不要再对组内的元素进行剪切，否则还是可能会在UI中出现“the group has changed outside group edit mode.”的警告。
                this._group.GroupType.Name = Constants.SoilGroupName; // "基坑土体";
                _isUnGrouped = false;
            }
            else
            {
                throw new InvalidOperationException("当前土体模型并未解组，不能再次组合。如果要进行解组，请先调用 UnGroup() 方法。");
            }
        }


        #endregion

        #region    ---   模型土体所在的 Group 中是否包含指定的开挖土体单元

        /// <summary>
        /// 模型土体所在的 Group 中是否包含指定的单元（除了模型土体单元本身）
        /// </summary>
        /// <param name="soilId"> 要搜索的开挖土体的 Id 值（可以包含模型土体，但是对应的值会返回 false）</param>
        /// <returns></returns>
        public bool ContainsExcavSoil(ElementId soilId)
        {
            var elems = this.GetExcavSoilsInGroup(false);
            // 排除了模型土体之外的所有土体中是否包含指定开挖土体的 id 值
            return elems.Where(r => r.IntegerValue == soilId.IntegerValue).ToList().Count > 0;
        }

        /// <summary>
        /// 模型土体所在的 Group 中是否包含指定的单元（除了模型土体单元本身）
        /// </summary>
        /// <param name="soilIds">要搜索的开挖土体的 Id 值（可以包含模型土体，但是对应的值会返回 false）</param>
        /// <returns> 与 soilIds 集合相对应，表示每一个开挖土体是否包含在集合中。 </returns>
        public bool[] ContainsExcavSoils(ICollection<ElementId> soilIds)
        {
            int count = soilIds.Count;

            bool[] contain = new bool[count];
            var excavSoilInGroup = this.GetExcavSoilsInGroup(false).Select(r => r.IntegerValue).ToArray();
            //
            var excavSoil = soilIds.GetEnumerator();

            for (int i = 0; i < count; i++)
            {
                excavSoil.MoveNext();  // 第 i 个 开挖土体
                if (excavSoilInGroup.Contains(excavSoil.Current.IntegerValue))
                {
                    contain[i] = true;
                }
                else
                {
                    contain[i] = false;
                }
            }

            // 
            return contain;
        }

        /// <summary> 获取模型土体所在的 group 中的其他所有的（开挖土体）单元 </summary>
        /// <param name="unGroup">只提取组合中的开挖土体，还是要附带将这个组合进行解组操作</param>
        /// <returns></returns>
        public List<ElementId> GetExcavSoilsInGroup(bool unGroup)
        {
            if (unGroup)
            {
                return Group.UngroupMembers().Where(r => r != Soil.Id).ToList();
            }
            else
            {
                return Group.GetMemberIds().Where(r => r != Soil.Id).ToList();
            }
        }

        #endregion
    }
}
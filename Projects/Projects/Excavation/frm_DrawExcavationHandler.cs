// VBConversions Note: VB project level imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using eZstd.Miscellaneous;
using rvtTools;
using rvtTools.Curves;
using View = System.Windows.Forms.View;

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
// End of VB project level imports

namespace OldW.Excavation
{
    public partial class frm_DrawExcavation
    {
        #region    ---   RequestParameter

        /// <summary>
        /// 每一个外部事件调用时所提出的需求，为了在Execute方法中充分获取窗口的需求，
        /// 所以将调用外部事件的窗口控件以及对应的触发事件参数也传入Execute方法中。
        /// </summary>
        /// <remarks></remarks>
        private class RequestParameter
        {
            private object _sender;

            /// <summary> 引发Form事件控件对象 </summary>
            public dynamic sender
            {
                get { return _sender; }
            }

            /// <summary> Form中的事件所对应的事件参数 </summary>
            public EventArgs e { get; }

            /// <summary> 具体的需求 </summary>
            public Request Id { get; }

            /// <summary>
            /// 定义事件需求与窗口中引发此事件的控件对象及对应的事件参数
            /// </summary>
            /// <param name="requestId">具体的需求</param>
            /// <param name="e">Form中的事件所对应的事件参数</param>
            /// <param name="sender">引发Form事件控件对象</param>
            /// <remarks></remarks>
            public RequestParameter(Request requestId, EventArgs e = null, object sender = null)
            {
                this._sender = sender;
                this.e = e;
                this.Id = requestId;
            }
        }

        #endregion

        #region    ---   enum Request

        /// <summary>
        /// ModelessForm的操作需求，用来从窗口向IExternalEventHandler对象传递需求。
        /// </summary>
        /// <remarks></remarks>
        private enum Request
        {
            /// <summary>
            /// 通过在UI界面绘制模型线来作为土体的轮廓
            /// </summary>
            DrawCurves,

            /// <summary>
            /// 删除绘制好的模型线并清空曲线集合数据
            /// </summary>
            DeleteCurves,

            /// <summary> 开始建模 </summary>
            StartModeling,


            /// <summary> 取消模型线的绘制操作 </summary>
            CancelDraw,
        }

        #endregion

        #region    ---   执行操作 IExternalEventHandler.Execute

        //'为每一项操作执行具体的实现
        /// <summary>
        /// 在执行ExternalEvent.Raise()方法之前，请先将操作需求信息赋值给其RequestHandler对象的RequestId属性。
        /// 当ExternalEvent.Raise后，Revit会在下个闲置时间（idling time cycle）到来时调用IExternalEventHandler.Execute方法的实现。
        /// </summary>
        /// <param name="app">此属性由Revit自动提供，其值不是Nothing，而是一个真实的UIApplication对象</param>
        /// <remarks>由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，
        /// 而是直接退出函数。所以要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。</remarks>
        public void Execute(UIApplication uiApp)
        {
            try // 由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，而是直接退出函数。所以这里要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。
            {
                UIDocument uiDoc = new UIDocument(Document);


                // 开始执行具体的操作
                switch (RequestPara.Id) // 判断具体要干什么
                {
                    case Request.DrawCurves:
                        // -------------------------------------------------------------------------------------------------------------------------
                        if (!ModelCurvesDrawer.IsBeenUsed)
                        {
                            _forbiddenFormClosing = true;
                            //
                            // 先确定模型土体或者开挖土体的轮廓曲线
                            drawnCurveArrArr = null;

                            // 绘制轮廓
                            this.ClosedCurveDrawer = new ClosedCurvesDrawer(uiApp, true, null);
                            this.ClosedCurveDrawer.ClosedDrawingCompleted += this.ClosedCurveDrawer_DrawingCompleted;
                            this.ClosedCurveDrawer.PostDraw();

                            // 由于 PostDraw 是异步操作，所以这里不能直接获得绘制好的模型线
                            // 而是要在 Drawer_DrawingCompleted 事件中来构造土体的轮廓线
                        }

                        break;
                    case Request.DeleteCurves:
                        if (drawnCurveArrArr != null)
                        {
                            // 删除模型线
                            using (Transaction t = new Transaction(Document, "删除绘制好的模型线。"))
                            {
                                List<ElementId> elemIds = new List<ElementId>();
                                t.Start();
                                try
                                {
                                    foreach (List<ElementId> ca in DrawnCurveIds)
                                    {
                                        elemIds.AddRange(ca.Where(r => r.Element(Document) != null));
                                    }
                                    //
                                    Document.Delete(elemIds);
                                    t.Commit();
                                }
                                catch (Exception)
                                {
                                    t.RollBack();
                                }
                            }

                            // 清空数据
                            drawnCurveArrArr = null;
                        }
                        break;
                    case Request.StartModeling: // 通过 多边形的族样板 来直接放置土体模型

                        // 考虑不同的建模方式
                        // -------------------------------------------------------------------------------------------------------------------------
                        if (this.RadioBtn_Draw.Checked == true)
                        {
                            if (this.drawnCurveArrArr != null)
                            {
                                // 根据选择好的轮廓线来进行土体的建模
                                DrawSoilFromCurve(drawnCurveArrArr);
                            }
                            else
                            {
                                MessageBox.Show("请先绘制好要进行建模的封闭轮廓。");
                            }
                            // -------------------------------------------------------------------------------------------------------------------------
                        }
                        else if (this.RadioBtn_PickShape.Checked == true)
                        {
                            // 先确定模型土体或者开挖土体的轮廓曲线
                            CurveArrArray CurveArrArr = null;

                            // 选择轮廓
                            ClosedCurveSelector cs = new ClosedCurveSelector(uiDoc, true);
                            CurveArrArr = cs.SendSelect();

                            if (!CurveArrArr.IsEmpty)
                            {
                                // 根据选择好的轮廓线来进行土体的建模
                                DrawSoilFromCurve(CurveArrArr);
                            }

                            // -------------------------------------------------------------------------------------------------------------------------
                        }
                        else if (this.RadioBtn_Polygon.Checked == true)
                        {
                        }
                        break;

                    // -------------------------------------------------------------------------------------------------------------------------

                    case Request.CancelDraw:
                        {
                            // 保存的实例需要进行释放
                            if (this.ClosedCurveDrawer != null)
                            {
                                ClosedCurveDrawer.ClosedDrawingCompleted -= ClosedCurveDrawer_DrawingCompleted;
                                ClosedCurveDrawer.Cancel();
                                ClosedCurveDrawer = null;
                            }
                            _forbiddenFormClosing = false;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                DebugUtils.ShowDebugCatch(ex, "外部事件执行出错", "出错");
            }
            finally
            {
                if (RequestPara.Id != Request.DrawCurves)
                {
                    // 刷新Form，将Form中的Controls的Enable属性设置为True
                    this.WarmUp();
                }
            }
        }

        /// <summary>
        /// 根据绘制或者选择出来的土体轮廓模型线来进行模型土体或者开挖土体的建模
        /// </summary>
        /// <param name="curveArrArr"></param>
        private void DrawSoilFromCurve(CurveArrArray curveArrArr)
        {
            Soil_Model soilModel;

            if (this.RadioBtn_ExcavSoil.Checked) // 绘制开挖土体
            {
                soilModel = this.ExcavDoc.FindSoilModel();
                if (soilModel != null)
                {
                    Soil_Excav excavSoil = this.ExcavDoc.CreateExcavationSoil(soilModel, this.Depth, curveArrArr,
                        DesiredName);

                    // 设置开挖开始或者完成的时间

                    using (Transaction tran = new Transaction(Document))
                    {
                        try
                        {
                            tran.SetName("设置开挖土体的开始或者完成的时间");
                            tran.Start();

                            if ((this.StartedDate != null) || (this.CompletedDate != null))
                            {
                                if (this.StartedDate != null)
                                {
                                    excavSoil.SetExcavatedDate(tran, true, StartedDate.Value);
                                }
                                if (this.CompletedDate != null)
                                {
                                    excavSoil.SetExcavatedDate(tran, false, CompletedDate.Value);
                                }
                            }

                            // 将开挖土体在模型土体中隐藏起来
                            soilModel.RemoveSoil(tran, excavSoil);

                            // 将用来剪切的开挖土体在视图中进行隐藏
                            Autodesk.Revit.DB.View v = Document.ActiveView;
                            if (v != null)
                            {
                                v.HideElements(new[] { excavSoil.Soil.Id });
                            }
                            //
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.RollBack();
                            DebugUtils.ShowDebugCatch(ex, $"事务“{tran.GetName()}”出错");
                        }
                    }
                }
            }
            else // 绘制模型土体
            {
                // 获得用来创建实体的模型线
                soilModel = this.ExcavDoc.CreateModelSoil(this.Depth, curveArrArr);
            }
        }

        #endregion

        #region    ---   DrawCurves 绘制土体轮廓

        /// <summary>
        /// 用来绘制封闭的模型线
        /// </summary>
        private ClosedCurvesDrawer ClosedCurveDrawer;

        // 绘制好的模型线
        private List<List<ElementId>> DrawnCurveIds;
        private CurveArrArray F_drawnCurveArrArr;

        /// <summary>
        /// 通过界面绘制出来的模型线
        /// </summary>
        private CurveArrArray drawnCurveArrArr
        {
            get { return F_drawnCurveArrArr; }
            set
            {
                if (value == null)
                {
                    this.CheckBox_DrawSucceeded.Checked = false;
                    Btn_ClearCurves.Enabled = false;
                }
                else
                {
                    this.CheckBox_DrawSucceeded.Checked = true;
                    Btn_ClearCurves.Enabled = true;
                }
                F_drawnCurveArrArr = value;
            }
        }

        private void ClosedCurveDrawer_DrawingCompleted(List<List<ElementId>> AddedCurves, bool FinishedExternally,
            bool Succeeded)
        {
            if (Succeeded)
            {
                // 构造土体轮廓
                drawnCurveArrArr = new CurveArrArray();
                ModelCurve c = default(ModelCurve);
                //
                foreach (List<ElementId> cs in AddedCurves)
                {
                    CurveArray CurveArr = new CurveArray();
                    foreach (ElementId cid in cs)
                    {
                        c = (ModelCurve)this.Document.GetElement(cid);
                        CurveArr.Append(c.GeometryCurve);
                    }
                    // 必须将曲线集合进行重排以使其连续。
                    CurvesFormator.GetContiguousCurvesFromCurves(CurveArr, out CurveArr);
                    drawnCurveArrArr.Append(CurveArr);
                }
                DrawnCurveIds = AddedCurves;
            }
            else
            {
                drawnCurveArrArr = null;
            }
            // 激活界面
            _forbiddenFormClosing = false;
            this.WarmUp();
            //
        }

        #endregion
    }
}
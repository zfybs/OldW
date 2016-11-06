using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using eZstd.Miscellaneous;
using RevitStd;
using RevitStd.Curves;
using RevitStd.Selector;
using View = Autodesk.Revit.DB.View;


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
            /// <summary> 通过在UI界面绘制模型线来作为土体的轮廓 </summary>
            DrawCurvesInUI,

            /// <summary> 通过 Selection.PickObject() 来在界面上选择出一组封闭模型线 </summary>
            PickCurves,

            /// <summary> 通过在UI界面中选择一个水平面来绘制出对应的轮廓 </summary>
            DrawCurvesFromFaceEdge,

            /// <summary> 删除绘制好的模型线并清空曲线集合数据 </summary>
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
        /// <param name="uiApp">此属性由Revit自动提供，其值不是Nothing，而是一个真实的UIApplication对象</param>
        /// <remarks>由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，
        /// 而是直接退出函数。所以要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。</remarks>
        public void Execute(UIApplication uiApp)
        {
            try // 由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，而是直接退出函数。所以这里要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。
            {
                UIDocument uiDoc = new UIDocument(Document);
                _forbiddenFormClosing = true;

                // 开始执行具体的操作
                switch (RequestPara.Id) // 判断具体要干什么
                {
                    case Request.DrawCurvesInUI:
                        // -------------------------------------------------------------------------------------------------------------------------
                        if (!ModelCurvesDrawer.IsBeenUsed)
                        {
                            _forbiddenFormClosing = true;
                            //
                            // 先确定模型土体或者开挖土体的轮廓曲线
                            DrawnCurveIds = null;

                            // 绘制轮廓
                            this._closedCurveDrawer = new ClosedCurvesDrawer(uiApp, true, null);
                            this._closedCurveDrawer.ClosedDrawingCompleted += this.ClosedCurveDrawer_DrawingCompleted;
                            this._closedCurveDrawer.PostDraw();

                            // 由于 PostDraw 是异步操作，所以这里不能直接获得绘制好的模型线
                            // 而是要在 Drawer_DrawingCompleted 事件中来构造土体的轮廓线
                        }

                        break;

                    // -------------------------------------------------------------------------------------------------------------------------
                    case Request.DrawCurvesFromFaceEdge:

                        FamilyInstance fi;
                        PlanarFace pFace;
                        Reference pFaceRef = Selector.SelectHorizontalPlanFace<FamilyInstance>(new UIDocument(Document), out fi, out pFace);
                        if (pFaceRef != null)
                        {
                            using (Transaction tranDoc = new Transaction(Document, "根据选择的水平面来绘制模型线。"))
                            {
                                tranDoc.Start();
                                try
                                {

                                    Transform transf = fi.GetTransform();
                                    List<Curve> curves;
                                    CurvesConverter.Convert(pFace.EdgeLoops, transf, out curves);

                                    // 绘制模型线
                                    ModelCurveArray mocelCurves = GeoHelper.DrawModelCurves(tranDoc, Document, curves, SketchPlane.Create(Document, pFaceRef));

                                    // 将模型线的几何信息保存下来  
                                    DrawnCurveIds = GetCurveIds(mocelCurves);
                                    CurvesConverter.Convert(pFace.EdgeLoops, transf, out this._curvesToDrawSoil);
                                    //
                                    tranDoc.Commit();
                                }
                                catch (Exception)
                                {
                                    tranDoc.RollBack();
                                }
                            }
                        }
                        break;

                    // -------------------------------------------------------------------------------------------------------------------------
                    case Request.DeleteCurves:
                        if (DrawnCurveIds != null)
                        {
                            // 删除模型线
                            using (Transaction t = new Transaction(Document, "删除绘制好的模型线。"))
                            {
                                List<ElementId> elemIds = new List<ElementId>();
                                t.Start();
                                try
                                {
                                    elemIds = _drawnCurveIds.Where(r => r.Element(Document) != null).ToList();

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
                            DrawnCurveIds = null;
                        }
                        break;

                    // -------------------------------------------------------------------------------------------------------------------------
                    case Request.PickCurves:

                        // 选择轮廓
                        CurveArrArray caa;
                        List<List<ModelCurve>> modelCurvess;
                        ClosedCurveSelector cs = new ClosedCurveSelector(uiDoc, multiple: true);
                        caa = cs.SendSelect(out modelCurvess);
                        // 
                        if (!caa.IsEmpty)
                        {
                            List<ElementId> curveIds = new List<ElementId>();
                            foreach (List<ModelCurve> mcs in modelCurvess)
                            {
                                curveIds.AddRange(mcs.Select(r => r.Id));
                            }
                            //
                            DrawnCurveIds = curveIds;
                            _curvesToDrawSoil = caa;
                        }
                        else  // 未成功选择封闭的模型线
                        {

                        }

                        break;

                    // -------------------------------------------------------------------------------------------------------------------------
                    case Request.StartModeling: // 通过 多边形的族样板 来直接放置土体模型
                        
                        // 通过用户绘制的轮廓来绘制土体
                        if (RadioBtn_UserDrawShape.Checked)
                        {

                            if (this._curvesToDrawSoil != null && _curvesToDrawSoil.Size > 0)
                            {

                                // 根据选择好的轮廓线来进行土体的建模
                                DrawSoilFromCurve(_curvesToDrawSoil);
                            }
                            else
                            {
                                MessageBox.Show(@"请先绘制好要进行建模的封闭轮廓。");
                            }
                        }
                        // 通过自适应族的模板来绘制土体
                        else if (this.RadioBtn_Polygon.Checked)
                        {
                        }
                        break;

                    // -------------------------------------------------------------------------------------------------------------------------

                    case Request.CancelDraw:
                        {
                            // 保存的实例需要进行释放
                            if (this._closedCurveDrawer != null)
                            {
                                _closedCurveDrawer.ClosedDrawingCompleted -= ClosedCurveDrawer_DrawingCompleted;
                                _closedCurveDrawer.Cancel();
                                _closedCurveDrawer = null;
                            }
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
                if (RequestPara.Id != Request.DrawCurvesInUI)
                {
                    // 刷新Form，将Form中的Controls的Enable属性设置为True
                    this.WarmUp(RequestPara.Id);
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
                    Soil_Excav excavSoil = this.ExcavDoc.CreateExcavationSoil(soilModel, this._soilDepth, curveArrArr,
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
                            View v = Document.ActiveView;
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
                soilModel = this.ExcavDoc.CreateModelSoil(this._soilDepth, curveArrArr);
            }
        }

        #endregion

        #region    ---   DrawCurves 绘制土体轮廓

        /// <summary> 用来绘制封闭的模型线 </summary>
        private ClosedCurvesDrawer _closedCurveDrawer;

        /// <summary> 绘制好的模型线所对应的Curve对象，记录下来以用来绘制土体。
        /// 此变量只应该在 成功地创建了封闭的模型线时为其赋值 </summary>
        private CurveArrArray _curvesToDrawSoil;

        /// <summary> 绘制好的模型线 </summary>
        private List<ElementId> _drawnCurveIds;
        /// <summary> 通过界面绘制出来的模型线，记录下来以用来被删除 </summary>
        private List<ElementId> DrawnCurveIds
        {
            get { return _drawnCurveIds; }
            set
            {
                if (value == null)
                {
                    ChangeDrawingPermission(false);
                }
                else
                {
                    ChangeDrawingPermission(true);
                }
                _drawnCurveIds = value;
            }
        }

        /// <summary> 在UI界面中绘制完模型线后，对其结果进行处理 </summary>
        /// <param name="addedCurves"></param>
        /// <param name="finishedExternally"></param>
        /// <param name="succeeded"></param>
        private void ClosedCurveDrawer_DrawingCompleted(List<List<ElementId>> addedCurves, bool finishedExternally, bool succeeded)
        {
            // 这些模型线可以被记录下来，用来进行删除
            List<ElementId> curveIds = new List<ElementId>();
            foreach (List<ElementId> ids in addedCurves)
            {
                curveIds.AddRange(ids);
            }
            //
            DrawnCurveIds = curveIds;

            if (succeeded)
            {
                _curvesToDrawSoil = GetCurvesFromModelCurves(Document, addedCurves);
            }
            else
            {
                // 不允许绘制土体
                _curvesToDrawSoil = null;
            }

            // 激活界面
            this.WarmUp(Request.DrawCurvesInUI);
            //
        }

        private static CurveArrArray GetCurvesFromModelCurves(Document doc, List<List<ElementId>> curvesIdss)
        {
            var caa = new CurveArrArray();

            ModelCurve c;
            //
            foreach (List<ElementId> cs in curvesIdss)
            {
                CurveArray ca = new CurveArray();
                foreach (ElementId cIds in cs)
                {
                    c = (ModelCurve)doc.GetElement(cIds);
                    ca.Append(c.GeometryCurve);
                }
                // 必须将曲线集合进行重排以使其连续。
                CurvesFormator.GetContiguousCurvesFromCurves(ca, out ca);
                caa.Append(ca);
            }
            return caa;
        }

        private static List<ElementId> GetCurveIds(ModelCurveArray modelCurves)
        {
            List<ElementId> ids = new List<ElementId>();
            foreach (ModelCurve mc in modelCurves)
            {
                ids.Add(mc.Id);
            }
            return ids;
        }

        #endregion
    }
}
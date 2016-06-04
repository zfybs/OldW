using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 测点_测斜管
    /// </summary>
    /// <remarks></remarks>
    public class Instrum_Incline : Instrum_Line
    {
        #region    ---   Properties

        #endregion

        #region    ---   Fields

        /// <summary> 测斜管的位置是在模型中的开挖土体的内部还是外部，即测斜管与开挖土体的Element是否相交。 </summary>
        /// <remarks>True if the inclinometer is inside the excavation earth,
        /// and False if the inclinometer is outside the excavation earth.</remarks>
        private bool _isInsideEarth;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="inclinometerElement">测斜管所对应的图元</param>
        public Instrum_Incline(FamilyInstance inclinometerElement) : base(inclinometerElement, InstrumentationType.墙体测斜)
        {
        }

        #region    ---   测斜点附近的土体开挖面

        /// <summary>
        /// 找到距离此测斜管最近的土体开挖面的标高值
        /// </summary>
        /// <param name="SoilElement">模型中的土体单元，即此测斜管附近的开挖土体单元</param>
        /// <returns>如果没有找到对应的标高值，则返回 null</returns>
        /// <remarks></remarks>
        public double? FindAdjacentEarthElevation(FamilyInstance SoilElement)
        {
            // 将测斜管的底部端点作为原点
            double Length = Convert.ToDouble(Monitor.Symbol.LookupParameter("深度").AsDouble()); // 测斜管长度

            LocationPoint lc = this.Monitor.Location as LocationPoint;
            XYZ ptOrigin = lc.Point; // 测斜管的顶点
            ptOrigin = ptOrigin.Add(new XYZ(0, 0, -Length)); // 测斜管的底部点


            // 将当前3D视图作为ReferenceIntersector的构造参数
            View3D view3d = null;
            view3d = Doc.ActiveView as View3D;
            if (view3d == null)
            {
                TaskDialog.Show("3D view", "current view should be 3D view");
                return null;
            }

            // 将土体单元作为搜索相交面的目标，搜索其中所有的相交面
            ReferenceIntersector IntersectedEarth = new ReferenceIntersector(SoilElement.Id, FindReferenceTarget.Face,
                view3d);
            var PtBottom = FindBottomPoint(ptOrigin, this._isInsideEarth, SoilElement, IntersectedEarth);

            // 将测点附近的那个底部点（开挖土体内部）向上发出一条射线，以得到真实的土体开挖面
            ReferenceWithContext refContext = IntersectedEarth.FindNearest(PtBottom, new XYZ(0, 0, 1));
            XYZ elevation = default(XYZ);
            elevation = refContext.GetReference().GlobalPoint;

            TaskDialog.Show("找到啦", elevation.ToString());

            //
            return elevation.Z;
        }


        /// <summary>
        /// 搜索一个底部坐标点，有了此点后，只要向上发射一条射线，即可以找到此时的开挖面
        /// </summary>
        /// <param name="ptInclinometerBottom">测斜管的底部坐标点</param>
        /// <param name="IsInside">测斜管是否在开挖土体Element的内部</param>
        /// <param name="Earth">开挖墙体单元</param>
        /// <param name="IntersectedEarth">用来搜索相交面的开挖土体</param>
        /// <returns></returns>
        /// <remarks>  如果测斜管就在土体内部，那么测斜管的底部点就可以直接用来向上发射射线了。
        /// 如果测斜管在土体外部，那么需要以测斜管的底部点为中心，向四周发射多条射线，
        /// 这些射线分别都与土体相交，找到距离土体最近的那一条射线所对应的相交点与相交面，然后将相交点向面内偏移一点点，即可以作为寻找开挖面的射线的原点了。</remarks>
        private XYZ FindBottomPoint(XYZ ptInclinometerBottom, bool IsInside, FamilyInstance Earth,
            ReferenceIntersector IntersectedEarth)
        {
            XYZ PtBottom = new XYZ();
            if (IsInside)
            {
                // 如果测斜管就在土体内部，那么测斜管的底部点就可以直接用来向上发射射线了。
                return ptInclinometerBottom;
            }
            else
            {
                // 如果测斜管在土体外部，那么需要以测斜管的底部点为中心，向四周发射多条射线，
                // 这些射线分别都与土体相交，找到距离土体最近的那一条射线所对应的相交点与相交面，然后将相交点向面内偏移一点点，即可以作为寻找开挖面的射线的原点了。
                double NearestDist = double.MaxValue;
                ReferenceWithContext NearestRefCont = null;
                XYZ NearestDir = null;
                XYZ dire = default(XYZ);
                for (var angle = 0.3/180*Math.PI; angle <= 2*Math.PI; angle += Math.PI/7)
                {
                    // 创建一个水平面上的方向向量，此向量与x轴的夹角为angle
                    dire = new XYZ(Math.Cos(angle), Math.Sin(angle), 0);
                    ReferenceWithContext refContext = IntersectedEarth.FindNearest(ptInclinometerBottom, dire);
                    if ((refContext != null) && (refContext.Proximity < NearestDist))
                    {
                        NearestDir = dire;
                        NearestDist = Convert.ToDouble(refContext.Proximity);
                        NearestRefCont = refContext;
                    }
                }

                // 找到了离测斜管最近的土体竖直面
                if (NearestRefCont != null)
                {
                    Reference NearestRef = NearestRefCont.GetReference();

                    // 找到最近的那个相交射线所对应的相交面，此面即为此离测斜管最近的那个土体的侧面（竖向）
                    // 注意，这里返回的Face是位于族类型中的Face，而不是模型空间中的族实例的Face
                    Face VerticalFace = (Face) Earth.GetGeometryObjectFromReference(NearestRef);

                    // 测斜管底部点到找到的最近的面的垂足点
                    XYZ Normal = VerticalFace.ComputeNormal(NearestRef.UVPoint);
                        // face normal where ray hits，注意：这里返回的法向量是相对于族类型的局部空间的
                    Normal = NearestRefCont.GetInstanceTransform().OfVector(Normal);
                        //将法向由族类型的局部空间转换到模型空间。 transformation to get it in terms of document coordinates instead of the parent symbol

                    // 将垂足点沿着面的法向的反方向延长 0.001 英尺，以进入土体内部（而不是位于竖直面的表面）
                    // 在实际操作中，这里并没有取法向，而是近似地取距离最近的相交射线的方向，这里由于：如果测点在开挖土体的拐角处，就找不到在面内的投影点了。
                    PtBottom = ptInclinometerBottom + NearestDir*(NearestDist + 0.001);
                }
                else
                {
                    TaskDialog.Show("出错！", "未找到离此测斜点最近的开挖面");
                    return null;
                }
            }
            return PtBottom;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace OldW.Excavation
{
    partial class frm_ExcavationInfo
    {
        /// <summary> Datagridview 中的每一行数据所对应的实体类 </summary>
        private class ExcavSoilEntity
        {
            /// <summary> 开挖土体对象 </summary>
            [Browsable(false)]
            public Soil_Excav Soil { get; set; }

            /// <summary> 土体单元的Id值 </summary>
            public ElementId Id { get; set; }

            /// <summary> 开挖土体的名称 </summary>
            public string Name { get; set; }

            /// <summary> 开挖土体的深度 </summary>
            public double Depth { get; set; }

            /// <summary> 开挖土体开挖完成的日期 </summary>
            public DateTime? StartedDate { get; set; }

            /// <summary> 开挖土体开挖完成的日期 </summary>
            public DateTime? CompletedDate { get; set; }

            /// <summary> 开挖土体在当前视图中是否可见 </summary>
            public bool Visible { get; set; }


            /// <summary>
            /// 将界面中设置的信息同步到Revit文档中
            /// </summary>
            /// <param name="tran"></param>
            /// <param name="View"></param>
            public void syncToDocument(Transaction tran, View View)
            {
                Soil.SetExcavatedDate(tran, true, StartedDate);
                Soil.SetExcavatedDate(tran, false, CompletedDate);
                Soil.SetName(tran, newName: Name);
                Soil.SetDepth(tran, depth: Depth);

            }

            /// <summary>
            /// 设置开挖土体的可见性
            /// </summary>
            /// <param name="tran"></param>
            /// <param name="show"> True代表设置其为可见 </param>
            /// <param name="view">当前视图对象</param>
            public void SetVisibility(Transaction tran, bool show, View view)
            {
                if (show)
                {
                    view.UnhideElements(new[] { Soil.Soil.Id });
                }
                else
                {
                    view.HideElements(new[] { Soil.Soil.Id });
                }
            }

            /// <summary>
            /// 设置开挖土体的可见性
            /// </summary>
            /// <param name="tran"></param>
            public void DeleteExcavSoil(Transaction tran)
            {
                Soil.Delete(tran);
            }

        }
    }
}
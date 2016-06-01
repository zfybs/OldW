using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using std_ez;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 所有类型的监测点，包括地表沉降、立柱隆起、支撑轴力等，但不包括测斜管
    /// </summary>
    /// <remarks></remarks>
    public class Instrum_Point : Instrumentation
    {
        #region    ---   Properties

        private List<MonitorData_Point> _monitorData;

        /// <summary>
        /// 点测点的整个施工阶段中的监测数据
        /// </summary>
        public List<MonitorData_Point> MonitorData
        {
            get
            {
                if (_monitorData == null)
                {
                    _monitorData = GetMonitorData();
                }
                return this._monitorData;
            }
            set { this._monitorData = value; }
        }

        #endregion

        #region    ---   构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="MonitorPoint">所有类型的监测点，包括地表沉降、立柱隆起、支撑轴力等，但不包括测斜管</param>
        /// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
        /// <remarks></remarks>
        internal Instrum_Point(FamilyInstance MonitorPoint, InstrumentationType Type) : base(MonitorPoint, Type)
        {
        }


        /// <summary>
        /// 从指定的Element集合中，找出所有的点测点元素
        /// </summary>
        /// <param name="Elements"> 要进行搜索过滤的Element集合</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public new static List<Instrum_Point> Lookup(Document Doc, ICollection<ElementId> Elements)
        {
            List<Instrum_Point> Points = new List<Instrum_Point>();
            List<Instrumentation> instrus;
            instrus = Instrumentation.Lookup(Doc, Elements);
            return Points;
        }

        #endregion

        #region   ---   监测数据的提取与保存

        /// <summary>
        /// 将测点对象中的监测数据提取为具体的序列化类
        /// </summary>
        /// <returns></returns>
        public List<MonitorData_Point> GetMonitorData()
        {
            string strData = base.getMonitorDataString();
            List<MonitorData_Point> mData = null;
            if (strData.Length > 0)
            {
                try
                {
                    mData = (List<MonitorData_Point>)StringSerializer.Decode64(strData);
                    return mData;
                }
                catch (Exception)
                {
                    TaskDialog.Show("Error",
                        this.Monitor.Name + " (" + this.Monitor.Id.IntegerValue.ToString() + ")" + " 中的监测数据不能正确地提取。",
                        TaskDialogCommonButtons.Ok, TaskDialogResult.Ok);
                    return null;
                }
            }
            return mData;
        }

        /// <summary>
        /// 将监测数据以序列化字符串保存到对应的Parameter对象中。
        /// </summary>
        /// <remarks></remarks>
        public bool SetMonitorData(Transaction tran, List<MonitorData_Point> data)
        {
            this.MonitorData = data;
            if (data != null)
            {
                // 将数据序列化为字符串
                string strData = StringSerializer.Encode64(data);
                base.setMonitorDataString(tran, strData);
                Parameter para = Monitor.get_Parameter(Constants.SP_MonitorData_Guid);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
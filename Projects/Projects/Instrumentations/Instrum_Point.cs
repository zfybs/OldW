using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using stdOldW;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 所有类型的监测点，包括地表沉降、立柱隆起、支撑轴力等，但不包括测斜管
    /// </summary>
    /// <remarks></remarks>
    public class Instrum_Point : Instrumentation
    {
        #region    ---   Properties

        /// <summary>
        /// 点测点的整个施工阶段中的监测数据
        /// </summary>
        private List<MonitorData_Point> _monitorData;

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
        /// <returns>如果在派生类中将此方法进行重写，则一定要对应地对 <see cref="SetMonitorData"/> 方法进行重写。</returns>
        public virtual List<MonitorData_Point> GetMonitorData()
        {
            if (_monitorData != null)
            {
                return _monitorData;
            }
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
        /// <remarks>如果在派生类中将此方法进行重写，则一定要对应地对 <see cref="GetMonitorData"/> 方法进行重写。</remarks>
        public virtual bool SetMonitorData(Transaction tran, List<MonitorData_Point> data)
        {

            if (data != null)
            {
                // 将数据序列化为字符串
                string strData = StringSerializer.Encode64(data);
                base.setMonitorDataString(tran, strData);

                // 将数据在类实例中保存下来
                _monitorData = data;
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
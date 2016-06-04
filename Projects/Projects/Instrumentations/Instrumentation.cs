#region

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;

#endregion

namespace OldW.Instrumentations
{
    /// <summary>
    /// 监测测点：包括线测点（测斜管）或点测点（地表沉降、立柱隆起、支撑轴力）等
    /// </summary>
    /// <remarks>
    /// 对于点测点而言，其监测数据是在不同的时间记录的，每一个时间上都只有一个数据。所以其监测数据是一个两列的表格，第一列为时间，第二列为监测数据。
    /// 对于线测点而言（比如测斜管），在每一个时间上都有两列数据，用来记录这一时间上，线测点中每一个位置的监测值。
    /// </remarks>
    public abstract class Instrumentation
    {
        #region    ---   Properties

        public Document Doc { get; }

        private FamilyInstance F_Monitor;

        /// <summary>
        /// 监测仪器，对于点测点，其包括地表沉降、立柱隆起、支撑轴力等；
        /// 对于线测点，包括测斜管
        /// </summary>
        public FamilyInstance Monitor
        {
            get { return F_Monitor; }
        }

        private InstrumentationType F_Type;

        /// <summary> 监测点的测点类型，也是测点所属的族的名称 </summary>
        public InstrumentationType Type
        {
            get { return F_Type; }
        }

        /// <summary> 每一个测点的名称，比如 CX1，LZ2等 </summary>
        public string Name { get; set; }

        #endregion

        #region    ---   构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Instrumentation">所有类型的监测仪器，包括线测点（测斜管）或点测点（地表沉降、立柱隆起、支撑轴力）等</param>
        /// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
        /// <remarks></remarks>
        internal Instrumentation(FamilyInstance Instrumentation, InstrumentationType Type)
        {
            if (Instrumentation != null)
            {
                this.F_Monitor = Instrumentation;
                this.Doc = Instrumentation.Document;
                this.F_Type = Type;
                //
            }
            else
            {
                throw new NullReferenceException("The specified element is not valid as an instrumentation.");
            }
        }

        #endregion

        #region    ---   从Element集合中过滤出监测点对象

        /// <summary>
        /// 从指定的Element集合中，找出所有的监测点元素
        /// </summary>
        /// <param name="Elements"> 要进行搜索过滤的Element集合</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<Instrumentation> Lookup(Document Doc, ICollection<ElementId> Elements)
        {
            List<Instrumentation> Instrus = new List<Instrumentation>();
            if (Elements.Count > 0)
            {
                FilteredElementCollector Coll = new FilteredElementCollector(Doc, Elements);
                // 集合中的族实例
                Coll = Coll.OfClass(typeof(FamilyInstance));

                // 找到指定的Element集合中，所有的族实例
                FilteredElementIterator FEI = Coll.GetElementIterator();
                string strName = "";
                FEI.Reset();
                while (FEI.MoveNext())
                {
                    //add level to list
                    FamilyInstance fi = FEI.Current as FamilyInstance;
                    if (fi != null)
                    {
                        // 一个Element所对应的族的名称（而不是族类型的名称）
                        strName = Convert.ToString(fi.Symbol.FamilyName);
                        InstrumentationType Tp = default(InstrumentationType);
                        if (Enum.TryParse(value: strName, result: out Tp))
                        {
                            switch (Tp)
                            {
                                case InstrumentationType.墙体测斜:
                                    Instrus.Add(new Instrum_Incline(fi));
                                    break;
                                case InstrumentationType.支撑轴力:
                                    Instrus.Add(new Instrum_StrutAxialForce(fi));
                                    break;
                                case InstrumentationType.地表隆沉:
                                    Instrus.Add(new Instrum_GroundSettlement(fi));
                                    break;
                                case InstrumentationType.立柱隆沉:
                                    Instrus.Add(new Instrum_ColumnHeave(fi));
                                    break;
                            }
                        }
                    }
                }
            }
            return Instrus;
        }

        #endregion

        #region   ---   族参数中数据的提取与保存

        /// <summary>
        /// 提取测点的名称，比如“CX1”。此参数是在测点族的设计时添加进去的，而不是通过API添加的。
        /// </summary>
        public string getMonitorName()
        {
            return Monitor.get_Parameter(Constants.SP_MonitorName_Guid).AsString();
        }

        /// <summary>
        /// 提取测点监测数据所对应的序列化字符。即测点族中“监测数据”参数中的字符。
        /// 如果要提取监测数据为对应的数据类，可以去调用具体派生类的 GetMonitorData 函数
        /// </summary>
        protected string getMonitorDataString()
        {
            return Monitor.get_Parameter(Constants.SP_MonitorData_Guid).AsString();
        }

        /// <summary>
        /// 将测点数据类序列化之后的字符保存到测点对象的参数中。
        /// </summary>
        protected void setMonitorDataString(Transaction tran, string dataString)
        {
            Parameter para = Monitor.get_Parameter(Constants.SP_MonitorData_Guid);
            para.Set(dataString);
        }
        #endregion
    }
}
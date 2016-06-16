#region

using System;
using System.Collections.Generic;
using System.Data.OleDb;
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

        /// <summary> 测点所在的Revit文档 </summary>
        public Document Document { get; }

        /// <summary> 测点的标志文字，一般格式为“墙体测斜（CX3）: 568742” </summary>
        public string IdName => F_Monitor.Name + "( " + GetMonitorName() + " ):" + F_Monitor.Id.IntegerValue;

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
                this.Document = Instrumentation.Document;
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

        /// <summary> 从指定的Element集合中，找出所有的监测点元素 </summary>
        /// <param name="Elements"> 要进行搜索过滤的Element集合</param>
        public static List<Instrumentation> Lookup(Document Doc, ICollection<ElementId> Elements)
        {
            if (Elements==null)
            {
                return new List<Instrumentation>();
            }
            FilteredElementCollector Coll = new FilteredElementCollector(Doc, Elements);
            List<Instrumentation> Instrus = LookupFromCollector(Coll);
            return Instrus;
        }

        /// <summary> 从整个文档中找出所有的监测点元素 </summary>
        public static List<Instrumentation> Lookup(Document Doc)
        {
            FilteredElementCollector Coll = new FilteredElementCollector(Doc);
            List<Instrumentation> Instrus = LookupFromCollector(Coll);
            return Instrus;
        }

        /// <summary>
        /// 从 FilteredElementCollector 集合中，找出所有的监测点元素
        /// </summary>
        /// <returns></returns>
        private static List<Instrumentation> LookupFromCollector(FilteredElementCollector Coll)
        {
            List<Instrumentation> Instrus = new List<Instrumentation>();

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
                                Instrus.Add(new Instrum_WallIncline(fi));
                                break;
                            case InstrumentationType.土体测斜:
                                Instrus.Add(new Instrum_Line(fi, InstrumentationType.土体测斜,true));
                                break;

                            case InstrumentationType.墙顶位移:
                                Instrus.Add(new Instrum_WallTop(fi));
                                break;

                            case InstrumentationType.地表隆沉:
                                Instrus.Add(new Instrum_GroundSettlement(fi));
                                break;
                            case InstrumentationType.立柱隆沉:
                                Instrus.Add(new Instrum_ColumnHeave(fi));
                                break;

                            case InstrumentationType.支撑轴力:
                                Instrus.Add(new Instrum_StrutAxialForce(fi));
                                break;
                            case InstrumentationType.水位:
                                Instrus.Add(new Instrum_Point(fi, InstrumentationType.水位));
                                break;

                            case InstrumentationType.其他线测点:
                                Instrus.Add(new Instrum_Line(fi, InstrumentationType.其他线测点, false));
                                break;
                            case InstrumentationType.其他点测点:
                                Instrus.Add(new Instrum_Point(fi, InstrumentationType.其他点测点));
                                break;
                        }

                    }
                }
            }
            return Instrus;
        }

        #endregion

        #region   ---   族参数中数据的提取与保存

        private string _monitorName;
        /// <summary>
        /// 提取测点的名称，比如“CX1”。此参数是在测点族的设计时添加进去的，而不是通过API添加的。
        /// </summary>
        public string GetMonitorName()
        {
            return _monitorName ?? Monitor.get_Parameter(Constants.SP_MonitorName_Guid).AsString();
        }

        /// <summary>
        /// 设置测点的名称，比如“CX1”。此参数是在测点族的设计时添加进去的，而不是通过API添加的。但是其值可以通过API设置。
        /// </summary>
        /// <summary>
        /// 将测点数据类序列化之后的字符保存到测点对象的参数中。
        /// </summary>
        public void SetMonitorName(Transaction tran, string MonitorName)
        {
            Parameter para = Monitor.get_Parameter(Constants.SP_MonitorName_Guid);
            para.Set(MonitorName);

            // store its name in the private variable.
            this._monitorName = MonitorName;
        }

        /// <summary>
        /// 提取测点监测数据所对应的序列化字符。即测点族中“监测数据”参数中的字符。
        /// 如果要提取监测数据为对应的数据类，可以去调用具体派生类的 GetMonitorData 函数
        /// </summary>
        protected string GetMonitorDataString()
        {
            return Monitor.get_Parameter(Constants.SP_MonitorData_Guid).AsString();
        }

        /// <summary>
        /// 将测点数据类序列化之后的字符保存到测点对象的参数中。
        /// </summary>
        protected void SetMonitorDataString(Transaction tran, string dataString)
        {
            Parameter para = Monitor.get_Parameter(Constants.SP_MonitorData_Guid);
            para.Set(dataString);
        }

        #endregion

        #region   ---   与Excel数据库的数据交互

        /// <summary> 从Excel中导入监测数据 </summary>
        /// <param name="tran"> 已经start的Revit事务 </param>
        /// <param name="conn">连接到Excel工作簿</param>
        /// <param name="sheetName">监测数据所在的Excel工作表，表名称中应该包含后缀$ </param>
        /// <param name="fieldName">监测数据在工作表中的哪个字段下。对于线测点，其fieldName是不必要的。</param>
        public abstract void ImportFromExcel(Transaction tran, OleDbConnection conn, string sheetName, string fieldName);

        #endregion
    }
}
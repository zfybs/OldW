using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldW.Instrumentations
{

    /// <summary>
    /// 测点收集器，用来对测点集合进行分类管理以及过滤等操作
    /// </summary>
    public class InstrumCollector
    {
        /// <summary> 所有的测点的集合 </summary>
        List<Instrumentation> _allInstrumentations;
        /// <summary> 所有的测点的集合 </summary>
        public List<Instrumentation> AllInstrumentations
        {
            get { return _allInstrumentations; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="instrums"> 要进行测点分类的测点集合 </param>
        public InstrumCollector(IEnumerable<Instrumentation> instrums)
        {
            //
            _allInstrumentations = new List<Instrumentation>();

            // 
            Truncate(instrums);
        }

        /// <summary>
        /// 清空原测点集合中的元素，并重新添加新的元素
        /// </summary>
        /// <param name="instrums"></param>
        public void Truncate(IEnumerable<Instrumentation> instrums)
        {
            //
            _allInstrumentations.Clear();

            Append(instrums);
        }

        /// <summary>
        /// 直接向集合中附加新的测点
        /// </summary>
        /// <param name="instrums"></param>
        public void Append(IEnumerable<Instrumentation> instrums)
        {
            _allInstrumentations.AddRange(instrums);

        }

        /// <summary> 按指定的类型过滤出集合中所有的测点 </summary>
        /// <param name="type"> 多种监测类型的按位组合 </param>
        public List<Instrumentation> GetMonitors(InstrumentationType type)
        {
            var q = from Instrumentation r in AllInstrumentations
                    where (r.Type & type) > 0
                    select r;
            return q.ToList();
        }

        #region ---   过滤出线测点 

        /// <summary> 过滤出集合中所有的线测点 </summary>
        public List<Instrum_Line> GetLineMonitors()
        {
            var q = from Instrumentation r in AllInstrumentations
                    where r is Instrum_Line
                    select (Instrum_Line)r;
            return q.ToList();
        }

        /// <summary> 按指定的选项过滤出集合中所有的线测点 </summary>
        /// <param name="type"> 多种监测类型的按位组合 </param>
        public List<Instrum_Line> GetLineMonitors(InstrumentationType type)
        {
            var q = from Instrumentation r in AllInstrumentations
                    where (r is Instrum_Line) && (r.Type & type) > 0
                    select (Instrum_Line)r;
            return q.ToList();
        }

        #endregion

        #region ---   过滤出点测点 

        /// <summary> 过滤出集合中所有的线测点 </summary>
        public List<Instrum_Point> GetPointMonitors()
        {
            var q = from Instrumentation r in AllInstrumentations
                    where r is Instrum_Point
                    select (Instrum_Point)r;
            return q.ToList();
        }

        /// <summary> 按指定的选项过滤出集合中所有的点测点 </summary>
        /// <param name="type"> 多种监测类型的按位组合 </param>
        public List<Instrum_Point> GetPointMonitors(InstrumentationType type)
        {
            var q = from Instrumentation r in AllInstrumentations
                    where r is Instrum_Point && (r.Type & type) > 0
                    select (Instrum_Point)r;
            return q.ToList();
        }

        #endregion
    }

}

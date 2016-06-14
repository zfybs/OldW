using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using stdOldW;
using stdOldW.DAL;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 所有类型的线监测，包括测斜管
    /// </summary>
    /// <remarks></remarks>
    public class Instrum_Line : Instrumentation
    {
        #region    ---   Properties

        /// <summary>
        /// 线测点的整个施工阶段中的监测数据
        /// </summary>
        private MonitorData_Line _monitorData;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="MonitorLine">所有类型的监测管线，包括测斜管，但不包括地表沉降、立柱隆起、支撑轴力等</param>
        /// <param name="Type">监测点的测点类型，也是测点所属的族的名称</param>
        /// <remarks></remarks>
        public Instrum_Line(FamilyInstance MonitorLine, InstrumentationType Type)
            : base(MonitorLine, Type)
        {
        }

        /// <summary>
        /// 从指定的Element集合中，找出所有的点测点元素
        /// </summary>
        /// <param name="elements"> 要进行搜索过滤的Element集合</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public new static List<Instrum_Line> Lookup(Document doc, ICollection<ElementId> elements)
        {
            List<Instrum_Line> lines = new List<Instrum_Line>();

            List<Instrumentation> instrus;
            instrus = Instrumentation.Lookup(doc, elements);
            foreach (var ins in instrus)
            {
                if (ins is Instrum_Line)
                {
                    lines.Add(ins as Instrum_Line);
                }
            }
            return lines;
        }

        #region   ---   监测数据的提取与保存

        /// <summary>
        /// 将测点对象中的监测数据提取为具体的序列化类。
        /// 其中包括线测点的每一个子节点的数据，以及整个线测点在整个施工过程中所有的测点数据。
        /// </summary>
        /// <returns>如果在派生类中将此方法进行重写，则一定要对应地对 <see cref="SetMonitorData"/> 方法进行重写。</returns>
        public virtual MonitorData_Line GetMonitorData()
        {
            if (_monitorData != null)
            {
                return _monitorData;
            }

            string strData = base.getMonitorDataString();
            MonitorData_Line mData = null;
            if (strData.Length > 0)
            {
                try
                {
                    mData = (MonitorData_Line)StringSerializer.Decode64(strData);
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
            return null;
        }

        /// <summary>
        /// 将监测数据以序列化字符串保存到对应的Parameter对象中。
        /// </summary>
        /// <remarks>如果在派生类中将此方法进行重写，则一定要对应地对 <see cref="GetMonitorData"/> 方法进行重写。</remarks>
        public virtual void SetMonitorData(Transaction tran, MonitorData_Line data)
        {

            // 将数据序列化为字符串
            // 如果data为空，则表示将原来的监测数据清空，所以也是可以的
            string strData = StringSerializer.Encode64(data);
            base.setMonitorDataString(tran, strData);

            // 将数据在类实例中保存下来
            _monitorData = data;

        }

        #endregion

        #region   ---   与Excel数据库的数据交互

        /// <summary> 从Excel中导入监测数据 </summary>
        /// <param name="tran"> 已经start的Revit事务 </param>
        /// <param name="conn">连接到Excel工作簿</param>
        /// <param name="sheetName">监测数据所在的Excel工作表，表名称中应该包含后缀$ </param>
        /// <param name="fieldName">监测数据在工作表中的哪个字段下。对于线测点，其fieldName是不必要的。</param>
        public override void ImportFromExcel(Transaction tran, OleDbConnection conn, string sheetName, string fieldName)
        {
            var dt = ExcelDbHelper.GetDataFromSheet(conn, sheetName);
            var data = MonitorData_Line.FromDataTable(dt);
            SetMonitorData(tran, data);
        }

        #endregion
    }
}
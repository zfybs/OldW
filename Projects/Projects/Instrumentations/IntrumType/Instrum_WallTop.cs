using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OldW.GlobalSettings;
using stdOldW.DAL;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 墙顶位移测点
    /// </summary>
    /// <remarks></remarks>
    public class Instrum_WallTop : Instrum_Line
    {
        #region    ---   Properties

        #endregion

        #region    ---   Fields


        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="inclinometerElement">测斜管所对应的图元</param>
        public Instrum_WallTop(FamilyInstance inclinometerElement) : base(inclinometerElement, InstrumentationType.墙顶位移, false)
        {

        }

        #region   ---   与 Excel 数据库的数据交互

        bool enteredOnce = false;

        /// <summary> 假设 Revit 开启的事务的HashCode不可能为0。 </summary>
        int enteredTransactionHashCode = 0;

        /// <summary> 将Excel中墙顶位移监测数据（两个点测点的格式）导入Revit的测点单元（线测点）中 </summary>
        /// <param name="tran"> 已经start的Revit事务 </param>
        /// <param name="conn">连接到Excel工作簿</param>
        /// <param name="sheetName">监测数据所在的Excel工作表，表名称中应该包含后缀$ </param>
        /// <param name="fieldName">监测数据在工作表中的哪个字段下。对于线测点，其fieldName是不必要的。</param>
        public override void ImportFromExcel(Transaction tran, OleDbConnection conn, string sheetName, string fieldName)
        {
            string node1 = Constants.ExcelDatabaseSheet_WallTopV;
            string node2 = Constants.ExcelDatabaseSheet_WallTopH;

            // 工作表名称有效性判断
            byte nodeIndex = 0;
            if (sheetName.StartsWith(node1))
            {
                nodeIndex = 0;
            }
            else if (sheetName.StartsWith(node2))
            {
                nodeIndex = 1;
            }
            else
            {
                throw new InvalidCastException($"指定的工作表名称不是有效的墙顶位移名称，请将Excel中工作表名称设置为以“ {node1} ”或者“ {node2} ” 开头。");
            }

            // 导入监测数据

            if (!enteredOnce)  // 采用先清空的方式导入第一个子节点的监测数据
            {
                TruncateAndAddOneField(tran, conn, sheetName, fieldName, nodeIndex);

                // 通过名称记录当前状态
                enteredOnce = true;
            }
            else     // 采用附加的方式导入另一个子节点的监测数据
            {
                if (enteredTransactionHashCode != tran.GetHashCode())
                {
                    TruncateAndAddOneField(tran, conn, sheetName, fieldName, nodeIndex);

                    // 通过名称记录当前状态
                    enteredOnce = true;
                }
                else  // 相同的事务第二次进入
                {
                    AppendOneField(tran, conn, sheetName, fieldName, nodeIndex);

                    //
                    enteredOnce = false;
                }
            }
            enteredTransactionHashCode = tran.GetHashCode();
            }

        /// <summary> 采用先清空的方式导入第一个子节点的监测数据 </summary>
        private void TruncateAndAddOneField(Transaction tran, OleDbConnection conn, string sheetName, string fieldName, byte nodeIndex)
        {
            // 采用先清空的方式导入第一个子节点的监测数据
            DataTable dt = ExcelDbHelper.GetFieldDataFromExcel(conn, sheetName, Constants.ExcelDatabasePrimaryKeyName, fieldName);
            MonitorData_Line lineData = new MonitorData_Line(new string[] { Constants.ExcelDatabaseSheet_WallTopV, Constants.ExcelDatabaseSheet_WallTopH }, false);
            foreach (DataRow r in dt.Rows)
            {
                // 这一天的多个节点的数据
                float?[] values = new float?[] { null, null };
                if (!Convert.IsDBNull(r[1]))
                {
                    values[nodeIndex] = Convert.ToSingle(r[1]);
                }
                // 记录这一天的监测数据信息
                lineData.MonitorData.Add((DateTime)r[0], values);
            }
            SetMonitorData(tran, lineData);

        }

        /// <summary> 采用附加的方式导入另一个子节点的监测数据 </summary>
        private void AppendOneField(Transaction tran, OleDbConnection conn, string sheetName, string fieldName, byte nodeIndex)
        {
            // 先提取已经Revit中保存过的监测数据
            MonitorData_Line lineData = GetMonitorData();
            // Excel中要进行附加的另一个子节点的信息
            DataTable dt = ExcelDbHelper.GetFieldDataFromExcel(conn, sheetName, Constants.ExcelDatabasePrimaryKeyName, fieldName);

            foreach (DataRow r in dt.Rows)
            {
                float?[] values;
                DateTime dtime = (DateTime)r[0];  // 日期数据
                if (lineData.MonitorData.Keys.Contains(dtime))
                {
                    // 修改原有记录中的数据
                    values = lineData.MonitorData[dtime];
                    if (Convert.IsDBNull(r[1]))
                    {
                        values[nodeIndex] = null;
                    }
                    else
                    {
                        values[nodeIndex] = Convert.ToSingle(r[1]);
                    }
                }
                else
                {
                    // 添加新的记录
                    values = new float?[] { null, null };
                    if (!Convert.IsDBNull(r[1]))
                    {
                        values[nodeIndex] = Convert.ToSingle(r[1]);
                    }

                    // 记录这一天的监测数据信息
                    lineData.MonitorData.Add((DateTime)r[0], values);
                }
            }
            SetMonitorData(tran, lineData);

        }

        #endregion
    }
}

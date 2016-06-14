using System;
using System.Data.OleDb;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace OldW.Instrumentations
{
    /// <summary>
    /// 墙顶位移测点
    /// </summary>
    /// <remarks></remarks>
    public class Instrum_WallTop: Instrum_Line
    {
        #region    ---   Properties

        #endregion

        #region    ---   Fields
        

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="inclinometerElement">测斜管所对应的图元</param>
        public Instrum_WallTop(FamilyInstance inclinometerElement) : base(inclinometerElement, InstrumentationType.墙顶位移)
        {
            
        }

        #region   ---   与Excel数据库的数据交互

        /// <summary> 从Excel中导入监测数据 </summary>
        /// <param name="tran"> 已经start的Revit事务 </param>
        /// <param name="conn">连接到Excel工作簿</param>
        /// <param name="sheetName">监测数据所在的Excel工作表，表名称中应该包含后缀$ </param>
        /// <param name="fieldName">监测数据在工作表中的哪个字段下。对于线测点，其fieldName是不必要的。</param>
        public override void ImportFromExcel(Transaction tran, OleDbConnection conn, string sheetName,string fieldName)
        {
       
        }
        #endregion
    }
}

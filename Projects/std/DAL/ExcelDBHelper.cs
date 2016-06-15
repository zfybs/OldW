using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace stdOldW.DAL
{
    /// <summary>
    /// 利用ADO.NET连接Excel数据库，并执行相应的操作：
    /// 创建表格，读取数据，写入数据，获取工作簿中的所有工作表名称。
    /// </summary>
    public static class ExcelDbHelper
    {

        #region Excel 数据库的连接

        /// <summary>
        /// 创建对Excel工作簿的连接
        /// </summary>
        /// <param name="excelWorkbookPath">要进行连接的Excel工作簿的路径</param>
        /// <param name="iMEX"> 数据库的连接方式。
        /// 	当 IMEX=0 时为“导出模式Export mode”，这个模式开启的 Excel 档案只能用来做“写入”用途；
        ///  	当 IMEX=1 时为“导入模式Import mode”，这个模式开启的 Excel 档案只能用来做“读取”用途。IMEX=1将前8行的值中有字符类型的字段的数据类型看作字符类型；
        /// 	当 IMEX=2 时为“连结模式Linked mode (full update capabilities)”，这个模式开启的 Excel 档案可同时支持“读取”与“写入”用途。</param>
        /// <returns>一个OleDataBase的Connection连接，此连接还没有Open。</returns>
        /// <remarks></remarks>
        public static OleDbConnection ConnectToExcel(string excelWorkbookPath, byte iMEX)
        {
            string strConn = string.Empty;
            if (excelWorkbookPath.EndsWith(".xls"))
            {
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0; " +
                          "Data Source=" + excelWorkbookPath + "; " +
                          $"Extended Properties='Excel 8.0;HDR=YES;IMEX={iMEX}'";
            }

            else if (excelWorkbookPath.EndsWith(".xlsx") || excelWorkbookPath.EndsWith(".xlsb"))
            {
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" +
                          "Data Source=" + excelWorkbookPath + ";" +
                          $"Extended Properties='Excel 12.0;HDR=YES;IMEX={iMEX}'";
            }
            OleDbConnection conn = new OleDbConnection(strConn);
            return conn;
        }


        /// <summary>
        /// 验证连接的数据源是否是Excel数据库
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>如果是Excel数据库，则返回True，否则返回False。</returns>
        /// <remarks></remarks>
        private static bool ConnectionSourceValidated(OleDbConnection conn)
        {
            //考察连接是否是针对于Excel文档的。
            string strDtSource = conn.DataSource; //"C:\Users\Administrator\Desktop\测试Visio与Excel交互\数据.xlsx"
            string strExt = Path.GetExtension(strDtSource);
            if (
                string.Compare(strExt, ".xlsx", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(strExt, ".xls", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(strExt, ".xlsb", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// 创建一个新的Excel工作表，并向其中插入一条数据
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sqlCreateTable"> 用来创建工作表的sql语句 </param>
        /// <remarks></remarks>
        public static void CreateNewTable(OleDbConnection conn, string sqlCreateTable)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                using (OleDbCommand ole_cmd = conn.CreateCommand())
                {
                    //----- 生成Excel表格 --------------------
                    //要新创建的表格不能是在Excel工作簿中已经存在的工作表。
                    //ole_cmd.CommandText = "CREATE TABLE CustomerInfo ([" + tableName + "] VarChar,[Customer] VarChar)";
                    ole_cmd.CommandText = sqlCreateTable; // "CREATE TABLE Employee(EmployeeID int PRIMARY KEY CLUSTERED";
                    try
                    {
                        //在工作簿中创建新表格时，Excel工作簿不能处于打开状态
                        ole_cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("创建Excel文档失败，错误信息： " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

            }
            else
            {
                MessageBox.Show("未正确连接到Excel数据库!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        #region Excel数据库元数据的获取

        /// <summary>
        /// 从对于Excel的数据连接中获取Excel工作簿中的所有工作表（不包含Excel中的命名区域NamedRange）
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>如果此连接不是连接到Excel数据库，则返回Nothing</returns>
        /// <remarks></remarks>
        public static List<string> GetSheetsName(OleDbConnection conn)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                //获取工作簿连接中的每一个工作表，
                //注意下面的Rows属性返回的并不是Excel工作表中的每一行，而是Excel工作簿中的所有工作表。
                DataRowCollection Tables = conn.GetSchema("Tables").Rows;
                //
                List<string> sheetNames = new List<string>();
                for (int i = 0; i <= Tables.Count - 1; i++)
                {
                    //注意这里的表格Table是以DataRow的形式出现的。
                    string name = Tables[i]["TABLE_NAME"].ToString();
                    if (name.EndsWith("$"))
                    {
                        // 如果是一般的工作表，其返回的工作表名中会以$作为后缀，而Excel中的命名区域也是一种表，但是其表名不含有后缀“$”
                        sheetNames.Add(name);
                    }
                }
                return sheetNames;
            }
            else
            {
                MessageBox.Show("未正确连接到Excel数据库!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 获取指定工作表中所有字段的名称，包括主键
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"> 要在哪一个工作表中提取字段信息，表名的格式为“Sheet1$”</param>
        /// <remarks></remarks>
        public static IList<string> GetFieldNames(OleDbConnection conn, string tableName)
        {
            var dt = conn.GetSchema("Columns", new string[] { null, null, tableName });
            var names = DataTableHelper.GetValue(dt, "COLUMN_NAME");
            return names.AsEnumerable().Select(r => r.ToString()).ToList(); ;
        }

        /// <summary>
        /// 获取指定工作表中所有字段的数据类型。
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"> 要在哪一个工作表中提取字段信息，表名的格式为“Sheet1$”</param>
        /// <remarks>Excel中字段的数据类型是以数字来表示的，其中：时间=7；</remarks>
        public static IList<string> GetFieldDataType(OleDbConnection conn, string tableName)
        {
            var dt = conn.GetSchema("Columns", new string[] { null, null, tableName });
            var names = DataTableHelper.GetValue(dt, "DATA_TYPE");
            return names.AsEnumerable().Select(r => r.ToString()).ToList();
        }



        #endregion

        #region 提取Excel中的数据记录

        /// <summary>
        /// 读取Excel整张工作表中的所有字段的数据
        /// </summary>
        /// <param name="conn">OleDB的数据连接</param>
        /// <param name="sheetName">要读取的数据所在的工作表，名称中请自行包括后缀$</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DataTable GetDataFromSheet(OleDbConnection conn, string sheetName)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                //创建向数据库发出的指令
                OleDbCommand olecmd = conn.CreateCommand();
                //类似SQL的查询语句这个[Sheet1$对应Excel文件中的一个工作表]
                //如果要提取Excel中的工作表中的某一个指定区域的数据，可以用："select * from [Sheet3$A1:C5]"
                olecmd.CommandText = "select * from [" + sheetName + "]";

                //创建数据适配器——根据指定的数据库指令
                OleDbDataAdapter Adapter = new OleDbDataAdapter(olecmd);

                //创建一个数据集以保存数据
                DataSet dtSet = new DataSet();

                //将数据适配器按指令操作的数据填充到数据集中的某一工作表中（默认为“Table”工作表）
                Adapter.Fill(dtSet);

                //索引数据集中的第一个工作表对象
                DataTable dataTable = dtSet.Tables[0]; // conn.GetSchema("Tables")

                return dataTable;
            }
            else
            {
                MessageBox.Show("未正确连接到Excel数据库!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 读取Excel工作表中的某一个字段数据
        /// </summary>
        /// <param name="conn">OleDB的数据连接</param>
        /// <param name="SheetName">要读取的数据所在的工作表，名称中请自行包括后缀$</param>
        /// <param name="FieldName">在读取的字段</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string[] GetFieldDataFromExcel(OleDbConnection conn, string SheetName, string FieldName)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                //创建向数据库发出的指令
                OleDbCommand olecmd = conn.CreateCommand();
                //类似SQL的查询语句这个[Sheet1$对应Excel文件中的一个工作表]
                //如果要提取Excel中的工作表中的某一个指定区域的数据，可以用："select * from [Sheet3$A1:C5]"
                olecmd.CommandText = $"select [{FieldName}] from [{SheetName}]";

                //创建数据适配器——根据指定的数据库指令
                OleDbDataAdapter Adapter = new OleDbDataAdapter(olecmd);

                //创建一个数据集以保存数据
                DataSet dtSet = new DataSet();

                //将数据适配器按指令操作的数据填充到数据集中的某一工作表中（默认为“Table”工作表）
                Adapter.Fill(dtSet);


                //索引数据集中的第一个工作表对象
                System.Data.DataTable DataTable = dtSet.Tables[0]; // conn.GetSchema("Tables")

                //工作表中的数据有8列9行(它的范围与用Worksheet.UsedRange所得到的范围相同。
                //不一定是写有数据的单元格才算进行，对单元格的格式，如底纹，字号等进行修改的单元格也在其中。)
                int intRowsInTable = DataTable.Rows.Count;

                //提取每一行数据中的“成绩”数据
                string[] Data = new string[intRowsInTable - 1 + 1];
                for (int i = 0; i <= intRowsInTable - 1; i++)
                {
                    // 如果单元格中没有数据的话，则对应的数据的类型为System.DBNull
                    Data[i] = DataTable.Rows[i][0].ToString();
                }
                return Data;
            }
            else
            {
                MessageBox.Show("未正确连接到Excel数据库!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 读取Excel工作表中的某一个字段数据
        /// </summary>
        /// <param name="conn">OleDB的数据连接</param>
        /// <param name="SheetName">要读取的数据所在的工作表，名称中请自行包括后缀$</param>
        /// <param name="FieldName">在读取的字段</param>
        /// <typeparam name="T">要提取的字段的数据类型，比如设置为 double? 等可空类型 </typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static T[] GetFieldDataFromExcel<T>(OleDbConnection conn, string SheetName, string FieldName)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                //创建向数据库发出的指令
                OleDbCommand olecmd = conn.CreateCommand();
                //类似SQL的查询语句这个[Sheet1$对应Excel文件中的一个工作表]
                //如果要提取Excel中的工作表中的某一个指定区域的数据，可以用："select * from [Sheet3$A1:C5]"
                olecmd.CommandText = "select * from [" + SheetName + "]";

                //创建数据适配器——根据指定的数据库指令
                OleDbDataAdapter Adapter = new OleDbDataAdapter(olecmd);

                //创建一个数据集以保存数据
                DataSet dtSet = new DataSet();

                //将数据适配器按指令操作的数据填充到数据集中的某一工作表中（默认为“Table”工作表）
                Adapter.Fill(dtSet);

                //其中的数据都是由 "select * from [" + SheetName + "$]"得到的Excel中工作表SheetName中的数据。
                int intTablesCount = dtSet.Tables.Count;

                //索引数据集中的第一个工作表对象
                System.Data.DataTable DataTable = dtSet.Tables[0]; // conn.GetSchema("Tables")

                //工作表中的数据有8列9行(它的范围与用Worksheet.UsedRange所得到的范围相同。
                //不一定是写有数据的单元格才算进行，对单元格的格式，如底纹，字号等进行修改的单元格也在其中。)
                int intRowsInTable = DataTable.Rows.Count;
                int intColsInTable = DataTable.Columns.Count;

                //提取每一行数据中的“成绩”数据
                T[] Data = new T[intRowsInTable - 1 + 1];
                for (int i = 0; i <= intRowsInTable - 1; i++)
                {
                    // 如果单元格中没有数据的话，则对应的数据的类型为System.DBNull
                    object v = DataTable.Rows[i][FieldName];

                    // 注意：Convert.IsDBNull(null)所返回的值为false
                    Data[i] = Convert.IsDBNull(v) ? default(T) : (T)v;
                }
                return Data;
            }
            else
            {
                MessageBox.Show("未正确连接到Excel数据库!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 读取Excel工作簿中的多个字段的数据
        /// </summary>
        /// <param name="conn">OleDB的数据连接</param>
        /// <param name="SheetName">要读取的数据所在的工作表，工作表名请自行添加后缀“$”</param>
        /// <param name="FieldNames">在读取的每一个字段的名称</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DataTable GetFieldDataFromExcel(OleDbConnection conn, string SheetName, params string[] FieldNames)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                //创建向数据库发出的指令
                OleDbCommand olecmd = conn.CreateCommand();

                //类似SQL的查询语句这个[Sheet1$对应Excel文件中的一个工作表]
                //如果要提取Excel中的工作表中的某一个指定区域的数据，可以用："select * from [Sheet3$A1:C5]"
                olecmd.CommandText = "select " + ConstructFieldNames(FieldNames) + " from [" + SheetName + "]";

                //创建数据适配器——根据指定的数据库指令
                OleDbDataAdapter Adapter = new OleDbDataAdapter(olecmd);

                //创建一个数据集以保存数据
                DataSet dtSet = new DataSet();

                //将数据适配器按指令操作的数据填充到数据集中的某一工作表中（默认为“Table”工作表）
                Adapter.Fill(dtSet);

                //索引数据集中的第一个工作表对象
                DataTable DataTable = dtSet.Tables[0]; // conn.GetSchema("Tables")
                return DataTable;
            }
            else
            {
                MessageBox.Show("未正确连接到Excel数据库!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return null;
        }

        /// <summary> 执行有参SQL语句，返回DataTable </summary>
        /// <param name="conn"></param>
        /// <param name="safeSql">数据查询语句，比如“ Selete * From [Sheet1$] ”</param>
        /// <returns> DataAdapter.Fill得到的DataSet中的第一个DataTable </returns>
        public static DataTable GetDataSet(OleDbConnection conn, string safeSql)
        {
            DataSet ds = new DataSet();
            OleDbCommand cmd = new OleDbCommand(safeSql, conn);
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(ds);
            return ds.Tables[0];
        }

        #endregion

        #region 数据写入Excel

        /// <summary>
        /// 将一个全新的 DataTable 对象写入 Excel 数据库中
        /// </summary>
        /// <param name="conn"> </param>
        /// <param name="tableSource"> 数据源，此工作表中的每一个字段中的数据都会被插入到Excel的指定工作表中。
        /// 请手动确保工作表Sheet中有与DataTable中每一列同名的字段，而且其数据类型是兼容的。 </param>
        /// <param name="sheetName"> 要进行插入的Excel工作表的名称，其格式为“Sheet1$”</param>
        public static void InsertDataTable(OleDbConnection conn, DataTable tableSource, string sheetName)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                string[] fields = new string[tableSource.Columns.Count];
                // 获取字段名
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = tableSource.Columns[i].ColumnName;
                }

                //创建向数据库发出的指令
                OleDbCommand olecmd = conn.CreateCommand();

                try
                {
                    string fieldNames = ConstructFieldNames(fields);
                    // 将DataTable中的每一行数据插入Excel工作表中的对应字段下。
                    foreach (DataRow row in tableSource.Rows)
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.Append($"INSERT INTO [{sheetName}] ({fieldNames}) values ( ");
                        // 将要赋的值添加到sql语句中
                        ConstructDbValue(row.ItemArray, ref sb);
                        sb.Append(")");

                        olecmd.CommandText = sb.ToString();
                        // 大致的效果是这样的。
                        // olecmd.CommandText = $"INSERT INTO [Sheet2$] (Field1, Field2, Field3) values (\'{row[0]}\',\'{row[1]}\',\'{row[1]}\')";

                        // 对于Excel，好像没有像SQL Server一样通过
                        // Insert Into[Sheet1$] ([Field3], [Field4]) VALUES(-1.475, -0.335), (1.2, 3.44)
                        // 来一次插入多条记录的方法，而且官方说明的案例中也是按上例中的为每一个Insert Into执行一次ExecuteNonQuery来实现多条记录的插入的。
                        olecmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowDebugCatch(ex, "DataTable中的数据插入Excel工作表出错");
                    throw;
                }
            }
        }


        /// <summary>
        /// 向Excel工作表中插入一条数据，此函数并不通用，不建议使用
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="TableName">要插入数据的工作表名称</param>
        /// <param name="FieldName">要插入到的字段</param>
        /// <param name="Value">实际插入的数据</param>
        /// <remarks></remarks>
        public static void InsertToTable(OleDbConnection conn, string TableName, string FieldName, object Value)
        {
            //如果连接已经关闭，则先打开连接
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            if (ConnectionSourceValidated(conn))
            {
                using (OleDbCommand ole_cmd = conn.CreateCommand())
                {
                    //在插入数据时，字段名必须是数据表中已经有的字段名，插入的数据类型也要与字段下的数据类型相符。

                    try
                    {
                        ole_cmd.CommandText = "insert into [" + TableName + ("$](" + FieldName) + ") values(\'" + Value.ToString() + "\')";

                        //这种插入方式在Excel中的实时刷新的，也就是说插入时工作簿可以处于打开的状态，
                        //而且这里插入后在Excel中会立即显示出插入的值。
                        ole_cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("数据插入失败，错误信息： " + ex.Message);
                        return;
                    }
                }

            }
            else
            {
                MessageBox.Show("未正确连接到Excel数据库!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// 将要提取的字段名称转换为SQL语句中的字段名称字符
        /// </summary>
        /// <param name="FieldNames"></param>
        /// <returns></returns>
        private static string ConstructFieldNames(IList<string> FieldNames)
        {
            string names = "";
            if (FieldNames.Count >= 1)
            {
                names = @"[" + FieldNames[0] + @"]";
            }
            for (int i = 1; i < FieldNames.Count; i++)
            {
                names += @", [" + FieldNames[i] + @"]";
            }
            return names;
        }

        /// <summary>
        /// 将要提取的字段名称转换为SQL语句中的字段名称字符
        /// </summary>
        /// <param name="values"></param>
        /// <param name="sb"></param>
        private static void ConstructDbValue(IList<object> values, ref StringBuilder sb)
        {
            // 不能将空字符串赋值给可为空的double类型字段，要先将空字符转换为null
            if (values.Count > 0)
            {
                sb.Append(Convert.IsDBNull(values[0]) ? "null" : "\'" + values[0].ToString() + "\'");
            }
            for (int i = 1; i < values.Count; i++)
            {
                // 注意这里有一个“,”的区别
                sb.Append(Convert.IsDBNull(values[i]) ? ",null" : ",\'" + values[i].ToString() + "\'");
            }
        }

        #endregion

    }
}
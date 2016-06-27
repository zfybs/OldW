using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace stdOldW.DAL
{
    /// <summary>
    /// 与 DataTable 相关的格式转换等操作
    /// </summary>
    public static class DataTableHelper
    {
        #region 将实体类集合转换为 DataTable

        /// <summary>
        /// 将实体类中所有的非[Browsable(false)]属性转换成DataTable
        /// </summary>
        /// <param name="modelList">实体类集合。如果集合中元素个数为0，则可以成功创建出一个空表格。</param>
        /// <typeparam name="TEntityClass">数据表所对应的实体类，
        /// 只有实体类中的属性Property才会被创建为数据表中的对应字段，而且此Property不能有[Browsable(false)]标记。</typeparam>
        /// <returns></returns>
        public static DataTable FillDataTable<TEntityClass>(IList<TEntityClass> modelList) where TEntityClass : class
        {
            if (modelList == null)
            {
                return null;
            }

            // 创建新表格，并添加对应字段
            DataTable dt = new DataTable(typeof(TEntityClass).Name);
            var tableFields = DataTableHelper.GetPropertiesForTableField<TEntityClass>();

            foreach (PropertyInfo propertyInfo in tableFields)
            {
                dt.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
            }

            // 为表格中填充数据
            foreach (TEntityClass model in modelList)
            {
                DataRow dataRow = dt.NewRow();
                foreach (PropertyInfo propertyInfo in tableFields)
                {
                    dataRow[propertyInfo.Name] = propertyInfo.GetValue(model, null);
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 将实体类中的指定名称的非[Browsable(false)]属性转换成DataTable
        /// </summary>
        /// <param name="modelList">实体类集合。如果集合中元素个数为0，则可以成功创建出一个空表格。</param>
        /// <param name="fieldName">要从实体类中提取成为数据表字段的属性名称（区分大小写）</param>
        /// <typeparam name="TEntityClass">数据表所对应的实体类，
        /// 只有实体类中的属性Property才会被创建为数据表中的对应字段，而且此Property不能有[Browsable(false)]标记。</typeparam>
        /// <returns></returns>
        public static DataTable FillDataTable<TEntityClass>(IList<TEntityClass> modelList, params string[] fieldName) where TEntityClass : class
        {
            if (modelList == null)
            {
                return null;
            }

            // 创建新表格，并添加对应字段
            DataTable dt = new DataTable(typeof(TEntityClass).Name);
            var tableFields = DataTableHelper.GetPropertiesForTableField<TEntityClass>(fieldName);

            foreach (PropertyInfo propertyInfo in tableFields)
            {
                dt.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
            }

            // 为表格中填充数据
            foreach (TEntityClass model in modelList)
            {
                DataRow dataRow = dt.NewRow();
                foreach (PropertyInfo propertyInfo in tableFields)
                {
                    dataRow[propertyInfo.Name] = propertyInfo.GetValue(model, null);
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }

        /// <summary>
        /// 将指定实体类中的属性进行甄别，以选择出其中可以作为表格字段的属性。
        /// 字段只能对应于属性Property，而且其不能有[Browsable(false)]标记。
        /// </summary>
        /// <typeparam name="TEntityClass">数据表所对应的实体类</typeparam>
        /// <returns></returns>
        private static List<PropertyInfo> GetPropertiesForTableField<TEntityClass>() where TEntityClass : class
        {
            List<PropertyInfo> p = new List<PropertyInfo>();
            foreach (PropertyInfo propertyInfo in typeof(TEntityClass).GetProperties())
            {

                // 检测1：
                object[] Att = propertyInfo.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), false);
                if (Att.Length > 0 && !((BrowsableAttribute)Att[0]).Browsable)
                {
                    // 如果此属性中包含有Browsable特性，而且其值为false，则不计入表格项
                    // [System.ComponentModel.Browsable(false)]
                    continue;
                }
                if ((propertyInfo.Name != "CTimestamp"))// CTimestamp字段为oracle中的Timesstarmp类型
                {
                    p.Add(propertyInfo);
                }
            }
            return p;
        }

        /// <summary>
        /// 将指定实体类中的指定名称的属性转换为DataTable中的字段对象
        /// 字段只能对应于属性Property，而且其不能有[Browsable(false)]标记。
        /// </summary>
        /// <param name="fieldName">要从实体类中提取成为数据表字段的属性名称（区分大小写）</param>
        /// <typeparam name="TEntityClass">数据表所对应的实体类</typeparam>
        /// <returns></returns>
        private static List<PropertyInfo> GetPropertiesForTableField<TEntityClass>(params string[] fieldName) where TEntityClass : class
        {
            List<PropertyInfo> p = new List<PropertyInfo>();
            foreach (PropertyInfo propertyInfo in typeof(TEntityClass).GetProperties())
            {

                // 检测1：
                object[] Att = propertyInfo.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), false);
                if (Att.Length > 0 && !((BrowsableAttribute)Att[0]).Browsable)
                {
                    // 如果此属性中包含有Browsable特性，而且其值为false，则不计入表格项
                    // [System.ComponentModel.Browsable(false)]
                    continue;
                }
                if (fieldName.Contains(propertyInfo.Name))
                {// 满足要求的属性的名称是否包含在输入参数的属性集合中。
                    p.Add(propertyInfo);
                }

            }
            //
            if (p.Count < fieldName.Length)
            {
                throw new ArgumentOutOfRangeException(@"某些指定的字段名在类型中找不到对应的有效属性。");
            }
            return p;
        }

        #endregion

        /// <summary> 如果输入的值为null，则返回 DBNull.Value，否则返回这个值本身 </summary>
        public static object FilterNull(object value)
        {
            return value ?? DBNull.Value;
        }

        #region 将表格打印为格式化的字符

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static IList<Object> GetValue(DataTable table, string columnName)
        {
            List<Object> ls = table.AsEnumerable().Select(r => r[columnName]).ToList();
            return ls;
        } 
        #endregion

        #region 将表格打印为格式化的字符

        /// <summary>
        /// 将表格打印为格式化的字符
        /// </summary>
        /// <param name="table">要打印的表格</param>
        /// <param name="label">表格的标题</param>
        /// <returns></returns>
        public static StringBuilder PrintTableOrView(DataTable table, string label)
        {
            StringBuilder sb = new StringBuilder();

            // 标题
            sb.Append(label + "\n");

            // 列名
            sb.Append("记录行");
            foreach (DataColumn col in table.Columns)
            {
                sb.Append("\t" + col.ColumnName);
            }
            sb.Append("\n");

            // 添加数据
            for (int i = 0; i < table.Rows.Count; i++)
            {
                // 一行数据，手动添加一列“Id”
                sb.Append(i + 1);

                for (int j = 0; j < table.Columns.Count; j++)
                {
                    sb.Append("\t" + table.Rows[i][j].ToString());
                }
                sb.Append("\n");
            }
            return sb;
        }

        /// <summary>
        /// 将表格视图打印为格式化的字符
        /// </summary>
        /// <param name="table">要打印的表格视图</param>
        /// <param name="label">视图的标题</param>
        /// <returns></returns>
        public static StringBuilder PrintTableOrView(DataView view, string label)
        {
            DataTable table = view.ToTable();
            return PrintTableOrView(table, label);
        }

        #endregion
    }
}

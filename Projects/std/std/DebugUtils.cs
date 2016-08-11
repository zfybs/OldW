using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace stdOldW
{
    /// <summary>
    /// 提供一些基础性的操作工具
    /// </summary>
    /// <remarks></remarks>
    public static class DebugUtils
    {
        #region 弹框显示集合中的某些属性或者字段的值

        /// <summary>
        /// 将集合中的每一个元素的ToString函数的结果组合到一个字符串中进行显示
        /// </summary>
        /// <param name="V"></param>
        /// <param name="title"></param>
        /// <param name="newLineHandling"> 如果元素之间是以换行分隔，则为True，否则是以逗号分隔。 </param>
        /// <remarks></remarks>
        public static void ShowEnumerable(IEnumerable V, string title = "集合中的元素", bool newLineHandling = true)
        {
            StringBuilder sb = new StringBuilder();
            if (newLineHandling)
            {
                foreach (object o in V)
                {
                    sb.AppendLine(o.ToString());
                }
            }
            else
            {
                foreach (object o in V)
                {
                    sb.Append(o.ToString() + ",\t");
                }
            }

            MessageBox.Show(sb.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 将集合中的每一个元素的指定属性的ToString函数的结果组合到一个字符串中进行显示
        /// </summary>
        /// <param name="V"></param>
        /// <param name="PropertyName">要读取的属性的名称，注意，此属性不能带参数。</param>
        /// <param name="newLineHandling"> 如果元素之间是以换行分隔，则为True，否则是以逗号分隔。 </param>
        /// <remarks></remarks>
        public static void ShowEnumerableProperty(IEnumerable V, string PropertyName, string Title = "集合中的元素", bool newLineHandling = true)
        {
            List<string> strings = new List<string>();

            Type tp = default(Type);
            MethodInfo MdInfo = default(MethodInfo);
            string res = "";
            foreach (object obj in V)
            {
                tp = obj.GetType();
                MdInfo = tp.GetProperty(PropertyName).GetMethod;
                res = MdInfo.Invoke(obj, null).ToString();
                //
                strings.Add(res);
            }
            ShowEnumerable(strings, Title, newLineHandling);
        }

        /// <summary>
        /// 将集合中的每一个元素的指定字段的ToString函数的结果组合到一个字符串中进行显示
        /// </summary>
        /// <param name="V"></param>
        /// <param name="FieldName">要读取的字段的名称。</param>
        /// <param name="newLineHandling"> 如果元素之间是以换行分隔，则为True，否则是以逗号分隔。 </param>
        /// <remarks></remarks>
        public static void ShowEnumerableField(IEnumerable V, string FieldName, string Title = "集合中的元素", bool newLineHandling = true)
        {
            List<string> strings = new List<string>();
            Type tp = default(Type);

            string res = "";
            foreach (object obj in V)
            {
                tp = obj.GetType();
                res = tp.GetField(FieldName).GetValue(obj).ToString();
                //
                strings.Add(res);
            }
            ShowEnumerable(strings, Title, newLineHandling);
        }

        #endregion

        /// <summary>
        /// 在调试阶段，为每一种报错显示对应的报错信息及出错位置。
        /// 在软件发布前，应将此方法中的内容修改为常规的报错提醒。
        /// </summary>
        /// <param name="ex"> Catch 块中的 Exception 对象</param>
        /// <param name="message">报错信息提示</param>
        /// <param name="title"> 报错对话框的标题 </param>
        public static void ShowDebugCatch(Exception ex, string message, string title = "出错")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine(ex.Message);

            // 一直向下提取InnerException
            Exception exInner = ex.InnerException;
            Exception exStack = ex;
            while (exInner != null)
            {
                exStack = exInner;
                sb.AppendLine(exInner.Message);
                exInner = exInner.InnerException;
            }
            // 最底层的出错位置
            sb.AppendLine("\r\n" + exStack.StackTrace);

            MessageBox.Show(sb.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
       
    }
}
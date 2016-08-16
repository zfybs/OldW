using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using stdOldW.UserControls;

namespace stdOldW
{
    /// <summary>
    /// 提供一些基础性的操作工具
    /// </summary>
    /// <remarks></remarks>
    public static class Utils
    {
        /// <summary>
        /// 返回Nullable所对应的泛型。如果不是Nullable泛型，则返回null。
        /// </summary>
        /// <param name="typeIn"></param>
        /// <returns></returns>
        public static Type GetNullableGenericArgurment(Type typeIn)
        {
            // We need to check whether the property is NULLABLE
            if (typeIn.IsGenericType && typeIn.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                return typeIn.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }
        }
        
        /// <summary> 根据当前指定的时间跨度来对当前时间的增减 </summary>
        /// <param name="originTime">初始时间</param>
        /// <param name="spanValue">时间跨度的数值。正值表示增加时间跨度，负值表示送去时间跨度。</param>
        /// <param name="spanUnit">时间跨度的单位</param>
        /// <returns></returns>
        public static DateTime GetTimeFromTimeSpan(DateTime originTime,double spanValue,TimeSpanUnit spanUnit)
        {
            //
            DateTime modifiedTime = default(DateTime);
            switch (spanUnit)
            {
                case TimeSpanUnit.Years:
                    {
                        modifiedTime = originTime.AddYears((int)spanValue);
                        break;
                    }
                case TimeSpanUnit.Months:
                    {
                        modifiedTime = originTime.AddMonths((int)spanValue);
                        break;
                    }
                case TimeSpanUnit.Days:
                    {
                        modifiedTime = originTime.AddDays(spanValue);
                        break;
                    }
                case TimeSpanUnit.Hours:
                    {
                        modifiedTime = originTime.AddHours(spanValue);
                        break;
                    }
                case TimeSpanUnit.Minites:
                    {
                        modifiedTime = originTime.AddMinutes(spanValue);
                        break;
                    }
            }

            return modifiedTime;
        }

    }
}

#region

using System;

#endregion

namespace stdOldW
{
    /// <summary>
    /// 与时间数据相关的操作，比如日期与字符、整数之间的相互转换等。
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary> 将精确到分钟的时间数据保存为对应的32位整数 </summary>
        /// <param name="dt">精确度为分钟，更精细的数据将被忽略。
        /// 由于32位数值的限制，能够识别的最大日期为4770年11月24日(23时59分)。即（2^20-1）所对应的日期。</param>
        /// <returns></returns>
        public static Int32 Time2Int(DateTime dt)
        {
            // 先分别获取日期与小时、分钟的数据
            Int32 day = (int) dt.Date.ToOADate();

            // 分钟信息最多有24*60=1440个值，所以可以用11位二进制值来保存； 
            Int32 mins = dt.Hour*60 + dt.Minute;

            // 将日期的二进制值的后面补上11个零，用以放置分钟数据
            day <<= 11;

            // 将日期与分钟数据整合为一个32位整数
            return day | mins;
        }

        /// <summary>
        /// 将由 Time2Int 函数生成的整数值再转换回日期数据。返回的最大日期为4770年11月24日(23时59分) ，精度为分钟。
        /// </summary>
        /// <param name="intValue">输入的整数值并没有明显的物理含义，
        /// 请确保这个值是通过<see cref="Time2Int"/>函数生成的。否则给出的结果不可预知，因为其中涉及到复杂的位运算操作。</param>
        /// <returns>返回的最大日期为4770年11月24日(23时59分) </returns>
        public static DateTime Int2Time(Int32 intValue)
        {
            // 还原日期与分钟数据
            int day = intValue >> 11;
            int mins = intValue & 2047; // 2047 = 2^11-1 ，对应的二进制数为11个1
            return DateTime.FromOADate(day).AddMinutes(mins);
        }

        /// <summary>
        /// 将字符转换为日期。除了.NET能够识别的日期格式外，
        /// 还增加了20160406（ 即 2016/04/06），以及 201604061330（即 2016/04/06 13:30）
        /// </summary>
        /// <param name="text">要转换为日期的字符。</param>
        /// <param name="resultedDate">字符所对应的日期。如果不能转换为日期，则返回Null</param>
        /// <returns></returns>
        public static bool String2Date(string text, ref DateTime? resultedDate)
        {
            bool blnSucceed = false;
            // 模式1. 正常的日期格式
            DateTime outDateTime;
            if (DateTime.TryParse(text, out outDateTime))
            {
                resultedDate = outDateTime;
                return true;
            }

            // 模式2. 20160406 ， 即 2016/04/06
            if (text.Length == 8)
            {
                try
                {
                    resultedDate = new DateTime(int.Parse(text.Substring(0, 4)),
                        int.Parse(text.Substring(4, 2)),
                        int.Parse(text.Substring(6, 2)));
                    return true;
                }
                catch (Exception)
                {
                    resultedDate = null;
                    return false;
                }
            }

            // 模式3. 201604061330 ， 即 2016/04/06 13:30
            if (text.Length == 12)
            {
                try
                {
                    resultedDate = new DateTime(int.Parse(text.Substring(0, 4)),
                        int.Parse(text.Substring(4, 2)),
                        int.Parse(text.Substring(6, 2)),
                        int.Parse(text.Substring(8, 2)),
                        int.Parse(text.Substring(10, 2)), 0);
                    return true;
                }
                catch (Exception)
                {
                    resultedDate = null;
                    return false;
                }
            }
            return blnSucceed;
        }
    }
}
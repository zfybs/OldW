using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace stdOldW
{
    public static class ExtensionMethods
    {
        #region "--------Icollection"

        /// <summary>
        /// 将某集合中的元素添加到另一个集合中去
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (items == null)
            {
                Debug.WriteLine("Do extension metody AddRange byly poslany items == null");
                return;
            }
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Strong-typed object cloning for objects that implement <see cref="ICloneable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Clone<T>(this T obj) where T : ICloneable
        {
            return (T)(obj as ICloneable).Clone();
        }


        /// <summary>
        /// 从集合中搜索指定项的下标位置（第一个元素的下标值为0），如果没有匹配项，则返回-1。
        /// </summary>
        /// <typeparam name="TCol"></typeparam>
        /// <param name="collection"> 匹配的数据源集合 </param>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="value"> 要进行匹配的值 </param>
        /// <returns> 从集合中搜索指定项的下标位置（第一个元素的下标值为0），如果没有匹配项，则返回-1。 </returns>
        public static int IndexOf<TCol, TVal>(this TCol collection, TVal value) where TCol : IEnumerable<TVal>
        {
            int index = -1;
            foreach (TVal item in collection)
            {
                index += 1;
                if (value.Equals(item))
                {
                    return index;
                }
            }
            return -1;
        }
        
        #endregion
    }
}
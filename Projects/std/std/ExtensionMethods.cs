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

        #endregion
    }
}
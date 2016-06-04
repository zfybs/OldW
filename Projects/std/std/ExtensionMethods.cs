﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stdOldW
{
    public static class ExtensionMethods
    {
        #region "--------Icollection"
        /// <summary>
        /// Adds range of items into collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (items == null)
            {
                System.Diagnostics.Debug.WriteLine("Do extension metody AddRange byly poslany items == null");
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
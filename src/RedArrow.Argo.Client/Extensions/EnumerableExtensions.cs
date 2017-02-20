using System;
using System.Collections;
using System.Collections.Generic;

namespace RedArrow.Argo.Client.Extensions
{
    public static class EnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> each)
        {
            foreach (var item in enumerable)
            {
                each(item);
            }
        }

        public static bool IsNullOrEmpty(this IEnumerable @this)
        {
            if (@this != null)
                return !@this.GetEnumerator().MoveNext();
            return true;
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items == null)
                return;
            foreach (T obj in items)
                action(obj);
        }
    }
}
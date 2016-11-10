using System;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Core.Extensions
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
    }
}
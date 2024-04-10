using System;
using System.Collections.Generic;

namespace EasyScript.Extensions
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Iterates through an enumerable and performs an action on each item.
        /// </summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
            return enumerable;
        }


        /// <summary>
        /// Iterates through an enumerable and performs an action on each item that matches the predicate.
        /// </summary>
        /// <param name="predicate">Ignore items that don't match this predicate.</param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action, Predicate<T> predicate)
        {
            foreach (T item in enumerable)
            {
                if (!predicate(item)) continue;
                action(item);
            }
            return enumerable;
        }
    }
}

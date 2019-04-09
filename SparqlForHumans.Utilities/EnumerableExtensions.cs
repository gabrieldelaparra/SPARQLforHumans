using System;
using System.Collections.Generic;

namespace SparqlForHumans.Utilities
{
    public static class EnumerableExtensions
    {
        public static (IList<T> TrueSlice, IList<T> FalseSlice) SliceBy<T>(this IEnumerable<T> enumerable,
            Predicate<T> predicate)
        {
            var trueSlice = new List<T>();
            var falseSlice = new List<T>();

            foreach (var t in enumerable)
                if (predicate(t))
                    trueSlice.Add(t);
                else
                    falseSlice.Add(t);

            return (trueSlice, falseSlice);
        }

        public static void AddSafe<T>(this IList<T> list, T value)
        {
            if (!list.Contains(value))
                list.Add(value);
        }

        //public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        //{
        //    return items.GroupBy(property).Select(x => x.First());
        //}
    }
}
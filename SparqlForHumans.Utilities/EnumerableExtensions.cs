using System;
using System.Collections.Generic;
using System.Linq;

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
            {
                if (predicate(t))
                {
                    trueSlice.Add(t);
                }
                else
                {
                    falseSlice.Add(t);
                }
            }

            return (trueSlice, falseSlice);
        }

        public static void AddSafe<T>(this IList<T> list, T value)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
            }
        }

        public static IEnumerable<T> IntersectIfAny<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source.Any() && target.Any())
                return source.Intersect(target);
            if (source.Any())
                return source;
            return target;
        }

        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int takeCount)
        {
            var r = new Random();
            return source.OrderBy(x => r.NextDouble()).Take(takeCount);
        }

        //public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        //{
        //    return items.GroupBy(property).Select(x => x.First());
        //}
    }
}
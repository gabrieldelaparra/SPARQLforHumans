using System;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Utilities
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<string>> GroupBySubject(this IEnumerable<string> lines)
        {
            var list = new List<string>();
            var last = string.Empty;

            foreach (var line in lines)
            {
                var entity = line.Split(' ').FirstOrDefault();

                //Base case: first value:
                if (last == string.Empty)
                {
                    list = new List<string>();
                    last = entity;
                }

                //Switch of entity:
                // - Return list,
                // - Create new list,
                // - Assign last to current
                else if (last != entity)
                {
                    yield return list;
                    list = new List<string>();
                    last = entity;
                }

                list.Add(line);
            }

            yield return list;
        }

        //public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        //{
        //    return items.GroupBy(property).Select(x => x.First());
        //}
    }
}
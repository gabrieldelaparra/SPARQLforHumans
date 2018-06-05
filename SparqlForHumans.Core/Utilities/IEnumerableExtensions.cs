using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Core.Utilities
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<string> GetFirstGroup(this IEnumerable<string> lines)
        {
            var firstLine = lines.FirstOrDefault();
            var firstEntity = firstLine.Split(" ").FirstOrDefault();
            return lines.TakeWhile(x => x.Split(" ").FirstOrDefault().Equals(firstEntity));
        }

        public static IEnumerable<string> SkipFirstGroup(this IEnumerable<string> lines)
        {
            var firstLine = lines.FirstOrDefault();
            var firstEntity = firstLine.Split(" ").FirstOrDefault();
            return lines.SkipWhile(x => x.Split(" ").FirstOrDefault().Equals(firstEntity));
        }

        public static IEnumerable<IEnumerable<string>> GroupByEntities(this IEnumerable<string> lines)
        {
            var list = new List<string>();
            var last = string.Empty;

            foreach (var line in lines)
            {
                var entity = line.Split(" ").FirstOrDefault();

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

        //public static IEnumerable<IEnumerable<string>> GroupByEntities(this IEnumerable<string> lines)
        //{
        //    while (lines.Any())
        //    {
        //        var group = lines.GetFirstGroup();
        //        lines = lines.SkipFirstGroup();
        //        yield return group;
        //    }
        //}
    }
}
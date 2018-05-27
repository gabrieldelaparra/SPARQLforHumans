using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparqlForHumans.Core.Utilities
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<string> GetFirstGroup(this IEnumerable<string> lines)
        {
            var firstLine = lines.FirstOrDefault();
            var firstEntity = firstLine.Split(" ").FirstOrDefault();
            var group = lines.TakeWhile(x => x.Split(" ").FirstOrDefault().Equals(firstEntity));
            return group;
        }

        public static IEnumerable<string> SkipFirstGroup(this IEnumerable<string> lines)
        {
            var firstLine = lines.FirstOrDefault();
            var firstEntity = firstLine.Split(" ").FirstOrDefault();
            return lines.SkipWhile(x => x.Split(" ").FirstOrDefault().Equals(firstEntity));
        }

        public static IEnumerable<IEnumerable<string>> GetSameEntityGroups(this IEnumerable<string> lines)
        {
            while (lines.Count() > 0)
            {
                var group = lines.GetFirstGroup();
                lines = lines.SkipFirstGroup();
                yield return group;
            }
        }
    }
}

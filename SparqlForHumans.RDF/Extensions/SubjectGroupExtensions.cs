using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.RDF.Extensions
{
    public static class SubjectGroupExtensions
    {
        public static IEnumerable<SubjectGroup> GroupBySubject(this IEnumerable<string> lines)
        {
            var subjectGroup = new SubjectGroup();
            var list = new List<string>();
            var last = string.Empty;

            foreach (var line in lines)
            {
                var entity = line.Split(' ').FirstOrDefault();

                //Base case: first value:
                if (last == string.Empty)
                {
                    list = new List<string>();
                    subjectGroup = new SubjectGroup(entity, list);
                    last = entity;
                }

                //Switch/Different of entity:
                // - Return list,
                // - Create new list,
                // - Assign last to current
                else if (last != entity)
                {
                    yield return subjectGroup;
                    list = new List<string>();
                    subjectGroup = new SubjectGroup(entity, list);
                    last = entity;
                }

                // Same entity
                list.Add(line);
            }

            yield return subjectGroup;
        }

        public static bool IsEntityQ(this SubjectGroup subjectGroup)
        {
            return subjectGroup.Id.StartsWith("Q");
        }

        public static bool IsEntityP(this SubjectGroup subjectGroup)
        {
            return subjectGroup.Id.StartsWith("P");
        }
    }
}
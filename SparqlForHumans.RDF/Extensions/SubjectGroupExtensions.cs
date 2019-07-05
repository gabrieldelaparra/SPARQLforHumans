using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

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
                var entity = line.Split(WikidataDump.BlankSpaceChar).FirstOrDefault();

                //Base case: first value:
                if (last == string.Empty)
                {
                    list = new List<string>();
                    subjectGroup = new SubjectGroup(entity, list);
                    last = entity;
                }

                //Switch/Different of entity:
                // - Return existing list,
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

        // TODO: Test
        public static bool IsEntityQ(this SubjectGroup subjectGroup)
        {
            return subjectGroup.Id.StartsWith(WikidataDump.EntityPrefix);
        }

        // TODO: Test
        public static bool IsEntityP(this SubjectGroup subjectGroup)
        {
            return subjectGroup.Id.StartsWith(WikidataDump.PropertyPrefix);
        }

        /// <summary>
        ///     Allow only triples that have property predicates.
        /// </summary>
        /// <param name="entityGroupTriples"></param>
        /// <returns></returns>
        public static IEnumerable<Triple> FilterPropertyPredicatesOnly(this SubjectGroup entityGroupTriples)
        {
            return entityGroupTriples.Where(x => x.Predicate.IsProperty());
        }
    }
}
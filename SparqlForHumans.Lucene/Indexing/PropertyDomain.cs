using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertyDomain
    {
        /// <summary>
        /// Given the following data:
        ///
        /// ```
        /// ...
        /// Q76 -> P31 (Type) -> Q5
        /// Q76 -> P27 -> Qxx
        /// Q76 -> P555 -> Qxx
        /// ...
        /// Q298 -> P31 -> Q17
        /// Q298 -> P555 -> Qxx
        /// Q298 -> P777 -> Qxx
        /// ...
        /// ```
        ///
        /// Returns the following domain:
        ///
        /// P27: Domain Q5
        /// P555: Domain Q5, Q17
        /// P777: Domain Q17
        ///
        /// Translated to the following KeyValue Pairs:
        /// Key: 27; Values[]: 5
        /// Key: 555; Values[]: 5, 17
        /// Key: 777; Values[]: 17
        /// </summary>
        public static Dictionary<int, int[]> GetPropertyDomainTypes(this IEnumerable<SubjectGroup> subjectGroups)
        {
            var dictionary = new Dictionary<int, List<int>>();
            foreach (var subjectGroup in subjectGroups)
            {
                //Talvez debería venir ya filtrado
                var propertiesTriples = subjectGroup.FilterPropertiesOnly();

                var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

                // InstanceOf Ids (Domain Types) and Properties
                var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).ToArray();
                var domainIds = instanceOfSlice.Select(x => x.Object.GetIntId()).ToArray();

                foreach (var propertyId in propertyIds)
                {
                    dictionary.AddSafe(propertyId, domainIds);
                }
            }

            return dictionary.ToArrayDictionary();
        }

        /// <summary>
        /// Allow only triples that have property predicates.
        /// </summary>
        /// <param name="entityGroupTriples"></param>
        /// <returns></returns>
        private static IEnumerable<Triple> FilterPropertiesOnly(this IEnumerable<Triple> entityGroupTriples)
        {
            return entityGroupTriples.Where(x => x.Predicate.IsProperty());
        }
    }
}

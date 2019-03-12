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

        private static IEnumerable<Triple> FilterPropertiesOnly(this IEnumerable<Triple> entityGroupTriples)
        {
            return entityGroupTriples.Where(x => x.Predicate.IsProperty());
        }
    }
}

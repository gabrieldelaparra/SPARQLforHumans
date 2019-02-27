using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertyDomain
    {
        // También agregar la opción que el input ya sea un group de Triples.
        public static (int[] InstanceOfIds, int[] PropertyDomainIds) GetPropertyDomainTypes(this IList<Triple> entityGroupTriples)
        {
            var propertiesTriples = entityGroupTriples.Where(x => x.Predicate.IsProperty()).ToList();
            //TODO: Refactor. For. Performance.
            var instanceOfTriples = propertiesTriples.Where(x => x.Predicate.IsInstanceOf()).ToList();
            var otherPropertiesTriples = propertiesTriples.Where(x => !x.Predicate.IsInstanceOf()).ToList();
            
            // InstanceOf Ids (Domain Types) and Properties
            var instanceOfIds = instanceOfTriples.Select(x => x.Object.GetIntId()).ToArray();
            var domainIds = otherPropertiesTriples.Select(x => x.Object.GetIntId()).ToArray();

            // Done
            return (instanceOfIds, domainIds);
        }
    }
}

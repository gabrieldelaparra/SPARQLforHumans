using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertyDomain
    {
        // Parece que esto está mal.
        // Para los domains, yo debería entregar los tipos
        // También agregar la opción que el input ya sea un group de Triples.
        public static (int[] PropertyIds, int[] DomainIds) GetPropertyDomainTypes(this IEnumerable<Triple> groupedTriples)
        {
            //Talvez debería venir ya filtrado
            var propertiesTriples = groupedTriples.FilterPropertiesOnly();

            var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

            // InstanceOf Ids (Domain Types) and Properties
            var domainIds = instanceOfSlice.Select(x => x.Object.GetIntId()).ToArray();
            var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).ToArray();

            // Done
            return (propertyIds, domainIds);
        }

        public static (int[] InstanceOfIds, int[] PropertyDomainIds) GetPropertyDomainTypes(
            this IEnumerable<string> groupedLines)
        {
            return groupedLines.Select(x => x.ToTriple()).GetPropertyDomainTypes();
        }

        private static IEnumerable<Triple> FilterPropertiesOnly(this IEnumerable<Triple> entityGroupTriples)
        {
            return entityGroupTriples.Where(x => x.Predicate.IsProperty());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class EntityTypeBuilder
    {
        // No muy eficiente.
        // Lee el grupo de nuevo.
        // Al final, lo leeré N-veces por cada cosa que quiera sacar.
        // Pero es más legible.
        // Pudo ser KeyValuePair, pero Tuple es más liviano.
        public static (int EntityId, int[] TypeIds) GetEntityTypes(this IEnumerable<string> entityGroupedLines)
        {
            //Prevent multiple enumeration.
            var entityGroupList = entityGroupedLines.ToList();

            //Id
            var entityId = entityGroupList.FirstOrDefault().GetTriple().Subject.GetIntId();

            //Types
            var triples = entityGroupList.Select(x => x.GetTripleAsTuple());
            var entityTypes = triples.Where(x => x.predicate.IsInstanceOf())
                    .Select(x => x.ntObject.GetIntId()).ToArray();

            //Done
            return (entityId, entityTypes);
        }
    }
}

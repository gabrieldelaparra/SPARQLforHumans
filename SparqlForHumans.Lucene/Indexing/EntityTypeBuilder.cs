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

        //TODO: TEST
        //TODO: No estoy seguro si esto sea necesario, pero maybe.
        public static int[][] ToJaggedArray(this (int entityId, int[] typeIds)[] entityTypesTuples)
        {
            var length = entityTypesTuples.Length;
            var jagged = new int[length][];
            for (var i = 0; i < length; i++)
                jagged[i] = entityTypesTuples[i].typeIds;

            return jagged;
        }

        public static string ToEntityTypesString(this (int entityId, int[] typeIds) entityTypesTuple)
        {
            return entityTypesTuple.typeIds.Length > 0 
                ? $"{entityTypesTuple.entityId} {string.Join(" ", entityTypesTuple.typeIds)}" 
                : entityTypesTuple.entityId.ToString();
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class EntityTypes
    {
        // No muy eficiente.
        // Lee el grupo de nuevo.
        // Al final, lo leeré N-veces por cada cosa que quiera sacar.
        // Pero es más legible.
        // Pudo ser KeyValuePair, pero Tuple es más liviano.
        // El Tuple<int, int> no es tán pesado: https://stackoverflow.com/questions/4676249/tupleint-int-versus-int2-memory-usage
        public static (int EntityId, int[] TypeIds) GetEntityTypes(this SubjectGroup entityGroupedLines)
        {
            // Overload
            return entityGroupedLines.GetEntityTypes(entityGroupedLines.IntId); 
        }

        // También agregar la opción que el input ya sea un group de Triples.
        private static (int EntityId, int[] TypeIds) GetEntityTypes(this IEnumerable<Triple> entityGroupTriples, int entityId)
        {
            // Types
            var entityTypes = entityGroupTriples
                .Where(x => x.Predicate.IsInstanceOf())
                .Select(x => x.Object.GetIntId()).ToArray();

            // Done
            return (entityId, entityTypes);
        }

        public static Dictionary<int, int[]> GetEntityTypes(this IEnumerable<SubjectGroup> entityGroups)
        {
            var dictionary = new Dictionary<int, List<int>>();
            foreach (var entityGroup in entityGroups)
            {
                var tuple = entityGroup.GetEntityTypes();
                //if(tuple.TypeIds.Length > 0) // This is inside the AddSafe
                dictionary.AddSafe(tuple.EntityId, tuple.TypeIds);
            }

            return dictionary.ToArrayDictionary();
        }

        // Definitively not the best practice. I'll need to define if int[] or List<int>.
        // The idea of converting it to int[] instead of keeping it as a List<int>
        // Is to "recover/reduce/optimize" the extra space allocate by the List internal array.
        public static Dictionary<int, int[]> ToDictionary(this IEnumerable<(int entityId, int[] typeIds)> entityTypesTuples)
        {
            var dictionary = new Dictionary<int, List<int>>();
            foreach (var tuple in entityTypesTuples)
                dictionary.AddSafe(tuple.entityId, tuple.typeIds);

            // Magic Cast. I will regret all my life.
            return dictionary.ToArrayDictionary();
        }

        public static string ToEntityTypesString(this (int entityId, int[] typeIds) entityTypesTuple)
        {
            return entityTypesTuple.typeIds.Length > 0
                ? $"{entityTypesTuple.entityId} {string.Join(" ", entityTypesTuple.typeIds)}"
                : entityTypesTuple.entityId.ToString();
        }


    }
}

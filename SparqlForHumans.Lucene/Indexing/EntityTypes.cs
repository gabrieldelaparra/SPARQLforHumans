using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
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
        public static (int EntityId, int[] TypeIds) GetEntityTypes(this IEnumerable<string> entityGroupedLines)
        {
            // Prevent multiple enumeration.
            var entityGroupTriples = entityGroupedLines.Select(x => x.GetTriple()).ToList();

            // Overload
            return GetEntityTypes(entityGroupTriples);
        }

        // También agregar la opción que el input ya sea un group de Triples.
        public static (int EntityId, int[] TypeIds) GetEntityTypes(this IList<Triple> entityGroupTriples)
        {
            // Id
            // ReSharper disable once PossibleNullReferenceException
            // Si no hay un primero, está bien que tire una exception.
            var entityId = entityGroupTriples.FirstOrDefault().Subject.GetIntId();

            // Types
            var entityTypes = entityGroupTriples.Where(x => x.Predicate.IsInstanceOf())
                .Select(x => x.Object.GetIntId()).ToArray();

            // Done
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

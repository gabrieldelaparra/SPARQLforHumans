using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class EntityTypes
    {
        public static Dictionary<int, int[]> GetTypeEntities(this IEnumerable<SubjectGroup> entityGroups)
        {
            var dictionary = new Dictionary<int, List<int>>();
            foreach (var entityGroup in entityGroups)
            {
                var entityTypes = entityGroup
                    .Where(x => x.Predicate.IsInstanceOf())
                    .Select(x => x.Object.GetIntId()).ToArray();

                foreach (var entityType in entityTypes) dictionary.AddSafe(entityType, entityGroup.IntId);
            }

            return dictionary.ToArrayDictionary();
        }

        public static Dictionary<int, int[]> GetEntityTypes(this IEnumerable<SubjectGroup> entityGroups)
        {
            var dictionary = new Dictionary<int, List<int>>();
            foreach (var entityGroup in entityGroups)
            {
                // Types
                var entityTypes = entityGroup
                    .Where(x => x.Predicate.IsInstanceOf())
                    .Select(x => x.Object.GetIntId()).ToArray();

                dictionary.AddSafe(entityGroup.IntId, entityTypes);
            }

            return dictionary.ToArrayDictionary();
        }

        // Definitively not the best practice. I'll need to define if int[] or List<int>.
        // The idea of converting it to int[] instead of keeping it as a List<int>
        // Is to "recover/reduce/optimize" the extra space allocate by the List internal array.
        public static Dictionary<int, int[]> ToDictionary(
            this IEnumerable<(int entityId, int[] typeIds)> entityTypesTuples)
        {
            var dictionary = new Dictionary<int, List<int>>();
            foreach (var tuple in entityTypesTuples)
                dictionary.AddSafe(tuple.entityId, tuple.typeIds);

            // Magic Cast. I will regret all my life.
            return dictionary.ToArrayDictionary();
        }
    }
}
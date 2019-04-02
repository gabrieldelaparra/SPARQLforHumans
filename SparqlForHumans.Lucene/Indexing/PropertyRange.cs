using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertyRange
    {
        // ! Checkear que Property no sea InstanceOf
        // ! Checkear que Object sea Q-Entity
        public static (int PropertyId, int[] RangeTypeIds) GetPropertyRangeType(this Triple propertyTriple,
            Dictionary<int, int[]> entityWithTypes)
        {
            //Property Id (Key), Object Id (Entity to get its Entity Types)
            var propertyId = propertyTriple.Predicate.GetIntId();
            var objectId = propertyTriple.Object.GetIntId();

            //Entity Types
            var entityTypes = entityWithTypes[objectId];

            //Done
            return (propertyId, entityTypes);
        }

        public static bool IsValidPropertyRangeTriple(this Triple triple)
        {
            return triple.Predicate.IsProperty()
                   && !triple.Predicate.IsInstanceOf()
                   && triple.Object.IsEntityQ();
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
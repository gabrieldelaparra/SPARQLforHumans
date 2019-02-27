using SparqlForHumans.RDF.Extensions;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertyRange
    {
        // TODO: Check if ref hace que sea menos memoria.
        // No estoy seguro si con los array se pasan en ref también.
        // Pero mejor irse a la segura con el ref.
        // ! Checkear que Property no sea InstanceOf
        // ! Checkear que Object sea Q-Entity
        public static (int PropertyId, int[] RangeTypeIds) GetPropertyRangeType(this Triple propertyTriple, ref int[][] entityWithTypesArray)
        {
            //Property Id (Key), Object Id (Entity to get its Entity Types)
            var propertyId = propertyTriple.Predicate.GetIntId();
            var objectId = propertyTriple.Object.GetIntId();

            //Entity Types
            var entityTypes = entityWithTypesArray[objectId];

            //Done
            return (propertyId, entityTypes);
        }

        // Overload
        public static (int PropertyId, int[] RangeTypeIds) GetPropertyRangeType(this string propertyLine,
            ref int[][] entityWithTypesArray)
        {
            var triple = propertyLine.GetTriple();
            return GetPropertyRangeType(triple, ref entityWithTypesArray);
        }
    }
}

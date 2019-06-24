using System.Collections.Generic;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing
{
    ///// <summary>
    /////     Este test crea un indice y agrega el Range (Destino) de las propiedades.
    /////     Se dan los siguientes ejemplios:
    /////     ```
    /////     Q76 (Obama) -> P31 (Type) -> Q5 (Human)
    /////     Q76 (Obama) -> P69 (EducatedAt) -> Q49088 (Columbia)
    /////     Q76 (Obama) -> P69 (EducatedAt) -> Q49122 (Harvard)
    /////     Q76 (Obama) -> P555 -> Qxx
    /////     ...
    /////     Q49088 (Columbia) -> P31 (Type) -> Q902104 (Private)
    /////     Q49088 (Columbia) -> P31 (Type) -> Q15936437 (Research)
    /////     Q49088 (Columbia) -> P31 (Type) -> Q1188663 (Colonial)
    /////     Q49088 (Columbia) -> P31 (Type) -> Q23002054 (NonProfit)
    /////     ...
    /////     Q49122 (Harvard) -> P31 (Type) -> Q13220391 (Graduate)
    /////     Q49122 (Harvard) -> P31 (Type) -> Q1321960 (Law)
    /////     ...
    /////     Q298 (Chile) -> P31 (Type) -> Q17 (Country)
    /////     Q298 (Chile) -> P38 (Currency) -> Q200050 (Peso)
    /////     Q298 (Chile) -> P38 (Currency) -> Q1573250 (UF)
    /////     Q298 (Chile) -> P777 -> Qxx
    /////     ...
    /////     Q200050 (Peso) -> P31 (Type) -> Q1643989 (Legal Tender)
    /////     Q200050 (Peso) -> P31 (Type) -> Q8142 (Currency)
    /////     ...
    /////     Q1573250 (UF) -> P31 (Type) -> Q747699 (UnitOfAccount)
    /////     ...
    /////     Otros
    /////     ```
    /////     El Range que se calcula, debe mostrar que:
    /////     ```
    /////     P69: Range (4+2) Q902104, Q15936437, Q1188663, Q23002054, Q13220391, Q1321960
    /////     P38: Range (2+1) Q1643989, Q8142, Q747699
    /////     ```
    ///// </summary>
    //public static class PropertyRange
    //{
    //    public static Dictionary<int, int[]> PostProcessDictionary(
    //        Dictionary<int, int[]> entityToTypesRelationDictionary,
    //        Dictionary<int, int[]> propertyToEntitiesDictionary)
    //    {
    //        var dictionary = new Dictionary<int, List<int>>();
    //        // Currently the `propertyToEntitiesDictionary` is created with `<PropertyId, ObjectIds[]>`
    //        // We need to replace the `ObjectTypeIds[]` of those `ObjectIds[]`
    //        foreach (var pair in propertyToEntitiesDictionary)
    //        foreach (var objectId in pair.Value)
    //        {
    //            if (!entityToTypesRelationDictionary.ContainsKey(objectId))
    //                continue;

    //            var objectTypes = entityToTypesRelationDictionary[objectId];
    //            dictionary.AddSafe(pair.Key, objectTypes);
    //        }

    //        return dictionary.ToArrayDictionary();
    //    }
    //}
}
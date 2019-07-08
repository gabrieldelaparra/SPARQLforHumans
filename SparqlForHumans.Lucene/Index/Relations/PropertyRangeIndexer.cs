using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index.Relations
{
    /// <summary>
    ///     Este test crea un indice y agrega el Range (Destino) de las propiedades.
    ///     Se dan los siguientes ejemplios:
    ///     ```
    ///     Q76 (Obama) -> P31 (Type) -> Q5 (Human)
    ///     Q76 (Obama) -> P69 (EducatedAt) -> Q49088 (Columbia)
    ///     Q76 (Obama) -> P69 (EducatedAt) -> Q49122 (Harvard)
    ///     Q76 (Obama) -> P555 -> Qxx
    ///     ...
    ///     Q49088 (Columbia) -> P31 (Type) -> Q902104 (Private)
    ///     Q49088 (Columbia) -> P31 (Type) -> Q15936437 (Research)
    ///     Q49088 (Columbia) -> P31 (Type) -> Q1188663 (Colonial)
    ///     Q49088 (Columbia) -> P31 (Type) -> Q23002054 (NonProfit)
    ///     ...
    ///     Q49122 (Harvard) -> P31 (Type) -> Q13220391 (Graduate)
    ///     Q49122 (Harvard) -> P31 (Type) -> Q1321960 (Law)
    ///     ...
    ///     Q298 (Chile) -> P31 (Type) -> Q17 (Country)
    ///     Q298 (Chile) -> P38 (Currency) -> Q200050 (Peso)
    ///     Q298 (Chile) -> P38 (Currency) -> Q1573250 (UF)
    ///     Q298 (Chile) -> P777 -> Qxx
    ///     ...
    ///     Q200050 (Peso) -> P31 (Type) -> Q1643989 (Legal Tender)
    ///     Q200050 (Peso) -> P31 (Type) -> Q8142 (Currency)
    ///     ...
    ///     Q1573250 (UF) -> P31 (Type) -> Q747699 (UnitOfAccount)
    ///     ...
    ///     Otros
    ///     ```
    ///     El Range que se calcula, debe mostrar que:
    ///     ```
    ///     P69: Range (4+2) Q902104, Q15936437, Q1188663, Q23002054, Q13220391, Q1321960
    ///     P38: Range (2+1) Q1643989, Q8142, Q747699
    ///     ```
    /// </summary>
    public class PropertyRangeIndexer : BaseOneToManyRelationMapper<int, int>, IFieldIndexer<StringField>
    {
        public PropertyRangeIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public PropertyRangeIndexer(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups)
        {
        }

        public override string NotifyMessage => "Building <Property, RangeIds[]> Dictionary";

        public override Dictionary<int, int[]> BuildIndex(IEnumerable<SubjectGroup> subjectGroups)
        {
            var propertyObjectIdsDictionary = new Dictionary<int, List<int>>();
            var subjectIdTypeIdsDictionary = new Dictionary<int, List<int>>();
            var propertyRangeDictionary = new Dictionary<int, List<int>>();

            foreach (var subjectGroup in subjectGroups)
            {
                if (!subjectGroup.IsEntityQ()) continue;

                var propertiesTriples = subjectGroup.FilterPropertyPredicatesOnly();
                var (instanceOfSlice, otherPropertiesSlice) =
                    propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

                foreach (var triple in otherPropertiesSlice)
                    propertyObjectIdsDictionary.AddSafe(triple.Predicate.GetIntId(), triple.Object.GetIntId());

                foreach (var triple in instanceOfSlice)
                    subjectIdTypeIdsDictionary.AddSafe(subjectGroup.IntId, triple.Object.GetIntId());
            }

            foreach (var pair in propertyObjectIdsDictionary)
            foreach (var objectId in pair.Value)
            {
                if (!subjectIdTypeIdsDictionary.ContainsKey(objectId)) continue;

                var objectTypes = subjectIdTypeIdsDictionary[objectId];
                propertyRangeDictionary.AddSafe(pair.Key, objectTypes);
            }

            return propertyRangeDictionary.ToArrayDictionary();
        }

        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            throw new NotImplementedException();
        }

        public string FieldName => Labels.Range.ToString();
        public double Boost { get; set; }

        public IReadOnlyList<StringField> GetField(SubjectGroup tripleGroup)
        {
            return RelationIndex.ContainsKey(tripleGroup.Id.ToNumbers())
                ? RelationIndex[tripleGroup.Id.ToNumbers()]
                    .Select(x => new StringField(FieldName, x.ToString(), Field.Store.YES)).ToList()
                : new List<StringField>();
        }
    }
}
//using Lucene.Net.Documents;
//using SparqlForHumans.Lucene.Index.Base;
//using SparqlForHumans.Models.LuceneIndex;
//using SparqlForHumans.RDF.Extensions;
//using SparqlForHumans.RDF.Models;
//using SparqlForHumans.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace SparqlForHumans.Lucene.Index.Relations
//{
//    /// <summary>
//    ///     Este test crea un indice y agrega el Range (Destino) de las propiedades.
//    ///     Se dan los siguientes ejemplios:
//    ///     ```
//    ///     Q76 (Obama) -> P31 (Type) -> Q5 (Human)
//    ///     Q76 (Obama) -> P69 (EducatedAt) -> Q49088 (Columbia)
//    ///     Q76 (Obama) -> P69 (EducatedAt) -> Q49122 (Harvard)
//    ///     Q76 (Obama) -> P555 -> Qxx
//    ///     ...
//    ///     Q49088 (Columbia) -> P31 (Type) -> Q902104 (Private)
//    ///     Q49088 (Columbia) -> P31 (Type) -> Q15936437 (Research)
//    ///     Q49088 (Columbia) -> P31 (Type) -> Q1188663 (Colonial)
//    ///     Q49088 (Columbia) -> P31 (Type) -> Q23002054 (NonProfit)
//    ///     ...
//    ///     Q49122 (Harvard) -> P31 (Type) -> Q13220391 (Graduate)
//    ///     Q49122 (Harvard) -> P31 (Type) -> Q1321960 (Law)
//    ///     ...
//    ///     Q298 (Chile) -> P31 (Type) -> Q17 (Country)
//    ///     Q298 (Chile) -> P38 (Currency) -> Q200050 (Peso)
//    ///     Q298 (Chile) -> P38 (Currency) -> Q1573250 (UF)
//    ///     Q298 (Chile) -> P777 -> Qxx
//    ///     ...
//    ///     Q200050 (Peso) -> P31 (Type) -> Q1643989 (Legal Tender)
//    ///     Q200050 (Peso) -> P31 (Type) -> Q8142 (Currency)
//    ///     ...
//    ///     Q1573250 (UF) -> P31 (Type) -> Q747699 (UnitOfAccount)
//    ///     ...
//    ///     Otros
//    ///     ```
//    ///     El Range que se calcula, debe mostrar que:
//    ///     ```
//    ///     P69: Range (4+2) Q902104, Q15936437, Q1188663, Q23002054, Q13220391, Q1321960
//    ///     P38: Range (2+1) Q1643989, Q8142, Q747699
//    ///     ```
//    /// </summary>
//    public class PropertyRangeIndexer : BaseOneToManyRelationMapper<int, int>, IFieldIndexer<StringField>
//    {
//        public PropertyRangeIndexer(string inputFilename) : base(inputFilename)
//        {
//        }

//        public PropertyRangeIndexer(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups)
//        {
//        }

//        public override string NotifyMessage => "Building <Property, RangeIds[]> Dictionary";

//        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
//        {
//            // Filter those the triples that are properties only (Exclude description, label, etc.)
//            var propertiesTriples = subjectGroup.Where(x => x.Predicate.IsReverseProperty() || x.Predicate.IsInstanceOf());

//            var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

//            // InstanceOf Ids (Domain Types) and Properties
//            var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).Distinct().ToArray();
//            var instanceOfIds = instanceOfSlice.Select(x => x.Object.GetIntId()).Distinct().ToArray();

//            foreach (var instanceOfId in instanceOfIds)
//            {
//                dictionary.AddSafe(31, instanceOfId);
//            }

//            foreach (var propertyId in propertyIds)
//            {
//                dictionary.AddSafe(propertyId, instanceOfIds);
//            }
//        }

//        public string FieldName => Labels.Range.ToString();
//        public double Boost { get; set; }

//        public IEnumerable<StringField> GetField(SubjectGroup tripleGroup)
//        {
//            return RelationIndex.ContainsKey(tripleGroup.Id.ToNumbers())
//                ? RelationIndex[tripleGroup.Id.ToNumbers()]
//                    .Select(x => new StringField(FieldName, x.ToString(), Field.Store.YES))
//                : new List<StringField>();
//        }
//    }
//}
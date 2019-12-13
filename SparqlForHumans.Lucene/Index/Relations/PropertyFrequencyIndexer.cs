//using Lucene.Net.Documents;
//using SparqlForHumans.Lucene.Index.Base;
//using SparqlForHumans.Models.LuceneIndex;
//using SparqlForHumans.RDF.Extensions;
//using SparqlForHumans.RDF.Models;
//using SparqlForHumans.Utilities;
//using System.Collections.Generic;

//namespace SparqlForHumans.Lucene.Index.Relations
//{
//    /// <summary>
//    ///     Given the following data:
//    ///     ```
//    ///     ...
//    ///     Qxx -> P45 -> Qxx
//    ///     Qxx -> P56 -> Qxx
//    ///     ...
//    ///     Qxx -> P34 -> Qxx
//    ///     Qxx -> P56 -> Qxx
//    ///     ...
//    ///     ```
//    ///     Returns the following:
//    ///     P45: 1
//    ///     P56: 2
//    ///     P34: 1
//    ///     Translated to the following KeyValue Pairs:
//    ///     Key: 45; Value: 1
//    ///     Key: 56; Value: 2
//    ///     Key: 34; Value: 1
//    /// </summary>
//    public class PropertyFrequencyIndexer : BaseOneToOneRelationMapper<int, int>, IFieldIndexer<DoubleField>
//    {
//        public PropertyFrequencyIndexer(string inputFilename) : base(inputFilename)
//        {
//        }

//        public double Boost { get; set; }

//        public string FieldName => Labels.Rank.ToString();

//        public IEnumerable<DoubleField> GetField(SubjectGroup subjectGroup)
//        {
//            var subjectId = subjectGroup.Id.ToNumbers();
//            return RelationIndex.ContainsKey(subjectId)
//                ? new List<DoubleField> { new DoubleField(FieldName, RelationIndex[subjectId], Field.Store.YES) }
//                : new List<DoubleField>();
//        }

//        public override string NotifyMessage { get; } = "Building <PropertyId, Count> Dictionary";

//        internal override void ParseTripleGroup(Dictionary<int, int> dictionary, SubjectGroup subjectGroup)
//        {
//            foreach (var triple in subjectGroup)
//            {
//                // Filter Properties Only
//                if (!triple.Predicate.IsProperty()) continue;

//                var predicateIntId = triple.Predicate.GetIntId();

//                if (!dictionary.ContainsKey(predicateIntId))
//                    dictionary.Add(predicateIntId, 0);

//                dictionary[predicateIntId]++;
//            }
//        }
//    }
//}
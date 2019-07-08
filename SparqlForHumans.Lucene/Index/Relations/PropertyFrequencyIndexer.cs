using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class PropertyFrequencyIndexer : BaseOneToOneRelationMapper<int, int>, IFieldIndexer<DoubleField>
    {
        public PropertyFrequencyIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public double Boost { get; set; }

        public string FieldName => Labels.Rank.ToString();

        public IReadOnlyList<DoubleField> GetField(SubjectGroup subjectGroup)
        {
            var subjectId = subjectGroup.Id.ToNumbers();
            return RelationIndex.ContainsKey(subjectId)
                ? new List<DoubleField> {new DoubleField(FieldName, RelationIndex[subjectId], Field.Store.YES)}
                : new List<DoubleField>();
        }

        public override string NotifyMessage { get; } = "Building <PropertyId, Count> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, int> dictionary, SubjectGroup subjectGroup)
        {
            foreach (var triple in subjectGroup)
            {
                // Filter Properties Only
                if (!triple.Predicate.IsProperty()) continue;

                var predicateIntId = triple.Predicate.GetIntId();

                if (!dictionary.ContainsKey(predicateIntId)) dictionary.Add(predicateIntId, 0);

                dictionary[predicateIntId]++;
            }
        }
    }
}
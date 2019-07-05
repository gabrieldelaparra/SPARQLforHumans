using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Indexing.Relations
{
    public class EntityPageRankBoostIndexer : EntityPageRankBoostMapper, IFieldIndexer<DoubleField>
    {
        public EntityPageRankBoostIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public double Boost { get; set; }

        public string FieldName => Labels.Rank.ToString();

        public IReadOnlyList<DoubleField> GetField(SubjectGroup subjectGroup)
        {
            var subjectId = subjectGroup.Id.ToNumbers();
            return RelationIndex.ContainsKey(subjectId)
                    ? new List<DoubleField> { new DoubleField(FieldName, RelationIndex[subjectId], Field.Store.YES) }
                    : new List<DoubleField>();
        }
    }
}
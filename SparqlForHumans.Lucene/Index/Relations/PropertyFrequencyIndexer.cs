using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class PropertyFrequencyIndexer : PropertyFrequencyMapper, IFieldIndexer<DoubleField>
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
    }
}
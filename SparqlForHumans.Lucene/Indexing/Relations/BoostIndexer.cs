using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Mappings;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.Relations
{
    public class BoostIndexer : BoostMapper, IFieldIndexer<StringField>
    {
        public BoostIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public string FieldName => Labels.Rank.ToString();

        public StringField GetField(SubjectGroup subjectGroup)
        {
            return RelationIndex.ContainsKey(subjectGroup.Id.ToNumbers())
                ? new StringField(FieldName, RelationIndex[subjectGroup.Id.ToNumbers()].ToString(), Field.Store.YES)
                : null;
        }
    }
}
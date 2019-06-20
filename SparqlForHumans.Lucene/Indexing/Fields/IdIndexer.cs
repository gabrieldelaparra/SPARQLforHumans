using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.Fields
{
    public class IdIndexer : IFieldIndexer<StringField>
    {
        public string FieldName => Labels.Id.ToString();

        public StringField GetField(SubjectGroup tripleGroup)
        {
            var value = tripleGroup.Id;
            return !string.IsNullOrWhiteSpace(value)
                ? new StringField(FieldName, value, Field.Store.YES)
                : null;
        }
    }
}
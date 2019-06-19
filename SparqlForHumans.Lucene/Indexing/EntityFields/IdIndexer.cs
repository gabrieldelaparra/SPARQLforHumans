using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.EntityFields
{
    public class IdIndexer: ISubjectGroupIndexer<StringField>
    {
        public string FieldName => Labels.Id.ToString();

        public StringField TriplesToField(SubjectGroup tripleGroup)
        {
            return new StringField(FieldName, tripleGroup.Id, Field.Store.YES);
        }
    }
}

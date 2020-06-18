using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Index.Fields
{
    public class IdIndexer : IFieldIndexer<StringField>
    {
        public string FieldName => Labels.Id.ToString();
        public double Boost { get; set; }

        public IEnumerable<StringField> GetField(SubjectGroup tripleGroup)
        {
            var value = tripleGroup.Id;
            return !string.IsNullOrWhiteSpace(value)
                ? new List<StringField> {new StringField(FieldName, value, Field.Store.YES)}
                : new List<StringField>();
        }
    }
}
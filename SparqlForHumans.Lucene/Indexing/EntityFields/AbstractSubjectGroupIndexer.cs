using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.BaseFields
{
    public abstract class AbstractSubjectGroupIndexer<T1> : ISubjectGroupIndexer<T1> where T1 : Field
    {
        public bool HasValue { get; protected set; }
        public abstract string FieldName { get; }
        public abstract T1 TriplesToField(SubjectGroup tripleGroup);

        public string TriplesToValue(SubjectGroup triples)
        {
            var values = string.Join(WikidataDump.PropertyValueSeparator,
                triples.Where(FilterValidTriples)
                    .Distinct()
                    .Select(SelectTripleValue));

            HasValue = !string.IsNullOrEmpty(values);
            return values;
        }

        public abstract bool FilterValidTriples(Triple triple);
        public abstract string SelectTripleValue(Triple triple);
    }
}
using System.Linq;
using Lucene.Net.Index;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.Base
{
    public abstract class BaseFieldIndexer<TField> : IFieldIndexer<TField>
        where TField : IIndexableField
    {
        public abstract string FieldName { get; }
        public abstract TField GetField(SubjectGroup tripleGroup);

        public abstract bool FilterValidTriples(Triple triple);
        public abstract string SelectTripleValue(Triple triple);

        public string TriplesToValue(SubjectGroup triples)
        {
            var values = string.Join(WikidataDump.PropertyValueSeparator,
                triples.Where(FilterValidTriples)
                    .Distinct()
                    .Select(SelectTripleValue)
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

            return values;
        }
    }
}
using Lucene.Net.Index;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.Base
{
    public abstract class BaseFieldIndexer<TField> : IFieldIndexer<TField>
        where TField : IIndexableField
    {
        public abstract string FieldName { get; }
        public double Boost { get; set; }
        public abstract IReadOnlyList<TField> GetField(SubjectGroup tripleGroup);

        public abstract bool FilterValidTriples(Triple triple);
        public abstract string SelectTripleValue(Triple triple);

        public IEnumerable<string> TriplesToValue(SubjectGroup triples)
        {
            var values = triples.Where(FilterValidTriples)
                    .Select(SelectTripleValue)
                    .Distinct()
                    .Where(x => !string.IsNullOrWhiteSpace(x));

            return values;
        }
    }
}
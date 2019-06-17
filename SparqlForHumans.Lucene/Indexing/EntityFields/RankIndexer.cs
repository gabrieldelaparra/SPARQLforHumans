using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Models.LuceneIndex;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.EntityFields
{
    public class RankIndexer : IFieldIndexer<DoubleField>
    {
        public double Boost { get; set; }
        public string FieldName => Labels.Rank.ToString();

        public DoubleField TriplesToField(IEnumerable<Triple> tripleGroup)
        {
            return new DoubleField(FieldName, Boost, Field.Store.YES);
        }
    }
}
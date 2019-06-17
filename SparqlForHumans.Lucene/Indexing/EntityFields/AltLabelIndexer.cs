using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.EntityFields
{
    public class AltLabelIndexer : AbstractFieldIndexer<TextField>
    {
        public override string FieldName => Labels.AltLabel.ToString();
        public double Boost { get; set; }

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.AltLabel);
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple.Object.GetLiteralValue();
        }

        public override TextField TriplesToField(IEnumerable<Triple> tripleGroup)
        {
            return new TextField(FieldName, TriplesToValue(tripleGroup), Field.Store.YES) {Boost = (float) Boost};
        }
    }
}
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.EntityFields
{
    public class AltLabelIndexer : AbstractSubjectGroupIndexer<TextField>
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

        public override TextField TriplesToField(SubjectGroup tripleGroup)
        {
            return new TextField(FieldName, TriplesToValue(tripleGroup), Field.Store.YES) {Boost = (float) Boost};
        }
    }
}
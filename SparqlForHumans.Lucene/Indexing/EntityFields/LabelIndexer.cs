using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.EntityFields
{
    public class LabelIndexer : AbstractSubjectGroupIndexer<TextField>
    {
        public override string FieldName => Labels.Label.ToString();

        public double Boost { get; set; }

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Label);
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
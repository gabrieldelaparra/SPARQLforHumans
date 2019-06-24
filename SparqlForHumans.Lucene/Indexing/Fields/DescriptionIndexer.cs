using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.Fields
{
    public class DescriptionIndexer : BaseFieldIndexer<TextField>
    {
        public override string FieldName => Labels.Description.ToString();

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Description);
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple.Object.GetLiteralValue();
        }

        public override TextField GetField(SubjectGroup tripleGroup)
        {
            var value = TriplesToValue(tripleGroup);
            return !string.IsNullOrWhiteSpace(value)
                ? new TextField(FieldName, value, Field.Store.YES)
                : null;
        }
    }
}
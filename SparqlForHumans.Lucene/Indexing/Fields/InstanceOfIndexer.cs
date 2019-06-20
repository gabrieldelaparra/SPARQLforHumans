using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.Fields
{
    public class InstanceOfIndexer : BaseFieldIndexer<StringField>
    {
        public override string FieldName => Labels.InstanceOf.ToString();

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Property)
                   && RDFExtensions.GetPropertyType(triple.Predicate, triple.Object)
                       .Equals(RDFExtensions.PropertyType.InstanceOf);
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple.Object.GetId();
        }

        public override StringField GetField(SubjectGroup tripleGroup)
        {
            var value = TriplesToValue(tripleGroup);
            return !string.IsNullOrWhiteSpace(value)
                ? new StringField(FieldName, value, Field.Store.YES)
                : null;
        }
    }
}
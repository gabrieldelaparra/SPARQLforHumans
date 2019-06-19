using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.EntityFields
{
    public class SubClassIndexer : AbstractSubjectGroupIndexer<StringField>
    {
        public override string FieldName => Labels.SubClass.ToString();

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Property)
                   && RDFExtensions.GetPropertyType(triple.Predicate, triple.Object)
                       .Equals(RDFExtensions.PropertyType.SubClass);
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple.Object.GetId();
        }

        public override StringField TriplesToField(SubjectGroup tripleGroup)
        {
            return new StringField(FieldName, TriplesToValue(tripleGroup), Field.Store.YES);
        }
    }
}
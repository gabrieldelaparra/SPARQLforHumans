using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Index.Fields
{
    public class ReverseIsTypeIndexer : BaseFieldIndexer<StringField>
    {
        public override string FieldName => Labels.IsTypeEntity.ToString();

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.IsReverseInstanceOf();
        }

        public override IEnumerable<StringField> GetField(SubjectGroup tripleGroup)
        {
            var values = SelectTripleValue(tripleGroup.FirstOrDefault(FilterValidTriples));
            return !string.IsNullOrWhiteSpace(values)
                ? new[] {new StringField(FieldName, true.ToString(), Field.Store.YES)}
                : new StringField[] { };
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple?.Subject.GetId();
        }
    }
}
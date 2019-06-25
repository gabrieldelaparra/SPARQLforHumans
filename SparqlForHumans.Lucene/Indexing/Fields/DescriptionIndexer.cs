using System.Collections.Generic;
using System.Linq;
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

        public override IReadOnlyList<TextField> GetField(SubjectGroup tripleGroup)
        {
            var values = TriplesToValue(tripleGroup);
            return values.Any()
                ? values.Select(x=>new TextField(FieldName, x, Field.Store.YES)).ToList()
                : new List<TextField>();
        }
    }
}
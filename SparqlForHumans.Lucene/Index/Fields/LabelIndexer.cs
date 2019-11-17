using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Index.Fields
{
    public class LabelIndexer : BaseFieldIndexer<TextField>, IFieldIndexer<TextField>
    {
        public override string FieldName => Labels.Label.ToString();

        public override IReadOnlyList<TextField> GetField(SubjectGroup tripleGroup)
        {
            var values = TriplesToValue(tripleGroup);
            return values.Any()
                ? new List<TextField>
                {
                    new TextField(FieldName, string.Join(Constants.PropertyValueSeparator, values), Field.Store.YES)
                        {Boost = (float) Boost}
                }
                : new List<TextField>();
        }

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(PredicateType.Label);
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple.Object.GetLiteralValue();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Index.Fields
{
    public class AltLabelIndexer : BaseFieldIndexer<TextField>
    {
        public override string FieldName => Labels.AltLabel.ToString();

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(PredicateType.AltLabel);
        }

        public override IEnumerable<TextField> GetField(SubjectGroup tripleGroup)
        {
            var values = TriplesToValue(tripleGroup);
            return values.Any()
                ? new List<TextField> {
                    new TextField(FieldName, string.Join(Constants.PropertyValueSeparator, values), Field.Store.YES)
                        {Boost = (float) Boost}
                }
                : new List<TextField>();
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple.Object.GetLiteralValue();
        }
    }
}
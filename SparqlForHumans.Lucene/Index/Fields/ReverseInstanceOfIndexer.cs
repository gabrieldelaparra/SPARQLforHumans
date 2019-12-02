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
    //public class ReverseInstanceOfIndexer : BaseFieldIndexer<StringField>
    //{
    //    public override string FieldName => Labels.ReverseInstanceOf.ToString();

    //    public override bool FilterValidTriples(Triple triple)
    //    {
    //        return triple.Predicate.IsReverseInstanceOf();
    //    }

    //    public override IEnumerable<StringField> GetField(SubjectGroup tripleGroup)
    //    {
    //        var values = TriplesToValue(tripleGroup);
    //        return values.Any()
    //            ? values.Select(x => new StringField(FieldName, x, Field.Store.YES))
    //            : new List<StringField>();
    //    }

    //    public override string SelectTripleValue(Triple triple)
    //    {
    //        return triple.Object.GetId();
    //    }
    //}
}
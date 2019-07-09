using Lucene.Net.Index;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Index.Base
{
    public interface IFieldIndexer
    {
        string FieldName { get; }
    }

    public interface IFieldIndexer<out TField> : IFieldIndexer
        where TField : IIndexableField
    {
        double Boost { get; set; }
        IReadOnlyList<TField> GetField(SubjectGroup tripleGroup);
    }
}
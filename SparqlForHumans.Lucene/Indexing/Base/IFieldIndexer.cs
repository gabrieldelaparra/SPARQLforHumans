using Lucene.Net.Index;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.Base
{
    public interface IFieldIndexer
    {
        string FieldName { get; }
    }

    public interface IFieldIndexer<out TField> : IFieldIndexer
        where TField : IIndexableField
    {
        TField GetField(SubjectGroup tripleGroup);
    }
}
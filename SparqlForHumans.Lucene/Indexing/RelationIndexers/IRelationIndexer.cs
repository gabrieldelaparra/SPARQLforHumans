using Lucene.Net.Index;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.BaseFields
{
    public interface IRelationIndexer<out T1, TDictionaryType> : IFieldIndexer
        where T1 : IIndexableField
    {
        TDictionaryType Dictionary { get; }
        T1 GetField(SubjectGroup subjectGroup);
    }
}

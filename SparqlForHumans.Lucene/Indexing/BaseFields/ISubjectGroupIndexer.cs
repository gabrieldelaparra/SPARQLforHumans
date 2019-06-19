using Lucene.Net.Index;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.BaseFields
{
    public interface ISubjectGroupIndexer<out T1> : IFieldIndexer
        where T1 : IIndexableField
    {
        T1 TriplesToField(SubjectGroup tripleGroup);
    }
}
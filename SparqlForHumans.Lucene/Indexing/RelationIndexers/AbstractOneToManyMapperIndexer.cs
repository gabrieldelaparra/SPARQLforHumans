using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.BaseFields
{
    public abstract class AbstractOneToManyMapperIndexer<T1, TKey, TValue> : IRelationIndexer<T1, Dictionary<TKey, TValue[]>>
    where T1 : IIndexableField
    {
        public abstract string FieldName { get; }
        public AbstractOneToManyRelationMapper<TKey, TValue> RelationMapper { get; internal set; }
        public Dictionary<TKey, TValue[]> Dictionary { get; internal set; }
        public abstract T1 GetField(SubjectGroup subjectGroup);
    }
}

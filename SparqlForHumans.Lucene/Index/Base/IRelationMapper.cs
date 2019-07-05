using System.Collections.Generic;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Index.Base
{
    public interface IRelationMapper
    {
    }

    public interface IRelationMapper<out TIndex> : IRelationMapper
        where TIndex : class
    {
        TIndex RelationIndex { get; }
        TIndex BuildIndex(IEnumerable<SubjectGroup> subjectGroups);
    }
}
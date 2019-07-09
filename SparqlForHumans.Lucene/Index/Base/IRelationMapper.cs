using SparqlForHumans.RDF.Models;
using System.Collections.Generic;

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
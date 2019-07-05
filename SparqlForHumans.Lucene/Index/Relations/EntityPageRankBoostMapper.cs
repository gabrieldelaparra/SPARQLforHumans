using System.Collections.Generic;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class EntityPageRankBoostMapper : BaseOneToOneRelationMapper<int, double>
    {
        public EntityPageRankBoostMapper(string inputFileName) : base(inputFileName)
        {
        }

        public override string NotifyMessage { get; } = "Building <EntityId, Boost> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, double> dictionary, SubjectGroup subjectGroup)
        {
        }

        public override Dictionary<int, double> BuildIndex(IEnumerable<SubjectGroup> subjectGroups)
        {
            LogProgress(0);
            return EntityPageRank.BuildPageRank(subjectGroups);
        }
    }
}
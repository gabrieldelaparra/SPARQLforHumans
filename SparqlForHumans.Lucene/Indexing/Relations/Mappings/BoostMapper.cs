using SparqlForHumans.Lucene.Indexing.Relations.Mappings.Base;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Indexing.Relations.Mappings
{
    public class BoostMapper : BaseOneToOneRelationMapper<int, double>
    {
        public BoostMapper(string inputFileName) : base(inputFileName)
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
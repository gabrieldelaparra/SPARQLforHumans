using System.Collections.Generic;
using SparqlForHumans.Lucene.Indexing.Mappings.Base;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.Mappings
{
    public class BoostMapper : BaseOneToOneRelationMapper<int, double>
    {
        public BoostMapper(string inputFileName) : base(inputFileName)
        {
        }

        public override string NotifyMessage { get;  } = "Building <EntityId, Boost> Dictionary";

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
using SparqlForHumans.Lucene.Indexing.Relations.Mappings.Base;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Indexing.Relations.Mappings
{
    public class InstanceOfMapper : BaseHashSetMapper<int>
    {
        public InstanceOfMapper(string inputFileName) : base(inputFileName)
        {
        }

        public InstanceOfMapper(IEnumerable<SubjectGroup> subjectGroup) : base(subjectGroup)
        {
        }

        public override string NotifyMessage { get; } = "Building <InstanceOfIndex> HashSet";

        internal override void ParseTripleGroup(HashSet<int> hashSet, SubjectGroup subjectGroup)
        {
            var entityTypes = subjectGroup
                .Where(x => x.Predicate.IsInstanceOf())
                .Select(x => x.Object.GetIntId()).ToArray();

            foreach (var entityType in entityTypes)
            {
                hashSet.Add(entityType);
            }
        }
    }
}
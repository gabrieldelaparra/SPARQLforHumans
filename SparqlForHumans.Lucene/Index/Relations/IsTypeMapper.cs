using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class IsTypeMapper : BaseHashSetMapper<int>
    {
        public IsTypeMapper(string inputFileName) : base(inputFileName)
        {
        }

        public IsTypeMapper(IEnumerable<SubjectGroup> subjectGroup) : base(subjectGroup)
        {
        }

        public override string NotifyMessage { get; } = "Building <InstanceOfIndex> HashSet";

        internal override void ParseTripleGroup(HashSet<int> hashSet, SubjectGroup subjectGroup)
        {
            var entityTypes = subjectGroup
                .Where(x => x.Predicate.IsInstanceOf())
                .Select(x => x.Object.GetIntId()).ToArray();

            foreach (var entityType in entityTypes) hashSet.Add(entityType);
        }
    }
}
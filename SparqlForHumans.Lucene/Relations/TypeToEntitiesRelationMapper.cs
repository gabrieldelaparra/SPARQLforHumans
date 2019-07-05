using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Relations
{
    /// <summary>
    ///     Given the following data:
    ///     ```
    ///     ...
    ///     Q76 -> P31 (InstanceOf) -> Q5
    ///     Q76 -> P31 (InstanceOf) -> Q6
    ///     ...
    ///     Q298 -> P31 (InstanceOf) -> Q17
    ///     Q298 -> P31 (InstanceOf) -> Q6
    ///     ...
    ///     ```
    ///     Returns the following:
    ///     Q5: Q76
    ///     Q6: Q76, Q298
    ///     Q17: Q298
    ///     Translated to the following KeyValue Pairs:
    ///     Key: 5; Values[]: 76
    ///     Key: 6; Values[]: 76, 298
    ///     Key: 17; Values[]: 298
    /// </summary>
    public class TypeToEntitiesRelationMapper : BaseOneToManyRelationMapper<int, int>
    {
        public TypeToEntitiesRelationMapper(string inputFileName) : base(inputFileName)
        {
        }

        public TypeToEntitiesRelationMapper(IEnumerable<SubjectGroup> subjectGroup) : base(subjectGroup)
        {
        }

        public override string NotifyMessage { get; } = "Building <Type, Entities[]> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            var entityTypes = subjectGroup
                .Where(x => x.Predicate.IsInstanceOf())
                .Select(x => x.Object.GetIntId()).ToArray();

            foreach (var entityType in entityTypes) dictionary.AddSafe(entityType, subjectGroup.IntId);
        }
    }
}
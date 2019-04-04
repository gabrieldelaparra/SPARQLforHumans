using System.Collections.Generic;
using System.Linq;
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
    ///     Q76: Q5, Q6
    ///     Q298: Q17, Q5
    ///     Translated to the following KeyValue Pairs:
    ///     Key: 76; Values[]: 5, 6
    ///     Key: 298; Values[]: 17, 5
    /// </summary>
    public class EntityToTypesRelationMapper : AbstractOneToManyRelationMapper<int, int>
    {
        public override string NotifyMessage { get; internal set; } = "Building <Entity, Types[]> Dictionary";

        internal override void AddToDictionary(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            // Types
            var entityTypes = subjectGroup
                .Where(x => x.Predicate.IsInstanceOf())
                .Select(x => x.Object.GetIntId());

            dictionary.AddSafe(subjectGroup.IntId, entityTypes);
        }
    }
}

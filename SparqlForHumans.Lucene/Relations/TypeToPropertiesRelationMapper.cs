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
    ///     Q76 -> P31 (Type) -> Q5
    ///     Q76 -> P27 -> Qxx
    ///     Q76 -> P555 -> Qxx
    ///     ...
    ///     Q298 -> P31 -> Q17
    ///     Q298 -> P555 -> Qxx
    ///     Q298 -> P777 -> Qxx
    ///     ...
    ///     ```
    ///     Returns the following domain:
    ///     Q5: P27, P555
    ///     Q17: P555, P777
    ///     Translated to the following KeyValue Pairs:
    ///     Key: 5; Values[]: 27, 555
    ///     Key: 17; Values[]: 555, 777
    /// </summary>
    public class TypeToPropertiesRelationMapper : AbstractOneToManyRelationMapper<int, int>
    {
        public override string NotifyMessage { get; internal set; } = "Building <Type, Properties[]> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            //Hopefully they should be already filtered.
            var propertiesTriples = subjectGroup.FilterPropertyPredicatesOnly();

            var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

            // InstanceOf Ids (Domain Types) and Properties
            var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).ToArray();
            var instanceOfIds = instanceOfSlice.Select(x => x.Object.GetIntId()).ToArray();

            foreach (var instanceOfId in instanceOfIds)
                dictionary.AddSafe(instanceOfId, propertyIds);
        }
    }
}

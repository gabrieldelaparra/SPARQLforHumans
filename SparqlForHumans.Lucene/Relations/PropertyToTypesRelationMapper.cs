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
    ///     P27: Domain Q5
    ///     P555: Domain Q5, Q17
    ///     P777: Domain Q17
    ///     Translated to the following KeyValue Pairs:
    ///     Key: 27; Values[]: 5
    ///     Key: 555; Values[]: 5, 17
    ///     Key: 777; Values[]: 17
    /// </summary>
    public class PropertyToTypesRelationMapper : AbstractOneToManyRelationMapper<int, int>
    {
        public override string NotifyMessage { get; set; } = "Building <Property, Types[]> Dictionary";

        internal override void AddToDictionary(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            // Filter those the triples that are properties only (Exclude description, label, etc.)
            var propertiesTriples = subjectGroup.FilterPropertyPredicatesOnly();

            var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

            // InstanceOf Ids (Domain Types) and Properties
            var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).ToArray();
            var instanceOfIds = instanceOfSlice.Select(x => x.Object.GetIntId()).ToArray();

            foreach (var propertyId in propertyIds)
                dictionary.AddSafe(propertyId, instanceOfIds);;
        }
    }
}

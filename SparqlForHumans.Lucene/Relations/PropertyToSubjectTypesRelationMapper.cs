using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Indexing.Relations;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings.Base;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Relations
{
    /// <summary>
    ///     Given the following data:
    ///     ```
    ///     ...
    ///     Qxx -> P31 (InstanceOf) -> Q5
    ///     Qxx -> P27 -> Qxx
    ///     Qxx -> P555 -> Qxx
    ///     ...
    ///     Qxx -> P31 (InstanceOf) -> Q17
    ///     Qxx -> P555 -> Qxx
    ///     Qxx -> P777 -> Qxx
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
    public class PropertyToSubjectTypesRelationMapper : BaseOneToManyRelationMapper<int, int>
    {
        public PropertyToSubjectTypesRelationMapper(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups){}
        public PropertyToSubjectTypesRelationMapper(string inputFilename) : base(inputFilename){}

        public override string NotifyMessage { get; } = "Building <Property, Types[]> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
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

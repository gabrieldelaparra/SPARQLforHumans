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
    ///     Qxx -> P27 -> Q13
    ///     Qxx -> P555 -> Q24
    ///     ...
    ///     Qxx -> P31 (InstanceOf) -> Q17
    ///     Qxx -> P555 -> Q35
    ///     Qxx -> P777 -> Q47
    ///     ...
    ///     ```
    ///     Returns the following domain:
    ///     P27: Q13
    ///     P555: Q24, Q35
    ///     P777: Q47
    ///     Translated to the following KeyValue Pairs:
    ///     Key: 27; Values[]: 13
    ///     Key: 555; Values[]: 24, 35
    ///     Key: 777; Values[]: 47
    /// </summary>
    public class PropertyToObjectEntitiesRelationMapper : BaseOneToManyRelationMapper<int, int>
    {
        public PropertyToObjectEntitiesRelationMapper(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups){}

        public override string NotifyMessage { get;} = "Building <Property, Entities[]> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            var validProperties = subjectGroup
                .Where(x => x.Predicate.IsProperty()
                            && !x.Predicate.IsInstanceOf()
                            && x.Object.IsEntityQ())
                .Select(x => new { PropertyId = x.Predicate.GetIntId(), ObjectId = x.Object.GetIntId() });

            foreach (var validProperty in validProperties)
                dictionary.AddSafe(validProperty.PropertyId, validProperty.ObjectId);
        }
    }
}

using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Relations
{
    /// <summary>
    ///     Given the following data:
    ///     ```
    ///     ...
    ///     Qxx -> P45 -> Qxx
    ///     Qxx -> P56 -> Qxx
    ///     ...
    ///     Qxx -> P34 -> Qxx
    ///     Qxx -> P56 -> Qxx
    ///     ...
    ///     ```
    ///     Returns the following:
    ///     P45: 1
    ///     P56: 2
    ///     P34: 1
    ///     Translated to the following KeyValue Pairs:
    ///     Key: 45; Value: 1
    ///     Key: 56; Value: 2
    ///     Key: 34; Value: 1
    /// </summary>
    public class PropertiesFrequencyRelationMapper : AbstractOneToOneRelationMapper<int, int>
    {
        public override string NotifyMessage { get; internal set; } = "Building <Property, Frequency> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, int> dictionary, SubjectGroup subjectGroup)
        {
            foreach (var triple in subjectGroup)
            {
                var predicate = triple.Predicate;

                // Filter Properties Only
                // TODO: Check if InstanceOf should also be added
                if (!predicate.IsProperty())
                    continue;

                var predicateIntId = predicate.GetIntId();

                if (!dictionary.ContainsKey(predicateIntId))
                    dictionary.Add(predicateIntId, 0);

                dictionary[predicateIntId]++;
            }
        }
    }
}

using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Relations
{
    public class PropertiesFrequencyRelationMapper : AbstractOneToOneRelationMapper<int, int>
    {
        public override string NotifyMessage { get; set; } = "Building <Property, Frequency> Dictionary";

        internal override void AddToDictionary(Dictionary<int, int> dictionary, SubjectGroup subjectGroup)
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

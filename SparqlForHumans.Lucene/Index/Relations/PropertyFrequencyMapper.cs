using System.Collections.Generic;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class PropertyFrequencyMapper : BaseOneToOneRelationMapper<int, int>
    {
        public PropertyFrequencyMapper(string inputFilename) : base(inputFilename)
        {
        }

        public override string NotifyMessage { get; } = "Building <PropertyId, Count> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, int> dictionary, SubjectGroup subjectGroup)
        {
            foreach (var triple in subjectGroup)
            {
                // Filter Properties Only
                if (!triple.Predicate.IsProperty()) continue;

                var predicateIntId = triple.Predicate.GetIntId();

                if (!dictionary.ContainsKey(predicateIntId)) dictionary.Add(predicateIntId, 0);

                dictionary[predicateIntId]++;
            }
        }
    }
}
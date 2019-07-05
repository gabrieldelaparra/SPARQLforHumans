using SparqlForHumans.Lucene.Indexing.Relations.Mappings.Base;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Indexing.Relations.Mappings
{
    public class FrequencyMapper : BaseOneToOneRelationMapper<int, int>
    {
        public FrequencyMapper(string inputFilename) : base(inputFilename)
        {
        }

        public override string NotifyMessage { get; } = "Building <PropertyId, Count> Dictionary";

        internal override void ParseTripleGroup(Dictionary<int, int> dictionary, SubjectGroup subjectGroup)
        {
            foreach (var triple in subjectGroup)
            {
                // Filter Properties Only
                if (!triple.Predicate.IsProperty())
                {
                    continue;
                }

                var predicateIntId = triple.Predicate.GetIntId();

                if (!dictionary.ContainsKey(predicateIntId))
                {
                    dictionary.Add(predicateIntId, 0);
                }

                dictionary[predicateIntId]++;
            }
        }
    }
}
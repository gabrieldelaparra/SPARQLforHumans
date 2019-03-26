using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertiesFrequency
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();
        public static int NotifyTicks { get; } = 100000;

        public static Dictionary<int, int> GetPropertiesFrequency(string triplesFilename)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupBySubject();

            var dictionary = GetPropertiesFrequency(groups);

            return dictionary;
        }

        public static Dictionary<int, int> GetPropertiesFrequency(IEnumerable<SubjectGroup> groups)
        {
            var nodeCount = 0;
            var dictionary = new Dictionary<int, int>();

            foreach (var group in groups)
            {
                if (nodeCount % NotifyTicks == 0)
                    Logger.Info($"Properties Frequency, Group: {nodeCount:N0}");

                foreach (var line in group)
                    //The rest of this method could be refactored.
                    ParsePropertyFrequencyLine(line, dictionary);

                nodeCount++;
            }

            Logger.Info($"Properties Frequency, Group: {nodeCount:N0}");

            return dictionary;
        }

        public static void ParsePropertyFrequencyLine(Triple line, Dictionary<int, int> dictionary)
        {
            var predicate = line.Predicate;

            //Not a Property
            if (!predicate.IsProperty())
                return;

            //var predicateId = line.GetTriple().Predicate.GetId();


            //if (!predicateId.Contains(WikidataDump.PropertyPrefix))
            //    return;

            var predicateIntId = predicate.GetIntId();

            if (!dictionary.ContainsKey(predicateIntId))
                dictionary.Add(predicateIntId, 0);

            dictionary[predicateIntId]++;
        }
    }
}
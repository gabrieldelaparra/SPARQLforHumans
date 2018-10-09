using System.Collections.Generic;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertiesFrequency
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();
        public static int NotifyTicks { get; } = 100000;

        public static Dictionary<string, int> GetPropertiesFrequency(string triplesFilename)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupBySubject();

            var dictionary = GetPropertiesFrequency(groups);

            return dictionary;
        }

        public static Dictionary<string, int> GetPropertiesFrequency(IEnumerable<IEnumerable<string>> groups)
        {
            var nodeCount = 0;
            var dictionary = new Dictionary<string, int>();

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

        public static void ParsePropertyFrequencyLine(string line, Dictionary<string, int> dictionary)
        {
            var predicateId = line.GetTriple().Predicate.GetId();

            if (!predicateId.Contains(WikidataDump.PropertyPrefix)) return;

            if (!dictionary.ContainsKey(predicateId))
                dictionary.Add(predicateId, 0);

            dictionary[predicateId]++;
        }
    }
}
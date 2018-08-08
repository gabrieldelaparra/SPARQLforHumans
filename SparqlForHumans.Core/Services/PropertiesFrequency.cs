using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.Core.Services
{
    public static class PropertiesFrequency
    {
        private static readonly NLog.Logger Logger = Utilities.Logger.Init();
        public static int NotifyTicks { get; } = 1000;

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
                    Logger.Info($"Group: {nodeCount:N0}");

                foreach (var line in group)
                {
                    //The rest of this method could be refactored.
                    ParsePropertyFrequencyLine(line, dictionary);
                }

                nodeCount++;
            }

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

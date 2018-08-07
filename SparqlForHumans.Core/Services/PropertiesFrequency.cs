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

            var nodeCount = 0;
            var dictionary = new Dictionary<string, int>();

            foreach (var group in groups)
            {
                if (nodeCount % NotifyTicks == 0)
                    Logger.Info($"Group: {nodeCount:N0}");

                foreach (var line in group)
                {
                    var predicateId = line.GetTriple().Predicate.GetId();

                    if (!predicateId.Contains(WikidataDump.PropertyPrefix)) continue;

                    dictionary[predicateId]++;
                }
                nodeCount++;
            }

            return dictionary;
        }
    }
}

using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Relations
{
    public abstract class AbstractOneToOneRelationMapper<T1, T2>
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();
        public int NotifyTicks { get; } = 100000;
        public abstract string NotifyMessage { get; internal set; }
        private int nodeCount = 0;

        public virtual Dictionary<T1, T2> GetRelationDictionary(string inputFilename)
        {
            var subjectGroups = FileHelper.GetInputLines(inputFilename).GroupBySubject();
            return GetRelationDictionary(subjectGroups);
        }

        public virtual Dictionary<T1, T2> GetRelationDictionary(IEnumerable<SubjectGroup> subjectGroups)
        {
            var dictionary = new Dictionary<T1, T2>();
            foreach (var subjectGroup in subjectGroups)
            {
                if (nodeCount % NotifyTicks == 0)
                    Logger.Info($"{NotifyMessage}, Entity Group: {nodeCount:N0}");
                
                AddToDictionary(dictionary, subjectGroup);
                
                nodeCount++;
            }

            Logger.Info($"{NotifyMessage}, Entity Group: {nodeCount:N0}");
            return dictionary;
        }

        internal abstract void AddToDictionary(Dictionary<T1, T2> dictionary, SubjectGroup subjectGroup);
    }
}

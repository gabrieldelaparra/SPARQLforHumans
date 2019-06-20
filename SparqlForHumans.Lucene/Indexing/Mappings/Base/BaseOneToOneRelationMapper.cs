using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.Mappings.Base
{
    public abstract class BaseOneToOneRelationMapper<TKey, TValue> : BaseNotifier,
        IRelationMapper<Dictionary<TKey, TValue>>
    {
        protected BaseOneToOneRelationMapper(string inputFilename)
        {
            RelationIndex = BuildIndex(FileHelper.ReadLines(inputFilename).GroupBySubject().Where(x => x.IsEntityQ()));
        }

        public Dictionary<TKey, TValue> RelationIndex { get; internal set; }

        public virtual Dictionary<TKey, TValue> BuildIndex(IEnumerable<SubjectGroup> subjectGroups)
        {
            var ticks = 0;
            var dictionary = new Dictionary<TKey, TValue>();

            foreach (var subjectGroup in subjectGroups)
            {
                if (ticks % NotifyTicks == 0)
                    LogProgress(ticks);

                ParseTripleGroup(dictionary, subjectGroup);
                ticks++;
            }

            LogProgress(ticks);
            return dictionary;
        }

        //Derived Class Implementation:
        internal abstract void ParseTripleGroup(Dictionary<TKey, TValue> dictionary, SubjectGroup subjectGroup) { }
    }
}
using System.Collections.Generic;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Relations
{
    public abstract class AbstractOneToOneRelationMapper<TKey, TValue> : IRelationMapper<Dictionary<TKey, TValue>>
    {
        public int NotifyTicks { get; set; } = 100000;
        public delegate void EventHandler(int Ticks);
        public event EventHandler OnNotifyTicks;
        public abstract string NotifyMessage { get; internal set; } 

        public virtual Dictionary<TKey, TValue> BuildDictionary(IEnumerable<SubjectGroup> subjectGroups)
        {
            var ticks = 0;
            var dictionary = new Dictionary<TKey, TValue>();

            foreach (var subjectGroup in subjectGroups)
            {
                if (ticks % NotifyTicks == 0)
                    OnNotifyTicks?.Invoke(ticks);

                ParseTripleGroup(dictionary, subjectGroup);
                ticks++;
            }

            OnNotifyTicks?.Invoke(ticks);
            return dictionary;
        }

        internal abstract void ParseTripleGroup(Dictionary<TKey, TValue> dictionary, SubjectGroup subjectGroup);
    }
}

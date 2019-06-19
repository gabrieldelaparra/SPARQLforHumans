using System.Collections.Generic;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Relations
{
    public abstract class AbstractOneToManyRelationMapper<TKey, TValue> : IRelationMapper<Dictionary<TKey, TValue[]>>
    {
        public int NotifyTicks { get; set; } = 100000;
        public delegate void EventHandler(int Ticks);
        public event EventHandler OnNotifyTicks;
        public abstract string NotifyMessage { get; internal set; } 

        public virtual Dictionary<TKey, TValue[]> BuildDictionary(IEnumerable<SubjectGroup> subjectGroups)
        {
            var ticks = 0;
            var dictionary = new Dictionary<TKey, List<TValue>>();

            foreach (var subjectGroup in subjectGroups)
            {
                if (ticks % NotifyTicks == 0) 
                    OnNotifyTicks?.Invoke(ticks);

                ParseTripleGroup(dictionary, subjectGroup);
                ticks++;
            }

            OnNotifyTicks?.Invoke(ticks);
            return dictionary.ToArrayDictionary();
        }

        internal abstract void ParseTripleGroup(Dictionary<TKey, List<TValue>> dictionary, SubjectGroup subjectGroup);
    }
}

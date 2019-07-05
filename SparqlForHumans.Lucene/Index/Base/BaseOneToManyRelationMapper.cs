using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Logger;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index.Base
{
    public abstract class BaseOneToManyRelationMapper<TKey, TValue> : BaseNotifier,
        IRelationMapper<Dictionary<TKey, TValue[]>>
    {
        protected BaseOneToManyRelationMapper(string inputFilename)
        {
            RelationIndex = BuildIndex(FileHelper.ReadLines(inputFilename).GroupBySubject().Where(x => x.IsEntityQ()));
        }

        protected BaseOneToManyRelationMapper(IEnumerable<SubjectGroup> subjectGroups)
        {
            RelationIndex = BuildIndex(subjectGroups);
        }

        public Dictionary<TKey, TValue[]> RelationIndex { get; internal set; }

        public virtual Dictionary<TKey, TValue[]> BuildIndex(IEnumerable<SubjectGroup> subjectGroups)
        {
            var ticks = 0;
            var dictionary = new Dictionary<TKey, List<TValue>>();

            foreach (var subjectGroup in subjectGroups)
            {
                ParseTripleGroup(dictionary, subjectGroup);
                LogProgress(ticks++);
            }

            LogProgress(ticks, true);
            return dictionary.ToArrayDictionary();
        }

        //Derived Class Implementation:
        internal abstract void ParseTripleGroup(Dictionary<TKey, List<TValue>> dictionary, SubjectGroup subjectGroup);
    }
}
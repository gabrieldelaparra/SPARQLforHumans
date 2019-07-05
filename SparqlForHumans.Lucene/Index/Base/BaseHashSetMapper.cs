using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Logger;

namespace SparqlForHumans.Lucene.Index.Base
{
    public abstract class BaseHashSetMapper<TValue> : BaseNotifier, IRelationMapper<HashSet<TValue>>
    {
        protected BaseHashSetMapper(string inputFilename)
        {
            RelationIndex = BuildIndex(FileHelper.ReadLines(inputFilename).GroupBySubject().Where(x => x.IsEntityQ()));
        }

        protected BaseHashSetMapper(IEnumerable<SubjectGroup> subjectGroups)
        {
            RelationIndex = BuildIndex(subjectGroups.Where(x => x.IsEntityQ()));
        }

        public HashSet<TValue> RelationIndex { get; internal set; }

        public HashSet<TValue> BuildIndex(IEnumerable<SubjectGroup> subjectGroups)
        {
            var ticks = 0;
            var hashSet = new HashSet<TValue>();

            foreach (var subjectGroup in subjectGroups)
            {
                ParseTripleGroup(hashSet, subjectGroup);
                LogProgress(ticks++);
            }

            LogProgress(ticks, true);
            return hashSet;
        }

        //Derived Class Implementation:
        internal abstract void ParseTripleGroup(HashSet<TValue> hashSet, SubjectGroup subjectGroup);
    }
}
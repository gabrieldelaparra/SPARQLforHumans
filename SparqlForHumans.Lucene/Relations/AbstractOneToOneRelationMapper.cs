using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Relations
{
    public abstract class AbstractOneToOneRelationMapper<T1, T2>
    {
        public virtual Dictionary<T1, T2> GetRelationDictionary(string inputFilename)
        {
            var lines = FileHelper.GetInputLines(inputFilename);
            var subjectGroups = lines.GroupBySubject();

            return GetRelationDictionary(subjectGroups);
        }

        public virtual Dictionary<T1, T2> GetRelationDictionary(IEnumerable<SubjectGroup> subjectGroups)
        {
            var dictionary = new Dictionary<T1, T2>();
            foreach (var subjectGroup in subjectGroups)
                AddToDictionary(dictionary, subjectGroup);

            return dictionary;
        }

        internal abstract void AddToDictionary(Dictionary<T1, T2> dictionary, SubjectGroup subjectGroup);
    }
}

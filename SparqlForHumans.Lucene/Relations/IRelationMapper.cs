using System.Collections.Generic;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Relations
{
    public interface IRelationMapper<out TDictionaryType>
    {
        TDictionaryType BuildDictionary(IEnumerable<SubjectGroup> subjectGroups);
    }
}

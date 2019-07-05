using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Indexing.Relations
{
    public class PropertyRangeIndexer : PropertyRangeMapper, IFieldIndexer<StringField>
    {
        public string FieldName => Labels.Range.ToString();
        public double Boost { get; set; }

        public PropertyRangeIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public PropertyRangeIndexer(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups)
        {
        }

        public IReadOnlyList<StringField> GetField(SubjectGroup tripleGroup)
        {
            return RelationIndex.ContainsKey(tripleGroup.Id.ToNumbers())
                ? RelationIndex[tripleGroup.Id.ToNumbers()].Select(x => new StringField(FieldName, x.ToString(), Field.Store.YES)).ToList()
                : new List<StringField>();
        }
    }
}

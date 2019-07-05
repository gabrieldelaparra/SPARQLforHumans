using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Indexing.Relations
{
    public class IsTypeIndexer : InstanceOfMapper, IFieldIndexer<StringField>
    {
        public IsTypeIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public IsTypeIndexer(IEnumerable<SubjectGroup> subjectGroup) : base(subjectGroup)
        {
        }

        public double Boost { get; set; }

        public string FieldName => Labels.IsTypeEntity.ToString();

        public IReadOnlyList<StringField> GetField(SubjectGroup subjectGroup)
        {
            return RelationIndex.Contains(subjectGroup.Id.ToNumbers())
                ? new List<StringField> { new StringField(FieldName, true.ToString(), Field.Store.YES) }
                : new List<StringField>();
        }
    }
}
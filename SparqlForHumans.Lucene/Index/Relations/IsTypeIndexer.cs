using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class IsTypeIndexer : IsTypeMapper, IFieldIndexer<StringField>
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
                ? new List<StringField> {new StringField(FieldName, true.ToString(), Field.Store.YES)}
                : new List<StringField>();
        }
    }
}
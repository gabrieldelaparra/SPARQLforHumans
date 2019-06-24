using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

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

        public StringField GetField(SubjectGroup subjectGroup)
        {
            return RelationIndex.Contains(subjectGroup.Id.ToNumbers())
                ? new StringField(FieldName, true.ToString(), Field.Store.YES)
                : null;
        }
    }
}
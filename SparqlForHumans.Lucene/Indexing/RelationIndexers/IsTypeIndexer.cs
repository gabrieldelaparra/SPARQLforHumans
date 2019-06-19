using System.Collections.Generic;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.EntityFields
{
    public class IsTypeIndexer : AbstractOneToManyMapperIndexer<StringField, int, int>
    {
        public override string FieldName => Labels.IsTypeEntity.ToString();

        public IsTypeIndexer(IEnumerable<SubjectGroup> subjectGroups)
        {
            this.RelationMapper = new TypeToEntitiesRelationMapper();
            this.Dictionary = RelationMapper.BuildDictionary(subjectGroups);
        }

        public override StringField GetField(SubjectGroup subjectGroup)
        {
            return Dictionary.ContainsKey(subjectGroup.Id.ToNumbers())
                ? new StringField(FieldName, true.ToString(), Field.Store.YES)
                : null;
        }
    }
}
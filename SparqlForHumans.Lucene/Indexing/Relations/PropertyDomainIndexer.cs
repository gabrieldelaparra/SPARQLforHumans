using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.Relations
{
    public class PropertyDomainIndexer : PropertyToSubjectTypesRelationMapper, IFieldIndexer<StringField>
    {
        public string FieldName => Labels.DomainType.ToString();
        public double Boost { get; set; }

        public PropertyDomainIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public PropertyDomainIndexer(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups)
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

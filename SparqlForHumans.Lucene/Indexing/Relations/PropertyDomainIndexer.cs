using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.Relations
{
    public class PropertyDomainIndexer : TypeToPropertiesMapper, IFieldIndexer<StringField>
    {
        public string FieldName => Labels.DomainType.ToString();
        public double Boost { get; set; }

        public PropertyDomainIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public PropertyDomainIndexer(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups)
        {
        }

        public StringField GetField(SubjectGroup tripleGroup)
        {
            if (RelationIndex.ContainsKey(tripleGroup.Id.ToNumbers()))
            {
                var values = string.Join(WikidataDump.PropertyValueSeparator,
                    RelationIndex[tripleGroup.Id.ToNumbers()]);
                return new StringField(FieldName, values, Field.Store.YES);
            }

            return null;
        }
    }
}

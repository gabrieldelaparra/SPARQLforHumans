using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class IndexBuilder
    {
        public static IndexWriterConfig CreateIndexWriterConfig()
        {
            Options.InternUris = false;
            var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, new KeywordAnalyzer())
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
                //Similarity = new DefaultSimilarity(),

            };
            return indexConfig;
        }

        public static void AddFields(Document doc, IEnumerable<Field> fields, double boost = 0)
        {
            var altLabelCount = fields.Count(x => x.Name.Equals(Labels.AltLabel.ToString()))+1;

            foreach (var field in fields)
            {
                if (field.Name.Equals(Labels.Label.ToString()))
                    field.Boost = (float)boost;

                if (field.Name.Equals(Labels.AltLabel.ToString()))
                    field.Boost = (float)(boost / altLabelCount);
                //field.Boost = ((float)boost / altLabelCount);

                doc.Add(field);
            }
        }

        public static Dictionary<int, int[]> CreateTypesAndPropertiesDictionary()
        {
            using (var entitiesIndexDirectory =
                FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
            {
                return CreateTypesAndPropertiesDictionary(entitiesIndexDirectory);
            }
        }

        public static Dictionary<int, int[]> CreateTypesAndPropertiesDictionary(
            Directory entitiesIndexDirectory)
        {
            var dictionary = new Dictionary<int, List<int>>();

            using (var entityReader = DirectoryReader.Open(entitiesIndexDirectory))
            {
                var docCount = entityReader.NumDocs;
                for (var i = 0; i < docCount; i++)
                {
                    var doc = entityReader.Document(i);

                    var entity = new Entity(doc.MapBaseSubject())
                        .MapInstanceOf(doc)
                        .MapRank(doc)
                        .MapBaseProperties(doc);

                    foreach (var instanceOf in entity.InstanceOf)
                        dictionary.AddSafe(instanceOf.ToInt(), entity.Properties.Select(x => x.Id.ToInt()));

                }
            }

            return dictionary.ToArrayDictionary();
        }
    }
}
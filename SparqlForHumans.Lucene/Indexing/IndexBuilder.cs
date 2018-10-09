using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class IndexBuilder
    {
        //private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        //public static int NotifyTicks { get; } = 100000;

        public static Dictionary<string, List<string>> CreateInvertedProperties(
            Dictionary<string, List<string>> typesAndPropertiesDictionary)
        {
            var dictionary = new Dictionary<string, List<string>>();

            foreach (var type in typesAndPropertiesDictionary)
            foreach (var property in type.Value)
            {
                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, new List<string>());

                dictionary[property].Add(type.Key);
            }

            return dictionary;
        }

        public static void AddFields(Document doc, IEnumerable<Field> fields, double boost = 0)
        {
            foreach (var field in fields)
                doc.Add(field);
        }

        public static Dictionary<string, List<string>> CreateTypesAndPropertiesDictionary()
        {
            using (var entitiesIndexDirectory =
                FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
            {
                return CreateTypesAndPropertiesDictionary(entitiesIndexDirectory);
            }
        }

        public static Dictionary<string, List<string>> CreateTypesAndPropertiesDictionary(
            Directory entitiesIndexDirectory)
        {
            var dictionary = new Dictionary<string, List<string>>();

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
                    {
                        if (!dictionary.ContainsKey(instanceOf))
                            dictionary.Add(instanceOf, new List<string>());

                        var valuesList = dictionary[instanceOf];

                        foreach (var entityProperty in entity.Properties)
                            if (!valuesList.Contains(entityProperty.Id))
                                valuesList.Add(entityProperty.Id);
                    }
                }
            }

            return dictionary;
        }
    }
}
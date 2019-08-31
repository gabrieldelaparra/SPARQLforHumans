using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class InMemoryQueryEngine
    {
        private static bool IsInit = false;
        private static string EntitiesIndexPath;
        private static string PropertiesIndexPath;

        private static Dictionary<int, int[]> EntityIdDomainPropertiesDictionary;
        private static Dictionary<int, int[]> EntityIdRangePropertiesDictionary;

        public static void Init(string entitiesIndexPath, string propertiesIndexPath)
        {
            if (IsInit) return;
            EntitiesIndexPath = entitiesIndexPath;
            PropertiesIndexPath = propertiesIndexPath;
            BuildDictionaries();
            IsInit = true;
        }

        public static List<int> BatchIdPropertyDomainQuery(IEnumerable<string> searchStrings)
        {
            var searchInts = searchStrings.Select(x => x.ToInt());
            var results = EntityIdDomainPropertiesDictionary.Where(x => searchInts.Contains(x.Key));
            return results.SelectMany(x => x.Value).Distinct().ToList();
        }

        public static List<int> BatchIdPropertyRangeQuery(IEnumerable<string> searchStrings)
        {
            var searchInts = searchStrings.Select(x => x.ToInt());
            var results = EntityIdRangePropertiesDictionary.Where(x => searchInts.Contains(x.Key));
            return results.SelectMany(x => x.Value).Distinct().ToList();
        }

        private static void BuildDictionaries()
        {
            var propertyIdDomainsDictList = new Dictionary<int, List<int>>();
            var propertyIdRangesDictList = new Dictionary<int, List<int>>();
            var logger = Logger.Logger.Init();
            logger.Info($"Building Inverted Properties Domain and Range Dictionary");

            using (var luceneDirectory = FSDirectory.Open(PropertiesIndexPath))
            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var docCount = luceneDirectoryReader.MaxDoc;
                for (int i = 0; i < docCount; i++)
                {
                    var doc = luceneDirectoryReader.Document(i);
                    var property = doc.MapProperty();
                    propertyIdDomainsDictList.AddSafe(property.Id.ToInt(), property.Domain);
                    propertyIdRangesDictList.AddSafe(property.Id.ToInt(), property.Range);
                }
            }
            var propertyIdDomainsDictionary = propertyIdDomainsDictList.ToArrayDictionary();
            var propertyIdRangesDictionary = propertyIdRangesDictList.ToArrayDictionary();
            EntityIdDomainPropertiesDictionary = propertyIdDomainsDictionary.InvertDictionary();
            EntityIdRangePropertiesDictionary = propertyIdRangesDictionary.InvertDictionary();
        }
    }
}

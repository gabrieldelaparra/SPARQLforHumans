using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class InMemoryQueryEngine
    {
        private static bool IsInit = false;
        private static string EntitiesIndexPath;
        private static string PropertiesIndexPath;

        private static Dictionary<int, int[]> _entityIdDomainPropertiesDictionary;
        private static Dictionary<int, int[]> _entityIdRangePropertiesDictionary;

        private static Dictionary<int, int[]> _propertyIdDomainPropertiesDictionary;
        private static Dictionary<int, int[]> _propertyIdRangePropertiesDictionary;


        public static void Init(string entitiesIndexPath, string propertiesIndexPath)
        {
            if (IsInit) return;
            EntitiesIndexPath = entitiesIndexPath;
            PropertiesIndexPath = propertiesIndexPath;
            BuildDictionaries();
            IsInit = true;
        }

        public static IEnumerable<string> BatchPropertyIdDomainTypesQuery(IEnumerable<string> propertyUris)
        {
            var queryTypes = propertyUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchPropertyIdDomainTypesQuery(queryTypes);
            return results.Select(x => $"{Constants.EntityIRI}{Constants.EntityPrefix}{x}");
        }

        public static IEnumerable<string> BatchPropertyIdRangeTypesQuery(IEnumerable<string> propertyUris)
        {
            var queryTypes = propertyUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchPropertyIdRangeTypesQuery(queryTypes);
            return results.Select(x => $"{Constants.EntityIRI}{Constants.EntityPrefix}{x}");
        }

        public static IEnumerable<int> BatchPropertyIdDomainTypesQuery(IEnumerable<int> propertyIds)
        {
            var results = _propertyIdDomainPropertiesDictionary.Where(x => propertyIds.Contains(x.Key));
            return results.SelectMany(x => x.Value).Distinct().ToList();
        }

        public static IEnumerable<int> BatchPropertyIdRangeTypesQuery(IEnumerable<int> propertyIds)
        {
            var results = _propertyIdRangePropertiesDictionary.Where(x => propertyIds.Contains(x.Key));
            return results.SelectMany(x => x.Value).Distinct().ToList();
        }

        public static IEnumerable<string> BatchEntityIdOutgoingPropertiesQuery(IEnumerable<string> entityUris)
        {
            var queryTypes = entityUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchEntityIdOutgoingPropertiesQuery(queryTypes);
            return results.Select(x => $"{Constants.PropertyIRI}{Constants.PropertyPrefix}{x}");
        }

        public static IEnumerable<string> BatchEntityIdIncomingPropertiesQuery(IEnumerable<string> entityUris)
        {
            var queryTypes = entityUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchEntityIdIncomingPropertiesQuery(queryTypes);
            return results.Select(x => $"{Constants.PropertyIRI}{Constants.PropertyPrefix}{x}");
        }

        public static IEnumerable<int> BatchEntityIdOutgoingPropertiesQuery(IEnumerable<int> entityIds)
        {
            var results = _entityIdDomainPropertiesDictionary.Where(x => entityIds.Contains(x.Key));
            return results.SelectMany(x => x.Value).Distinct().ToList();
        }

        public static IEnumerable<int> BatchEntityIdIncomingPropertiesQuery(IEnumerable<int> entityIds)
        {
            var results = _entityIdRangePropertiesDictionary.Where(x => entityIds.Contains(x.Key));
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
                for (var i = 0; i < docCount; i++)
                {
                    var doc = luceneDirectoryReader.Document(i);
                    var property = doc.MapProperty();
                    propertyIdDomainsDictList.AddSafe(property.Id.ToInt(), property.Domain);
                    propertyIdRangesDictList.AddSafe(property.Id.ToInt(), property.Range);
                }
            }
            _propertyIdDomainPropertiesDictionary = propertyIdDomainsDictList.ToArrayDictionary();
            _propertyIdRangePropertiesDictionary = propertyIdRangesDictList.ToArrayDictionary();
            _entityIdDomainPropertiesDictionary = _propertyIdDomainPropertiesDictionary.InvertDictionary();
            _entityIdRangePropertiesDictionary = _propertyIdRangePropertiesDictionary.InvertDictionary();
        }
    }
}

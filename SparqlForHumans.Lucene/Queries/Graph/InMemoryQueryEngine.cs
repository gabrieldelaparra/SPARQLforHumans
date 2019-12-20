using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Support;
using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class InMemoryQueryEngine
    {
        private static bool _isInit = false;
        private static string _entitiesIndexPath;
        private static string _propertiesIndexPath;

        private static Dictionary<int, int[]> _entityIdDomainPropertiesDictionary;
        private static Dictionary<int, int[]> _entityIdRangePropertiesDictionary;

        private static Dictionary<int, int[]> _propertyIdDomainPropertiesDictionary;
        private static Dictionary<int, int[]> _propertyIdRangePropertiesDictionary;

        private static Dictionary<int, int[]> _propertyIdOutgoingPropertiesId;
        private static Dictionary<int, int[]> _propertyIdIncomingPropertiesId;

        public static void Init(string entitiesIndexPath, string propertiesIndexPath)
        {
            if (_isInit) return;
            _entitiesIndexPath = entitiesIndexPath;
            _propertiesIndexPath = propertiesIndexPath;
            BuildDictionaries();
            _isInit = true;
        }

        public static IEnumerable<string> BatchPropertyIdDomainTypesQuery(IEnumerable<string> propertyUris)
        {
            var queryTypes = propertyUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchPropertyIdDomainTypesQuery(queryTypes);
            return results.Select(x => $"{Constants.EntityPrefix}{x}");
        }

        public static IEnumerable<string> BatchPropertyIdRangeTypesQuery(IEnumerable<string> propertyUris)
        {
            var queryTypes = propertyUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchPropertyIdRangeTypesQuery(queryTypes);
            return results.Select(x => $"{Constants.EntityPrefix}{x}");
        }

        public static IEnumerable<string> PropertyIdOutgoingPropertiesQuery(string propertyUri)
        {
            var propertyIntId = propertyUri.GetUriIdentifier().ToInt();
            return _propertyIdOutgoingPropertiesId.ContainsKey(propertyIntId)
                ? _propertyIdOutgoingPropertiesId[propertyIntId].Select(x => $"{Constants.PropertyPrefix}{x}")
                : new string[0];
        }

        public static IEnumerable<string> PropertyIdIncomingPropertiesQuery(string propertyUri)
        {
            var propertyIntId = propertyUri.GetUriIdentifier().ToInt();
            return _propertyIdIncomingPropertiesId.ContainsKey(propertyIntId)
                ? _propertyIdIncomingPropertiesId[propertyIntId].Select(x => $"{Constants.PropertyPrefix}{x}")
                : new string[0];
        }

        private static IEnumerable<int> BatchPropertyIdDomainTypesQuery(IEnumerable<int> propertyIds)
        {
            var set = new HashSet<int>();
            var results = _propertyIdDomainPropertiesDictionary.Where(x => propertyIds.Contains(x.Key));
            foreach (var keyValuePair in results)
            {
                foreach (var value in keyValuePair.Value)
                {
                    set.Add(value);
                }
            }
            return set;
        }

        private static IEnumerable<int> BatchPropertyIdRangeTypesQuery(IEnumerable<int> propertyIds)
        {
            var set = new HashSet<int>();
            var results = _propertyIdRangePropertiesDictionary.Where(x => propertyIds.Contains(x.Key));
            foreach (var keyValuePair in results)
            {
                foreach (var value in keyValuePair.Value)
                {
                    set.Add(value);
                }
            }
            return set;
        }
        public static IEnumerable<string> BatchEntityIdOutgoingPropertiesQuery(IEnumerable<string> entityUris)
        {
            var queryTypes = entityUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchEntityIdOutgoingPropertiesQuery(queryTypes);
            return results.Select(x => $"{Constants.PropertyPrefix}{x}");
        }

        public static IEnumerable<string> BatchEntityIdIncomingPropertiesQuery(IEnumerable<string> entityUris)
        {
            var queryTypes = entityUris.Select(x => x.GetUriIdentifier().ToInt());
            var results = BatchEntityIdIncomingPropertiesQuery(queryTypes);
            return results.Select(x => $"{Constants.PropertyPrefix}{x}");
        }
        private static IEnumerable<int> BatchEntityIdOutgoingPropertiesQuery(IEnumerable<int> entityIds)
        {
            var set = new HashSet<int>();
            var results = _entityIdDomainPropertiesDictionary.Where(x => entityIds.Contains(x.Key));
            foreach (var keyValuePair in results)
            {
                foreach (var value in keyValuePair.Value)
                {
                    set.Add(value);
                }
            }
            return set;
        }
        private static IEnumerable<int> BatchEntityIdIncomingPropertiesQuery(IEnumerable<int> entityIds)
        {
            var set = new HashSet<int>();
            var results = _entityIdRangePropertiesDictionary.Where(x => entityIds.Contains(x.Key));
            foreach (var keyValuePair in results)
            {
                foreach (var value in keyValuePair.Value)
                {
                    set.Add(value);
                }
            }
            return set;
        }
        private static void BuildDictionaries()
        {
            var propertyIdDomainsDictList = new Dictionary<int, HashSet<int>>();
            var propertyIdRangesDictList = new Dictionary<int, HashSet<int>>();
            var logger = Logger.Logger.Init();
            logger.Info($"Building Inverted Properties Domain and Range Dictionary");

            using (var luceneDirectory = FSDirectory.Open(_propertiesIndexPath))
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

            var propertyIdOutgoingPropertiesId = new Dictionary<int, HashSet<int>>();
            var propertyIdIncomingPropertiesId = new Dictionary<int, HashSet<int>>();

            foreach (var propertyId in propertyIdDomainsDictList.Select(x => x.Key).Union(propertyIdRangesDictList.Select(x=>x.Key)))
            {
                if (propertyIdDomainsDictList.ContainsKey(propertyId))
                {
                    propertyIdOutgoingPropertiesId[propertyId] = new HashSet<int>();
                    var domain = propertyIdDomainsDictList[propertyId];
                    foreach (var domainId in domain)
                    {
                        var properties = _entityIdDomainPropertiesDictionary[domainId];
                        propertyIdOutgoingPropertiesId[propertyId].AddAll(properties);
                    }
                }

                if (propertyIdRangesDictList.ContainsKey(propertyId))
                {
                    propertyIdIncomingPropertiesId[propertyId] = new HashSet<int>();
                    var range = propertyIdRangesDictList[propertyId];
                    foreach (var rangeId in range)
                    {
                        var properties = _entityIdRangePropertiesDictionary[rangeId];
                        propertyIdIncomingPropertiesId[propertyId].AddAll(properties);
                    }
                }
            }

            _propertyIdOutgoingPropertiesId = propertyIdOutgoingPropertiesId.ToArrayDictionary();
            _propertyIdIncomingPropertiesId = propertyIdIncomingPropertiesId.ToArrayDictionary();

            logger.Info($"InMemory Domain Range Query Engine Complete");
        }
    }
}

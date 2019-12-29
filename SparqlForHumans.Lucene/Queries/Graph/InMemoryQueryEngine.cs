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

        private static Dictionary<int, int[]> _typeIdDomainPropertiesDictionary;
        private static Dictionary<int, int[]> _typeIdRangePropertiesDictionary;

        private static Dictionary<int, int[]> _propertyIdDomainTypesDictionary;
        private static Dictionary<int, int[]> _propertyIdRangeTypesDictionary;

        private static Dictionary<int, int[]> _propertyDomainOutgoingPropertiesIds;
        private static Dictionary<int, int[]> _propertyDomainIncomingPropertiesIds;
        private static Dictionary<int, int[]> _propertyRangeOutgoingPropertiesIds;
        private static Dictionary<int, int[]> _propertyRangeIncomingPropertiesIds;

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

        public static IEnumerable<string> PropertyDomainOutgoingPropertiesQuery(string propertyUri)
        {
            var propertyIntId = propertyUri.GetUriIdentifier().ToInt();
            return _propertyDomainOutgoingPropertiesIds.ContainsKey(propertyIntId)
                ? _propertyDomainOutgoingPropertiesIds[propertyIntId].Select(x => $"{Constants.PropertyPrefix}{x}")
                : new string[0];
        }

        public static IEnumerable<string> PropertyDomainIncomingPropertiesQuery(string propertyUri)
        {
            var propertyIntId = propertyUri.GetUriIdentifier().ToInt();
            return _propertyDomainIncomingPropertiesIds.ContainsKey(propertyIntId)
                ? _propertyDomainIncomingPropertiesIds[propertyIntId].Select(x => $"{Constants.PropertyPrefix}{x}")
                : new string[0];
        }

        public static IEnumerable<string> PropertyRangeOutgoingPropertiesQuery(string propertyUri)
        {
            var propertyIntId = propertyUri.GetUriIdentifier().ToInt();
            return _propertyRangeOutgoingPropertiesIds.ContainsKey(propertyIntId)
                ? _propertyRangeOutgoingPropertiesIds[propertyIntId].Select(x => $"{Constants.PropertyPrefix}{x}")
                : new string[0];
        }

        public static IEnumerable<string> PropertyRangeIncomingPropertiesQuery(string propertyUri)
        {
            var propertyIntId = propertyUri.GetUriIdentifier().ToInt();
            return _propertyRangeIncomingPropertiesIds.ContainsKey(propertyIntId)
                ? _propertyRangeIncomingPropertiesIds[propertyIntId].Select(x => $"{Constants.PropertyPrefix}{x}")
                : new string[0];
        }

        private static IEnumerable<int> BatchPropertyIdDomainTypesQuery(IEnumerable<int> propertyIds)
        {
            var set = new HashSet<int>();
            var results = _propertyIdDomainTypesDictionary.Where(x => propertyIds.Contains(x.Key));
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
            var results = _propertyIdRangeTypesDictionary.Where(x => propertyIds.Contains(x.Key));
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
            var results = _typeIdDomainPropertiesDictionary.Where(x => entityIds.Contains(x.Key));
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
            var results = _typeIdRangePropertiesDictionary.Where(x => entityIds.Contains(x.Key));
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
            _propertyIdDomainTypesDictionary = propertyIdDomainsDictList.ToArrayDictionary();
            _propertyIdRangeTypesDictionary = propertyIdRangesDictList.ToArrayDictionary();
            _typeIdDomainPropertiesDictionary = _propertyIdDomainTypesDictionary.InvertDictionary();
            _typeIdRangePropertiesDictionary = _propertyIdRangeTypesDictionary.InvertDictionary();

            var propertyDomainOutgoingPropertiesIds = new Dictionary<int, HashSet<int>>();
            var propertyDomainIncomingPropertiesIds = new Dictionary<int, HashSet<int>>();
            var propertyRangeOutgoingPropertiesIds = new Dictionary<int, HashSet<int>>();
            var propertyRangeIncomingPropertiesIds = new Dictionary<int, HashSet<int>>();

            foreach (var propertyId in _propertyIdDomainTypesDictionary.Select(x => x.Key))
            {
                propertyDomainOutgoingPropertiesIds[propertyId] = new HashSet<int>();
                propertyDomainIncomingPropertiesIds[propertyId] = new HashSet<int>();

                if (!_propertyIdDomainTypesDictionary.ContainsKey(propertyId)) continue;
                var domainIds = _propertyIdDomainTypesDictionary[propertyId];

                foreach (var domainId in domainIds)
                {
                    if (_typeIdDomainPropertiesDictionary.ContainsKey(domainId))
                    {
                        var domainProperties = _typeIdDomainPropertiesDictionary[domainId];
                        propertyDomainOutgoingPropertiesIds[propertyId].AddAll(domainProperties);
                    }

                    if (_typeIdRangePropertiesDictionary.ContainsKey(domainId))
                    {
                        var rangeProperties = _typeIdRangePropertiesDictionary[domainId];
                        propertyDomainIncomingPropertiesIds[propertyId].AddAll(rangeProperties);
                    }
                }
            }

            foreach (var propertyId in _propertyIdRangeTypesDictionary.Select(x => x.Key))
            {
                propertyRangeOutgoingPropertiesIds[propertyId] = new HashSet<int>();
                propertyRangeIncomingPropertiesIds[propertyId] = new HashSet<int>();

                if (!_propertyIdRangeTypesDictionary.ContainsKey(propertyId)) continue;

                var rangeIds = _propertyIdRangeTypesDictionary[propertyId];
                foreach (var rangeId in rangeIds)
                {
                    if (_typeIdDomainPropertiesDictionary.ContainsKey(rangeId))
                    {
                        var domainProperties = _typeIdDomainPropertiesDictionary[rangeId];
                        propertyRangeOutgoingPropertiesIds[propertyId].AddAll(domainProperties);
                    }

                    if (_typeIdRangePropertiesDictionary.ContainsKey(rangeId))
                    {
                        var rangeProperties = _typeIdRangePropertiesDictionary[rangeId];
                        propertyRangeIncomingPropertiesIds[propertyId].AddAll(rangeProperties);
                    }
                }
            }

            _propertyDomainOutgoingPropertiesIds = propertyDomainOutgoingPropertiesIds.ToArrayDictionary();
            _propertyDomainIncomingPropertiesIds = propertyDomainIncomingPropertiesIds.ToArrayDictionary();
            _propertyRangeOutgoingPropertiesIds = propertyRangeOutgoingPropertiesIds.ToArrayDictionary();
            _propertyRangeIncomingPropertiesIds = propertyRangeIncomingPropertiesIds.ToArrayDictionary();

            logger.Info($"InMemory Domain Range Query Engine Complete");
        }
    }
}

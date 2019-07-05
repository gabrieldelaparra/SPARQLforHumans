using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiDocumentQueries
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        public static IEnumerable<Entity> QueryEntitiesByLabel(string searchText, bool isType = false)
        {
            try
            {
                using (var luceneDirectory =
                    FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
                {
                    return QueryDocumentsByLabel(searchText, luceneDirectory, isType)?.Select(x => x.MapEntity());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public static IEnumerable<Entity> QueryEntitiesByIds(IEnumerable<string> searchIds)
        {
            try
            {
                using (var luceneDirectory =
                    FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
                {
                    return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapEntity());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public static IEnumerable<Property> QueryPropertiesByLabel(string searchText, bool isType = false)
        {
            try
            {
                using (var luceneDirectory =
                    FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
                {
                    return QueryDocumentsByLabel(searchText, luceneDirectory, isType)?.Select(x => x.MapProperty());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public static IEnumerable<Property> QueryPropertiesByIds(IEnumerable<string> searchIds)
        {
            try
            {
                using (var luceneDirectory =
                    FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
                {
                    return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapProperty());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public static IEnumerable<Entity> QueryEntitiesTopRankedResults(Directory luceneDirectory, bool isType = false)
        {
            return QueryDocumentsByLabel("*", luceneDirectory, isType)?.Select(x => x.MapEntity());
        }

        public static IEnumerable<Entity> QueryEntitiesByLabel(string searchText, Directory luceneDirectory,
            bool isType = false)
        {
            return QueryDocumentsByLabel(searchText, luceneDirectory, isType)?.Select(x => x.MapEntity());
        }

        public static IEnumerable<Entity> QueryEntitiesByIds(IEnumerable<string> searchIds,
            Directory luceneDirectory)
        {
            return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapEntity());
        }

        public static IEnumerable<Property> QueryPropertiesTopRankedResults(Directory luceneDirectory, bool isType = false)
        {
            return QueryDocumentsByLabel("*", luceneDirectory, isType)?.Select(x => x.MapProperty());
        }

        public static IEnumerable<Property> QueryPropertiesByLabel(string searchText, Directory luceneDirectory,
            bool isType = false)
        {
            return QueryDocumentsByLabel(searchText, luceneDirectory, isType)?.Select(x => x.MapProperty());
        }

        public static IEnumerable<Property> QueryPropertiesByIds(IEnumerable<string> searchIds,
            Directory luceneDirectory)
        {
            return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapProperty());
        }

        public static IEnumerable<Document> QueryDocumentsByIds(IEnumerable<string> searchIds,
            Directory luceneDirectory)
        {
            var documents = new List<Document>();

            // NotEmpty Validation
            if (searchIds == null)
            {
                return documents;
            }

            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                foreach (var searchText in searchIds)
                {
                    documents.Add(BaseParser.QueryDocumentByIdAndRank(searchText, searcher));
                }
            }

            return documents;
        }

        public static IEnumerable<Document> QueryDocumentsByLabel(string searchText, Directory luceneDirectory,
            bool isType, int resultsLimit = 20)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return new List<Document>();
            }

            var list = new List<Document>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(Regex.Replace(searchText, @"[^a-zA-Z0-9 - *]", string.Empty)))
            {
                return list;
            }

            searchText = BaseParser.PrepareSearchTerm(searchText);

            Filter filter = null;
            if (isType)
            {
                filter = new PrefixFilter(new Term(Labels.IsTypeEntity.ToString(), true.ToString()));
            }

            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                var parser = BaseParser.GetMultiFieldParser();
                list = SearchDocumentsByRank(searchText, searcher, parser, resultsLimit, filter);
            }

            return list;
        }

        private static List<Document> SearchDocumentsByRank(string searchText, IndexSearcher searcher,
            QueryParser parser, int resultsLimit, Filter filter = null)
        {
            var query = BaseParser.ParseQuery(searchText, parser);

            var hits = searcher.Search(query, filter, resultsLimit).ScoreDocs;

            foreach (var scoreDoc in hits)
            {
                //var explain = searcher.Explain(query, scoreDoc.Doc);
                var score = scoreDoc.Score;
                //var entity = searcher.Doc(scoreDoc.Doc).MapEntity();
            }

            return hits.Select(hit => searcher.Doc(hit.Doc)).ToList();
        }
    }
}
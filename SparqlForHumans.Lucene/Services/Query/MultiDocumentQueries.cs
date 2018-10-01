using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SparqlForHumans.Core.Utilities;
using SparqlForHumans.Models;

namespace SparqlForHumans.Core.Services
{
    public class MultiDocumentQueries
    {
        public static IEnumerable<Entity> QueryEntitiesByLabel(string searchText, bool isType = false)
        {
            using (var luceneDirectory = FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
                return QueryDocumentsByLabel(searchText, luceneDirectory, isType)?.Select(x => x.MapEntity());
        }

        public static IEnumerable<Entity> QueryEntitiesByIds(IEnumerable<string> searchIds)
        {
            using (var luceneDirectory = FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
                return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapEntity());
        }

        public static IEnumerable<Property> QueryPropertiesByLabel(string searchText, bool isType = false)
        {
            using (var luceneDirectory = FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
                return QueryDocumentsByLabel(searchText, luceneDirectory, isType)?.Select(x => x.MapProperty());
        }

        public static IEnumerable<Property> QueryPropertiesByIds(IEnumerable<string> searchIds)
        {
            using (var luceneDirectory = FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
                return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapProperty());
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
                return documents;

            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            using (var queryAnalyzer = new KeywordAnalyzer())
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                foreach (var searchText in searchIds)
                    documents.Add(BaseParser.QueryDocumentByIdAndRank(searchText, queryAnalyzer, searcher));
            }

            return documents;
        }

        public static IEnumerable<Document> QueryDocumentsByLabel(string searchText, Directory luceneDirectory,
            bool isType, int resultsLimit = 20)
        {
            if (string.IsNullOrEmpty(searchText))
                return new List<Document>();

            searchText = BaseParser.PrepareSearchTerm(searchText);

            var list = new List<Document>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchText.Replace("*", "").Replace("?", "")))
                return list;

            Filter filter = null;
            if (isType)
                filter = new PrefixFilter(new Term(Labels.IsTypeEntity.ToString(), "true"));

            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            using (var queryAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                list = SearchDocuments(searchText, queryAnalyzer, searcher, resultsLimit, filter);

                //queryAnalyzer.Close();
                //searcher.Dispose();
            }

            return list;
        }

        //TODO: Test Search Alt-Label
        //TODO: Test Search by Id
        //TODO: UI When searching by Person shows Human but can show Person and Alt-Labels as options
        //TODO: Some instances have more than one InstanceOf
        private static List<Document> SearchDocuments(string searchText, Analyzer queryAnalyzer, IndexSearcher searcher,
            int resultsLimit, Filter filter = null)
        {
            QueryParser parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48,
                new[] {Labels.Label.ToString(), Labels.AltLabel.ToString()},
                queryAnalyzer);

            return SearchDocumentsByRank(searchText, searcher, parser, resultsLimit, filter);
        }

        private static List<Document> SearchDocumentsByRank(string searchText, IndexSearcher searcher,
            QueryParser parser,
            int resultsLimit, Filter filter)
        {
            var sort = new Sort(SortField.FIELD_SCORE,
                new SortField(Labels.Rank.ToString(), SortFieldType.DOUBLE, true));

            var query = BaseParser.ParseQuery(searchText, parser);
            var hits = searcher.Search(query, filter, resultsLimit, sort).ScoreDocs;

            return hits.Select(hit => searcher.Doc(hit.Doc)).ToList();
        }
    }
}
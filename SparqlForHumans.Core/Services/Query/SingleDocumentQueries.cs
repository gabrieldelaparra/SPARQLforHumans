using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
using Version = Lucene.Net.Util.Version;

namespace SparqlForHumans.Core.Services
{
    public static class SingleDocumentQueries
    {
        public static Entity QueryEntityById(string searchId, Directory luceneIndexDirectory)
        {
            return QueryDocumentById(searchId, luceneIndexDirectory).MapEntity();
        }

        public static Property QueryPropertyById(string searchId, Directory luceneIndexDirectory)
        {
            return QueryDocumentById(searchId, luceneIndexDirectory).MapProperty();
        }

        public static Entity QueryEntityByLabel(string searchText, Directory luceneIndexDirectory)
        {
            return QueryDocumentByLabel(searchText, luceneIndexDirectory).MapEntity();
        }

        public static Property QueryPropertyByLabel(string searchText, Directory luceneIndexDirectory)
        {
            return QueryDocumentByLabel(searchText, luceneIndexDirectory).MapProperty();
        }

        public static Document QueryDocumentById(string searchId, Directory luceneIndexDirectory)
        {
            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchId))
                return null;

            var document = new Document();

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            using (var queryAnalyzer = new KeywordAnalyzer())
            {
                document = QueryDocumentByIdAndRank(searchId, queryAnalyzer, searcher);

                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return document;
        }
       
        public static Document QueryDocumentByLabel(string searchText, Directory luceneIndexDirectory)
        {
            if (string.IsNullOrEmpty(searchText))
                return null;

            searchText = QueriesParser.PrepareSearchTerm(searchText);

            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchText.Replace("*", "").Replace("?", "")))
                return null;

            var document = new Document();

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            using (var queryAnalyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                document = QueryDocumentByLabelAndRank(searchText, queryAnalyzer, searcher);

                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return document;
        }

        // Pass SingleFieldQuery(Id), for searching by Id. Returns results sorted by rank.
        internal static Document QueryDocumentByIdAndRank(string searchId, Analyzer queryAnalyzer, Searcher searcher)
        {
            var parser = new QueryParser(Version.LUCENE_30, Labels.Id.ToString(), queryAnalyzer);

            return QueryDocumentByRank(searchId, searcher, parser);
        }

        // Pass MultiFieldQuery(Label, AltLabel), for searching Labels. Returns results sorted by rank.
        private static Document QueryDocumentByLabelAndRank(string searchText, Analyzer queryAnalyzer, Searcher searcher)
        {
            QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30,
                new[] { Labels.Label.ToString(), Labels.AltLabel.ToString() },
                queryAnalyzer);

            return QueryDocumentByRank(searchText, searcher, parser);
        }

        //Does the search with the specific QueryParser (Label or Id). Returns results sorted by rank.
        private static Document QueryDocumentByRank(string searchText, Searcher searcher, QueryParser parser)
        {
            //Adds Sorting
            var sort = new Sort(SortField.FIELD_SCORE, new SortField(Labels.Rank.ToString(), SortField.DOUBLE, true));

            var query = QueriesParser.ParseQuery(searchText, parser);
            var hit = searcher.Search(query, null, 1, sort).ScoreDocs;

            if (hit == null || hit.Length.Equals(0))
                return null;

            return searcher.Doc(hit.FirstOrDefault().Doc);
        }
    }
}

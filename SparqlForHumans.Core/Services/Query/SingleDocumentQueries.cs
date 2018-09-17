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
                document = BaseParser.QueryDocumentByIdAndRank(searchId, queryAnalyzer, searcher);

                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return document;
        }
       
        public static Document QueryDocumentByLabel(string searchText, Directory luceneIndexDirectory)
        {
            if (string.IsNullOrEmpty(searchText))
                return null;

            searchText = BaseParser.PrepareSearchTerm(searchText);

            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchText.Replace("*", "").Replace("?", "")))
                return null;

            var document = new Document();

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            using (var queryAnalyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                document = BaseParser.QueryDocumentByLabelAndRank(searchText, queryAnalyzer, searcher);

                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return document;
        }


    }
}

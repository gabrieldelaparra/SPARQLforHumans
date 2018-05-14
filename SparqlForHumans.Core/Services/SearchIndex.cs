using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SparqlForHumans.Core.Services
{
    public static class SearchIndex
    {
        static Analyzer analyzer;
        static string indexPath = @"../LuceneIndex";

        static private Lucene.Net.Store.Directory luceneIndexDirectory;
        static public Lucene.Net.Store.Directory LuceneIndexDirectory
        {
            get
            {
                if (luceneIndexDirectory == null) luceneIndexDirectory = FSDirectory.Open(new DirectoryInfo(indexPath));
                if (IndexWriter.IsLocked(luceneIndexDirectory)) IndexWriter.Unlock(luceneIndexDirectory);
                var lockFilePath = Path.Combine(indexPath, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return luceneIndexDirectory;
            }
        }

        public static IEnumerable<LuceneQuery> SearchByLabel(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new List<LuceneQuery>();

            input = PrepareSearchTerm(input);

            return SearchLuceneLabels(input);
        }

        private static string PrepareSearchTerm(string input)
        {
            var terms = input.Trim()
                            .Replace("-", " ")
                            .Split(' ')
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Select(x => x.Trim() + "*");

            return string.Join(" ", terms); ;
        }

        public static IEnumerable<LuceneQuery> SearchLuceneTypeLabels(string typeName)
        {
            var searchField = "Name";
            var list = new List<LuceneQuery>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(typeName))
                return list;

            using (var searcher = new IndexSearcher(LuceneIndexDirectory, true))
            {
                var hitsLimit = 1;
                analyzer = new KeywordAnalyzer();

                QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchField, analyzer);

                var query = ParseQuery(typeName, parser);
                var hits = searcher.Search(query, null, hitsLimit).ScoreDocs;

                foreach (var hit in hits)
                {
                    var doc = searcher.Doc(hit.Doc);
                    var item = MapLuceneDocumentToData(doc);
                    list.Add(item);
                }

                analyzer.Close();
                searcher.Dispose();

                return list;
            }
        }

        private static IEnumerable<LuceneQuery> SearchLuceneLabels(string searchQuery)
        {
            var list = new List<LuceneQuery>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
                return list;

            using (var searcher = new IndexSearcher(LuceneIndexDirectory, true))
            {
                var hitsLimit = 20;
                analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

                QueryParser parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                                                                new[] { "Label", "AltLabel" },
                                                                analyzer);

                var query = ParseQuery(searchQuery, parser);
                var hits = searcher.Search(query, null, hitsLimit).ScoreDocs;

                foreach (var hit in hits)
                {
                    var doc = searcher.Doc(hit.Doc);
                    var item = MapLuceneDocumentToData(doc);
                    list.Add(item);
                }

                analyzer.Close();
                searcher.Dispose();

                return list;
            }
        }

        //private static IEnumerable<LuceneQuery> SearchLuceneLabels(string searchQuery, string searchField = "")
        //{
        //    var list = new List<LuceneQuery>();

        //    // NotEmpty Validation
        //    if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
        //        return list;

        //    using (var searcher = new IndexSearcher(LuceneIndexDirectory, true))
        //    {
        //        searcher.SetDefaultFieldSortScoring(true, true);

        //        var hitsLimit = 20;
        //        analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

        //        QueryParser parser;

        //        if (!string.IsNullOrEmpty(searchField))
        //        {
        //            parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchField, analyzer);
        //        }
        //        else
        //        {
        //            parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
        //                                                new[] { "Label", "Description" },
        //                                                analyzer);
        //        }

        //        var query = ParseQuery(searchQuery, parser);
        //        var hits = searcher.Search(query, null, hitsLimit, Sort.RELEVANCE).ScoreDocs;

        //        foreach (var hit in hits)
        //        {
        //            var doc = searcher.Doc(hit.Doc);
        //            var item = MapLuceneDocumentToData(doc);
        //            list.Add(item);
        //        }

        //        analyzer.Close();
        //        searcher.Dispose();

        //        return list;
        //    }
        //}

        private static IEnumerable<LuceneQuery> MapLuceneDocumentToData(IEnumerable<Document> documents)
        {
            foreach (var doc in documents)
            {
                yield return MapLuceneDocumentToData(doc);
            }
        }

        private static LuceneQuery MapLuceneDocumentToData(Document document)
        {
            return new LuceneQuery()
            {
                Name = document.Get("Name"),
                Type = document.Get("Type"),
                Label = document.Get("Label"),
                Description = document.Get("Description")
            };

        }

        private static Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }
    }
}

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using SparqlForHumans.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Core.Services
{
    public static class QueryService
    {
        public static int ResultsLimit { get; set; } = 20;

        static Analyzer analyzer;

        public static IEnumerable<LuceneQuery> QueryByLabel(string labelText)
        {
            if (string.IsNullOrEmpty(labelText))
                return new List<LuceneQuery>();

            labelText = PrepareSearchTerm(labelText);

            return QueryLabels(labelText);
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

        public static IEnumerable<LuceneQuery> GetTypeLabel(string typeName)
        {
            var resultLimit = 1;
            var searchField = "Name";
            var list = new List<LuceneQuery>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(typeName))
                return list;

            using (var searcher = new IndexSearcher(IndexProperties.LuceneIndexDirectory, true))
            {
                analyzer = new KeywordAnalyzer();

                QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchField, analyzer);

                var query = ParseQuery(typeName, parser);
                var hits = searcher.Search(query, null, resultLimit).ScoreDocs;

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

        private static IEnumerable<LuceneQuery> QueryLabels(string labelText)
        {
            var list = new List<LuceneQuery>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(labelText.Replace("*", "").Replace("?", "")))
                return list;

            using (var searcher = new IndexSearcher(IndexProperties.LuceneIndexDirectory, true))
            {
                analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

                QueryParser parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                                                                new[] { "Label", "AltLabel" },
                                                                analyzer);

                var query = ParseQuery(labelText, parser);
                var hits = searcher.Search(query, null, ResultsLimit).ScoreDocs;

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

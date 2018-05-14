﻿using Lucene.Net.Analysis;
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

        static Dictionary<string, string> typeLabels = new Dictionary<string, string>();
        static Dictionary<string, string> propertyLabels = new Dictionary<string, string>();

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
        
        private static IEnumerable<LuceneQuery> QueryLabels(string labelText)
        {
            var list = new List<LuceneQuery>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(labelText.Replace("*", "").Replace("?", "")))
                return list;

            using (var searcher = new IndexSearcher(Properties.Paths.LuceneIndexDirectory, true))
            {
                analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

                QueryParser parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                                                                new[] { Properties.Labels.Label.ToString(), Properties.Labels.AltLabel.ToString() },
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
                Name = document.Get(Properties.Labels.Name.ToString()),
                Type = document.Get(Properties.Labels.Type.ToString()),
                Label = document.Get(Properties.Labels.Label.ToString()),
                Properties = document.GetPropertiesFromIndex(),
                Description = document.Get(Properties.Labels.Description.ToString()),
                TypeLabel = GetTypeLabel(document.Get(Properties.Labels.Type.ToString())),
            };
        }

        public static string GetLabelFromIndex(string name)
        {
            var resultLimit = 1;
            var searchField = Properties.Labels.Name.ToString();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            using (var searcher = new IndexSearcher(Properties.Paths.LuceneIndexDirectory, true))
            {
                //Plain Keyword Analyzer:
                analyzer = new KeywordAnalyzer();

                QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchField, analyzer);

                var query = ParseQuery(name, parser);
                var hits = searcher.Search(query, null, resultLimit).ScoreDocs;

                var result = string.Empty;

                if (hits.Count() > 0)
                {
                    var doc = searcher.Doc(hits.FirstOrDefault().Doc);
                    result = doc.Get(Properties.Labels.Label.ToString());
                }

                analyzer.Close();
                searcher.Dispose();

                return result;
            }
        }

        public static IEnumerable<(string, string)> GetPropertiesFromIndex(this Document doc)
        {
            var list = new List<(string, string)>();

            foreach (var item in doc.GetValues(Properties.Labels.Property.ToString()))
            {
                var propertyLabel = GetProperty(item);
                list.Add((item, propertyLabel));
            }
            return list;
        }

        public static string GetProperty(string propertyCode)
        {
            if (propertyCode == null) return string.Empty;

            if (propertyLabels.ContainsKey(propertyCode))
            {
                return propertyLabels.FirstOrDefault(x => x.Key.Equals(propertyCode)).Value;
            }
            else
            {
                var label = GetLabelFromIndex(propertyCode);

                if (!string.IsNullOrWhiteSpace(label))
                    propertyLabels.Add(propertyCode, label);

                return label;
            }
        }

        public static string GetTypeLabel(string typeCode)
        {
            if (typeCode == null) return string.Empty;

            if (typeLabels.ContainsKey(typeCode))
            {
                return typeLabels.FirstOrDefault(x => x.Key.Equals(typeCode)).Value;
            }
            else
            {
                var label = GetLabelFromIndex(typeCode);
                if (!string.IsNullOrWhiteSpace(label))
                    typeLabels.Add(typeCode, label);

                return label;
            }
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

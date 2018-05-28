using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.Core.Services
{
    public static class QueryService
    {
        private static Analyzer analyzer;

        private static readonly Dictionary<string, string> typeLabels = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> propertyLabels = new Dictionary<string, string>();
        public static int ResultsLimit { get; set; } = 20;

        public static IEnumerable<Entity> QueryByLabel(string labelText)
        {
            return QueryByLabel(labelText, LuceneHelper.LuceneIndexDirectory);
        }


        public static IEnumerable<Entity> QueryByLabel(string labelText, Directory luceneIndexDirectory)
        {
            if (string.IsNullOrEmpty(labelText))
                return new List<Entity>();

            labelText = PrepareSearchTerm(labelText);

            return QueryLabels(labelText, luceneIndexDirectory);
        }

        public static string GetLabelFromIndex(string name, Directory luceneIndexDirectory)
        {
            var resultLimit = 1;
            var searchField = Labels.Id.ToString();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            {
                //Plain Keyword Analyzer:
                analyzer = new KeywordAnalyzer();

                var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);

                var query = ParseQuery(name, parser);
                var hits = searcher.Search(query, null, resultLimit).ScoreDocs;

                var result = string.Empty;

                if (hits.Any())
                {
                    var doc = searcher.Doc(hits.FirstOrDefault().Doc);
                    result = doc.Get(Labels.Label.ToString());
                }

                analyzer.Close();
                searcher.Dispose();

                return result;
            }
        }

        public static IEnumerable<Property> GetPropertiesFromIndex(this Document doc, Directory luceneIndexDirectory)
        {
            var list = new List<Property>();

            foreach (var item in doc.GetValues(Labels.Property.ToString()))
            {
                var propertyLabel = GetProperty(item, luceneIndexDirectory);
                list.Add(new Property
                {
                    Id = item,
                    Label = propertyLabel,
                    Value = string.Empty
                });
            }

            return list;
        }

        public static string GetProperty(string propertyCode, Directory luceneIndexDirectory)
        {
            if (propertyCode == null) return string.Empty;

            if (propertyLabels.ContainsKey(propertyCode))
                return propertyLabels.FirstOrDefault(x => x.Key.Equals(propertyCode)).Value;

            var label = GetLabelFromIndex(propertyCode, luceneIndexDirectory);

            if (!string.IsNullOrWhiteSpace(label))
                propertyLabels.Add(propertyCode, label);

            return label;
        }

        public static string GetTypeLabel(string typeCode, Directory luceneIndexDirectory)
        {
            if (typeCode == null) return string.Empty;

            if (typeLabels.ContainsKey(typeCode)) return typeLabels.FirstOrDefault(x => x.Key.Equals(typeCode)).Value;

            var label = GetLabelFromIndex(typeCode, luceneIndexDirectory);
            if (!string.IsNullOrWhiteSpace(label))
                typeLabels.Add(typeCode, label);

            return label;
        }

        private static string PrepareSearchTerm(string input)
        {
            var terms = input.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            return string.Join(" ", terms);
            ;
        }

        private static IEnumerable<Entity> QueryLabels(string labelText, Directory luceneIndexDirectory)
        {
            var list = new List<Entity>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(labelText.Replace("*", "").Replace("?", "")))
                return list;

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            {
                analyzer = new StandardAnalyzer(Version.LUCENE_30);

                QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30,
                    new[] {Labels.Label.ToString(), Labels.AltLabel.ToString()},
                    analyzer);

                var query = ParseQuery(labelText, parser);
                var hits = searcher.Search(query, null, ResultsLimit).ScoreDocs;

                foreach (var hit in hits)
                {
                    var doc = searcher.Doc(hit.Doc);
                    var item = MapLuceneDocumentToData(doc, luceneIndexDirectory);
                    list.Add(item);
                }

                analyzer.Close();
                searcher.Dispose();

                return list;
            }
        }

        private static IEnumerable<Entity> MapLuceneDocumentToData(IEnumerable<Document> documents,
            Directory luceneIndexDirectory)
        {
            foreach (var doc in documents) yield return MapLuceneDocumentToData(doc, luceneIndexDirectory);
        }

        public static IEnumerable<string> GetLabels(this Document doc)
        {
            var list = new List<string>();

            foreach (var item in doc.GetValues(Labels.Label.ToString()).Union(doc.GetValues(Labels.AltLabel.ToString()))
            ) list.Add(item);
            return list;
        }

        private static Entity MapLuceneDocumentToData(Document document, Directory luceneIndexDirectory)
        {
            return new Entity
            {
                Id = document.Get(Labels.Id.ToString()),
                InstanceOf = document.Get(Labels.InstanceOf.ToString()),
                AltLabels = document.GetLabels(),
                Properties = document.GetPropertiesFromIndex(luceneIndexDirectory),
                Description = document.Get(Labels.Description.ToString()),
                InstanceOfLabel = GetTypeLabel(document.Get(Labels.InstanceOf.ToString()), luceneIndexDirectory)
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
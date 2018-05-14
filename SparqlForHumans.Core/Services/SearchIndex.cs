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

        public static IEnumerable<LuceneQuery> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input)) return new List<LuceneQuery>();

            var terms = input.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }



        private static IEnumerable<LuceneQuery> _search(string searchQuery, string searchField = "")
        {
            var list = new List<LuceneQuery>();
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return list;

            using (var searcher = new IndexSearcher(LuceneIndexDirectory, true))
            {
                searcher.SetDefaultFieldSortScoring(true, true);

                var hits_limit = 20;
                var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

                QueryParser parser;

                if (!string.IsNullOrEmpty(searchField))
                {
                    parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchField, analyzer);
                }
                else
                {
                    parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                                                        new[] { "Label", "Description" },
                                                        analyzer);
                }

                var query = parseQuery(searchQuery, parser);
                var hits = searcher.Search(query, null, hits_limit, Sort.RELEVANCE).ScoreDocs;

                foreach (var hit in hits)
                {
                    var doc = searcher.Doc(hit.Doc);
                    var item = mapLuceneDocumentToData(doc);
                    list.Add(item);
                }

                analyzer.Close();
                searcher.Dispose();

                return list;
            }
        }

        private static IEnumerable<LuceneQuery> mapLuceneDocumentToData(IEnumerable<Document> documents)
        {
            foreach (var doc in documents)
            {
                yield return mapLuceneDocumentToData(doc);
            }
        }

        private static LuceneQuery mapLuceneDocumentToData(Document document)
        {
            return new LuceneQuery()
            {
                Name = document.Get("Name"),
                Type = document.Get("Type"),
                Label = document.Get("Label"),
                Description = document.Get("Description")
            };

        }

        private static Query parseQuery(string searchQuery, QueryParser parser)
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

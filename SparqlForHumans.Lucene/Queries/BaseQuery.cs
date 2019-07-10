using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Models;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries
{
    public abstract class BaseQuery<T> : IQuery
        where T : Subject
    {
        public BaseQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1)
        {
            LuceneIndexPath = luceneIndexPath;
            SearchStrings = searchStrings;
            ResultsLimit = resultsLimit;
            queryParser = QueryParser.GetQueryParser();
        }

        public BaseQuery(string luceneIndexPath, string searchString, int resultsLimit = 1)
        {
            LuceneIndexPath = luceneIndexPath;
            SearchStrings = new List<string> { searchString };
            ResultsLimit = resultsLimit;
            queryParser = QueryParser.GetQueryParser();
        }

        internal IEnumerable<string> SearchStrings { get; set; }
        public string LuceneIndexPath { get; set; }
        internal abstract IQueryParser QueryParser { get; }
        internal QueryParser queryParser { get; }
        internal int ResultsLimit { get; set; }
        internal virtual Filter Filter { get; set; } = null;
        public abstract List<T> Query();
        
        internal virtual bool IsInvalidSearchString(string inputString) => false;

        internal virtual string PrepareSearchTerm(string inputString) => inputString;

        internal virtual IReadOnlyList<Document> GetDocuments()
        {
            var list = new List<Document>();

            if(SearchStrings.All(IsInvalidSearchString)) 
                return list;

            using (var luceneDirectory = FSDirectory.Open(LuceneIndexPath))
            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);

                foreach (var searchString in SearchStrings)
                {
                    if(IsInvalidSearchString(searchString)) continue;
                    var preparedSearchTerm = PrepareSearchTerm(searchString);

                    var query = BaseParser.ParseQuery(preparedSearchTerm, queryParser);
                    var hits = searcher.Search(query, Filter, ResultsLimit).ScoreDocs;
                    list.AddRange(hits.Select(hit => searcher.Doc(hit.Doc)));
                }
            }
            return list;
        }
    }
}

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Queries.Base;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public interface IQuery
    {
        string LuceneIndexPath { get; set; }
        string SearchString { get; set; }
    }
    public class SingleIdQuery : BaseQuery
    {
        public SingleIdQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 1) { }

        public override IQueryParser QueryParser => new IdQueryParser();

        public override bool IsInvalidSearchString() => string.IsNullOrEmpty(SearchString);
    }

    public class MultiIdQuery : BaseQuery
    {
        public MultiIdQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        public override IQueryParser QueryParser => new IdQueryParser();
        public override bool IsInvalidSearchString() => string.IsNullOrEmpty(SearchString);
    }
    public class MultiLabelQuery : BaseQuery
    {
        public MultiLabelQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        public override IQueryParser QueryParser => new LabelsQueryParser();
        public override bool IsInvalidSearchString() => string.IsNullOrEmpty(Regex.Replace(SearchString, @"[^a-zA-Z0-9-*]", string.Empty));
        public override string PrepareSearchTerm() => BaseParser.PrepareSearchTerm(SearchString);
    }
    public class SingleLabelQuery : BaseQuery
    {
        public SingleLabelQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 1) { }

        public override IQueryParser QueryParser => new LabelsQueryParser();

        public override bool IsInvalidSearchString() => string.IsNullOrEmpty(Regex.Replace(SearchString, @"[^a-zA-Z0-9-*]", string.Empty));
        public override string PrepareSearchTerm() => BaseParser.PrepareSearchTerm(SearchString);
    }
    public abstract class BaseQuery : IQuery
    {
        public BaseQuery(string luceneIndexPath, string searchString, int resultsLimit = 1)
        {
            LuceneIndexPath = luceneIndexPath;
            SearchString = searchString;
            ResultsLimit = resultsLimit;
        }
        public string LuceneIndexPath { get; set; }
        public abstract IQueryParser QueryParser { get; }
        public int ResultsLimit { get; set; }
        public virtual Filter Filter { get; set; } = null;
        public string SearchString { get; set; }

        public virtual bool IsInvalidSearchString() => false;

        public virtual string PrepareSearchTerm() => SearchString;

        public virtual IReadOnlyList<Document> QueryDocuments()
        {
            if (IsInvalidSearchString())
                return new List<Document>();

            SearchString = PrepareSearchTerm();

            using (var luceneDirectory = FSDirectory.Open(LuceneIndexPath))
            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                var query = BaseParser.ParseQuery(SearchString, QueryParser.GetQueryParser());

                var hits = searcher.Search(query, Filter, ResultsLimit).ScoreDocs;

                return hits.Select(hit => searcher.Doc(hit.Doc)).ToList();
            }
        }
    }
}

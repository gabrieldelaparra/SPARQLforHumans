using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.Classic;
using SparqlForHumans.Models.LuceneIndex;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public abstract class BaseSingleFieldQueryParser<TAnalyzer> : IQueryParser<TAnalyzer>
        where TAnalyzer : Analyzer
    {
        public abstract Labels FieldLabel { get; }
        public abstract TAnalyzer Analyzer { get; }

        public virtual QueryParser GetQueryParser()
        {
            return new QueryParser(LuceneIndexDefaults.IndexVersion, FieldLabel.ToString(), Analyzer);
        }
    }
}
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.Classic;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public interface IQueryParser
    {
        QueryParser GetQueryParser();
    }
    public interface IQueryParser<TAnalyzer> : IQueryParser
        where TAnalyzer : Analyzer
    {
        TAnalyzer Analyzer { get; }
    }
}

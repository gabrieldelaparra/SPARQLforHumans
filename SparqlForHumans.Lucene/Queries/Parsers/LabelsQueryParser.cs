using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Models.LuceneIndex;

namespace SparqlForHumans.Lucene.Queries.Parsers
{
    public class LabelsQueryParser : IQueryParser<StandardAnalyzer>
    {
        public StandardAnalyzer Analyzer => new StandardAnalyzer(LuceneIndexDefaults.IndexVersion);

        public QueryParser GetQueryParser()
        {
            QueryParser parser = new MultiFieldQueryParser(
                LuceneIndexDefaults.IndexVersion,
                new[]
                {
                    Labels.Label.ToString(),
                    Labels.AltLabel.ToString()
                },
                Analyzer
            );

            parser.MultiTermRewriteMethod = new MultiTermQuery.TopTermsScoringBooleanQueryRewrite(int.MaxValue);
            parser.AllowLeadingWildcard = true;
            return parser;
        }
    }
}

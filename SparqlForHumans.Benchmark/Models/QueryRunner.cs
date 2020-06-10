using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Lucene.Queries.Graph;

namespace SparqlForHumans.Benchmark.Models {
    public abstract class QueryRunner
    {
        internal QueryGraph QueryGraph { get; set; }
        public abstract QueryBenchmark RunBenchmark();
    }
}
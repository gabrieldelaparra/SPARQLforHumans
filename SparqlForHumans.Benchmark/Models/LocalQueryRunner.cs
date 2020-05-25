using System.Diagnostics;
using System.Linq;
using SparqlForHumans.Lucene.Queries.Graph;

namespace SparqlForHumans.Benchmark.Models
{
    public class LocalQueryRunner : QueryRunner
    {
        public LocalQueryRunner(QueryGraph graph)
        {
            graph.ResetTraverse();
            QueryGraph = graph;
        }

        public override QueryBenchmark RunBenchmark()
        {
            var benchmark = new QueryBenchmark();
            var variables = QueryGraph.Edges.Select(x => x.Value).Where(x => !x.IsGivenType).Select(x => x.name).ToArray();
            benchmark.GraphQuery = QueryGraph.Nodes.First().Value.ToSparql(QueryGraph, variables).ToString().FixQuery();
            benchmark.GraphString = QueryGraph.ToString();
            benchmark.GraphHashCode = QueryGraph.GetHashCode();
            benchmark.BenchmarkType = "Local";

            // Begin timing
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //Query
            new QueryGraphResults().GetGraphQueryResults(QueryGraph, QueryGraph.EntitiesIndexPath, QueryGraph.PropertiesIndexPath, false, false);

            // Write result
            stopwatch.Stop();
            benchmark.ElapsedTime = stopwatch.Elapsed;

            //Set Results
            foreach (var edge in QueryGraph.Edges.Select(x => x.Value))
            {
                benchmark.ResultsDictionary.Add(edge.name, edge.Results.Select(x => x.Id).ToArray());
            }

            return benchmark;
        }
    }
}

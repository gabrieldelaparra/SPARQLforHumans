using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Wikidata.Services;
using VDS.RDF.Query;

namespace SparqlForHumans.Benchmark.Models {
    public class WikidataEndpointQueryRunner : QueryRunner
    {
        public WikidataEndpointQueryRunner(QueryGraph graph)
        {
            graph.ResetTraverse();
            QueryGraph = graph;
        }

        private Dictionary<string, string[]> GetEndpointResults(SparqlResultSet resultsSet)
        {
            var queryResultsGroup = resultsSet.Results.SelectMany(x => x).GroupBy(x => x.Key);
            var dictionary = new Dictionary<string, string[]>();

            foreach (var queryGroup in queryResultsGroup)
            {
                if (!queryGroup.Key.StartsWith("prop")) continue;

                var itemResults = queryGroup.Select(x => x.Value).Select(x => x.GetId()).Distinct().ToArray();
                dictionary.Add(queryGroup.Key, itemResults);
            }

            return dictionary;
        }

        public override QueryBenchmark RunBenchmark()
        {
            var benchmark = new QueryBenchmark();
            var variables = QueryGraph.Edges.Select(x => x.Value).Where(x => !x.IsGivenType).Select(x => x.name).ToArray();
            benchmark.GraphQuery = QueryGraph.Nodes.First().Value.ToSparql(QueryGraph, variables).ToString().FixQuery();
            benchmark.GraphString = QueryGraph.ToString();
            benchmark.GraphHashCode = QueryGraph.GetHashCode();
            benchmark.BenchmarkType = "Endpoint";

            // Begin timing
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //Query
            GraphApiQueries.QueryTimeoutMs = 50000; //50 secs
            var results = GraphApiQueries.RunQuery(benchmark.GraphQuery);

            // Write result
            stopwatch.Stop();
            benchmark.ElapsedTime = stopwatch.Elapsed;

            //Set Results
            if (results != null)
                benchmark.ResultsDictionary = GetEndpointResults(results);

            return benchmark;
        }
    }
}
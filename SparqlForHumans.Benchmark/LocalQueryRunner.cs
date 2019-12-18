using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Wikidata.Services;
using VDS.RDF.Query;

namespace SparqlForHumans.Benchmark
{

    public class QueryBenchmark
    {
        public TimeSpan ElapsedTime { get; set; }
        public int GraphHashCode { get; set; }
        public string GraphString { get; set; }
        public string GraphQuery { get; set; }
        public Dictionary<string, string[]> ResultsDictionary { get; set; } = new Dictionary<string, string[]>();
        public string BenchmarkType { get; set; }
        public virtual void Print()
        {
            if (GraphHashCode.Equals(0))
            {
                Console.WriteLine("No query was run");
                return;
            }
            Console.WriteLine();
            Console.WriteLine($"Results for {BenchmarkType}");
            Console.WriteLine($"Query: {GraphQuery}");
            Console.WriteLine($"Graph: {GraphString}");
            Console.WriteLine($"Hashcode: {GraphHashCode}");
            Console.WriteLine($"Time elapsed: {ElapsedTime}");
            foreach (var result in ResultsDictionary)
            {
                if (!result.Value.Any()) continue;
                Console.WriteLine($"Result ({result.Key}): {string.Join(",", result.Value)}");
            }
            Console.WriteLine("#######################################");
            Console.WriteLine();
        }
    }

    public abstract class QueryRunner
    {
        internal QueryGraph QueryGraph { get; set; }
        public abstract QueryBenchmark RunBenchmark();
    }

    public class LocalQueryRunner : QueryRunner
    {
        public LocalQueryRunner(QueryGraph graph)
        {
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
            QueryGraph.GetGraphQueryResults(QueryGraph.EntitiesIndexPath, QueryGraph.PropertiesIndexPath, false, false);

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

    public class WikidataEndpointQueryRunner : QueryRunner
    {
        public WikidataEndpointQueryRunner(QueryGraph graph)
        {
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
            benchmark.ResultsDictionary = GetEndpointResults(results);

            return benchmark;
        }
    }
}

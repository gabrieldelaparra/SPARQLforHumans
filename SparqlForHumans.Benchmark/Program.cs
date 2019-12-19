using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            RunQueries();
            //AnalyzeQueries();
            QueryPointByPoint();
            //Console.WriteLine("Press Enter to exit");
            //Console.ReadLine();
        }

        static void QueryPointByPoint()
        {
            var filename = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\benchmark.json";
            var benchmarkResults = JsonSerialization.DeserializeJson<List<QueryBenchmark>>(filename);
            var byHashcode = benchmarkResults.GroupBy(x => x.GraphHashCode);
            var results = new List<string>();
            results.Add($"QueryHashcode,Local,Remote");
            foreach (var benchmark in byHashcode) {
                if (benchmark.Count() != 2)
                    continue;
                var local = benchmark.First(x => x.BenchmarkType.Equals("Local"));
                var remote = benchmark.First(x => x.BenchmarkType.Equals("Endpoint"));
                results.Add($"{benchmark.Key},{local.ElapsedTime},{remote.ElapsedTime}");
            }
            File.WriteAllLines(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\points.csv", results);
        }

        static void AnalyzeQueries()
        {
            var filename = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\benchmark.json";
            var benchmarkResults = JsonSerialization.DeserializeJson<List<QueryBenchmark>>(filename);
            var localResults = benchmarkResults.Where(x => x.BenchmarkType.Equals("Local")).ToArray();
            var endpointResults = benchmarkResults.Where(x => x.BenchmarkType.Equals("Endpoint")).ToArray();

            var minLocal = localResults.Min(x => x.ElapsedTime);
            var maxLocal = localResults.Max(x => x.ElapsedTime);
            var countLocal = localResults.Count();
            var avgLocal = new TimeSpan(Convert.ToInt64(localResults.Average(x => x.ElapsedTime.Ticks)));
            var medianLocal = new TimeSpan(Convert.ToInt64(Median(localResults.Select(x => x.ElapsedTime.Ticks))));

            var minRemote = endpointResults.Min(x => x.ElapsedTime);
            var maxRemote = endpointResults.Max(x => x.ElapsedTime);
            var countRemote = endpointResults.Count();
            var avgRemote = new TimeSpan(Convert.ToInt64(endpointResults.Average(x => x.ElapsedTime.Ticks)));
            var medianRemote = new TimeSpan(Convert.ToInt64(Median(endpointResults.Select(x => x.ElapsedTime.Ticks))));

            var results = new List<string>();
            results.Add($"Local Count: {countLocal}");
            results.Add($"Local Min: {minLocal}");
            results.Add($"Local Max: {maxLocal}");
            results.Add($"Local Avg: {avgLocal}");
            results.Add($"Local Med: {medianLocal}");

            results.Add($"Remote Count: {countRemote}");
            results.Add($"Remote Min: {minRemote}");
            results.Add($"Remote Max: {maxRemote}");
            results.Add($"Remote Avg: {avgRemote}");
            results.Add($"Remote Med: {medianRemote}");

            File.WriteAllLines(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\results.txt", results);
        }

        public static T Median<T>(IEnumerable<T> items)
        {
            var i = (int)Math.Ceiling((double)(items.Count() - 1) / 2);
            if (i >= 0)
            {
                var values = items.ToList();
                values.Sort();
                return values[i];
            }

            return default(T);
        }

        static void RunQueries()
        {
            InMemoryQueryEngine.Init(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath);
            var queryBenchmarks = new List<QueryBenchmark>();
            var path = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\";
            var files = Directory.EnumerateFiles(path);
            foreach (var file in files) {
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
                Console.WriteLine($"File: {file}");
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
                var text = File.ReadAllText(file);
                var converter = new SparqlToGraphConverter();
                var rdfGraph = converter.ConvertToGraph(text);

                var graphs = rdfGraph.GetGraphVariations();

                foreach (var rdfExplorerGraph in graphs)
                {
                    Console.WriteLine("####################################################################################");
                    var graph = new QueryGraph(rdfExplorerGraph)
                    {
                        EntitiesIndexPath = LuceneDirectoryDefaults.EntityIndexPath,
                        PropertiesIndexPath = LuceneDirectoryDefaults.PropertyIndexPath
                    };

                    //Do not run if there are no variables to query.
                    if (graph.Edges.Select(x => x.Value).All(x => x.IsGivenType)) continue;

                    var endpoint = new WikidataEndpointQueryRunner(graph).RunBenchmark();
                    endpoint.Print();
                    var local = new LocalQueryRunner(graph).RunBenchmark();
                    local.Print();
                    queryBenchmarks.Add(endpoint);
                    queryBenchmarks.Add(local);
                }
            }

            queryBenchmarks.SerializeJson(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\benchmark.json");

            foreach (var benchmark in queryBenchmarks)
            {
                benchmark.Print();
            }
        }

    }
}

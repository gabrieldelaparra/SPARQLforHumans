using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SparqlForHumans.Benchmark.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Benchmark
{
    public static class BenchmarkAnalysis
    {
        public static void LogPointByPoint()
        {
            var filename = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\benchmark.json";
            var benchmarkResults = JsonSerialization.DeserializeJson<List<QueryBenchmark>>(filename);
            var byHashcode = benchmarkResults.GroupBy(x => x.GraphHashCode);
            var results = new List<string>();
            results.Add($"QueryHashcode,Local,Remote");
            foreach (var benchmark in byHashcode)
            {
                if (benchmark.Count() != 2)
                    continue;
                var local = benchmark.First(x => x.BenchmarkType.Equals("Local"));
                var remote = benchmark.First(x => x.BenchmarkType.Equals("Endpoint"));
                results.Add($"{benchmark.Key},{local.ElapsedTime},{remote.ElapsedTime}");
            }
            File.WriteAllLines(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\points.csv", results);
        }

        public static void LogBaseMetrics()
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
    }
}

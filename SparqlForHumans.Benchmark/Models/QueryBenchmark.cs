using System;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Benchmark.Models {
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
}
using System;
using System.Collections.Generic;

namespace SparqlForHumans.Benchmark.Models.Results
{
    public class BenchmarkResult
    {
        public BenchmarkResult(TimeSpan timeSpan, List<string> values)
        {
            TimeSpan = timeSpan;
            Values = values;
        }

        public TimeSpan TimeSpan { get; set; }
        public List<string> Values { get; set; }

        public override string ToString()
        {
            return $"Results: {Values.Count} results in {TimeSpan}";
        }
    }
}

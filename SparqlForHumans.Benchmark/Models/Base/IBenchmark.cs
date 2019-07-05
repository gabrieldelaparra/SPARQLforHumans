using SparqlForHumans.Benchmark.Models.Results;
using System.Collections.Generic;

namespace SparqlForHumans.Benchmark.Models.Base
{
    interface IBenchmark
    {
        List<string> RunQuery();
        BenchmarkResult RunBenchmark();
    }
}

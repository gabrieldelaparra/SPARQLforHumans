using SparqlForHumans.Benchmark.Models.Results;
using System;
using System.Collections.Generic;

namespace SparqlForHumans.Benchmark.Models.Base
{
    public abstract class AbstractBenchmark : IBenchmark
    {
        public static NLog.Logger Logger { get; } = SparqlForHumans.Logger.Logger.Init();

        public virtual BenchmarkResult RunBenchmark()
        {
            //Start Watch
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Run the query
            var results = RunQuery();

            //Stop Watch
            watch.Stop();

            // Format 00:00:02.0001008
            return new BenchmarkResult(watch.Elapsed, results);
            //Logger.Info($"Query Runtime: {watch.Elapsed}");
        }

        public abstract List<string> RunQuery();
    }
}

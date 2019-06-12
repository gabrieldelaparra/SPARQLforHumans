namespace SparqlForHumans.Benchmark.Models
{
    public abstract class AbstractBenchmark : IBenchmark
    {
        public static NLog.Logger Logger { get; } = SparqlForHumans.Logger.Logger.Init();

        public virtual void RunBenchmark()
        {
            //Start Watch
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Run the query
            RunQuery();

            //Stop Watch
            watch.Stop();

            // Format 00:00:02.0001008
            Logger.Info($"Query Runtime: {watch.Elapsed}");
        }

        public abstract void RunQuery();
    }
}

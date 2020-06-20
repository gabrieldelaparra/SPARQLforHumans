using System;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.RunQueries();
            BenchmarkRunner.RunQueriesFromProperties();
            BenchmarkAnalysis.LogBaseMetrics();
            BenchmarkAnalysis.LogPointByPoint();

            BenchmarkAnalysis.DoTheAverageRecallF1Thing();

            //Console.ReadLine();
            //PropertiesDistribution.CreatePropertiesDistribution();
            Console.WriteLine("Finished");
        }

        

    }
}

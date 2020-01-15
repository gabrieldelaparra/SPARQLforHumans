using System;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.RunQueries();
            BenchmarkRunner.RunQueriesFromProperties();
            BenchmarkRunner.LogBaseMetrics();
            BenchmarkRunner.LogPointByPoint();
            Console.WriteLine("Press Enter to exit");
            //Console.ReadLine();
        }

        

    }
}

using System;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.RunQueries();
            BenchmarkRunner.RunDomainRangeQueries();
            BenchmarkRunner.AnalyzeQueries();
            BenchmarkRunner.QueryPointByPoint();
            Console.WriteLine("Press Enter to exit");
            //Console.ReadLine();
        }

        

    }
}

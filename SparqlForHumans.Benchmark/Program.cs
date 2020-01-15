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
            //BenchmarkRunner.RunQueries();
            BenchmarkRunner.RunDomainRangeQueries();
            BenchmarkRunner.AnalyzeQueries();
            BenchmarkRunner.QueryPointByPoint();
            //Console.WriteLine("Press Enter to exit");
            //Console.ReadLine();
        }

        

    }
}

using System;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //var query = @"SELECT DISTINCT ?item 
            //                WHERE { 
            //                    ?item wdt:P31 wd:Q39715 ; 
            //                    wdt:P17 wd:Q20 ; 
            //                }";
            //var wikidataBenchmark = new WikidataEndpointBenchmark(query);
            //var results = wikidataBenchmark.RunBenchmark();
            //Console.WriteLine(results);


            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

    }
}

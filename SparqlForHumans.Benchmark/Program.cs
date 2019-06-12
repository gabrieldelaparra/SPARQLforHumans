using System;
using SparqlForHumans.Benchmark.Models;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var query = @"SELECT DISTINCT ?item 
                            WHERE { 
                                ?item wdt:P31 wd:Q39715 ; 
                                wdt:P17 wd:Q20 ; 
                            }";
            new WikidataEndpointBenchmark(query);
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

    }
}

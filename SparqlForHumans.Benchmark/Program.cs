using System;
using System.Linq;
using SparqlForHumans.Benchmark.Sources;
using SparqlForHumans.Lucene.Indexing.Relations;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;

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

            

            string filename = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.UnitTests\Resources\Filter5k.nt";
            var mapper = new BoostMapper(filename);

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

    }
}

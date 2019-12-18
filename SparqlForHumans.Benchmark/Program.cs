using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var queryText = @"SELECT DISTINCT ?item 
                            WHERE { 
                                ?item wdt:P31 wd:Q39715 ; 
                                wdt:P17 wd:Q20 ; 
                            }";
            var converter = new SparqlToGraphConverter();
            var rdfGraph = converter.ConvertToGraph(queryText);
            

            var graph = new QueryGraph(rdfGraph);

            //var subjects = triples.Select(x => x.Subject);
            //var predicates = triples.Select(x => x.Predicate);
            //var objects = triples.Select(x => x.Object);

            //var nodes = subjects.Union(objects).Select(ToNode);

            //var nodes = triples.Select(x=>x.)
            //var wikidataBenchmark = new WikidataEndpointBenchmark(query);
            //var results = wikidataBenchmark.RunBenchmark();
            //Console.WriteLine(results);


            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
        
    }
}

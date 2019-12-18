using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;

namespace SparqlForHumans.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            InMemoryQueryEngine.Init(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath);
            var queryBenchmarks = new List<QueryBenchmark>();

            var queryText = @"SELECT DISTINCT ?item 
                            WHERE { 
                                ?item wdt:P31 wd:Q39715 ; 
                                wdt:P17 wd:Q20 ; 
                            }";

            var converter = new SparqlToGraphConverter();
            var rdfGraph = converter.ConvertToGraph(queryText);

            var graphs = rdfGraph.GetGraphVariations();

            foreach (var rdfExplorerGraph in graphs)
            {
                var graph = new QueryGraph(rdfExplorerGraph)
                {
                    EntitiesIndexPath = LuceneDirectoryDefaults.EntityIndexPath,
                    PropertiesIndexPath = LuceneDirectoryDefaults.PropertyIndexPath
                };

                //Do not run if there are no variables to query.
                if (graph.Edges.Select(x => x.Value).Any(x => !x.IsGivenType)) continue;

                var local = new LocalQueryRunner(graph).RunBenchmark();
                var endpoint = new WikidataEndpointQueryRunner(graph).RunBenchmark();
                queryBenchmarks.Add(local);
                queryBenchmarks.Add(endpoint);
            }

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SparqlForHumans.Benchmark.Models;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Benchmark
{
    public static class BenchmarkRunner
    {
        public static void RunQueriesFromProperties()
        {
            InMemoryQueryEngine.Init(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath);
            var queryBenchmarks = new List<QueryBenchmark>();

            var propertiesList = new List<string> {
                "P17",
                "P19",
                "P21",
                "P31",
                "P50",
                "P102",
                "P106",
                "P112",
                "P127",
                "P131",
                "P135",
                "P138",
                "P150",
                "P155",
                "P156",
                "P159",
                "P180",
                "P184",
                "P276",
                "P279",
                "P287",
                "P301",
                "P361",
                "P364",
                "P413",
                "P421",
                "P511",
                "P527",
                "P531",
                "P611",
                "P641",
                "P682",
                "P710",
                "P793",
                "P805",
                "P807",
                "P873",
                "P910",
                "P915",
                "P921",
                "P971",
                "P1204",
                "P1344",
                "P1423",
                "P1424",
                "P1433",
                "P1435",
                "P1445",
                "P1464",
                "P1478",
                "P1889",
                "P2156",
                "P2239",
                "P2522",
                "P2614",
                "P2860",
                "P2894",
                "P3092",
                "P3096",
                "P3259",
                "P3919",
                "P3985",
                "P4000",
                "P4224",
                "P4599",
                "P4809",
                "P4878",
            };

            foreach (var property in propertiesList)
            {
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
                Console.WriteLine($"Property: {property}");
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();

                var text1 = $"SELECT * WHERE {{?v1 wdt:{property} ?v2 . ?v1 ?p1 ?v3 . }} ";
                var converter = new SparqlToGraphConverter();
                var rdfGraph1 = converter.ConvertToGraph(text1);


                Console.WriteLine("####################################################################################");
                var graph = new QueryGraph(rdfGraph1)
                {
                    EntitiesIndexPath = LuceneDirectoryDefaults.EntityIndexPath,
                    PropertiesIndexPath = LuceneDirectoryDefaults.PropertyIndexPath
                };

                //Do not run if there are no variables to query.
                if (graph.Edges.Select(x => x.Value).All(x => x.IsConstant)) continue;

                var endpoint = new WikidataEndpointQueryRunner(graph).RunBenchmark();
                endpoint.Print();
                var local = new LocalQueryRunner(graph).RunBenchmark();
                local.Print();
                queryBenchmarks.Add(endpoint);
                queryBenchmarks.Add(local);

                var text2 = $"SELECT * WHERE {{?v1 wdt:{property} ?v2 . ?v2 ?p1 ?v3 . }} ";
                converter = new SparqlToGraphConverter();
                var rdfGraph2 = converter.ConvertToGraph(text2);

                Console.WriteLine("####################################################################################");
                graph = new QueryGraph(rdfGraph2)
                {
                    EntitiesIndexPath = LuceneDirectoryDefaults.EntityIndexPath,
                    PropertiesIndexPath = LuceneDirectoryDefaults.PropertyIndexPath
                };

                //Do not run if there are no variables to query.
                if (graph.Edges.Select(x => x.Value).All(x => x.IsConstant)) continue;

                endpoint = new WikidataEndpointQueryRunner(graph).RunBenchmark();
                endpoint.Print();
                local = new LocalQueryRunner(graph).RunBenchmark();
                local.Print();
                queryBenchmarks.Add(endpoint);
                queryBenchmarks.Add(local);

                var text3 = $"SELECT * WHERE {{?v1 wdt:{property} ?v2 . ?v3 ?p1 ?v1 . }} ";
                converter = new SparqlToGraphConverter();
                var rdfGraph3 = converter.ConvertToGraph(text3);

                Console.WriteLine("####################################################################################");
                graph = new QueryGraph(rdfGraph3)
                {
                    EntitiesIndexPath = LuceneDirectoryDefaults.EntityIndexPath,
                    PropertiesIndexPath = LuceneDirectoryDefaults.PropertyIndexPath
                };

                //Do not run if there are no variables to query.
                if (graph.Edges.Select(x => x.Value).All(x => x.IsConstant)) continue;

                endpoint = new WikidataEndpointQueryRunner(graph).RunBenchmark();
                endpoint.Print();
                local = new LocalQueryRunner(graph).RunBenchmark();
                local.Print();
                queryBenchmarks.Add(endpoint);
                queryBenchmarks.Add(local);

                var text4 = $"SELECT * WHERE {{?v1 wdt:{property} ?v2 . ?v3 ?p1 ?v2 . }} ";
                converter = new SparqlToGraphConverter();
                var rdfGraph4 = converter.ConvertToGraph(text4);

                Console.WriteLine("####################################################################################");
                graph = new QueryGraph(rdfGraph4)
                {
                    EntitiesIndexPath = LuceneDirectoryDefaults.EntityIndexPath,
                    PropertiesIndexPath = LuceneDirectoryDefaults.PropertyIndexPath
                };

                //Do not run if there are no variables to query.
                if (graph.Edges.Select(x => x.Value).All(x => x.IsConstant)) continue;

                endpoint = new WikidataEndpointQueryRunner(graph).RunBenchmark();
                endpoint.Print();
                local = new LocalQueryRunner(graph).RunBenchmark();
                local.Print();
                queryBenchmarks.Add(endpoint);
                queryBenchmarks.Add(local);
            }

            queryBenchmarks.SerializeJson(@"benchmark.json");

            foreach (var benchmark in queryBenchmarks)
            {
                benchmark.Print();
            }
        }

        public static void RunQueriesFromFiles()
        {
            InMemoryQueryEngine.Init(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath);
            var queryBenchmarks = new List<QueryBenchmark>();
            var path = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.Benchmark\Queries\";
            var files = Directory.EnumerateFiles(path, "*.sparql");
            foreach (var file in files)
            {
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
                Console.WriteLine($"File: {file}");
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
                var text = File.ReadAllText(file);
                var converter = new SparqlToGraphConverter();
                var rdfGraph = converter.ConvertToGraph(text);

                var graphs = rdfGraph.GetGraphVariations();

                foreach (var rdfExplorerGraph in graphs)
                {
                    Console.WriteLine("####################################################################################");
                    var graph = new QueryGraph(rdfExplorerGraph)
                    {
                        EntitiesIndexPath = LuceneDirectoryDefaults.EntityIndexPath,
                        PropertiesIndexPath = LuceneDirectoryDefaults.PropertyIndexPath
                    };

                    //Do not run if there are no variables to query.
                    if (graph.Edges.Select(x => x.Value).All(x => x.IsConstant)) continue;

                    var endpoint = new WikidataEndpointQueryRunner(graph).RunBenchmark();
                    endpoint.Print();
                    var local = new LocalQueryRunner(graph).RunBenchmark();
                    local.Print();
                    queryBenchmarks.Add(endpoint);
                    queryBenchmarks.Add(local);
                }
            }

            queryBenchmarks.SerializeJson(@"benchmark.json");

            foreach (var benchmark in queryBenchmarks)
            {
                benchmark.Print();
            }
        }
    }
}

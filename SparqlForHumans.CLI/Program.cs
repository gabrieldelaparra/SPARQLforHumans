using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.RDF.Filtering;
using SparqlForHumans.Utilities;
using System;
using SparqlForHumans.Lucene.Index;
using VDS.RDF;

namespace SparqlForHumans.CLI
{
    internal class Program
    {
        //private static bool keepRunning = true;

        private static void Main(string[] args)
        {
            Options.InternUris = false;
            //FilterAll();
            //Filter5k();
            //Filter500k();
            //CreateIndex("filtered-All-5k.nt", true);
            //var cli = new CommandLineInterface();
            //Filter500();
            ////Filter500k();
            //Filter2MM();
            //CreateIndex("filtered-All-5k.nt", true);
            //CreateIndex("filtered-All-2MM.nt", true);
            //CreateIndex("filtered-All-500k.nt", true);
            // CreateIndex(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.CLI\bin\Debug\netcoreapp2.1\filtered-All.nt", true);
            QueryEntities("obam");
            QueryEntities("hum");
            QueryEntities("person");
            QueryEntities("city");
            QueryEntities("michelle obama");
            QueryProperties("city");

            //Console.WriteLine(dictionary.Count);
            Console.Read();

            //TestBuilderHelper.GetFirst20ObamaTriplesGroups();

            //Filter5k();
            //FilterAll();
            //Filter2MM();
            //CreateIndex2MM(true);
            //CreatePropertyIndex(true);
            //IndexBuilder.CreateTypesIndex();
        }

        public static void CreateIndex(string filename, bool overwrite = false)
        {
            var entitiesOutputPath = LuceneDirectoryDefaults.EntityIndexPath;
            var propertyOutputPath = LuceneDirectoryDefaults.PropertyIndexPath;

            entitiesOutputPath.DeleteIfExists(overwrite);
            propertyOutputPath.DeleteIfExists(overwrite);

            new EntitiesIndexer(filename, LuceneDirectoryDefaults.EntityIndexPath).Index();
            new PropertiesIndexer(filename, LuceneDirectoryDefaults.PropertyIndexPath).Index();

            //EntitiesIndex.CreateEntitiesIndex(filename, true);

            //var typesAndPropertiesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary();
            ////EntitiesIndex.AddIsTypeEntityToEntitiesIndex(typesAndPropertiesDictionary);

            //PropertiesIndex.CreatePropertiesIndex(filename, true);

            //var invertedPropertiesDictionary = typesAndPropertiesDictionary.InvertDictionary();
            //PropertiesIndex.AddDomainTypesToPropertiesIndex(invertedPropertiesDictionary);
        }

        private static void Filter2MM()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All-2MM.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 2000000);
        }

        private static void Filter500k()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputFilename = "filtered-All-500k.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 500000);
        }

        private static void Filter5k()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputFilename = "filtered-All-5k.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 5000);
        }

        private static void Filter500()
        {
            var inputFilename = @"filtered-All-5k.nt";
            var outputFilename = "filtered-All-500.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 500);
        }

        private static void FilterAll()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, -1);
        }

        private static void QueryEntities(string query)
        {
            Console.WriteLine($"Query Entity: {query}\n");
            var results = MultiDocumentQueries.QueryEntitiesByLabel(query);
            foreach (var result in results)
            {
                Console.WriteLine(result.ToRankedString());
            }
        }

        private static void QueryProperties(string query)
        {
            Console.WriteLine($"Query Property: {query}\n");
            var results = MultiDocumentQueries.QueryPropertiesByLabel(query);
            foreach (var result in results)
            {
                Console.WriteLine(result.ToRankedString());
            }
        }
    }
}
using System;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.RDF.Filtering;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.CLI
{
    internal class Program
    {
        //private static bool keepRunning = true;

        private static void Main(string[] args)
        {
            //FilterAll();
            //Filter5k();
            //Filter500k();
            //CreateIndex("filtered-All-5k.nt", true);
            //CreateIndex("filtered-All.nt", true);
            //CreateIndex("filtered-All-500k.nt", true);
            QueryEntities("obama");
            QueryProperties("city");
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
            var entitiesOutputPath = LuceneIndexExtensions.EntityIndexPath;
            var propertyOutputPath = LuceneIndexExtensions.PropertyIndexPath;

            entitiesOutputPath.DeleteIfExists(overwrite);
            propertyOutputPath.DeleteIfExists(overwrite);

            EntitiesIndex.CreateEntitiesIndex(filename, true);

            var typesAndPropertiesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary();
            EntitiesIndex.AddIsTypeEntityToEntitiesIndex(typesAndPropertiesDictionary);

            PropertiesIndex.CreatePropertiesIndex(filename, true);

            var invertedPropertiesDictionary = IndexBuilder.CreateInvertedProperties(typesAndPropertiesDictionary);
            PropertiesIndex.AddDomainTypesToPropertiesIndex(invertedPropertiesDictionary);
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

        private static void FilterAll()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, -1);
        }

        private static void CreateIndex2k(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2k.nt";
            var outputPath = "Index2k";

            outputPath.DeleteIfExists(overwrite);

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(inputFilename, luceneDirectory, true);
            }
        }

        private static void CreateIndex2MM(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneIndexExtensions.EntityIndexPath;

            outputPath.DeleteIfExists(overwrite);
            EntitiesIndex.CreateEntitiesIndex(inputFilename, true);
        }

        private static void CreatePropertyIndex(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneIndexExtensions.PropertyIndexPath;

            outputPath.DeleteIfExists(overwrite);
            PropertiesIndex.CreatePropertiesIndex(inputFilename, true);
        }

        private static void QueryEntities(string query)
        {
            Console.WriteLine($"Query Entity: {query}\n");
            var results = MultiDocumentQueries.QueryEntitiesByLabel(query);
            foreach (var result in results)
                Console.WriteLine(result.ToRankedString());
        }

        private static void QueryProperties(string query)
        {
            Console.WriteLine($"Query Property: {query}\n");
            var results = MultiDocumentQueries.QueryPropertiesByLabel(query);
            foreach (var result in results)
                Console.WriteLine(result.ToRankedString());
        }
    }
}
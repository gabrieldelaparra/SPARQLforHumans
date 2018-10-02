using System;
using System.IO;
using Lucene.Net.Store;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.CLI
{
    class Program
    {
        //private static bool keepRunning = true;

        static void Main(string[] args)
        {
            //Filter500k();
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

            IndexBuilder.CreateEntitiesIndex(filename, true);

            var typesAndPropertiesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary();
            IndexBuilder.CreatePropertiesIndex(filename, true);

            var invertedPropertiesDictionary = IndexBuilder.CreateInvertedProperties(typesAndPropertiesDictionary);
            IndexBuilder.AddDomainTypesToPropertiesIndex(invertedPropertiesDictionary);
        }

        static void Filter2MM()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All-2MM.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 2000000);
        }

        static void Filter500k()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputFilename = "filtered-All-500k.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 500000);
        }

        static void Filter5k()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputFilename = "filtered-All-5k.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 5000);
        }

        static void FilterAll()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, -1);
        }

        static void CreateIndex2k(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2k.nt";
            var outputPath = "Index2k";

            outputPath.DeleteIfExists(overwrite);

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
                IndexBuilder.CreateEntitiesIndex(inputFilename, luceneDirectory, true);

        }

        static void CreateIndex2MM(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneIndexExtensions.EntityIndexPath;

            outputPath.DeleteIfExists(overwrite);
            IndexBuilder.CreateEntitiesIndex(inputFilename, true);

        }

        static void CreatePropertyIndex(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneIndexExtensions.PropertyIndexPath;

            outputPath.DeleteIfExists(overwrite);
            IndexBuilder.CreatePropertiesIndex(inputFilename, true);
        }

        static void QueryEntities(string query)
        {
            Console.WriteLine($"Query Entity: {query}\n");
            var results = MultiDocumentQueries.QueryEntitiesByLabel(query);
            foreach (var result in results)
            {
                Console.WriteLine(result.ToRankedString());
            }
        }

        static void QueryProperties(string query)
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

using System;
using SparqlForHumans.Core.Services;
using System.IO;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.CLI
{
    class Program
    {
        //private static bool keepRunning = true;

        static void Main(string[] args)
        {
            //Filter5k();
            //FilterAll();
            //Filter2MM();
            //CreateIndex2MM(true);
            //CreatePropertyIndex();
            QueryEntities("obama");
        }

        static void Filter2MM()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All-2MM.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 2000000);
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

            if (Directory.Exists(outputPath) && overwrite)
                Directory.Delete(outputPath, true);

            IndexBuilder.CreateEntitiesIndex(inputFilename, outputPath);

        }

        static void CreateIndex2MM(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneIndexExtensions.IndexPath;

            if (Directory.Exists(outputPath) && overwrite)
                Directory.Delete(outputPath, true);

            IndexBuilder.CreateEntitiesIndex(inputFilename, outputPath, true);

        }

        static void CreatePropertyIndex(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneIndexExtensions.IndexPath;

            if (Directory.Exists(outputPath) && overwrite)
            Directory.Delete(outputPath, true);

            IndexBuilder.CreatePropertyIndex(inputFilename, outputPath, true);
        }

        static void QueryEntities(string query)
        {
            Console.WriteLine(query);
            var results = QueryService.QueryEntitiesByLabel(query);
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }

            Console.ReadLine();
        }
    }
}

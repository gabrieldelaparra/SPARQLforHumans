using System;
using SparqlForHumans.Core.Services;
using System.IO;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.CLI
{
    class Program
    {
        private static bool keepRunning = true;

        static void Main(string[] args)
        {
            //Filter2MM();
            //FilterAll();
            CreateIndex2MM(true);
            CreatePropertyIndex(false);
        }

        static void Filter2MM()
        {
            var inputFilename = @"C:\Users\delapa\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All-2MM.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 2000000);
        }

        static void FilterAll()
        {
            var inputFilename = @"C:\Users\delapa\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, -1);
        }

        static void CreateIndex2MM(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneHelper.IndexPath;

            if (Directory.Exists(outputPath) && overwrite)
                Directory.Delete(outputPath, true);

            IndexBuilder.CreateEntitiesIndex(inputFilename, outputPath);

        }

        static void CreatePropertyIndex(bool overwrite = false)
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = LuceneHelper.IndexPath;

            if (Directory.Exists(outputPath) && overwrite)
            Directory.Delete(outputPath, true);

            IndexBuilder.CreatePropertyIndex(inputFilename, outputPath, true);
        }

        
    }
}

using SparqlForHumans.Core.Services;
using System.IO;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            //Filter2MM();
            //FilterAll();
            //CreateIndex2MM();
            CreatePropertyIndex();
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

        static void CreateIndex2MM()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = "Index2MM";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            IndexBuilder.CreateEntitiesIndex(inputFilename, outputPath);

        }

        static void CreatePropertyIndex()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = "PropertyIndex";
            IndexBuilder.CreatePropertyIndex(inputFilename, outputPath, true);
        }
    }
}

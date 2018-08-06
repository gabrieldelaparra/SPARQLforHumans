using SparqlForHumans.Core.Services;
using System.IO;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            //var inputFilename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt";
            //
            //var inputFilename = @"C:\Users\delapa\Desktop\DCC\SparQLforHumans.Dataset\filtered-All.nt.gz";
            //var inputFilename = @"filtered-All-2MM.nt";
            //var inputFilename = @"Out-filtered-All-2MM.nt";
            //
            //var outputFilename = "filtered-All.nt";
            //var outputPath = "IndexFull";

            //

            var inputFilename = @"filtered-All-2MM.nt";
            var outputPath = "Index2MM";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            IndexBuilder.CreateEntitiesIndex(inputFilename, outputPath);

            //GetLineCount(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz");

            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");
            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //CreateLuceneIndex(@"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //Optimize();

            //var res = QueryService.GetLabelFromIndex("Q5", LuceneHelper.LuceneIndexDirectory);
            //var res2 = QueryService.GetLabelFromIndex("P41", LuceneHelper.LuceneIndexDirectory);
            //var res3 = QueryService.QueryByLabel("Obama", LuceneHelper.LuceneIndexDirectory);
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
    }
}

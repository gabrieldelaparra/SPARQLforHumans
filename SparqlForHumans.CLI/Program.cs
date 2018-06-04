using SparqlForHumans.Core.Services;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            //var inputFilename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt";
            var inputFilename = @"Out-filtered-All-500.nt";
            var outputFilename = "filtered-All-500.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 500);

            //GetLineCount(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz");

            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");
            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //CreateLuceneIndex(@"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //Optimize();

            //var res = QueryService.GetLabelFromIndex("Q5", LuceneHelper.LuceneIndexDirectory);
            //var res2 = QueryService.GetLabelFromIndex("P41", LuceneHelper.LuceneIndexDirectory);
            //var res3 = QueryService.QueryByLabel("Obama", LuceneHelper.LuceneIndexDirectory);
        }
    }
}

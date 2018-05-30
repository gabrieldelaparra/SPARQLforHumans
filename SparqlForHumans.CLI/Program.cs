using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SparqlForHumans.Core.Properties;
using VDS.RDF;
using VDS.RDF.Parsing;
using FileHelper = SparqlForHumans.Core.Utilities.FileHelper;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt";
            FileHelper.ReadLinesTrace(inputFilename, 100000000).ToList();
            //var lines = FileHelper.GetInputLines(inputFilename);
            //int read = 0;
            //foreach (var line in lines)
            //{
            //    read++;
            //    if(read%100000 == 0)
            //        Console.WriteLine(read.ToString());

            //    var subject = line.Split(" ").FirstOrDefault();
            //    if (subject.Contains("/P"))
            //        Console.WriteLine(subject);
            //}

            //DumpHelper.FilterTriples()

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

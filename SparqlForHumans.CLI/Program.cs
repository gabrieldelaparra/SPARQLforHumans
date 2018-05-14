using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.CLI
{
    class Program
    {
        //static string indexPath = @"LuceneIndex";

        //static private Lucene.Net.Store.Directory luceneIndexDirectory;
        //static public Lucene.Net.Store.Directory LuceneIndexDirectory
        //{
        //    get
        //    {
        //        if (luceneIndexDirectory == null) luceneIndexDirectory = FSDirectory.Open(new DirectoryInfo(indexPath));
        //        if (IndexWriter.IsLocked(luceneIndexDirectory)) IndexWriter.Unlock(luceneIndexDirectory);
        //        var lockFilePath = Path.Combine(SearchIndex., "write.lock");
        //        if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
        //        return luceneIndexDirectory;
        //    }
        //}

        


        static void Main(string[] args)
        {
            //GetLineCount(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz");

            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");
            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //CreateLuceneIndex(@"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //Optimize();

            //SearchIndex.SearchByLabel("Barack Obama");
            var res = QueryService.GetTypeLabel("Q5");
        }

        

        

        

        

        

        


        
    }
}

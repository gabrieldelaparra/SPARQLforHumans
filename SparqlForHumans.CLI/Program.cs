using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Core.Utilities;
using System;
using System.IO;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static string entityIRI = "http://www.wikidata.org/entity/";
        static string entityPrefix = "Q";

        static string propertyIRI = "http://www.wikidata.org/prop/direct/";

        static string labelIRI = "http://www.w3.org/2000/01/rdf-schema#label";
        static string prefLabel = "http://www.w3.org/2004/02/skos/core#prefLabel";
        static string nameIRI = "http://schema.org/name";
        static string alt_labelIRI = "http://www.w3.org/2004/02/skos/core#altLabel";

        static string descriptionIRI = "http://schema.org/description";

        static string instanceOf = "P31";
        static string image = "P18";

        
        static void Main(string[] args)
        {
            FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

        }

        static void CreateLuceneIndex(string inputTriples)
        {
            //var directory = FSDirectory.GetDirectory("LuceneIndex");
            //var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT);
            //var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);


            //Document doc = new Document();
            //doc.Add(new Field("id", i.ToString(), Field.Store.YES, Field.Index.NO));
            //doc.Add(new Field("postBody", text, Field.Store.YES, Field.Index.ANALYZED));
            //writer.AddDocument(doc);

            //writer.Optimize();
            ////Close the writer
            ////writer.Flush();
            //writer.Close();

            //directory.Close();
        }

        /// <summary>
        /// Reads an Wikidata GZipped N-triples dump.
        /// Foreach line in the Triples file:
        /// - Parse the RDF triple
        /// - For the following cases, skip:
        ///      - If Subject Q-Code > 2mill
        ///      - If Object Q-Code > 2mill
        ///      - If Language != EN
        /// Else, Add the triple to a new NT File
        /// </summary>
        /// <param name="inputTriples">Wikidata GZipped N-triples dump</param>
        /// <param name="outputTriples">Filtered Wikidata (Non-GZipped) N-triples dump</param>
        static void FilterTriples(string inputTriples, string outputTriples)
        {
            if (!new FileInfo(inputTriples).Exists) return;

            var outputFileInfo = new FileInfo(outputTriples);
            if (outputFileInfo.Directory != null && !outputFileInfo.Directory.Exists)
                outputFileInfo.Directory.Create();

            Options.InternUris = false;
            //Read GZip File
            var lines = GZipHandler.ReadGZip(inputTriples);
            var notifyTicks = 100000;
            var readCount = 0;
            var writeCount = 0;

            using (var logStreamWriter = new StreamWriter(new FileStream("Process.log", FileMode.Create)))
            using (var errorStreamWriter = new StreamWriter(new FileStream("Errors.log", FileMode.Create)))
            using (var fileStream = new FileStream(outputTriples, FileMode.Create))
            using (var filteredWriter = new StreamWriter(fileStream))
            {
                logStreamWriter.WriteLine("DateTime,Read,Write");
                foreach (var item in lines)
                {
                    readCount++;
                    if (readCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{DateTime.Now},{readCount},{writeCount}");
                    }
                    try
                    {
                        var g = new NonIndexedGraph();
                        StringParser.Parse(g, item);
                        var statement = g.Triples.Last();

                        if (statement.Subject.NodeType != NodeType.Uri) continue;

                        var ntSubject = (UriNode)statement.Subject;
                        var ntPredicate = statement.Predicate;
                        var ntObject = statement.Object;

                        //Condition: Subject is not Entity: Skip
                        if (!ntSubject.ToSafeString().Contains(entityIRI)) continue;

                        //Condition: Subject is Entity and Q > 2.000.000: Skip
                        if (ntSubject.Uri.Segments.Last().Contains(entityPrefix))
                        {
                            var index = ntSubject.Uri.Segments.Last().Replace(entityPrefix, string.Empty);
                            int.TryParse(index, out int indexInt);
                            if (indexInt > 2000000) continue;
                        }

                        //Condition: Object is Entity and Q > 2.000.000: Skip
                        if (ntObject.NodeType == NodeType.Uri && ((UriNode)ntObject).Uri.Segments.Count() > 0 && ((UriNode)ntObject).Uri.Segments.Last().Contains(entityPrefix))
                        {
                            var index = ((UriNode)ntObject).Uri.Segments.Last().Replace(entityPrefix, string.Empty);
                            int.TryParse(index, out int indexInt);
                            if (indexInt > 2000000) continue;
                        }

                        //Condition: Object is Literal: Filter @en only
                        if (ntObject.NodeType == NodeType.Literal)
                            if (!((LiteralNode)ntObject).Language.Equals("en")) continue;

                        filteredWriter.WriteLine(item);
                        writeCount++;
                    }
                    catch (Exception e)
                    {
                        errorStreamWriter.WriteLine($"{DateTime.Now},{readCount},{item},{e.Message}");
                    }
                }
                logStreamWriter.WriteLine($"{DateTime.Now},{readCount},{writeCount}");
            }
        }
    }
}

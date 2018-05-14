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
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static Analyzer analyzer;
        static string indexPath = @"LuceneIndex";

        static private Lucene.Net.Store.Directory luceneIndexDirectory;
        static public Lucene.Net.Store.Directory LuceneIndexDirectory
        {
            get
            {
                if (luceneIndexDirectory == null) luceneIndexDirectory = FSDirectory.Open(new DirectoryInfo(indexPath));
                if (IndexWriter.IsLocked(luceneIndexDirectory)) IndexWriter.Unlock(luceneIndexDirectory);
                var lockFilePath = Path.Combine(indexPath, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return luceneIndexDirectory;
            }
        }

        static string entityIRI = "http://www.wikidata.org/entity/";
        static string entityPrefix = "Q";

        static string propertyIRI = "http://www.wikidata.org/prop/direct/";

        static string labelIRI = "http://www.w3.org/2000/01/rdf-schema#label";
        //static string prefLabel = "http://www.w3.org/2004/02/skos/core#prefLabel";
        //static string nameIRI = "http://schema.org/name";
        static string alt_labelIRI = "http://www.w3.org/2004/02/skos/core#altLabel";

        static string descriptionIRI = "http://schema.org/description";

        static string instanceOf = "P31";
        //static string image = "P18";


        static void Main(string[] args)
        {
            //GetLineCount(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz");

            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");
            //FilterTriples(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt", @"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //CreateLuceneIndex(@"C:\Users\admin\Desktop\DCC\SparqlForHumans\Out\filtered-triples.nt");

            //Optimize();

            //SearchIndex.SearchByLabel("Barack Obama");
            var res = SearchIndex.SearchLuceneTypeLabels("Q5");
        }

        

        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var writer = new IndexWriter(LuceneIndexDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }

        static IEnumerable<string> ReadLines(string filename)
        {
            using (StreamReader streamReader = new StreamReader(new FileStream(filename, FileMode.Open)))
                while (!streamReader.EndOfStream)
                {
                    yield return streamReader.ReadLine();
                }
        }

        

        static void CreateLuceneIndex(string inputTriples)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var notifyTicks = 100000;
            var readCount = 0;

            analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            Options.InternUris = false;
            var lines = ReadLines(inputTriples);

            string lastNode = string.Empty;
            var doc = new Document();
            var ps = new List<string>();

            using (var logStreamWriter = new StreamWriter(new FileStream("IndexProgressLog.txt", FileMode.Create)))
            using (var errorStreamWriter = new StreamWriter(new FileStream("IndexErrorsLog.txt", FileMode.Create)))
            using (var writer = new IndexWriter(LuceneIndexDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var line in lines)
                {
                    readCount++;
                    if (readCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount}");
                        Console.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{readCount / (double)21488204 * 100}");
                    }
                    var g = new NonIndexedGraph();
                    StringParser.Parse(g, line);
                    var statement = g.Triples.Last();

                    var ntSubject = statement.Subject.ToSafeString();
                    var ntPredicate = statement.Predicate.ToSafeString();
                    var ntObject = statement.Object;

                    // NEW SUBJECT
                    if (string.IsNullOrEmpty(lastNode))
                    {
                        lastNode = ntSubject;
                        var name = lastNode.Replace(entityIRI, string.Empty);
                        doc = new Document();
                        ps = new List<string>();
                        doc.Add(new Field("Name", name, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }

                    // NEW SUBJECT
                    if (!lastNode.Equals(ntSubject))
                    {
                        lastNode = ntSubject;
                        try
                        {
                            writer.AddDocument(doc);
                        }
                        catch (Exception e)
                        {
                            errorStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{line},{e.Message}");
                            Console.WriteLine(e.Message);
                        }
                        var name = lastNode.Replace(entityIRI, string.Empty);
                        doc = new Document();
                        ps = new List<string>();
                        doc.Add(new Field("Name", name, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }

                    if (ntPredicate.Contains(propertyIRI))
                    {
                        string p = ntPredicate.Replace(propertyIRI, "");
                        if (!ps.Contains(p))
                        {
                            ps.Add(p);
                            doc.Add(new Field("Property", p, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }

                        string value = ntObject.ToSafeString().Replace(entityIRI, "");
                        if (p.Equals(instanceOf))
                        {
                            doc.Add(new Field("Type", value, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }
                        if (value.StartsWith(entityPrefix))
                        {
                            String po = p + "##" + value;
                            doc.Add(new Field("PO", po, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }
                    }
                    else
                    {
                        // LITERAL VALUES
                        if (ntObject.NodeType != NodeType.Literal) continue;
                        var value = ((LiteralNode)ntObject).Value;

                        if (ntPredicate.Equals(labelIRI))
                        {
                            doc.Add(new Field("Label", value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                        else if (ntPredicate.Equals(descriptionIRI))
                        {
                            doc.Add(new Field("Description", value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                        else if (ntPredicate.Equals(alt_labelIRI))
                        {
                            doc.Add(new Field("AltLabel", value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                    }
                }
                writer.AddDocument(doc);
                writer.Dispose();
                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount}");
            }
            analyzer.Close();
            stopwatch.Stop();
        }

        static void GetLineCount(string inputTriples)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var logStreamWriter = new StreamWriter(new FileStream("LineCountLog.txt", FileMode.Create)))
            {
                var notifyTicks = 100000;
                var lineCount = 0;
                var lines = GZipHandler.ReadGZip(inputTriples);
                foreach (var item in lines)
                {
                    lineCount++;
                    if (lineCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
                    }
                }
                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
            }
            stopwatch.Stop();
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!new FileInfo(inputTriples).Exists) return;

            var outputFileInfo = new FileInfo(outputTriples);
            if (outputFileInfo.Directory != null && !outputFileInfo.Directory.Exists)
                outputFileInfo.Directory.Create();

            Options.InternUris = false;
            //Read GZip File
            //var wikidataDumpLines = GZipHandler.ReadGZip(inputTriples);
            var wikidataDumpLines = ReadLines(inputTriples);
            var notifyTicks = 100000;
            var readCount = 0;
            var writeCount = 0;

            using (var logStreamWriter = new StreamWriter(new FileStream("ProgressLog.txt", FileMode.Create)))
            using (var errorStreamWriter = new StreamWriter(new FileStream("ErrorsLog.txt", FileMode.Create)))
            using (var filteredStreamWriter = new StreamWriter(new FileStream(outputTriples, FileMode.Create)))
            {
                logStreamWriter.AutoFlush = true;
                errorStreamWriter.AutoFlush = true;
                filteredStreamWriter.AutoFlush = true;

                logStreamWriter.WriteLine("ElapsedTime,Read,Write");
                foreach (var line in wikidataDumpLines)
                {
                    readCount++;
                    if (readCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{writeCount}");
                        Console.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{((double)readCount / (double)447622549) * 100}");
                        //DEBUG:
                        //if (readCount == 100 * notifyTicks) break;
                    }
                    try
                    {
                        var g = new NonIndexedGraph();
                        StringParser.Parse(g, line);
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

                        filteredStreamWriter.WriteLine(line);
                        writeCount++;
                    }
                    catch (Exception e)
                    {
                        errorStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{line},{e.Message}");
                        //errorStreamWriter.Flush();
                    }
                }
                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{writeCount}");
            }
            stopwatch.Stop();
        }
    }
}

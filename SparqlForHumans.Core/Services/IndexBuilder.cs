using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.Core.Services
{
    public static class IndexBuilder
    {
        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var writer = new IndexWriter(Properties.Paths.LuceneIndexDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }

        public static void CreateLuceneIndex(string inputTriples)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var notifyTicks = 100000;
            var readCount = 0;

            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            Options.InternUris = false;
            var lines = FileHelper.ReadLines(inputTriples);

            string lastNode = string.Empty;
            var doc = new Document();
            var ps = new List<string>();

            using (var logStreamWriter = new StreamWriter(new FileStream("IndexProgressLog.txt", FileMode.Create)))
            using (var errorStreamWriter = new StreamWriter(new FileStream("IndexErrorsLog.txt", FileMode.Create)))
            using (var writer = new IndexWriter(Properties.WikidataDump.LuceneIndexDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
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
                        var name = lastNode.Replace(Properties.WikidataDump.EntityIRI, string.Empty);
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
                        var name = lastNode.Replace(Properties.WikidataDump.EntityIRI, string.Empty);
                        doc = new Document();
                        ps = new List<string>();
                        doc.Add(new Field("Name", name, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }

                    if (ntPredicate.Contains(Properties.WikidataDump.PropertyIRI))
                    {
                        string p = ntPredicate.Replace(Properties.WikidataDump.PropertyIRI, "");
                        if (!ps.Contains(p))
                        {
                            ps.Add(p);
                            doc.Add(new Field("Property", p, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }

                        string value = ntObject.ToSafeString().Replace(Properties.WikidataDump.EntityIRI, "");
                        if (p.Equals(Properties.WikidataDump.InstanceOf))
                        {
                            doc.Add(new Field("Type", value, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }
                        if (value.StartsWith(Properties.WikidataDump.EntityPrefix))
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

                        if (ntPredicate.Equals(Properties.WikidataDump.LabelIRI))
                        {
                            doc.Add(new Field("Label", value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                        else if (ntPredicate.Equals(Properties.WikidataDump.DescriptionIRI))
                        {
                            doc.Add(new Field("Description", value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                        else if (ntPredicate.Equals(Properties.WikidataDump.Alt_labelIRI))
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

        private static object ReadLines(string inputTriples)
        {
            throw new NotImplementedException();
        }
    }
}

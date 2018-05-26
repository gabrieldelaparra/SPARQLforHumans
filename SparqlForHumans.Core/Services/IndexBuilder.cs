using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
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

        public static void CreateIndex(string inputTriplesFilename, string outputDirectory)
        {
            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);

            using (var writer = new IndexWriter(Paths.GetLuceneDirectory(outputDirectory), analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var line in lines)
                {
                    var triple = line.GetTriple();

                    var ntSubject = triple.Subject;
                    var ntPredicate = triple.Predicate;
                    var ntObject = triple.Object;

                }
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
            var lines = FileHelper.GetInputLines(inputTriples);

            string lastNode = string.Empty;
            var luceneDocument = new Document();
            var ps = new List<string>();

            using (var logStreamWriter = new StreamWriter(new FileStream("IndexProgressLog.txt", FileMode.Create)))
            using (var errorStreamWriter = new StreamWriter(new FileStream("IndexErrorsLog.txt", FileMode.Create)))
            using (var writer = new IndexWriter(Properties.Paths.LuceneIndexDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var line in lines)
                {
                    readCount++;

                    if (readCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount}");
                        Console.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{readCount / (double)21488204 * 100}");
                    }

                    var triple = line.GetTriple();

                    var ntSubject = triple.Subject.GetUri();
                    var ntPredicate = triple.Predicate.GetUri();
                    var ntObject = triple.Object;

                    // First time Subject
                    if (string.IsNullOrEmpty(lastNode))
                    {
                        lastNode = ntSubject;
                        var name = lastNode.Replace(Properties.WikidataDump.EntityIRI, string.Empty);
                        luceneDocument = new Document();
                        ps = new List<string>();
                        luceneDocument.Add(new Field(Properties.Labels.Name.ToString(), name, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }

                    // New Subject, different from previous
                    //Guardo el documento anterior y se crea uno nuevo
                    if (!lastNode.Equals(ntSubject))
                    {
                        lastNode = ntSubject;
                        try
                        {
                            writer.AddDocument(luceneDocument);
                        }
                        catch (Exception e)
                        {
                            errorStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{line},{e.Message}");
                            Console.WriteLine(e.Message);
                        }
                        var name = lastNode.Replace(Properties.WikidataDump.EntityIRI, string.Empty);
                        luceneDocument = new Document();
                        ps = new List<string>();
                        luceneDocument.Add(new Field(Properties.Labels.Name.ToString(), name, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }

                    // On the existing Subject
                    if (ntPredicate.Contains(Properties.WikidataDump.PropertyIRI))
                    {
                        string p = ntPredicate.Replace(Properties.WikidataDump.PropertyIRI, "");
                        if (!ps.Contains(p))
                        {
                            ps.Add(p);
                            luceneDocument.Add(new Field(Properties.Labels.Property.ToString(), p, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }

                        string value = ntObject.ToSafeString().Replace(Properties.WikidataDump.EntityIRI, "");
                        if (p.Equals(Properties.WikidataDump.InstanceOf))
                        {
                            luceneDocument.Add(new Field(Properties.Labels.Type.ToString(), value, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }
                        if (value.StartsWith(Properties.WikidataDump.EntityPrefix))
                        {
                            String po = p + "##" + value;
                            luceneDocument.Add(new Field(Properties.Labels.PO.ToString(), po, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }
                    }
                    else
                    {
                        // LITERAL VALUES
                        if (ntObject.NodeType != NodeType.Literal) continue;
                        var value = ((LiteralNode)ntObject).Value;

                        if (ntPredicate.Equals(Properties.WikidataDump.LabelIRI))
                        {
                            luceneDocument.Add(new Field(Properties.Labels.Label.ToString(), value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                        else if (ntPredicate.Equals(Properties.WikidataDump.DescriptionIRI))
                        {
                            luceneDocument.Add(new Field(Properties.Labels.Description.ToString(), value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                        else if (ntPredicate.Equals(Properties.WikidataDump.Alt_labelIRI))
                        {
                            luceneDocument.Add(new Field(Properties.Labels.AltLabel.ToString(), value, Field.Store.YES, Field.Index.ANALYZED));
                        }
                    }
                }
                writer.AddDocument(luceneDocument);
                writer.Dispose();
                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount}");
            }
            analyzer.Close();
            stopwatch.Stop();
        }
    }
}

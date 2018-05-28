using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using NLog;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
using VDS.RDF;
using Version = Lucene.Net.Util.Version;

namespace SparqlForHumans.Core.Services
{
    public static class IndexBuilder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int NotifyTicks { get; } = 100000;

        public static Analyzer Analyzer { get; set; } = new StandardAnalyzer(Version.LUCENE_30);

        public static void CreateIndex(string inputTriplesFilename, string outputDirectory)
        {
            var readCount = 0;
            Options.InternUris = false;
            Analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);

            using (var writer = new IndexWriter(LuceneHelper.GetLuceneDirectory(outputDirectory), Analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                //Group them by QCode.
                var entiyGroups = lines.GroupByEntities();

                //A list to check and not add the same property twice
                var entityProperties = new List<string>();

                //Lucene document for each entity
                var luceneDocument = new Document();

                foreach (var group in entiyGroups)
                {
                    //Flag to create a new Lucene Document
                    var hasDocument = false;
                    foreach (var line in group)
                        try
                        {
                            readCount++;

                            if (readCount % NotifyTicks == 0)
                                Logger.Info($"{readCount}");

                            var (ntSubject, ntPredicate, ntObject) = line.GetTripleAsTuple();

                            if (!hasDocument)
                            {
                                var id = ntSubject.GetQCode();
                                luceneDocument = new Document();
                                entityProperties = new List<string>();
                                luceneDocument.Add(new Field(Labels.Id.ToString(), id, Field.Store.YES,
                                    Field.Index.NOT_ANALYZED));
                                hasDocument = true;
                            }

                            // On the existing Subject
                            // If the predicate is a Propery, add the property to a list of Properties and link it to the entity.
                            // Else, (predicate not a property: Labels, Alt-Labels, Description, etc.)
                            //  If the object is not a literal value, continue;
                            // Otherwise, add the value to the index on each case.
                            var value = string.Empty;

                            switch (ntPredicate.GetPredicateType())
                            {
                                case RDFExtensions.PredicateType.Property:
                                    ParsePropertyPredicate(ntPredicate, ntObject, luceneDocument, entityProperties);
                                    break;
                                case RDFExtensions.PredicateType.Label:
                                    if (!ntObject.IsLiteral()) continue;
                                    luceneDocument.Add(new Field(Labels.Label.ToString(), ntObject.GetLiteralValue(),
                                        Field.Store.YES, Field.Index.ANALYZED));
                                    break;
                                case RDFExtensions.PredicateType.Description:
                                    if (!ntObject.IsLiteral()) continue;
                                    luceneDocument.Add(new Field(Labels.Description.ToString(),
                                        ntObject.GetLiteralValue(), Field.Store.YES, Field.Index.ANALYZED));
                                    break;
                                case RDFExtensions.PredicateType.AltLabel:
                                    if (!ntObject.IsLiteral()) continue;
                                    luceneDocument.Add(new Field(Labels.AltLabel.ToString(), ntObject.GetLiteralValue(),
                                        Field.Store.YES, Field.Index.ANALYZED));
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"{readCount},{line}");
                            Logger.Error(e);
                        }

                    writer.AddDocument(luceneDocument);
                }

                writer.Dispose();
                Logger.Info($"{readCount}");
            }

            Analyzer.Close();
        }

        private static void ParsePropertyPredicate(INode ntPredicate, INode ntObject, Document luceneDocument,
            List<string> entityProperties)
        {
            var propertyCode = ntPredicate.GetPCode();

            //Do not add the same property twice. Why?
            if (!entityProperties.Contains(propertyCode))
            {
                entityProperties.Add(propertyCode);
                luceneDocument.Add(new Field(Labels.Property.ToString(), propertyCode, Field.Store.YES,
                    Field.Index.NOT_ANALYZED));
            }

            //Ignore properties which have literal values, somehow, it is only adding those which have entities as values.
            //I am not sure that this is a desired bahviour. I have to check the original code to see if this is as desired.
            if (!ntObject.IsUriNode()) return;

            var value = ntObject.GetQCode();

            if (ntPredicate.IsInstanceOf())
                luceneDocument.Add(new Field(Labels.InstanceOf.ToString(), value, Field.Store.YES,
                    Field.Index.NOT_ANALYZED));

            if (!ntObject.HasQCode()) return;

            var po = propertyCode + "##" + value;
            luceneDocument.Add(new Field(Labels.PO.ToString(), po, Field.Store.YES, Field.Index.NOT_ANALYZED));
        }

        //public static void CreateLuceneIndex(string inputTriples)
        //{
        //    var readCount = 0;
        //    Analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

        //    Options.InternUris = false;
        //    var lines = FileHelper.GetInputLines(inputTriples);

        //    string lastNode = string.Empty;
        //    var luceneDocument = new Document();
        //    var ps = new List<string>();

        //    using (var writer = new IndexWriter(Properties.Paths.LuceneIndexDirectory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
        //    {
        //        foreach (var line in lines)
        //        {
        //            readCount++;

        //            if (readCount % NotifyTicks == 0)
        //                Logger.Info($"{readCount}");

        //            var triple = line.GetTriple();

        //            var ntSubject = triple.Subject.GetUri();
        //            var ntPredicate = triple.Predicate.GetUri();
        //            var ntObject = triple.Object;

        //            // First time Subject
        //            if (string.IsNullOrEmpty(lastNode))
        //            {
        //                lastNode = ntSubject;
        //                var name = lastNode.Replace(Properties.WikidataDump.EntityIRI, string.Empty);
        //                luceneDocument = new Document();
        //                ps = new List<string>();
        //                luceneDocument.Add(new Field(Properties.Labels.Name.ToString(), name, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //            }

        //            // New Subject, different from previous
        //            //Guardo el documento anterior y se crea uno nuevo
        //            if (!lastNode.Equals(ntSubject))
        //            {
        //                try
        //                {
        //                    writer.AddDocument(luceneDocument);
        //                }
        //                catch (Exception e)
        //                {
        //                    Logger.Error($"{readCount},{line}");
        //                    Logger.Error(e);
        //                }

        //                lastNode = ntSubject;
        //                var name = lastNode.Replace(Properties.WikidataDump.EntityIRI, string.Empty);
        //                luceneDocument = new Document();
        //                ps = new List<string>();
        //                luceneDocument.Add(new Field(Properties.Labels.Name.ToString(), name, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //            }

        //            // On the existing Subject
        //            if (ntPredicate.Contains(Properties.WikidataDump.PropertyIRI))
        //            {
        //                string p = ntPredicate.Replace(Properties.WikidataDump.PropertyIRI, "");
        //                if (!ps.Contains(p))
        //                {
        //                    ps.Add(p);
        //                    luceneDocument.Add(new Field(Properties.Labels.Property.ToString(), p, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //                }

        //                string value = ntObject.ToSafeString().Replace(Properties.WikidataDump.EntityIRI, "");
        //                if (p.Equals(Properties.WikidataDump.InstanceOf))
        //                {
        //                    luceneDocument.Add(new Field(Properties.Labels.Type.ToString(), value, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //                }
        //                if (value.StartsWith(Properties.WikidataDump.EntityPrefix))
        //                {
        //                    String po = p + "##" + value;
        //                    luceneDocument.Add(new Field(Properties.Labels.PO.ToString(), po, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //                }
        //            }
        //            else
        //            {
        //                // LITERAL VALUES
        //                if (ntObject.NodeType != NodeType.Literal) continue;
        //                var value = ((LiteralNode)ntObject).Value;

        //                if (ntPredicate.Equals(Properties.WikidataDump.LabelIRI))
        //                {
        //                    luceneDocument.Add(new Field(Properties.Labels.Label.ToString(), value, Field.Store.YES, Field.Index.ANALYZED));
        //                }
        //                else if (ntPredicate.Equals(Properties.WikidataDump.DescriptionIRI))
        //                {
        //                    luceneDocument.Add(new Field(Properties.Labels.Description.ToString(), value, Field.Store.YES, Field.Index.ANALYZED));
        //                }
        //                else if (ntPredicate.Equals(Properties.WikidataDump.Alt_labelIRI))
        //                {
        //                    luceneDocument.Add(new Field(Properties.Labels.AltLabel.ToString(), value, Field.Store.YES, Field.Index.ANALYZED));
        //                }
        //            }
        //        }
        //        writer.AddDocument(luceneDocument);
        //        writer.Dispose();
        //        Logger.Info($"{readCount}");
        //    }
        //    Analyzer.Close();
        //}

        public static void Optimize()
        {
            Analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var writer = new IndexWriter(LuceneHelper.LuceneIndexDirectory, Analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                Analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }
    }
}
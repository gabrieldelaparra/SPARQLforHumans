using System;
using System.Collections.Generic;
using System.Linq;
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
            long readCount = 0;
            int nodeCount = 0;
            Options.InternUris = false;
            Analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);

            //Ranking:
            var nodesGraph = IndexRanker.BuildNodesGraph(inputTriplesFilename);
            IndexRanker.CalculateRanks(nodesGraph, 25);

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
                                var id = ntSubject.GetId();
                                luceneDocument = new Document();
                                entityProperties = new List<string>();
                                luceneDocument.Add(new Field(Labels.Id.ToString(), id, Field.Store.YES,
                                    Field.Index.NOT_ANALYZED));
                                hasDocument = true;
                            }

                            ParsePredicate(ntPredicate, ntObject, entityProperties, luceneDocument);
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"{readCount},{line}");
                            Logger.Error(e);
                        }

                    luceneDocument.Boost = (float)nodesGraph.ElementAt(nodeCount).Rank;
                    writer.AddDocument(luceneDocument);
                    nodeCount++;
                }

                writer.Dispose();
                Logger.Info($"{readCount}");
            }

            Analyzer.Close();
        }

        private static void ParsePredicate(INode ntPredicate, INode ntObject, List<string> entityProperties, Document luceneDocument)
        {
            // On the existing Subject
            // If the predicate is a Propery, add the property to a list of Properties and link it to the entity.
            // Else, (predicate not a property: Labels, Alt-Labels, Description, etc.)
            //  If the object is not a literal value, continue;
            // Otherwise, add the value to the index on each case.
            switch (ntPredicate.GetPredicateType())
            {
                case RDFExtensions.PredicateType.Property:
                    ParsePropertyPredicate(ntPredicate, ntObject, entityProperties, luceneDocument);
                    break;
                case RDFExtensions.PredicateType.Label:
                    luceneDocument.Add(new Field(Labels.Label.ToString(), ntObject.GetLiteralValue(),
                        Field.Store.YES, Field.Index.ANALYZED));
                    break;
                case RDFExtensions.PredicateType.Description:
                    luceneDocument.Add(new Field(Labels.Description.ToString(),
                        ntObject.GetLiteralValue(), Field.Store.YES, Field.Index.ANALYZED));
                    break;
                case RDFExtensions.PredicateType.AltLabel:
                    luceneDocument.Add(new Field(Labels.AltLabel.ToString(), ntObject.GetLiteralValue(),
                        Field.Store.YES, Field.Index.ANALYZED));
                    break;
                case RDFExtensions.PredicateType.Other:
                    break;
            }
        }

        private static void ParsePropertyPredicate(INode ntPredicate, INode ntObject, ICollection<string> entityProperties,
            Document luceneDocument)
        {
            var propertyCode = ntPredicate.GetId();

            //Do not add the same property twice in the document. The values will be added twice in the PropertyAndValue list;
            if (!entityProperties.Contains(propertyCode))
            {
                entityProperties.Add(propertyCode);
                luceneDocument.Add(new Field(Labels.Property.ToString(), propertyCode, Field.Store.YES,
                    Field.Index.NOT_ANALYZED));
            }

            switch (RDFExtensions.GetPropertyType(ntPredicate, ntObject))
            {
                case RDFExtensions.PropertyType.InstanceOf:
                    luceneDocument.Add(new Field(Labels.InstanceOf.ToString(), ntObject.GetId(), Field.Store.YES,
                        Field.Index.NOT_ANALYZED));
                    break;
                case RDFExtensions.PropertyType.EntityDirected:
                    var propertyAndValue = propertyCode + "##" + ntObject.GetId();
                    luceneDocument.Add(new Field(Labels.PropertyAndValue.ToString(), propertyAndValue, Field.Store.YES,
                        Field.Index.NOT_ANALYZED));
                    break;
                case RDFExtensions.PropertyType.LiteralDirected:
                    break;
                case RDFExtensions.PropertyType.Other:
                    break;
            }
        }

        //public static void Optimize()
        //{
        //    Analyzer = new StandardAnalyzer(Version.LUCENE_30);

        //    using (var writer = new IndexWriter(LuceneHelper.LuceneIndexDirectory, Analyzer,
        //        IndexWriter.MaxFieldLength.UNLIMITED))
        //    {
        //        Analyzer.Close();
        //        writer.Optimize();
        //        writer.Dispose();
        //    }
        //}
    }
}
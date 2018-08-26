using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
using VDS.RDF;
using Version = Lucene.Net.Util.Version;

namespace SparqlForHumans.Core.Services
{
    public static class IndexBuilder
    {
        private static readonly NLog.Logger Logger = Utilities.Logger.Init();

        public static int NotifyTicks { get; } = 100000;

        // PropertyIndex:
        /// Include Subjects only if Id starts with P;
        /// Rank with frequency;
        public static void CreatePropertyIndex(string inputTriplesFilename, string outputDirectory, bool indexFrequency = false)
        {
            long readCount = 0;
            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var dictionary = new Dictionary<string, int>();

            if (indexFrequency)
                dictionary = PropertiesFrequency.GetPropertiesFrequency(inputTriplesFilename);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);
            Logger.Info("Building Index");
            using (var writer = new IndexWriter(outputDirectory.GetLuceneDirectory(), analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                //Group them by QCode.
                var entiyGroups = lines.GroupBySubject();

                //Lucene document for each entity
                var luceneDocument = new Document();

                foreach (var group in entiyGroups)
                {
                    var subject = group.FirstOrDefault().GetTripleAsTuple().subject;

                    //Excludes Entities, will only add properties.
                    if (!subject.IsEntityP())
                        continue;

                    var propertyId = subject.GetId();

                    //Flag to create a new Lucene Document
                    var hasDocument = false;

                    foreach (var line in group)
                    {
                        readCount++;

                        if (readCount % NotifyTicks == 0)
                            Logger.Info($"{readCount:N0}");

                        var (ntSubject, ntPredicate, ntObject) = line.GetTripleAsTuple();

                        if (!hasDocument)
                        {
                            var id = ntSubject.GetId();
                            propertyId = id;
                            Logger.Trace($"Indexing: {id}");
                            luceneDocument = new Document();
                            var field = new Field(Labels.Id.ToString(), id, Field.Store.YES,
                                Field.Index.NOT_ANALYZED);

                            luceneDocument.Add(field);
                            hasDocument = true;
                        }

                        ParsePredicate(ntPredicate, ntObject, luceneDocument);
                    }

                    if (indexFrequency)
                    {
                        if (dictionary.TryGetValue(propertyId, out var value))
                            luceneDocument.Add(new Field(Labels.PropertyFrequency.ToString(),
                                value.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }

                    writer.AddDocument(luceneDocument);
                }

                writer.Dispose();
                Logger.Info($"{readCount:N0}");
            }

            analyzer.Close();
        }

        //TODO: No 'prefLabel' in the current index:
        //TODO: No 'name' in the current index:
        /// EntitiesIndex
        /// Include Subjects only if Id starts with Q;
        /// Rank with boosts;
        /// Entities have properties, not sure if properties have properties;
        public static void CreateEntitiesIndex(string inputTriplesFilename, string outputDirectory, bool addBoosts = true)
        {
            long readCount = 0;
            var nodeCount = 0;
            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);

            double[] nodesGraphRanks = null;

            if (addBoosts)
            {
                //Ranking:
                Logger.Info("Building Graph");
                var nodesGraphArray = EntityRanker.BuildSimpleNodesGraph(inputTriplesFilename);
                Logger.Info("Calculating Ranks");
                nodesGraphRanks = EntityRanker.CalculateRanks(nodesGraphArray, 25);
            }

            Logger.Info("Building Index");
            using (var writer = new IndexWriter(outputDirectory.GetLuceneDirectory(), analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                //Group them by QCode.
                var entityGroups = lines.GroupBySubject();

                //Lucene document for each entity
                var luceneDocument = new Document();

                foreach (var group in entityGroups)
                {
                    var subject = group.FirstOrDefault().GetTripleAsTuple().subject;

                    //Excludes Properties, will only add entities.
                    if (!subject.IsEntityQ())
                        continue;

                    //Flag to create a new Lucene Document
                    var hasDocument = false;

                    foreach (var line in group)
                    {
                        readCount++;

                        if (readCount % NotifyTicks == 0)
                            Logger.Info($"{readCount:N0}");

                        var (ntSubject, ntPredicate, ntObject) = line.GetTripleAsTuple();

                        if (!hasDocument)
                        {
                            var id = ntSubject.GetId();
                            Logger.Trace($"Indexing: {id}");
                            luceneDocument = new Document();
                            var field = new Field(Labels.Id.ToString(), id, Field.Store.YES,
                                Field.Index.NOT_ANALYZED);

                            luceneDocument.Add(field);
                            hasDocument = true;
                        }

                        ParsePredicate(ntPredicate, ntObject, luceneDocument);
                    }

                    if (addBoosts)
                    {
                        luceneDocument.Boost = (float) nodesGraphRanks[nodeCount];
                        var field = new Field(Labels.Rank.ToString(), value: nodesGraphRanks[nodeCount].ToString(), store: Field.Store.YES,
                            index: Field.Index.NOT_ANALYZED);
                        luceneDocument.Add(field);
                    }

                    writer.AddDocument(luceneDocument);
                    nodeCount++;
                }

                writer.Dispose();
                Logger.Info($"{readCount:N0}");
            }

            analyzer.Close();
        }

        private static void ParsePredicate(INode ntPredicate, INode ntObject, Document luceneDocument)
        {
            // On the existing Subject
            // If the predicate is a Property, add the property to a list of Properties and link it to the entity.
            // Else, (predicate not a property: Labels, Alt-Labels, Description, etc.)
            //  If the object is not a literal value, continue;
            // Otherwise, add the value to the index on each case.
            switch (ntPredicate.GetPredicateType())
            {
                case RDFExtensions.PredicateType.Property:
                    ParsePropertyPredicate(ntPredicate, ntObject, luceneDocument);
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

        private static void ParsePropertyPredicate(INode ntPredicate, INode ntObject, Document luceneDocument)
        {
            var propertyCode = ntPredicate.GetId();

            luceneDocument.Add(new Field(Labels.Property.ToString(), propertyCode, Field.Store.YES,
                Field.Index.NOT_ANALYZED));

            switch (RDFExtensions.GetPropertyType(ntPredicate, ntObject))
            {
                case RDFExtensions.PropertyType.InstanceOf:
                    luceneDocument.Add(new Field(Labels.InstanceOf.ToString(), ntObject.GetId(), Field.Store.YES,
                        Field.Index.NOT_ANALYZED));
                    break;
                case RDFExtensions.PropertyType.EntityDirected:
                    var propertyAndValue = propertyCode + WikidataDump.PropertyValueSeparator + ntObject.GetId();
                    luceneDocument.Add(new Field(Labels.PropertyAndValue.ToString(), propertyAndValue, Field.Store.YES,
                        Field.Index.NOT_ANALYZED));
                    break;
                case RDFExtensions.PropertyType.LiteralDirected:
                case RDFExtensions.PropertyType.Other:
                    break;
            }
        }
    }
}
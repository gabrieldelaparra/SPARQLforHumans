using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using SparqlForHumans.Core.Models;
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

        public static void CreateTypesIndex()
        {
            CreateTypesIndex(LuceneIndexExtensions.TypesIndexPath);
        }

        public static void CreateTypesIndex(string outputDirectory)
        {
            var dictionary = CreateTypesAndPropertiesDictionary();
            CreateTypesIndex(dictionary, outputDirectory);
        }

        public static void CreateTypesIndex(Dictionary<string, List<string>> typePropertiesDictionary, string outputDirectory)
        {
            long readCount = 0;
            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var writer = new IndexWriter(outputDirectory.GetLuceneDirectory(), analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var typeAndProperties in typePropertiesDictionary)
                {
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Types Index, Group: {readCount:N0}");

                    var id = typeAndProperties.Key;
                    var entity = QueryService.QueryEntityById(id, LuceneIndexExtensions.EntitiesIndexDirectory);
                    var typeLabel = entity.Label;
                    var typeDescription = entity.Description;
                    var typeAltLabel = entity.AltLabels;
                    var typeRank = entity.RankValue;

                    //Lucene document for each entity
                    var luceneDocument = new Document();

                    luceneDocument.Add(new Field(Labels.Id.ToString(), id, Field.Store.YES, Field.Index.NOT_ANALYZED));

                    luceneDocument.Add(new Field(Labels.Label.ToString(), typeLabel, Field.Store.YES, Field.Index.ANALYZED));

                    luceneDocument.Add(new Field(Labels.Description.ToString(), typeDescription, Field.Store.YES, Field.Index.ANALYZED));

                    foreach (var altLabel in typeAltLabel)
                    {
                        luceneDocument.Add(new Field(Labels.AltLabel.ToString(), altLabel, Field.Store.YES, Field.Index.ANALYZED));
                    }

                    var rankField = new NumericField(Labels.Rank.ToString(), Field.Store.YES, true);
                    rankField.SetDoubleValue(typeRank);
                    luceneDocument.Add(rankField);

                    //TODO: How to store more than one property and frequency here? Should I store them as Id##Label##Frequency?
                    foreach (var propertyId in typeAndProperties.Value)
                    {
                        var property = QueryService.QueryPropertyById(propertyId,
                            LuceneIndexExtensions.PropertiesIndexDirectory);
                        var propertyLabel = property.Label;
                        var propertyFrequency = property.Frequency;
                        var propertyConcat =
                            $"{propertyId}{WikidataDump.PropertyValueSeparator}{propertyLabel}{WikidataDump.PropertyValueSeparator}{propertyFrequency}";
                        luceneDocument.Add(new Field(Labels.Property.ToString(), propertyConcat, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }

                    writer.AddDocument(luceneDocument);
                    readCount++;
                }
                writer.Dispose();
                Logger.Info($"Build Types Index, Group: {readCount:N0}");
            }

            analyzer.Close();
        }

        public static Dictionary<string, List<string>> CreateTypesAndPropertiesDictionary()
        {
            var dictionary = new Dictionary<string, List<string>>();

            using (var entityReader = IndexReader.Open(LuceneIndexExtensions.EntitiesIndexDirectory, true))
            {
                var docCount = entityReader.MaxDoc;
                for (var i = 0; i < docCount; i++)
                {
                    var doc = entityReader.Document(i);

                    var entity = ((Entity)doc.MapBaseSubject())
                        .MapInstanceOf(doc)
                        .MapRank(doc)
                        .MapBaseProperties(doc);

                    foreach (var instanceOf in entity.InstanceOf)
                    {
                        if (!dictionary.ContainsKey(instanceOf))
                            dictionary.Add(instanceOf, new List<string>());

                        var valuesList = dictionary[instanceOf];
                        foreach (var entityProperty in entity.Properties)
                        {
                            if (!valuesList.Contains(entityProperty.Id))
                                valuesList.Add(entityProperty.Id);
                        }
                    }
                }
            }

            return dictionary;
        }

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
            Logger.Info("Building Properties Index");
            using (var writer = new IndexWriter(outputDirectory.GetLuceneDirectory(), analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                //Group them by QCode.
                var entityGroups = lines.GroupBySubject();

                foreach (var group in entityGroups)
                {
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Property Index, Group: {readCount:N0}");

                    var subject = group.FirstOrDefault().GetTripleAsTuple().subject;

                    //Excludes Entities, will only add properties.
                    if (!subject.IsEntityP())
                        continue;

                    var propertyId = subject.GetId();

                    //Flag to create a new Lucene Document
                    var hasDocument = false;

                    //Lucene document for each property
                    var luceneDocument = new Document();

                    foreach (var line in group)
                    {
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
                        {
                            luceneDocument.Boost = value;

                            var frequencyField = new NumericField(Labels.Frequency.ToString(), Field.Store.YES, true);
                            frequencyField.SetIntValue(value);

                            luceneDocument.Add(frequencyField);
                        }
                            
                    }

                    writer.AddDocument(luceneDocument);
                    readCount++;
                }

                writer.Dispose();
                Logger.Info($"Build Property Index, Group: {readCount:N0}");
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
            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);

            double[] nodesGraphRanks = null;
            Dictionary<string, int> nodesDictionary = null;
            if (addBoosts)
            {
                //Ranking:
                Logger.Info("Building Dictionary");
                nodesDictionary = EntityRanker.BuildNodesDictionary(inputTriplesFilename);
                Logger.Info("Building Graph");
                var nodesGraphArray = EntityRanker.BuildSimpleNodesGraph(inputTriplesFilename, nodesDictionary);
                Logger.Info("Calculating Ranks");
                nodesGraphRanks = EntityRanker.CalculateRanks(nodesGraphArray, 20);
            }

            Logger.Info("Building Index");
            using (var writer = new IndexWriter(outputDirectory.GetLuceneDirectory(), analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                //Group them by QCode.
                var entityGroups = lines.GroupBySubject();

                foreach (var group in entityGroups)
                {
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Entity Index, Group: {readCount:N0}");

                    var subject = group.FirstOrDefault().GetTripleAsTuple().subject;

                    //Excludes Properties, will only add entities.
                    if (!subject.IsEntityQ())
                        continue;

                    //Flag to create a new Lucene Document
                    var hasDocument = false;

                    //entityId
                    var id = string.Empty;

                    //Lucene document for each entity
                    var luceneDocument = new Document();

                    foreach (var line in group)
                    {
                        var (ntSubject, ntPredicate, ntObject) = line.GetTripleAsTuple();

                        if (!hasDocument)
                        {
                            id = ntSubject.GetId();
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
                        nodesDictionary.TryGetValue(id, out var subjectIndex);

                        luceneDocument.Boost = (float)nodesGraphRanks[subjectIndex];

                        var rankField = new NumericField(Labels.Rank.ToString(), Field.Store.YES, true);
                        rankField.SetDoubleValue(nodesGraphRanks[subjectIndex]);

                        luceneDocument.Add(rankField);
                    }

                    writer.AddDocument(luceneDocument);
                    readCount++;
                }

                writer.Dispose();
                Logger.Info($"Build Entity Index, Group: {readCount:N0}");
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
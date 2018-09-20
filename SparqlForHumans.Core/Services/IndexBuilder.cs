using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
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

        //public static void CreateTypesIndex()
        //{
        //    CreateTypesIndex(LuceneIndexExtensions.TypesIndexPath);
        //}

        //public static void CreateTypesIndex(string outputDirectory)
        //{
        //    var dictionary = CreateTypesAndPropertiesDictionary(LuceneIndexExtensions.EntitiesIndexDirectory);
        //    CreateTypesIndex(dictionary, outputDirectory);
        //}

        //public static void CreateTypesIndex(Dictionary<string, List<string>> typePropertiesDictionary, string outputDirectory)
        //{
        //    long readCount = 0;
        //    Options.InternUris = false;
        //    var analyzer = new StandardAnalyzer(Version.LUCENE_30);

        //    using (var writer = new IndexWriter(outputDirectory.GetLuceneDirectory(), analyzer,
        //        IndexWriter.MaxFieldLength.UNLIMITED))
        //    {
        //        foreach (var typeAndProperties in typePropertiesDictionary)
        //        {
        //            if (readCount % NotifyTicks == 0)
        //                Logger.Info($"Build Types Index, Group: {readCount:N0}");

        //            var id = typeAndProperties.Key;
        //            var entity = SingleDocumentQueries.QueryEntityById(id, LuceneIndexExtensions.EntitiesIndexDirectory);
        //            var typeLabel = entity.Label;
        //            var typeDescription = entity.Description;
        //            var typeAltLabel = entity.AltLabels;
        //            var typeRank = entity.RankValue;

        //            //Lucene document for each entity
        //            var luceneDocument = new Document();

        //            luceneDocument.Add(new Field(Labels.Id.ToString(), id, Field.Store.YES, Field.Index.NOT_ANALYZED));

        //            luceneDocument.Add(new Field(Labels.Label.ToString(), typeLabel, Field.Store.YES, Field.Index.ANALYZED));

        //            luceneDocument.Add(new Field(Labels.Description.ToString(), typeDescription, Field.Store.YES, Field.Index.ANALYZED));

        //            foreach (var altLabel in typeAltLabel)
        //            {
        //                luceneDocument.Add(new Field(Labels.AltLabel.ToString(), altLabel, Field.Store.YES, Field.Index.ANALYZED));
        //            }

        //            var rankField = new NumericField(Labels.Rank.ToString(), Field.Store.YES, true);
        //            rankField.SetDoubleValue(typeRank);
        //            luceneDocument.Add(rankField);

        //            //TODO: How to store more than one property and frequency here? Should I store them as Id##Label##Frequency?
        //            foreach (var propertyId in typeAndProperties.Value)
        //            {
        //                var property = SingleDocumentQueries.QueryPropertyById(propertyId,
        //                    LuceneIndexExtensions.PropertiesIndexDirectory);
        //                var propertyLabel = property.Label;
        //                var propertyFrequency = property.Frequency;
        //                var propertyConcat =
        //                    $"{propertyId}{WikidataDump.PropertyValueSeparator}{propertyLabel}{WikidataDump.PropertyValueSeparator}{propertyFrequency}";
        //                luceneDocument.Add(new Field(Labels.Property.ToString(), propertyConcat, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //            }
        //            writer.AddDocument(luceneDocument);
        //            readCount++;
        //        }
        //        writer.Dispose();
        //        Logger.Info($"Build Types Index, Group: {readCount:N0}");
        //    }
        //    analyzer.Close();
        //}

        /// <summary>
        /// Takes a dictionary(QEntityType, ListOfProperties) and adds a new Field (IsEntityType) to all Entities
        /// that have the QEntityType as InstanceOf.
        /// </summary>
        /// <param name="typePropertiesDictionary"></param>
        /// <param name="entitiesIndexDirectory"></param>
        public static void AddIsTypeEntityToEntitiesIndex(Dictionary<string, List<string>> typePropertiesDictionary,
            Directory entitiesIndexDirectory)
        {
            long readCount = 1;

            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var writer = new IndexWriter(entitiesIndexDirectory, analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var documents = MultiDocumentQueries.QueryDocumentsByIds(typePropertiesDictionary.Select(x => x.Key),
                    entitiesIndexDirectory);

                foreach (var document in documents)
                {
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Types Index, Group: {readCount:N0}");

                    var field = new Field(Labels.IsTypeEntity.ToString(), "true", Field.Store.YES,
                        Field.Index.NOT_ANALYZED_NO_NORMS);

                    document.Add(field);
                    writer.UpdateDocument(new Term(Labels.Id.ToString(), document.MapBaseSubject().Id), document);
                    readCount++;
                }
            }
        }

        public static Dictionary<string, List<string>> CreateInvertedProperties(
            Dictionary<string, List<string>> typesAndPropertiesDictionary)
        {
            var dictionary = new Dictionary<string, List<string>>();

            foreach (var type in typesAndPropertiesDictionary)
            {
                foreach (var property in type.Value)
                {
                    if (!dictionary.ContainsKey(property))
                        dictionary.Add(property, new List<string>());

                    dictionary[property].Add(type.Key);
                }
            }
            return dictionary;
        }

        public static Dictionary<string, List<string>> CreateTypesAndPropertiesDictionary(Directory entitiesIndexDirectory)
        {
            var dictionary = new Dictionary<string, List<string>>();

            using (var entityReader = IndexReader.Open(entitiesIndexDirectory, true))
            {
                var docCount = entityReader.MaxDoc;
                for (var i = 0; i < docCount; i++)
                {
                    var doc = entityReader.Document(i);

                    var entity = new Entity(doc.MapBaseSubject())
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

        public static void CreateEntitiesIndex(string inputTriplesFilename, string outputPath, bool addBoosts = true)
        {
            CreateEntitiesIndex(inputTriplesFilename, outputPath.GetLuceneDirectory(), addBoosts);
        }

        //TODO: No 'prefLabel' in the current index:
        //TODO: No 'name' in the current index:
        /// EntitiesIndex
        /// Include Subjects only if Id starts with Q;
        /// Rank with boosts;
        /// Entities have properties, not sure if properties have properties;
        public static void CreateEntitiesIndex(string inputTriplesFilename, Directory outputDirectory, bool addBoosts = false)
        {
            long readCount = 1;
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
            using (var writer = new IndexWriter(outputDirectory, analyzer,
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
                default:
                case RDFExtensions.PredicateType.Other:
                    break;
            }
        }

        public static void AddDomainTypesToPropertiesIndex(Directory propertiesIndexDirectory,
            Dictionary<string, List<string>> invertedProperties)
        {
            long readCount = 0;

            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var writer = new IndexWriter(propertiesIndexDirectory, analyzer,
                IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var invertedProperty in invertedProperties)
                {
                    var document =
                        SingleDocumentQueries.QueryDocumentById(invertedProperty.Key, propertiesIndexDirectory);
                 
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Types Index, Group: {readCount:N0}");

                    foreach (var domainType in invertedProperty.Value)
                    {
                        var field = new Field(Labels.DomainType.ToString(), domainType , Field.Store.YES,
                            Field.Index.ANALYZED);

                        document.Add(field);
                    }
                    writer.UpdateDocument(new Term(Labels.Id.ToString(), invertedProperty.Key), document);
                    readCount++;
                }
            }

        }

        // PropertyIndex:
        /// Include Subjects only if Id starts with P;
        /// Rank with frequency;
        public static void CreatePropertiesIndex(string inputTriplesFilename, Directory outputDirectory, bool indexFrequency = false)
        {
            long readCount = 1;
            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var dictionary = new Dictionary<string, int>();

            if (indexFrequency)
                dictionary = PropertiesFrequency.GetPropertiesFrequency(inputTriplesFilename);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);
            
            Logger.Info("Building Properties Index");

            using (var writer = new IndexWriter(outputDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
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

                            var frequencyField = new NumericField(Labels.Rank.ToString(), Field.Store.YES, true);
                            frequencyField.SetDoubleValue(value);

                            luceneDocument.Add(frequencyField);
                        }
                    }

                    writer.AddDocument(luceneDocument);
                    readCount++;
                }

                writer.Dispose();
            }
            Logger.Info($"Build Property Index, Group: {readCount:N0}");
            analyzer.Close();
        }

        /// <summary>
        /// Used on creating a PropertyIndex.
        /// Line example:
        /// - P333 P31 P444.
        ///     - P333 is the subject.
        ///     - P31 is InstanceOf. (Could be a EntityDirected Predicate)
        ///     - P444 is the Type (InstanceOf) of P333.
        /// Called for each property of a Property Subject.
        /// Takes a Predicate, Object and a Property (Subject) Document.
        /// Adds a new Field to the Document: InstanceOf, EntityDirected (With separators, not sure why).
        /// </summary>
        /// <param name="ntPredicate"></param>
        /// <param name="ntObject"></param>
        /// <param name="luceneDocument"></param>
        private static void ParsePropertyPredicate(INode ntPredicate, INode ntObject, Document luceneDocument)
        {
            var propertyCode = ntPredicate.GetId();

            //Stores the P-Id of the PropertyPredicate (of the PropertyEntity).
            luceneDocument.Add(new Field(Labels.Property.ToString(), propertyCode, Field.Store.YES,
                Field.Index.NOT_ANALYZED));

            switch (RDFExtensions.GetPropertyType(ntPredicate, ntObject))
            {
                //PropertyPredicate is InstanceOf another type of Property:
                case RDFExtensions.PropertyType.InstanceOf:
                    luceneDocument.Add(new Field(Labels.InstanceOf.ToString(), ntObject.GetId(), Field.Store.YES,
                        Field.Index.NOT_ANALYZED));
                    break;

                //PropertyPredicate points to another Subject, most properties fall in here.
                //Properties are stored like
                case RDFExtensions.PropertyType.EntityDirected:
                    var propertyAndValue = propertyCode + WikidataDump.PropertyValueSeparator + ntObject.GetId();
                    luceneDocument.Add(new Field(Labels.PropertyAndValue.ToString(), propertyAndValue, Field.Store.YES,
                        Field.Index.NOT_ANALYZED));
                    break;

                //Other cases, considered but not used.
                default:
                case RDFExtensions.PropertyType.LiteralDirected:
                case RDFExtensions.PropertyType.Other:
                    break;
            }
        }
    }
}
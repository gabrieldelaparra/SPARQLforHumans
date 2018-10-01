using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Support;
using Lucene.Net.Util;
using SparqlForHumans.Core.Utilities;
using SparqlForHumans.Models;
using VDS.RDF;
using Directory = Lucene.Net.Store.Directory;

namespace SparqlForHumans.Core.Services
{
    public static class IndexBuilder
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        public static int NotifyTicks { get; } = 100000;

        /// <summary>
        ///     Takes a dictionary(QEntityType, ListOfProperties) and adds a new Field (IsEntityType) to all Entities
        ///     that have the QEntityType as InstanceOf.
        /// </summary>
        /// <param name="typePropertiesDictionary"></param>
        /// <param name="entitiesIndexDirectory"></param>
        public static void AddIsTypeEntityToEntitiesIndex(Dictionary<string, List<string>> typePropertiesDictionary,
            Directory entitiesIndexDirectory)
        {
            long readCount = 1;

            Options.InternUris = false;
            var analyzer = new KeywordAnalyzer();

            var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            using (var writer = new IndexWriter(entitiesIndexDirectory, indexConfig))
            {
                var documents = MultiDocumentQueries.QueryDocumentsByIds(typePropertiesDictionary.Select(x => x.Key),
                    entitiesIndexDirectory);

                foreach (var document in documents)
                {
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Types Index, Group: {readCount:N0}");

                    var field = new StringField(Labels.IsTypeEntity.ToString(), "true", Field.Store.YES);

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
                foreach (var property in type.Value)
                {
                    if (!dictionary.ContainsKey(property))
                        dictionary.Add(property, new List<string>());

                    dictionary[property].Add(type.Key);
                }

            return dictionary;
        }

        public static Dictionary<string, List<string>> CreateTypesAndPropertiesDictionary(Directory entitiesIndexDirectory)
        {
            var dictionary = new Dictionary<string, List<string>>();

            using (var entityReader = DirectoryReader.Open(entitiesIndexDirectory))
            {
                var docCount = entityReader.NumDocs;
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
                            if (!valuesList.Contains(entityProperty.Id))
                                valuesList.Add(entityProperty.Id);
                    }
                }
            }

            return dictionary;
        }

        //TODO: No 'prefLabel' in the current index:
        //TODO: No 'name' in the current index:
        /// EntitiesIndex
        /// Include Subjects only if Id starts with Q;
        /// Rank with boosts;
        /// Entities have properties, not sure if properties have properties;
        public static void CreateEntitiesIndex(string inputTriplesFilename, Directory outputDirectory,
            bool addBoosts = false)
        {
            long readCount = 1;
            Options.InternUris = false;

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
            var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);

            using (var writer = new IndexWriter(outputDirectory, indexConfig))
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

                    var fields = new List<Field>();

                    foreach (var line in group)
                    {
                        var (ntSubject, ntPredicate, ntObject) = line.GetTripleAsTuple();

                        if (!hasDocument)
                        {
                            id = ntSubject.GetId();
                            Logger.Trace($"Indexing: {id}");
                            luceneDocument = new Document();
                            fields.Add(new StringField(Labels.Id.ToString(), id, Field.Store.YES));

                            hasDocument = true;
                        }

                        fields.AddRange(ParsePredicate(ntPredicate, ntObject));
                    }

                    if (addBoosts)
                    {
                        nodesDictionary.TryGetValue(id, out var subjectIndex);
                       
                        fields.Add(new DoubleField(Labels.Rank.ToString(), nodesGraphRanks[subjectIndex], Field.Store.YES));  
                        AddFields(luceneDocument, fields, nodesGraphRanks[subjectIndex]);
                    }
                    else
                    {
                        AddFields(luceneDocument, fields, 0);
                    }

                    writer.AddDocument(luceneDocument);
                    readCount++;
                }

                writer.Dispose();
                Logger.Info($"Build Entity Index, Group: {readCount:N0}");
            }
        }

        private static IEnumerable<Field> ParsePredicate(INode ntPredicate, INode ntObject)
        {
            var fields = new List<Field>();
            // On the existing Subject
            // If the predicate is a Property, add the property to a list of Properties and link it to the entity.
            // Else, (predicate not a property: Labels, Alt-Labels, Description, etc.)
            //  If the object is not a literal value, continue;
            // Otherwise, add the value to the index on each case.
            switch (ntPredicate.GetPredicateType())
            {
                case RDFExtensions.PredicateType.Property:
                    fields.AddRange(ParsePropertyPredicate(ntPredicate, ntObject));
                    break;
                case RDFExtensions.PredicateType.Label:
                    fields.Add(new TextField(Labels.Label.ToString(), ntObject.GetLiteralValue(),
                        Field.Store.YES));
                    break;
                case RDFExtensions.PredicateType.Description:
                    fields.Add(new TextField(Labels.Description.ToString(),
                        ntObject.GetLiteralValue(), Field.Store.YES));
                    break;
                case RDFExtensions.PredicateType.AltLabel:
                    fields.Add(new TextField(Labels.AltLabel.ToString(), ntObject.GetLiteralValue(),
                        Field.Store.YES));
                    break;
                default:
                case RDFExtensions.PredicateType.Other:
                    break;
            }

            return fields;
        }

        public static void AddDomainTypesToPropertiesIndex(Directory propertiesIndexDirectory,
            Dictionary<string, List<string>> invertedProperties)
        {
            long readCount = 0;

            Options.InternUris = false;
            var analyzer = new KeywordAnalyzer();
            var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            indexConfig.OpenMode = OpenMode.CREATE_OR_APPEND;

            using (var writer = new IndexWriter(propertiesIndexDirectory, indexConfig))
            {
                foreach (var invertedProperty in invertedProperties)
                {
                    var document =
                        SingleDocumentQueries.QueryDocumentById(invertedProperty.Key, propertiesIndexDirectory);

                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Types Index, Group: {readCount:N0}");

                    foreach (var domainType in invertedProperty.Value)
                    {
                        var field = new TextField(Labels.DomainType.ToString(), domainType, Field.Store.YES);

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
        public static void CreatePropertiesIndex(string inputTriplesFilename, Directory outputDirectory,
            bool indexFrequency = false)
        {
            long readCount = 1;
            Options.InternUris = false;
            var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            var dictionary = new Dictionary<string, int>();

            if (indexFrequency)
                dictionary = PropertiesFrequency.GetPropertiesFrequency(inputTriplesFilename);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);

            Logger.Info("Building Properties Index");
            var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            using (var writer = new IndexWriter(outputDirectory, indexConfig))
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

                    var fields = new List<Field>();

                    foreach (var line in group)
                    {
                        var (ntSubject, ntPredicate, ntObject) = line.GetTripleAsTuple();

                        if (!hasDocument)
                        {
                            var id = ntSubject.GetId();
                            propertyId = id;
                            Logger.Trace($"Indexing: {id}");
                            luceneDocument = new Document();
                            fields.Add(new StringField(Labels.Id.ToString(), id, Field.Store.YES));
                            
                            hasDocument = true;
                        }

                        fields.AddRange(ParsePredicate(ntPredicate, ntObject));
                    }

                    if (indexFrequency)
                    {
                        if (dictionary.TryGetValue(propertyId, out var value))
                        {
                            fields.Add(new DoubleField(Labels.Rank.ToString(), value, Field.Store.YES));
                            AddFields(luceneDocument, fields, value);
                        }
                    }
                    else
                    {
                        AddFields(luceneDocument, fields, 0);
                    }


                    writer.AddDocument(luceneDocument);
                    readCount++;
                }

                writer.Dispose();
            }

            Logger.Info($"Build Property Index, Group: {readCount:N0}");
        }

        public static void AddFields(Document doc, IEnumerable<Field> fields, double boost = 0)
        {
            foreach (var field in fields)
            {
                //if (boost != 0)
                //    if(!field.FieldType.OmitNorms && field.FieldType.IsIndexed)
                //        field.Boost = (float) boost;

                doc.Add(field);
            }
        }

        /// <summary>
        ///     Used on creating a PropertyIndex.
        ///     Line example:
        ///     - P333 P31 P444.
        ///     - P333 is the subject.
        ///     - P31 is InstanceOf. (Could be a EntityDirected Predicate)
        ///     - P444 is the Type (InstanceOf) of P333.
        ///     Called for each property of a Property Subject.
        ///     Takes a Predicate, Object and a Property (Subject) Document.
        ///     Adds a new Field to the Document: InstanceOf, EntityDirected (With separators, not sure why).
        /// </summary>
        /// <param name="ntPredicate"></param>
        /// <param name="ntObject"></param>
        /// <param name="luceneDocument"></param>
        private static IEnumerable<Field> ParsePropertyPredicate(INode ntPredicate, INode ntObject)
        {
            var fields = new List<Field>();
            var propertyCode = ntPredicate.GetId();

            //Stores the P-Id of the PropertyPredicate (of the PropertyEntity).
            fields.Add(new StringField(Labels.Property.ToString(), propertyCode, Field.Store.YES));

            switch (RDFExtensions.GetPropertyType(ntPredicate, ntObject))
            {
                //PropertyPredicate is InstanceOf another type of Property:
                case RDFExtensions.PropertyType.InstanceOf:
                    fields.Add(new StringField(Labels.InstanceOf.ToString(), ntObject.GetId(), Field.Store.YES));
                    break;

                //PropertyPredicate points to another Subject, most properties fall in here.
                //Properties are stored like
                case RDFExtensions.PropertyType.EntityDirected:
                    var propertyAndValue = propertyCode + WikidataDump.PropertyValueSeparator + ntObject.GetId();
                    fields.Add(new StringField(Labels.PropertyAndValue.ToString(), propertyAndValue, Field.Store.YES));
                    break;

                //Other cases, considered but not used.
                default:
                case RDFExtensions.PropertyType.LiteralDirected:
                case RDFExtensions.PropertyType.Other:
                    break;
            }

            return fields;
        }
    }
}
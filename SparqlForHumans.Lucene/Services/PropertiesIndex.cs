using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Services.Query;
using SparqlForHumans.Lucene.Utilities;
using SparqlForHumans.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Services
{
    public static class PropertiesIndex
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();
        public static int NotifyTicks { get; } = 100000;

        public static void CreatePropertiesIndex(string inputTriplesFilename, bool indexFrequency = false)
        {
            using (var propertiesDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                CreatePropertiesIndex(inputTriplesFilename, propertiesDirectory, indexFrequency);
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

                        fields.AddRange(EntitiesIndex.ParsePredicate(ntPredicate, ntObject));
                    }

                    if (indexFrequency)
                    {
                        if (dictionary.TryGetValue(propertyId, out var value))
                        {
                            fields.Add(new DoubleField(Labels.Rank.ToString(), value, Field.Store.YES));
                            IndexBuilder.AddFields(luceneDocument, fields, value);
                        }
                    }
                    else
                    {
                        IndexBuilder.AddFields(luceneDocument, fields, 0);
                    }


                    writer.AddDocument(luceneDocument);
                    readCount++;
                }

                writer.Dispose();
            }

            Logger.Info($"Build Property Index, Group: {readCount:N0}");
        }

        public static void AddDomainTypesToPropertiesIndex(Dictionary<string, List<string>> invertedProperties)
        {
            using (var propertiesIndexDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                AddDomainTypesToPropertiesIndex(propertiesIndexDirectory, invertedProperties);
            }
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

                    if (document == null) continue;

                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Add Domain Types to Types Index, Group: {readCount:N0}");

                    foreach (var domainType in invertedProperty.Value)
                    {
                        var field = new TextField(Labels.DomainType.ToString(), domainType, Field.Store.YES);

                        document.Add(field);
                    }

                    writer.UpdateDocument(new Term(Labels.Id.ToString(), invertedProperty.Key), document);
                    readCount++;
                }
            }

            Logger.Info($"Add Domain Types to Types Index, Group: {readCount:N0}");
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
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

            var dictionary = new Dictionary<int, int>();

            if (indexFrequency)
                dictionary = PropertiesFrequency.GetPropertiesFrequency(inputTriplesFilename);

            var lines = FileHelper.GetInputLines(inputTriplesFilename);

            Logger.Info("Building Properties Index");
            var indexConfig = IndexConfiguration.CreateStandardIndexWriterConfig();
            using (var writer = new IndexWriter(outputDirectory, indexConfig))
            {
                //Group them by QCode.
                var entityGroups = lines.GroupBySubject();

                foreach (var group in entityGroups)
                {
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Property Index, Group: {readCount:N0}");

                    var subject = group.FirstOrDefault().AsTuple().subject;

                    //Excludes Entities, will only add properties.
                    if (!subject.IsEntityP())
                        continue;

                    var propertyId = subject.GetIntId();

                    //Flag to create a new Lucene Document
                    var hasDocument = false;

                    //Lucene document for each property
                    var luceneDocument = new Document();

                    var fields = new List<Field>();

                    foreach (var line in group)
                    {
                        var (ntSubject, ntPredicate, ntObject) = line.AsTuple();

                        if (!hasDocument)
                        {
                            var id = ntSubject.GetId();
                            propertyId = id.ToNumbers();
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

        public static void AddDomainTypesToPropertiesIndex(Dictionary<int, int[]> invertedProperties)
        {
            using (var propertiesIndexDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                AddDomainTypesToPropertiesIndex(propertiesIndexDirectory, invertedProperties);
            }
        }

        public static void AddDomainTypesToPropertiesIndex(Directory propertiesIndexDirectory,
            Dictionary<int, int[]> invertedProperties)
        {
            long readCount = 0;

            // Not sure why Keyword and not Standard Analyzer. (Tests Fail) 
            var indexConfig = IndexConfiguration.CreateKeywordIndexWriterConfig();

            using (var writer = new IndexWriter(propertiesIndexDirectory, indexConfig))
            {
                foreach (var invertedProperty in invertedProperties)
                {
                    var propertyId = $"P{invertedProperty.Key}";
                    var document =
                        SingleDocumentQueries.QueryDocumentById(propertyId, propertiesIndexDirectory);

                    if (document == null) continue;

                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Add Domain Types to Types Index, Group: {readCount:N0}");

                    foreach (var domainType in invertedProperty.Value)
                    {
                        var typeIds = $"Q{domainType}";
                        var field = new TextField(Labels.DomainType.ToString(), typeIds, Field.Store.YES);

                        document.Add(field);
                    }

                    writer.UpdateDocument(new Term(Labels.Id.ToString(), propertyId), document);
                    readCount++;
                }
            }

            Logger.Info($"Add Domain Types to Types Index, Group: {readCount:N0}");
        }
    }
}
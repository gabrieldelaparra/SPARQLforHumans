using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using System.Linq;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests.Index
{
    public class EntitiesIndexerTests
    {
        [Fact]
        public void TestCreateEntityIndex5k()
        {
            const string filename = "Resources/EntityIndex5k.nt";
            const string outputPath = "IndexEntities5k";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();

            //using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                Assert.True(Directory.Exists(outputPath));

                //var queryBerlin = MultiDocumentQueries.QueryEntitiesByLabel("Berli", luceneDirectory).ToArray();
                var queryBerlin = new MultiLabelQuery(outputPath, "Berli").QueryDocuments().ToEntities().ToArray();
                Assert.NotEmpty(queryBerlin);
                var result = queryBerlin[0];
                Assert.Equal("Q64", result.Id);
                Assert.Equal("Berlin", result.Label);
                Assert.Equal("capital city of Germany", result.Description);
                Assert.Contains("Berlin, Germany", result.AltLabels);
                var properties = result.Properties.Select(x => x.Id).ToArray();
                Assert.Contains("P17", properties);
                Assert.Contains("P1376", properties);
                Assert.False(result.IsType);
                Assert.Contains("Q515", result.InstanceOf);
                Assert.Contains("Q5119", result.InstanceOf);
                Assert.Contains("Q999", result.SubClass);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateEntityIndexAddsFolders()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexCreateFolder";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();
            ;

            Assert.True(Directory.Exists(outputPath));

            outputPath.DeleteIfExists();
        }


        [Fact]
        public void TestCreateEntityMultipleInstanceIndex()
        {
            const string filename = "Resources/EntityIndexMultipleInstance.nt";
            const string outputPath = "IndexMultiple";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            ;

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var docCount = reader.MaxDoc;

                    Assert.Equal(3, docCount);

                    //Q26, Q27, Q29
                    var doc = reader.Document(0);
                    Assert.NotNull(doc);
                    Assert.Equal("Q26", doc.GetField(Labels.Id.ToString()).GetStringValue());
                    Assert.Equal("Northern Ireland", doc.GetField(Labels.Label.ToString()).GetStringValue());

                    doc = reader.Document(1);
                    Assert.NotNull(doc);
                    Assert.Equal("Q27", doc.GetField(Labels.Id.ToString()).GetStringValue());
                    Assert.Equal("Ireland", doc.GetField(Labels.Label.ToString()).GetStringValue());

                    doc = reader.Document(2);
                    Assert.NotNull(doc);
                    Assert.Equal("Q29", doc.GetField(Labels.Id.ToString()).GetStringValue());
                    Assert.Equal("Spain", doc.GetField(Labels.Label.ToString()).GetStringValue());
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateEntityMultipleInstanceIndexDocCount()
        {
            const string filename = "Resources/EntityIndexMultipleInstance.nt";
            const string outputPath = "IndexDocCount";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            ;

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var docCount = reader.MaxDoc;
                    Assert.Equal(3, docCount);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestCreateIndexAddTypesFields()
        {
            const string filename = "Resources/EntityIndexTypes.nt";
            const string outputPath = "EntityIndexAddIsType";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var obamaDocument = new SingleIdQuery(outputPath, "Q76").QueryDocuments().FirstOrDefault();
                var personDocument = new SingleIdQuery(outputPath, "Q5").QueryDocuments().FirstOrDefault();
                var countryDocument = new SingleIdQuery(outputPath, "Q17").QueryDocuments().FirstOrDefault();
                var chileDocument = new SingleIdQuery(outputPath, "Q298").QueryDocuments().FirstOrDefault();

                Assert.Equal("Q76", obamaDocument.GetValue(Labels.Id));
                Assert.Equal("Q5", personDocument.GetValue(Labels.Id));
                Assert.Equal("Q17", countryDocument.GetValue(Labels.Id));
                Assert.Equal("Q298", chileDocument.GetValue(Labels.Id));

                Assert.Empty(obamaDocument.GetValue(Labels.IsTypeEntity));
                Assert.Empty(chileDocument.GetValue(Labels.IsTypeEntity));
                Assert.True(bool.Parse(personDocument.GetValue(Labels.IsTypeEntity)));
                Assert.True(bool.Parse(countryDocument.GetValue(Labels.IsTypeEntity)));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateIndexDocumentCountEqualsGroupsCounts()
        {
            var filename = "Resources/Filter500.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var outputPath = "Index500Count";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneIndexDirectory))
                {
                    Assert.Equal(entitiesCount, reader.NumDocs);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexAltLabel()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexSingleAltLabel";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);
                    Assert.Equal(3, doc.GetValues(Labels.AltLabel).Length);

                    Assert.Equal("NIR", doc.GetValues(Labels.AltLabel)[0]);
                    Assert.Equal("UKN", doc.GetValues(Labels.AltLabel)[1]);
                    Assert.Equal("North Ireland", doc.GetValues(Labels.AltLabel)[2]);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDescription()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexSingleDescription";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);
                    Assert.Equal("region in north-west Europe, part of the United Kingdom",
                        doc.GetValue(Labels.Description));
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDocCount()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexDocCount";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var docCount = reader.MaxDoc;
                    Assert.Equal(1, docCount);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDocNotNull()
        {
            const string filename = "Resources/EntityIndexMultipleInstance.nt";
            const string outputPath = "IndexDocNotNull";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc1 = reader.Document(0);
                    var doc2 = reader.Document(1);
                    var doc3 = reader.Document(2);

                    Assert.NotNull(doc1);
                    Assert.NotNull(doc2);
                    Assert.NotNull(doc3);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexId()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexSingleId";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);
                    Assert.Equal("Q26", doc.GetValue(Labels.Id));
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexInstanceOf()
        {
            const string filename = "Resources/EntityIndexTwoInstanceOf.nt";
            const string outputPath = "TwoInstanceOfIndex";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);

                    var instanceOfArray = doc.GetValues(Labels.InstanceOf);
                    Assert.NotNull(instanceOfArray);
                    Assert.Equal(2, instanceOfArray.Length);
                    Assert.Equal("Q145", instanceOfArray[0]);
                    Assert.Equal("Q27", instanceOfArray[1]);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexIsType()
        {
            const string filename = "Resources/EntityIndexTwoInstanceOfWithTypes.nt";
            const string outputPath = "TwoInstanceOfIndexWithTypes";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);
                    Assert.Empty(doc.GetValue(Labels.IsTypeEntity));
                    doc = reader.Document(1);
                    Assert.True(bool.Parse(doc.GetValue(Labels.IsTypeEntity)));
                    doc = reader.Document(2);
                    Assert.True(bool.Parse(doc.GetValue(Labels.IsTypeEntity)));
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexLabel()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexSingleLabel";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);
                    Assert.Equal("Northern Ireland", doc.GetValue(Labels.Label));
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexProperties()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexSingleProperties";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);

                    Assert.Equal(3, doc.GetValues(Labels.Property).Length);

                    Assert.Equal("P17", doc.GetValues(Labels.Property)[0]);
                    Assert.Equal("P47", doc.GetValues(Labels.Property)[1]);
                    Assert.Equal("P131", doc.GetValues(Labels.Property)[2]);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexPropertiesAndValues()
        {
            Assert.False(true);
            //const string filename = "Resources/SingleInstanceDump.nt";
            //const string outputPath = "IndexSinglePropAndValues";

            //outputPath.DeleteIfExists();

            //using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            //{
            //    IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
            //    using (var reader = DirectoryReader.Open(luceneDirectory))
            //    {
            //        var doc = reader.Document(0);

            //        Assert.Equal(4, doc.GetValues(Labels.PropertyAndValue).Length);

            //        Assert.Equal("P17##Q145", doc.GetValue(Labels.PropertyAndValue));

            //        Assert.Equal("P17##Q145", doc.GetValues(Labels.PropertyAndValue)[0]);
            //        Assert.Equal("P47##Q27", doc.GetValues(Labels.PropertyAndValue)[1]);
            //        Assert.Equal("P30##Q46", doc.GetValues(Labels.PropertyAndValue)[2]);
            //        Assert.Equal("P131##Q145", doc.GetValues(Labels.PropertyAndValue)[3]);
            //    }
            //}

            //outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexSubClass()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexSingleSubClass";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);

                    Assert.Single(doc.GetValues(Labels.SubClass));

                    Assert.Equal("Q46", doc.GetValues(Labels.SubClass)[0]);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestIndexHasTypes()
        {
            const string filename = "Resources/EntityTypes.nt";
            const string outputPath = "IndexTypes";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();

            var typesQuery = new MultiLabelQuery(outputPath, "*").QueryDocuments().ToEntities().ToArray();
            //using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                //var typesQuery = MultiDocumentQueries.QueryEntitiesByLabel("*", luceneDirectory, true).ToArray();
                Assert.NotEmpty(typesQuery);
                Assert.Contains(typesQuery, x => x.Id.Equals("Q5"));
            }

            outputPath.DeleteIfExists();
        }
    }
}
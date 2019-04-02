using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class EntityIndexTests
    {
        [Fact]
        public void TestCreateEntityIndex500()
        {
            const string filename = "Resources/Filter5k.nt";
            const string outputPath = "Index5k";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);

                Assert.True(Directory.Exists(outputPath));

                var q1 = MultiDocumentQueries.QueryEntitiesByLabel("Berlin", luceneDirectory);
                Assert.NotNull(q1);
                Assert.Contains("Berlin", q1.FirstOrDefault().Label);

                var q2 = MultiDocumentQueries.QueryEntitiesByLabel("Obama", luceneDirectory);
                Assert.NotNull(q2);
                Assert.Contains("Barack Obama", q2.FirstOrDefault().Label);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
            }

            Assert.True(Directory.Exists(outputPath));

            outputPath.DeleteIfExists();
        }


        [Fact]
        public void TestCreateEntityMultipleInstanceIndex()
        {
            const string filename = "Resources/EntityIndexMultipleInstance.nt";
            const string outputPath = "IndexMultiple";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var docCount = reader.MaxDoc;
                    Assert.Equal(3, docCount);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var docCount = reader.MaxDoc;
                    Assert.Equal(1, docCount);
                }
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

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory);
                using (var reader = DirectoryReader.Open(luceneIndexDirectory))
                {
                    Assert.Equal(entitiesCount, reader.NumDocs);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
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
        public void TestCreateSingleInstanceIndexLabel()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "IndexSingleLabel";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
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

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, false);
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);

                    Assert.Equal(4, doc.GetValues(Labels.Property).Length);

                    Assert.Equal("P17", doc.GetValues(Labels.Property)[0]);
                    Assert.Equal("P47", doc.GetValues(Labels.Property)[1]);
                    Assert.Equal("P30", doc.GetValues(Labels.Property)[2]);
                    Assert.Equal("P131", doc.GetValues(Labels.Property)[3]);
                }
            }

            outputPath.DeleteIfExists();
        }

        //[Fact]
        //public void TestCreateSingleInstanceIndexPropertiesAndValues()
        //{
        //    const string filename = "Resources/SingleInstanceDump.nt";
        //    const string outputPath = "IndexSinglePropAndValues";

        //    outputPath.DeleteIfExists();

        //    using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
        //    {
        //        IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
        //        using (var reader = DirectoryReader.Open(luceneDirectory))
        //        {
        //            var doc = reader.Document(0);

        //            Assert.Equal(4, doc.GetValues(Labels.PropertyAndValue).Length);

        //            Assert.Equal("P17##Q145", doc.GetValue(Labels.PropertyAndValue));

        //            Assert.Equal("P17##Q145", doc.GetValues(Labels.PropertyAndValue)[0]);
        //            Assert.Equal("P47##Q27", doc.GetValues(Labels.PropertyAndValue)[1]);
        //            Assert.Equal("P30##Q46", doc.GetValues(Labels.PropertyAndValue)[2]);
        //            Assert.Equal("P131##Q145", doc.GetValues(Labels.PropertyAndValue)[3]);
        //        }
        //    }

        //    outputPath.DeleteIfExists();
        //}
    }
}
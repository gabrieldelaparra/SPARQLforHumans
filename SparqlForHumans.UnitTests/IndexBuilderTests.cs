using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using SparqlForHumans.Models;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class IndexBuilderTests
    {
        [Fact]
        public void TestCreateIndex500()
        {
            const string filename = "Resources/filtered-All-500.nt";
            const string outputPath = "Index500";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory);

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
        public void TestCreateIndexAddsFolders()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexCreateFolder";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
            }

            Assert.True(Directory.Exists(outputPath));

            outputPath.DeleteIfExists();
        }


        [Fact]
        public void TestCreateMultipleInstanceIndex()
        {
            const string filename = "Resources/MultipleInstanceDump.nt";
            const string outputPath = "IndexMultiple";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
        public void TestCreateMultipleInstanceIndexDocCount()
        {
            const string filename = "Resources/MultipleInstanceDump.nt";
            const string outputPath = "IndexDocCount";
            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleAltLabel";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);
                    Assert.Equal(3, doc.GetValues(Labels.AltLabel).Length);

                    Assert.Equal("NIR", doc.GetValue(Labels.AltLabel));

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
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleDescription";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexDocCount";
            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
            const string filename = "Resources/MultipleInstanceDump.nt";
            const string outputPath = "IndexDocNotNull";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleId";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
            const string filename = "Resources/TwoInstanceOf.nt";
            const string outputPath = "TwoInstanceOfIndex";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleLabel";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
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
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleProperties";

            outputPath.DeleteIfExists();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneDirectory, false);
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);

                    Assert.Equal(4, doc.GetValues(Labels.Property).Length);

                    Assert.Equal("P17", doc.GetValue(Labels.Property));

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

        /// <summary>
        ///     An issue while testing ranking, is that ranking it's not being displayed when the index is read.
        ///     Not sure why is this or if this might an issue of the current Lucene.Net library version.
        ///     So far, the only found way of testing whether the index has ranking or not, is to create
        ///     an index of a closed set, then search for specific items and check whether they appear
        ///     as first on the search list.
        /// </summary>
        [Fact]
        public void TestHasRanking()
        {
            const string filename = "Resources/filtered-All-500.nt";
            const string outputPath1 = "IndexRank1";
            const string outputPath2 = "IndexRank2";

            outputPath1.DeleteIfExists();
            outputPath2.DeleteIfExists();

            var found = 0;

            using (var outputDirectory1 = FSDirectory.Open(outputPath1.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, outputDirectory1, true);
            }

            using (var outputDirectory2 = FSDirectory.Open(outputPath2.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, outputDirectory2, false);
            }

            using (var outputDirectory1 = FSDirectory.Open(outputPath1.GetOrCreateDirectory()))
            using (var outputDirectory2 = FSDirectory.Open(outputPath2.GetOrCreateDirectory()))
            using (var outputReader1 = DirectoryReader.Open(outputDirectory1))
            using (var outputReader2 = DirectoryReader.Open(outputDirectory2))
            {
                var readerNoBoost = new IndexSearcher(outputReader1);
                var readerWithBoost = new IndexSearcher(outputReader2);

                var docCountNoBoost = readerNoBoost.IndexReader.MaxDoc;
                var docCountWithBoost = readerWithBoost.IndexReader.MaxDoc;

                Assert.Equal(docCountNoBoost, docCountWithBoost);

                var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

                QueryParser parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48,
                    new[] {Labels.Label.ToString(), Labels.AltLabel.ToString()},
                    analyzer);

                string[] testWords =
                    {"obama", "europe", "ireland", "apple", "america", "human", "park", "city", "germany", "france"};

                foreach (var searchQuery in testWords)
                {
                    const int resultsLimit = 10;
                    var queryNoBoost =
                        MultiDocumentQueries.QueryDocumentsByLabel(searchQuery, outputDirectory1, false, resultsLimit);
                    var queryBoost =
                        MultiDocumentQueries.QueryDocumentsByLabel(searchQuery, outputDirectory2, false, resultsLimit);

                    Assert.Equal(queryNoBoost.Count(), queryBoost.Count());

                    for (var i = 0; i < queryNoBoost.Count(); i++)
                        if (!queryNoBoost.ElementAt(i).GetValue(Labels.Id)
                            .Equals(queryBoost.ElementAt(i).GetValue(Labels.Id)))
                            found++;
                }
            }

            Assert.NotEqual(0, found);

            outputPath1.DeleteIfExists();
            outputPath2.DeleteIfExists();
        }
    }
}
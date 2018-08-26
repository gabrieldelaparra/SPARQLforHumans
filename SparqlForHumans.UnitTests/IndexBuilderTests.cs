using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Util;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class IndexBuilderTests
    {
        [Fact]
        public void TestCreateIndexAddsFolders()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexCreateFolder";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));
            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);
            Assert.True(Directory.Exists(outputPath));

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDocCount()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexDocCount";
            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;
                Assert.Equal(1, docCount);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateMultipleInstanceIndexDocCount()
        {
            const string filename = "Resources/MultipleInstanceDump.nt";
            const string outputPath = "IndexDocCount";
            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;
                Assert.Equal(3, docCount);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDocNotNull()
        {
            const string filename = "Resources/MultipleInstanceDump.nt";
            const string outputPath = "IndexDocNotNull";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc1 = reader.Document(0);
                var doc2 = reader.Document(1);
                var doc3 = reader.Document(2);

                Assert.NotNull(doc1);
                Assert.NotNull(doc2);
                Assert.NotNull(doc3);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexId()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleId";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc = reader.Document(0);
                Assert.Equal("Q26", doc.GetValue(Labels.Id));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexLabel()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleLabel";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc = reader.Document(0);
                Assert.Equal("Northern Ireland", doc.GetValue(Labels.Label));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexAltLabel()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleAltLabel";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc = reader.Document(0);
                Assert.Equal(3, doc.GetValues(Labels.AltLabel).Length);

                Assert.Equal("NIR", doc.GetValue(Labels.AltLabel));

                Assert.Equal("NIR", doc.GetValues(Labels.AltLabel)[0]);
                Assert.Equal("UKN", doc.GetValues(Labels.AltLabel)[1]);
                Assert.Equal("North Ireland", doc.GetValues(Labels.AltLabel)[2]);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDescription()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleDescription";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc = reader.Document(0);
                Assert.Equal("region in north-west Europe, part of the United Kingdom", doc.GetValue(Labels.Description));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexInstanceOf()
        {
            const string filename = "Resources/TwoInstanceOf.nt";
            const string outputPath = "TwoInstanceOfIndex";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc = reader.Document(0);

                var instanceOfArray = doc.GetValues(Labels.InstanceOf);
                Assert.NotNull(instanceOfArray);
                Assert.Equal(2, instanceOfArray.Length);
                Assert.Equal("Q145",instanceOfArray[0]);
                Assert.Equal("Q27",instanceOfArray[1]);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexPropertiesAndValues()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSinglePropAndValues";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc = reader.Document(0);

                Assert.Equal(4, doc.GetValues(Labels.PropertyAndValue).Length);

                Assert.Equal("P17##Q145", doc.GetValue(Labels.PropertyAndValue));

                Assert.Equal("P17##Q145", doc.GetValues(Labels.PropertyAndValue)[0]);
                Assert.Equal("P47##Q27", doc.GetValues(Labels.PropertyAndValue)[1]);
                Assert.Equal("P30##Q46", doc.GetValues(Labels.PropertyAndValue)[2]);
                Assert.Equal("P131##Q145", doc.GetValues(Labels.PropertyAndValue)[3]);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexProperties()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingleProperties";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var doc = reader.Document(0);

                Assert.Equal(4, doc.GetValues(Labels.Property).Length);

                Assert.Equal("P17", doc.GetValue(Labels.Property));

                Assert.Equal("P17", doc.GetValues(Labels.Property)[0]);
                Assert.Equal("P47", doc.GetValues(Labels.Property)[1]);
                Assert.Equal("P30", doc.GetValues(Labels.Property)[2]);
                Assert.Equal("P131", doc.GetValues(Labels.Property)[3]);
            }

            outputPath.DeleteIfExists();
        }



        [Fact]
        public void TestCreateMultipleInstanceIndex()
        {
            const string filename = "Resources/MultipleInstanceDump.nt";
            const string outputPath = "IndexMultiple";

            outputPath.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;

                Assert.Equal(3, docCount);

                //Q26, Q27, Q29
                var doc = reader.Document(0);
                Assert.NotNull(doc);
                Assert.Equal("Q26", doc.GetField(Labels.Id.ToString()).StringValue);
                Assert.Equal("Northern Ireland", doc.GetField(Labels.Label.ToString()).StringValue);

                doc = reader.Document(1);
                Assert.NotNull(doc);
                Assert.Equal("Q27", doc.GetField(Labels.Id.ToString()).StringValue);
                Assert.Equal("Ireland", doc.GetField(Labels.Label.ToString()).StringValue);

                doc = reader.Document(2);
                Assert.NotNull(doc);
                Assert.Equal("Q29", doc.GetField(Labels.Id.ToString()).StringValue);
                Assert.Equal("Spain", doc.GetField(Labels.Label.ToString()).StringValue);
            }

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }

        /// <summary>
        /// An issue while testing ranking, is that ranking it's not being displayed when the index is read.
        /// Not sure why is this or if this might an issue of the current Lucene.Net library version.
        /// So far, the only found way of testing whether the index has ranking or not, is to create
        /// an index of a closed set, then search for specific items and check whether they appear
        /// as first on the search list.
        /// </summary>
        [Fact]
        public void TestHasRanking()
        {
            const string filename = "Resources/filtered-All-500.nt";
            const string outputPath1 = "IndexRank1";
            const string outputPath2 = "IndexRank2";

            outputPath1.DeleteIfExists();
            outputPath2.DeleteIfExists();

            IndexBuilder.CreateEntitiesIndex(filename, outputPath1);
            IndexBuilder.CreateEntitiesIndex(filename, outputPath2, false);

            var found = 0;

            using (var readerNoBoost = new IndexSearcher(outputPath1.GetLuceneDirectory(), true))
            using (var readerWithBoost = new IndexSearcher(outputPath2.GetLuceneDirectory(), true))
            {
                var docCountNoBoost = readerNoBoost.MaxDoc;
                var docCountWithBoost = readerWithBoost.MaxDoc;

                Assert.Equal(docCountNoBoost, docCountWithBoost);

                var analyzer = new StandardAnalyzer(Version.LUCENE_30);

                QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30,
                    new[] { Labels.Label.ToString(), Labels.AltLabel.ToString() },
                    analyzer);

                string[] testWords = { "obama", "europe", "ireland", "apple", "america", "human", "park", "city", "germany", "france" };

                foreach (var searchQuery in testWords)
                {
                    const int resultsLimit = 10;

                    var hitsNoBoost = readerNoBoost.Search(parser.Parse(searchQuery.Trim()), null, resultsLimit).ScoreDocs;
                    var hitsWithBoost = readerWithBoost.Search(parser.Parse(searchQuery.Trim()), null, resultsLimit).ScoreDocs;

                    Assert.Equal(hitsNoBoost.Count(), hitsWithBoost.Count());

                    for (var i = 0; i < hitsWithBoost.Count(); i++)
                    {
                        var docNoBoost = readerNoBoost.Doc(hitsNoBoost[i].Doc);
                        var docWithBoost = readerWithBoost.Doc(hitsWithBoost[i].Doc);
                        if (!docNoBoost.GetValue(Labels.Label).Equals(docWithBoost.GetValue(Labels.Label)))
                            found++;
                    }
                }
                analyzer.Close();
                readerNoBoost.Dispose();
                readerWithBoost.Dispose();
            }
            Assert.NotEqual(0, found);

            outputPath1.DeleteIfExists();
            outputPath2.DeleteIfExists();
        }

        [Fact]
        public void TestCreateIndex500()
        {
            const string filename = "Resources/filtered-All-500.nt";
            const string outputPath = "Index500";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath);

            Assert.True(Directory.Exists(outputPath));

            var q1 = QueryService.QueryEntitiesByLabel("Berlin", outputPath.GetLuceneDirectory());
            Assert.NotNull(q1);
            Assert.Contains("Berlin", q1.FirstOrDefault().Label);

            var q2 = QueryService.QueryEntitiesByLabel("Obama", outputPath.GetLuceneDirectory());
            Assert.NotNull(q2);
            Assert.Contains("Barack Obama", q2.FirstOrDefault().Label);

            outputPath.DeleteIfExists();
        }
    }
}
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
        public void TestCreateBasicSingleInstanceIndex()
        {
            const string filename = "Resources/SingleInstanceDump.nt";
            const string outputPath = "IndexSingle";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            Assert.True(Directory.Exists(outputPath));

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;

                Assert.Equal(1, docCount);

                var doc = reader.Document(0);
                Assert.NotNull(doc);
                Assert.Equal("Q26", doc.GetField(Labels.Id.ToString()).StringValue);
                Assert.Equal("Northern Ireland", doc.GetField(Labels.Label.ToString()).StringValue);

                //No 'prefLabel' in the current index:
                //Assert.Equal("Northern Ireland", doc.GetField(Labels.Label.ToString()).StringValue);

                //No 'name' in the current index:
                //Assert.Equal("Northern Ireland", doc.GetField(Labels.Label.ToString()).StringValue);

                //Alt-Label:
                Assert.Equal(3, doc.GetFields(Labels.AltLabel.ToString()).Length);

                Assert.Equal("NIR", doc.GetField(Labels.AltLabel.ToString()).StringValue);

                Assert.Equal("NIR", doc.GetFields(Labels.AltLabel.ToString())[0].StringValue);
                Assert.Equal("UKN", doc.GetFields(Labels.AltLabel.ToString())[1].StringValue);
                Assert.Equal("North Ireland", doc.GetFields(Labels.AltLabel.ToString())[2].StringValue);

                //Description
                Assert.Equal("region in north-west Europe, part of the United Kingdom", doc.GetField(Labels.Description.ToString()).StringValue);

                //Properties and Values
                Assert.Equal(4, doc.GetFields(Labels.PropertyAndValue.ToString()).Length);

                Assert.Equal("P17##Q145", doc.GetField(Labels.PropertyAndValue.ToString()).StringValue);

                Assert.Equal("P17##Q145", doc.GetFields(Labels.PropertyAndValue.ToString())[0].StringValue);
                Assert.Equal("P47##Q27", doc.GetFields(Labels.PropertyAndValue.ToString())[1].StringValue);
                Assert.Equal("P30##Q46", doc.GetFields(Labels.PropertyAndValue.ToString())[2].StringValue);
                Assert.Equal("P131##Q145", doc.GetFields(Labels.PropertyAndValue.ToString())[3].StringValue);

                //Properties
                Assert.Equal(4, doc.GetFields(Labels.Property.ToString()).Length);

                Assert.Equal("P17", doc.GetField(Labels.Property.ToString()).StringValue);

                Assert.Equal("P17", doc.GetFields(Labels.Property.ToString())[0].StringValue);
                Assert.Equal("P47", doc.GetFields(Labels.Property.ToString())[1].StringValue);
                Assert.Equal("P30", doc.GetFields(Labels.Property.ToString())[2].StringValue);
                Assert.Equal("P131", doc.GetFields(Labels.Property.ToString())[3].StringValue);

            }

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }

        [Fact]
        public void TestCreateBasicMultipleInstanceIndex()
        {
            const string filename = "Resources/MultipleInstanceDump.nt";
            const string outputPath = "IndexMultiple";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, false);

            Assert.True(Directory.Exists(outputPath));

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

            if (Directory.Exists(outputPath1))
                Directory.Delete(outputPath1, true);

            if (Directory.Exists(outputPath2))
                Directory.Delete(outputPath2, true);

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

                string[] testWords = { "obama", "chile", "ireland", "apple", "orange", "human", "country", "car", "plane", "bed" };

                foreach (var searchQuery in testWords)
                {
                    const int resultsLimit = 10;

                    var hits1 = readerNoBoost.Search(parser.Parse(searchQuery.Trim()), null, resultsLimit).ScoreDocs;
                    var hits2 = readerWithBoost.Search(parser.Parse(searchQuery.Trim()), null, resultsLimit).ScoreDocs;

                    Assert.Equal(hits1.Count(), hits2.Count());

                    for (var i = 0; i < hits2.Count(); i++)
                    {
                        var doc1 = readerNoBoost.Doc(hits1[i].Doc);
                        var doc2 = readerWithBoost.Doc(hits2[i].Doc);
                        if (!doc1.GetLabel().Equals(doc2.GetLabel()))
                            found++;
                    }
                }
                analyzer.Close();
                readerNoBoost.Dispose();
                readerWithBoost.Dispose();
            }
            Assert.NotEqual(0, found);

            if (Directory.Exists(outputPath1))
                Directory.Delete(outputPath1, true);

            if (Directory.Exists(outputPath2))
                Directory.Delete(outputPath2, true);
        }

        [Fact]
        public void TestCreateIndex500()
        {
            const string filename = "Resources/filtered-All-500.nt";
            const string outputPath = "Index500";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath);

            Assert.True(Directory.Exists(outputPath));

            var q1 = QueryService.QueryEntitiesByLabel("Berlin", outputPath.GetLuceneDirectory());
            Assert.NotNull(q1);
            Assert.Contains("Berlin", q1.FirstOrDefault().Label);

            var q2 = QueryService.QueryEntitiesByLabel("Obama", outputPath.GetLuceneDirectory());
            Assert.NotNull(q2);
            Assert.Contains("Barack Obama", q2.FirstOrDefault().Label);

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }
    }
}
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
        public void TestCreateBasicIndex()
        {
            var filename = "Resources/filtered.nt";
            var outputPath = "Index";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath);

            Assert.True(Directory.Exists(outputPath));

            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupByEntities();
            var groupsCount = groups.Count();

            using (var reader = IndexReader.Open(LuceneHelper.GetLuceneDirectory(outputPath), true))
            {
                var docCount = reader.MaxDoc;

                Assert.Equal(docCount, groupsCount);

                for (var i = 0; i < (docCount > 10 ? 10 : docCount); i++)
                {
                    var doc = reader.Document(i);
                    Assert.NotNull(doc);
                    Assert.NotEmpty(doc.GetField(Labels.Id.ToString()).StringValue);
                    Assert.NotEmpty(doc.GetField(Labels.Label.ToString()).StringValue);
                }
            }

            var q1 = QueryService.QueryByLabel("Berlin", LuceneHelper.GetLuceneDirectory(outputPath));
            Assert.NotNull(q1);
            Assert.Contains("Berlin", q1.FirstOrDefault().Label);
        }

        [Fact]
        public void TestIfItsRanking()
        {
            var filename = "Resources/filtered-All-500.nt";
            var outputPath1 = "IndexRank1";
            var outputPath2 = "IndexRank2";

            if (Directory.Exists(outputPath1))
                Directory.Delete(outputPath1, true);

            if (Directory.Exists(outputPath2))
                Directory.Delete(outputPath2, true);

            IndexBuilder.CreateEntitiesIndex(filename, outputPath1, true);
            IndexBuilder.CreateEntitiesIndex(filename, outputPath2, false);

            var found = 0;

            using (var readerNoBoost = new IndexSearcher(LuceneHelper.GetLuceneDirectory(outputPath1), true))
            using (var readerWithBoost = new IndexSearcher(LuceneHelper.GetLuceneDirectory(outputPath2), true))
            {
                var docCountNoBoost = readerNoBoost.MaxDoc;
                var docCountWithBoost = readerWithBoost.MaxDoc;

                Assert.Equal(docCountNoBoost, docCountWithBoost);

                var analyzer = new StandardAnalyzer(Version.LUCENE_30);

                QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30,
                    new[] { Labels.Label.ToString(), Labels.AltLabel.ToString() },
                    analyzer);

                string[] testWords = { "obama", "chile", "ireland", "apple", "orange", "human", "country", "car", "plane", "bed" };

                for (int j = 0; j < testWords.Length; j++)
                {
                    //var searchQuery = char.ConvertFromUtf32((char.Parse("a") + j)).ToString() + "*";
                    //var searchQuery = "Obama";
                    var searchQuery = testWords[j];
                    var ResultsLimit = 10;

                    var hits1 = readerNoBoost.Search(parser.Parse(searchQuery.Trim()), null, ResultsLimit).ScoreDocs;
                    var hits2 = readerWithBoost.Search(parser.Parse(searchQuery.Trim()), null, ResultsLimit).ScoreDocs;

                    Assert.Equal(hits1.Count(), hits2.Count());

                    for (int i = 0; i < hits2.Count(); i++)
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
        }

        [Fact]
        public void TestCreateIndex500()
        {
            var filename = "Resources/filtered-All-500.nt";
            var outputPath = "Index500";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath);

            Assert.True(Directory.Exists(outputPath));

            var q1 = QueryService.QueryByLabel("Berlin", LuceneHelper.GetLuceneDirectory(outputPath));
            Assert.NotNull(q1);
            Assert.Contains("Berlin", q1.FirstOrDefault().Label);

            var q2 = QueryService.QueryByLabel("Obama", LuceneHelper.GetLuceneDirectory(outputPath));
            Assert.NotNull(q2);
            Assert.Contains("Barack Obama", q2.FirstOrDefault().Label);
        }

        //[Fact]
        //public void TestCreateIndexFull()
        //{
        //    var filename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt";
        //    var outputPath = "IndexFull";

        //    if (Directory.Exists(outputPath))
        //        Directory.Delete(outputPath, true);

        //    Assert.False(Directory.Exists(outputPath));

        //    IndexBuilder.CreateIndex(filename, outputPath);

        //    Assert.True(Directory.Exists(outputPath));
        //}
    }
}
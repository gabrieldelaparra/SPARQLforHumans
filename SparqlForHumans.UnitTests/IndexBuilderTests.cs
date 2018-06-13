using System.IO;
using System.Linq;
using Lucene.Net.Index;
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

            IndexBuilder.CreateIndex(filename, outputPath);

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

            //var q1 = QueryService.QueryByLabel("Berlin", LuceneHelper.GetLuceneDirectory(outputPath));
            //Assert.NotNull(q1);
            //Assert.Single(q1);
            //Assert.Contains("Berlin", q1.FirstOrDefault().Label);

            //var q2 = QueryService.QueryByLabel("Obama", LuceneHelper.GetLuceneDirectory(outputPath));
            //Assert.NotNull(q2);
            //Assert.Empty(q2);
        }

        [Fact]
        public void TestCreateIndex500()
        {
            var filename = "Resources/filtered-All-500.nt";
            var outputPath = "Index500";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateIndex(filename, outputPath);

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
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class IndexBuilderTests
    {
        [Fact]
        public void TestCreateIndex()
        {
            var filename = "Resources/filtered.nt";
            var outputPath = "Index";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateIndex(filename, outputPath);

            Assert.True(Directory.Exists(outputPath));

            var q1 = QueryService.QueryByLabel("Ireland", LuceneHelper.GetLuceneDirectory(outputPath));
            var q2 = QueryService.QueryByLabel("Obama", LuceneHelper.GetLuceneDirectory(outputPath));

            Assert.NotNull(q1);
            Assert.Equal(3, q1.Count());
            Assert.True(q1.All(x => x.Label.ToUpper().Contains("Ireland")));

            Assert.NotNull(q2);
            Assert.Empty(q2);
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class IndexRankerTests
    {
        //[Fact]
        //public void TestRankIndex()
        //{
        //    var filename = "Resources/filtered.nt";
        //    var directoryPath = "Index";
        //    var rankedPath = "RankedIndex";

        //    if (Directory.Exists(directoryPath))
        //        Directory.Delete(directoryPath, true);

        //    Assert.False(Directory.Exists(directoryPath));

        //    IndexBuilder.CreateIndex(filename, directoryPath);

        //    Assert.True(Directory.Exists(directoryPath));

        //    var q1 = QueryService.QueryByLabel("Berlin", LuceneHelper.GetLuceneDirectory(directoryPath));
        //    Assert.NotNull(q1);
        //    Assert.Single(q1);
        //    Assert.Contains("Berlin", q1.FirstOrDefault().Label);

        //    var luceneDirectory = LuceneHelper.GetLuceneDirectory(directoryPath);
        //    Assert.False(luceneDirectory.HasRank()); 
            
        //    IndexRanker.Rank(luceneDirectory, filename, rankedPath);

        //    var rankedIndexDirectory = LuceneHelper.GetLuceneDirectory(rankedPath);
        //    Assert.True(rankedIndexDirectory.HasRank());
        //}
    }
}

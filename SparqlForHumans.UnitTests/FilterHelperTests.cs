using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class FilterHelperTests
    {
        [Fact]
        public void TestFilterZero()
        {
            var filename = @"TrimmedTestSet.nt";
            Assert.True(File.Exists(filename));

            int limit = 0;

            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);

            if (File.Exists(outputFilename))
                File.Delete(outputFilename);

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename, 0);

            Assert.True(File.Exists(outputFilename));
            Assert.Equal(0, FileHelper.GetLineCount(outputFilename));
        }

        [Fact]
        public void TestFilterSome()
        {
            var filename = @"TrimmedTestSet.nt";
            Assert.True(File.Exists(filename));

            int limit = 500;

            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);

            if (File.Exists(outputFilename))
                File.Delete(outputFilename);

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename, limit);

            Assert.True(File.Exists(outputFilename));
            Assert.NotEqual(0, FileHelper.GetLineCount(outputFilename));

            var lines = FileHelper.GetInputLines(outputFilename);
            var firstItemList = lines.Select(x => x.Split(" ").FirstOrDefault());

            Assert.NotNull(firstItemList);
            Assert.NotEmpty(firstItemList);

            var qCodesList = firstItemList.Select(x => x.Split(@"<http://www.wikidata.org/entity/Q").LastOrDefault().Replace(">", ""));
            var intCodesList = qCodesList.Select(x => int.Parse(x));
            Assert.True(intCodesList.All(x => x < limit));
        }
    }
}

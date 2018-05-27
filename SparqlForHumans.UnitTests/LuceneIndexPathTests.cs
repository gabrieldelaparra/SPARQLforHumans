using SparqlForHumans.Core.Utilities;
using System.IO;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class LuceneIndexPathTests
    {
        [Fact]
        public void TestGetLucenePath()
        {
            var path = "Temp";
            var luceneDirectory = LuceneHelper.GetLuceneDirectory(path);
            Assert.NotNull(luceneDirectory);
            Assert.IsAssignableFrom<Lucene.Net.Store.Directory>(luceneDirectory);

            Assert.True(Directory.Exists(path));
        }
    }
}

using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests.Index
{
    public class EntitiesIndexerTests
    {
        [Fact]
        public void TestCreateEntityIndex5k()
        {
            const string filename = "Resources/Filter5k-Index.nt";
            const string outputPath = "Index5k";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            var indexer = new EntitiesIndexer(filename, outputPath);
            indexer.Index();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
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
    }
}

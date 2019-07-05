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

            new EntitiesIndexer(filename, outputPath).Index();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                Assert.True(Directory.Exists(outputPath));

                var queryBerlin = MultiDocumentQueries.QueryEntitiesByLabel("Berli", luceneDirectory).ToArray();
                Assert.NotEmpty(queryBerlin);
                var result = queryBerlin[0];
                Assert.Equal("Q64", result.Id);
                Assert.Equal("Berlin", result.Label);
                Assert.Equal("capital city of Germany", result.Description);
                Assert.Contains("Berlin, Germany", result.AltLabels);
                var properties = result.Properties.Select(x => x.Id).ToArray();
                Assert.Contains("P17", properties);
                Assert.Contains("P1376", properties);
                Assert.False(result.IsType);
                Assert.Contains("Q515", result.InstanceOf);
                Assert.Contains("Q5119", result.InstanceOf);
                Assert.Contains("Q999",result.SubClass);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestIndexHasTypes()
        {
            const string filename = "Resources/EntityTypes.nt";
            const string outputPath = "IndexTypes";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var typesQuery = MultiDocumentQueries.QueryEntitiesByLabel("*", luceneDirectory, true).ToArray();
                Assert.NotEmpty(typesQuery);
                Assert.Contains(typesQuery, x =>x.Id.Equals("Q5"));
            }
            outputPath.DeleteIfExists();
        }
    }
}

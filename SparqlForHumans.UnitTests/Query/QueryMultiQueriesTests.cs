using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryMultiQueriesTests
    {
        [Fact]
        public void TestMultiQueryBarackObamaShouldShowFirst()
        {
            const string filename = "Resources/QueryMulti.nt";
            const string outputPath = "QueryMultiIndexBarack";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("Obama", luceneIndexDirectory);
                var entity = entities.FirstOrDefault();
                Assert.Equal("Q76", entity.Id);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestMultiQueryMichelleObamaShouldShowFirst()
        {
            const string filename = "Resources/QueryMulti.nt";
            const string outputPath = "QueryMultiIndexMichelle";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("Michelle Obama", luceneIndexDirectory);
                var entity = entities.FirstOrDefault();
                Assert.Equal("Q13133", entity.Id);
            }

            outputPath.DeleteIfExists();
        }
    }
}
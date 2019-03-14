using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class QueryMultiQueriesTests
    {
        [Fact]
        public void TestMultiQueryBarackObamaShouldShowFirst()
        {
            const string filename = "Resources/QueryMulti.nt";
            const string outputPath = "QueryMultiIndexBarack";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, addBoosts: true);
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

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, addBoosts: true);
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("Michelle Obama", luceneIndexDirectory);
                var entity = entities.FirstOrDefault();
                Assert.Equal("Q13133", entity.Id);
            }

            outputPath.DeleteIfExists();
        }
    }
}

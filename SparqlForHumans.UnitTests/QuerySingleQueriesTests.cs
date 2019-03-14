using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class QuerySingleQueriesTests
    {
        [Fact]
        public void TestSingleQueryBarackObamaShouldShowFirst()
        {
            const string filename = "Resources/QuerySingle.nt";
            const string outputPath = "QuerySingleIndexBarack";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var entity = SingleDocumentQueries.QueryEntityByLabel("Obama", luceneIndexDirectory);
                Assert.Equal("Q76", entity.Id);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestSingleQueryMichelleObamaShouldShowFirst()
        {
            const string filename = "Resources/QuerySingle.nt";
            const string outputPath = "QuerySingleIndexMichelle";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var entity = SingleDocumentQueries.QueryEntityByLabel("Michelle Obama", luceneIndexDirectory);
                Assert.Equal("Q13133", entity.Id);
            }

            outputPath.DeleteIfExists();
        }
    }
}

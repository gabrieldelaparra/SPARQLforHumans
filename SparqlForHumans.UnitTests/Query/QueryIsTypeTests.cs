using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryIsTypeTests
    {
        [Fact]
        public static void TestQueryIsTypeFields()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "CreateIndexIsTypeFields";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var query = "chile";
                var types = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, true);
                var all = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, false);

                Assert.Empty(types);
                Assert.Single(all);

                query = "country";
                types = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, true);
                all = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, false);

                Assert.Single(types);
                Assert.Single(all);
            }

            outputPath.DeleteIfExists();
        }
    }
}

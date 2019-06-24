using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class QueryWildcardTests
    {
        [Fact]
        public void TestFullQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var actual = MultiDocumentQueries.QueryEntitiesByLabel("Obama", luceneIndexDirectory)
                    .FirstOrDefault();
                Assert.Equal("Q76000000", actual.Id);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestNoEndWildcardQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardHalfWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var actual = MultiDocumentQueries.QueryEntitiesByLabel("Oba", luceneIndexDirectory)
                    .FirstOrDefault();
                Assert.Equal("Q76000000", actual.Id);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestWithEndWildcardQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardWithAsterisk";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var actual = MultiDocumentQueries.QueryEntitiesByLabel("Oba*", luceneIndexDirectory)
                    .FirstOrDefault();
                Assert.Equal("Q76000000", actual.Id);
            }

            outputPath.DeleteIfExists();
        }
    }
}
using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryTopResults
    {
        [Fact]
        public void TestFullQueryEntitiesResults()
        {
            const string filename = "Resources/QueryEntityWildcardAllResults.nt";
            const string outputPath = "AllEntitiesResultsWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var actual = MultiDocumentQueries.QueryEntitiesByLabel("*", luceneIndexDirectory, false).ToArray();

                Assert.NotEmpty(actual);
                Assert.Equal("Q6", actual[0].Id); //0.222
                Assert.Equal("Q4", actual[1].Id); //0.180
                Assert.Equal("Q7", actual[2].Id); //0.180
                Assert.Equal("Q1", actual[3].Id); //0.138
                Assert.Equal("Q5", actual[4].Id); //0.128
                Assert.Equal("Q2", actual[5].Id); //0.087
                Assert.Equal("Q3", actual[6].Id); //0.061
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestFullQueryPropertiesResults()
        {
            const string filename = @"Resources/QueryPropertyWildcardAllResults.nt";
            const string outputPath = "AllPropertiesResultsWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new PropertiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var actual = MultiDocumentQueries.QueryPropertiesByLabel("*", luceneIndexDirectory, false).ToArray();

                Assert.NotEmpty(actual);
                Assert.Equal("P530", actual[0].Id);//50
                Assert.Equal("P47", actual[1].Id);//5
                Assert.Equal("P17", actual[2].Id); //3
                Assert.Equal("P30", actual[3].Id); //3
            }

            outputPath.DeleteIfExists();
        }
    }
}

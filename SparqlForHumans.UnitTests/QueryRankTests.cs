using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class QueryRankTests
    {
        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_AllWithAltLabels_Q1More()
        {
            const string filename = "Resources/QueryRanksAllWithAltLabelsQ1More.nt";
            const string outputPath = "QueryRanksAllWithAltLabelsQ1SortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("EntityQ", luceneIndexDirectory).ToArray();

                Assert.Equal("Q6", entities[0].Id); //0.222
                Assert.Equal("Q4", entities[1].Id); //0.180
                Assert.Equal("Q7", entities[2].Id); //0.180
                Assert.Equal("Q1", entities[3].Id); //0.138
                Assert.Equal("Q5", entities[4].Id); //0.128
                Assert.Equal("Q2", entities[5].Id); //0.087
                Assert.Equal("Q3", entities[6].Id); //0.061
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_AllWithSameAltLabels()
        {
            const string filename = "Resources/QueryRanksAllWithAltLabels.nt";
            const string outputPath = "QueryRanksAllSameAltLabelsSortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("EntityQ", luceneIndexDirectory).ToArray();

                Assert.Equal("Q6", entities[0].Id); //0.222
                Assert.Equal("Q4", entities[1].Id); //0.180
                Assert.Equal("Q7", entities[2].Id); //0.180
                Assert.Equal("Q1", entities[3].Id); //0.138
                Assert.Equal("Q5", entities[4].Id); //0.128
                Assert.Equal("Q2", entities[5].Id); //0.087
                Assert.Equal("Q3", entities[6].Id); //0.061
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OneWithAltLabels()
        {
            const string filename = "Resources/QueryRanksOneWithAltLabels.nt";
            const string outputPath = "QueryRanksOneAltLabelsSortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("EntityQ", luceneIndexDirectory).ToArray();

                // Had to fix these tests to take PageRank and Boost altogether to pass.
                Assert.Equal("Q1", entities[0].Id); //0.138
                Assert.Equal("Q6", entities[1].Id); //0.222
                Assert.Equal("Q4", entities[2].Id); //0.180
                Assert.Equal("Q7", entities[3].Id); //0.180
                Assert.Equal("Q5", entities[4].Id); //0.128
                Assert.Equal("Q2", entities[5].Id); //0.087
                Assert.Equal("Q3", entities[6].Id); //0.061
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OnlyAltLabels()
        {
            const string filename = "Resources/QueryRanksOnlyAltLabels.nt";
            const string outputPath = "QueryRanksSortedByPageRankOnlyAltLabels";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("EntityQ", luceneIndexDirectory).ToArray();

                Assert.Equal("Q6", entities[0].Id); //0.222
                Assert.Equal("Q4", entities[1].Id); //0.180
                Assert.Equal("Q7", entities[2].Id); //0.180
                Assert.Equal("Q1", entities[3].Id); //0.138
                Assert.Equal("Q5", entities[4].Id); //0.128
                Assert.Equal("Q2", entities[5].Id); //0.087
                Assert.Equal("Q3", entities[6].Id); //0.061
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OnlyLabels()
        {
            const string filename = "Resources/QueryRanksOnlyLabels.nt";
            const string outputPath = "QueryRanksSortedByPageRankOnlyLabels";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByLabel("EntityQ", luceneIndexDirectory).ToArray();

                Assert.Equal("Q6", entities[0].Id); //0.222
                Assert.Equal("Q4", entities[1].Id); //0.180
                Assert.Equal("Q7", entities[2].Id); //0.180
                Assert.Equal("Q1", entities[3].Id); //0.138
                Assert.Equal("Q5", entities[4].Id); //0.128
                Assert.Equal("Q2", entities[5].Id); //0.087
                Assert.Equal("Q3", entities[6].Id); //0.061
            }

            outputPath.DeleteIfExists();
        }
    }
}
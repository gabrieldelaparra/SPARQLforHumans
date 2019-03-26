using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class QueryRankTests
    {
        /// <summary>
        ///     An issue while testing ranking, is that ranking it's not being displayed when the index is read.
        ///     Not sure why is this or if this might an issue of the current Lucene.Net library version.
        ///     So far, the only found way of testing whether the index has ranking or not, is to create
        ///     an index of a closed set, then search for specific items and check whether they appear
        ///     as first on the search list.
        /// </summary>
        [Fact]
        public void TestHasRanking()
        {
            //const string filename = "Resources/filtered-All-500.nt";
            //const string outputPath1 = "IndexRank1";
            //const string outputPath2 = "IndexRank2";

            //outputPath1.DeleteIfExists();
            //outputPath2.DeleteIfExists();

            //var found = 0;

            //using (var outputDirectory1 = FSDirectory.Open(outputPath1.GetOrCreateDirectory()))
            //{
            //    EntitiesIndex.CreateEntitiesIndex(filename, outputDirectory1, true);
            //}

            //using (var outputDirectory2 = FSDirectory.Open(outputPath2.GetOrCreateDirectory()))
            //{
            //    EntitiesIndex.CreateEntitiesIndex(filename, outputDirectory2, false);
            //}

            //using (var outputDirectory1 = FSDirectory.Open(outputPath1.GetOrCreateDirectory()))
            //using (var outputDirectory2 = FSDirectory.Open(outputPath2.GetOrCreateDirectory()))
            //using (var outputReader1 = DirectoryReader.Open(outputDirectory1))
            //using (var outputReader2 = DirectoryReader.Open(outputDirectory2))
            //{
            //    var readerNoBoost = new IndexSearcher(outputReader1);
            //    var readerWithBoost = new IndexSearcher(outputReader2);

            //    var docCountNoBoost = readerNoBoost.IndexReader.MaxDoc;
            //    var docCountWithBoost = readerWithBoost.IndexReader.MaxDoc;

            //    Assert.Equal(docCountNoBoost, docCountWithBoost);

            //    var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            //    QueryParser parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48,
            //        new[] { Labels.Label.ToString(), Labels.AltLabel.ToString() },
            //        analyzer);

            //    string[] testWords =
            //        {"obama", "europe", "ireland", "apple", "america", "human", "park", "city", "germany", "france"};

            //    foreach (var searchQuery in testWords)
            //    {
            //        const int resultsLimit = 10;
            //        var queryNoBoost =
            //            MultiDocumentQueries.QueryDocumentsByLabel(searchQuery, outputDirectory1, false, resultsLimit);
            //        var queryBoost =
            //            MultiDocumentQueries.QueryDocumentsByLabel(searchQuery, outputDirectory2, false, resultsLimit);

            //        Assert.Equal(queryNoBoost.Count(), queryBoost.Count());

            //        for (var i = 0; i < queryNoBoost.Count(); i++)
            //            if (!queryNoBoost.ElementAt(i).GetValue(Labels.Id)
            //                .Equals(queryBoost.ElementAt(i).GetValue(Labels.Id)))
            //                found++;
            //    }
            //}

            //Assert.NotEqual(0, found);

            //outputPath1.DeleteIfExists();
            //outputPath2.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_AllWithAltLabels_Q1More()
        {
            const string filename = "Resources/QueryRanksAllWithAltLabelsQ1More.nt";
            const string outputPath = "QueryRanksAllWithAltLabelsQ1SortedByPageRank";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

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

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

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

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

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

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

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

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

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
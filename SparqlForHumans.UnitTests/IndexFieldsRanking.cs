using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class IndexFieldsRanking
    {
        [Fact]
        public void TestIndexHasCorrectRankingFields()
        {
            const string filename = "Resources/PageRankBuildGraph.nt";
            const string outputPath = "IndexRanks";

            var nodesGraph = EntityPageRank.BuildSimpleNodesGraph(filename);

            var ranks = EntityPageRank.CalculateRanks(nodesGraph, 20);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, true);
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    Assert.True(Directory.Exists(outputPath));

                    var docCount = reader.MaxDoc;

                    Assert.Equal(7, docCount);

                    for (var i = 0; i < docCount; i++)
                    {
                        var doc = reader.Document(i);
                        double.TryParse(doc.GetValue(Labels.Rank), out var rank);
                        Assert.Equal(ranks[i].ToThreeDecimals(), rank.ToThreeDecimals());
                    }
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestIndexWithDifferentIdsHasCorrectRankingFields()
        {
            const string filename = "Resources/PageRankBuildGraphDifferentIds-Index.nt";
            const string outputPath = "IndexRanks";

            var nodesGraph = EntityPageRank.BuildSimpleNodesGraph(filename);

            var ranks = EntityPageRank.CalculateRanks(nodesGraph, 20);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneDirectory, true);

                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    Assert.True(Directory.Exists(outputPath));

                    var docCount = reader.MaxDoc;

                    Assert.Equal(7, docCount);

                    for (var i = 0; i < docCount; i++)
                    {
                        var doc = reader.Document(i);
                        double.TryParse(doc.GetValue(Labels.Rank), out var rank);
                        Assert.Equal(ranks[i].ToThreeDecimals(), rank.ToThreeDecimals());
                    }
                }
            }

            outputPath.DeleteIfExists();
        }
    }
}
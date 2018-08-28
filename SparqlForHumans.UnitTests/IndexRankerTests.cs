using System;
using System.IO;
using System.Linq;
using Lucene.Net.Index;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class IndexRankerTests
    {
        [Fact]
        public void TestBuildDictionary()
        {
            var filename = "Resources/buildGraph.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var dictionary = EntityRanker.BuildNodesDictionary(filename);
            Assert.Equal(entitiesCount, dictionary.Count);
            Assert.Equal("Q1", dictionary.Keys.ElementAt(0));
            Assert.Equal("Q2", dictionary.Keys.ElementAt(1));
            Assert.Equal("Q3", dictionary.Keys.ElementAt(2));
            Assert.Equal("Q4", dictionary.Keys.ElementAt(3));
            Assert.Equal("Q5", dictionary.Keys.ElementAt(4));
            Assert.Equal("Q6", dictionary.Keys.ElementAt(5));
            Assert.Equal("Q7", dictionary.Keys.ElementAt(6));
        }

        [Fact]
        public void TestBuildNodeGraph()
        {
            var filename = "Resources/buildGraph.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var nodesGraph = EntityRanker.BuildNodesGraph(filename);

            Assert.Equal(entitiesCount, nodesGraph.Count());
            Assert.Equal(2, nodesGraph.ElementAt(0).ConnectedNodes.Length);
            Assert.Equal(3, nodesGraph.ElementAt(1).ConnectedNodes.Length);
            Assert.Equal(2, nodesGraph.ElementAt(2).ConnectedNodes.Length);
            Assert.Equal(2, nodesGraph.ElementAt(3).ConnectedNodes.Length);
            Assert.Equal(6, nodesGraph.ElementAt(4).ConnectedNodes.Length);
            Assert.Equal(2, nodesGraph.ElementAt(5).ConnectedNodes.Length);
            Assert.Empty(nodesGraph.ElementAt(6).ConnectedNodes);

            var initialRank = 1d / entitiesCount;
            foreach (var graphNode in nodesGraph)
            {
                Assert.Equal(initialRank, graphNode.Rank);
            }
        }

        [Fact]
        public void TestCalculateRankOneIteration()
        {
            var filename = "Resources/buildGraph.nt";
            var nodesGraph = EntityRanker.BuildNodesGraph(filename);

            EntityRanker.CalculateRanks(nodesGraph, 1);

            Assert.Equal(1, Math.Round(nodesGraph.Sum(x => x.Rank)), 10);

            Assert.Equal(0.119, nodesGraph.ElementAt(0).Rank.ToThreeDecimals());
            Assert.Equal(0.119, nodesGraph.ElementAt(1).Rank.ToThreeDecimals());
            Assert.Equal(0.059, nodesGraph.ElementAt(2).Rank.ToThreeDecimals());
            Assert.Equal(0.160, nodesGraph.ElementAt(3).Rank.ToThreeDecimals());
            Assert.Equal(0.160, nodesGraph.ElementAt(4).Rank.ToThreeDecimals());
            Assert.Equal(0.220, nodesGraph.ElementAt(5).Rank.ToThreeDecimals());
            Assert.Equal(0.160, nodesGraph.ElementAt(6).Rank.ToThreeDecimals());
        }

        [Fact]
        public void TestCalculateRankSevenIterations()
        {
            var filename = "Resources/buildGraph.nt";
            var nodesGraph = EntityRanker.BuildNodesGraph(filename);

            EntityRanker.CalculateRanks(nodesGraph, 7);

            Assert.Equal(1, Math.Round(nodesGraph.Sum(x => x.Rank)), 10);

            Assert.Equal(0.138, nodesGraph.ElementAt(0).Rank.ToThreeDecimals());
            Assert.Equal(0.087, nodesGraph.ElementAt(1).Rank.ToThreeDecimals());
            Assert.Equal(0.061, nodesGraph.ElementAt(2).Rank.ToThreeDecimals());
            Assert.Equal(0.180, nodesGraph.ElementAt(3).Rank.ToThreeDecimals());
            Assert.Equal(0.128, nodesGraph.ElementAt(4).Rank.ToThreeDecimals());
            Assert.Equal(0.222, nodesGraph.ElementAt(5).Rank.ToThreeDecimals());
            Assert.Equal(0.180, nodesGraph.ElementAt(6).Rank.ToThreeDecimals());
        }

        [Fact]
        public void TestBuildNodeArrayGraph()
        {
            var filename = "Resources/buildGraph.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var nodesGraph = EntityRanker.BuildSimpleNodesGraph(filename);

            Assert.Equal(entitiesCount, nodesGraph.Count());
            Assert.Equal(2, nodesGraph[0].Length);
            Assert.Equal(3, nodesGraph[1].Length);
            Assert.Equal(2, nodesGraph[2].Length);
            Assert.Equal(2, nodesGraph[3].Length);
            Assert.Equal(6, nodesGraph[4].Length);
            Assert.Equal(2, nodesGraph[5].Length);
            Assert.Empty(nodesGraph[6]);
        }

        [Fact]
        public void TestCalculateFloatRankOneIteration()
        {
            var filename = "Resources/buildGraph.nt";
            var nodesGraph = EntityRanker.BuildSimpleNodesGraph(filename);

            var ranks = EntityRanker.CalculateRanks(nodesGraph, 1);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            Assert.Equal(0.119, ranks[0].ToThreeDecimals());
            Assert.Equal(0.119, ranks[1].ToThreeDecimals());
            Assert.Equal(0.059, ranks[2].ToThreeDecimals());
            Assert.Equal(0.160, ranks[3].ToThreeDecimals());
            Assert.Equal(0.160, ranks[4].ToThreeDecimals());
            Assert.Equal(0.220, ranks[5].ToThreeDecimals());
            Assert.Equal(0.160, ranks[6].ToThreeDecimals());
        }

        [Fact]
        public void TestCalculateFloatRankSevenIterations()
        {
            var filename = "Resources/buildGraph.nt";
            var nodesGraph = EntityRanker.BuildSimpleNodesGraph(filename);

            var ranks = EntityRanker.CalculateRanks(nodesGraph, 7);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            Assert.Equal(0.138, ranks[0].ToThreeDecimals());
            Assert.Equal(0.087, ranks[1].ToThreeDecimals());
            Assert.Equal(0.061, ranks[2].ToThreeDecimals());
            Assert.Equal(0.180, ranks[3].ToThreeDecimals());
            Assert.Equal(0.128, ranks[4].ToThreeDecimals());
            Assert.Equal(0.222, ranks[5].ToThreeDecimals());
            Assert.Equal(0.180, ranks[6].ToThreeDecimals());
        }

        [Fact]
        public void TestCalculateFloatRankSevenIterationsMultipleProperties()
        {
            var filename = "Resources/buildGraphMultipleProperties.nt";
            var nodesGraph = EntityRanker.BuildSimpleNodesGraph(filename);

            var ranks = EntityRanker.CalculateRanks(nodesGraph, 7);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            Assert.Equal(0.138, ranks[0].ToThreeDecimals());
            Assert.Equal(0.087, ranks[1].ToThreeDecimals());
            Assert.Equal(0.061, ranks[2].ToThreeDecimals());
            Assert.Equal(0.180, ranks[3].ToThreeDecimals());
            Assert.Equal(0.128, ranks[4].ToThreeDecimals());
            Assert.Equal(0.222, ranks[5].ToThreeDecimals());
            Assert.Equal(0.180, ranks[6].ToThreeDecimals());
        }

        [Fact]
        public void TestCalculateFloatRankSevenIterationsDifferentIds()
        {
            var filename = "Resources/buildGraphDifferentIds.nt";
            var nodesGraph = EntityRanker.BuildSimpleNodesGraph(filename);

            var ranks = EntityRanker.CalculateRanks(nodesGraph, 7);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            Assert.Equal(0.138, ranks[0].ToThreeDecimals());
            Assert.Equal(0.087, ranks[1].ToThreeDecimals());
            Assert.Equal(0.061, ranks[2].ToThreeDecimals());
            Assert.Equal(0.180, ranks[3].ToThreeDecimals());
            Assert.Equal(0.128, ranks[4].ToThreeDecimals());
            Assert.Equal(0.222, ranks[5].ToThreeDecimals());
            Assert.Equal(0.180, ranks[6].ToThreeDecimals());
        }

        [Fact]
        public void TestIndexRanks()
        {
            var filename = "Resources/buildGraph.nt";
            var outputPath = "IndexRanks";

            var nodesGraph = EntityRanker.BuildSimpleNodesGraph(filename);

            var ranks = EntityRanker.CalculateRanks(nodesGraph, 20);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, true);

            Assert.True(Directory.Exists(outputPath));

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;

                Assert.Equal(7, docCount);

                for (var i = 0; i < docCount; i++)
                {
                    var doc = reader.Document(i);
                    double.TryParse(doc.GetValue(Labels.Rank), out var rank);
                    Assert.Equal(ranks[i].ToThreeDecimals(), rank.ToThreeDecimals());
                }
            }

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }

        [Fact]
        public void TestIndexRanksDifferentIds()
        {
            var filename = "Resources/buildGraphDifferentIds.nt";
            var outputPath = "IndexRanks";

            var nodesGraph = EntityRanker.BuildSimpleNodesGraph(filename);

            var ranks = EntityRanker.CalculateRanks(nodesGraph, 20);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreateEntitiesIndex(filename, outputPath, true);

            Assert.True(Directory.Exists(outputPath));

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;

                Assert.Equal(7, docCount);

                for (var i = 0; i < docCount; i++)
                {
                    var doc = reader.Document(i);
                    double.TryParse(doc.GetValue(Labels.Rank), out var rank);
                    Assert.Equal(ranks[i].ToThreeDecimals(), rank.ToThreeDecimals());
                }
            }

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }
    }
}

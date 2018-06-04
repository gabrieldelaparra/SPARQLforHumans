using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class IndexRankerTests
    {
        [Fact]
        public void TestBuildNodeGraph()
        {
            var filename = "Resources/buildGraph.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupByEntities();
            var entitiesCount = groups.Count();

            var nodesGraph = IndexRanker.BuildNodesGraph(filename);

            Assert.Equal(entitiesCount, nodesGraph.Count());
            Assert.Equal(2, nodesGraph.ElementAt(0).ConnectedNodes.Count);
            Assert.Equal(3, nodesGraph.ElementAt(1).ConnectedNodes.Count);
            Assert.Equal(2, nodesGraph.ElementAt(2).ConnectedNodes.Count);
            Assert.Equal(2, nodesGraph.ElementAt(3).ConnectedNodes.Count);
            Assert.Equal(6, nodesGraph.ElementAt(4).ConnectedNodes.Count);
            Assert.Equal(2, nodesGraph.ElementAt(5).ConnectedNodes.Count);
            Assert.Equal(0, nodesGraph.ElementAt(6).ConnectedNodes.Count);

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
            var nodesGraph = IndexRanker.BuildNodesGraph(filename);

            IndexRanker.CalculateRanks(nodesGraph);

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
            var nodesGraph = IndexRanker.BuildNodesGraph(filename);

            IndexRanker.CalculateRanks(nodesGraph, 7);

            Assert.Equal(1, Math.Round(nodesGraph.Sum(x => x.Rank)), 10);

            Assert.Equal(0.138, nodesGraph.ElementAt(0).Rank.ToThreeDecimals());
            Assert.Equal(0.087, nodesGraph.ElementAt(1).Rank.ToThreeDecimals());
            Assert.Equal(0.061, nodesGraph.ElementAt(2).Rank.ToThreeDecimals());
            Assert.Equal(0.180, nodesGraph.ElementAt(3).Rank.ToThreeDecimals());
            Assert.Equal(0.128, nodesGraph.ElementAt(4).Rank.ToThreeDecimals());
            Assert.Equal(0.222, nodesGraph.ElementAt(5).Rank.ToThreeDecimals());
            Assert.Equal(0.180, nodesGraph.ElementAt(6).Rank.ToThreeDecimals());
        }

        //[Fact]
        //public void TestRankIndex()
        //{
        //    var filename = "Resources/filtered.nt";
        //    var directoryPath = "Index";
        //    var rankedPath = "RankedIndex";

        //    if (Directory.Exists(directoryPath))
        //        Directory.Delete(directoryPath, true);

        //    Assert.False(Directory.Exists(directoryPath));

        //    IndexBuilder.CreateIndex(filename, directoryPath);

        //    Assert.True(Directory.Exists(directoryPath));

        //    var q1 = QueryService.QueryByLabel("Berlin", LuceneHelper.GetLuceneDirectory(directoryPath));
        //    Assert.NotNull(q1);
        //    Assert.Single(q1);
        //    Assert.Contains("Berlin", q1.FirstOrDefault().Label);

        //    var luceneDirectory = LuceneHelper.GetLuceneDirectory(directoryPath);
        //    Assert.False(luceneDirectory.HasRank()); 

        //    IndexRanker.Rank(luceneDirectory, filename, rankedPath);

        //    var rankedIndexDirectory = LuceneHelper.GetLuceneDirectory(rankedPath);
        //    Assert.True(rankedIndexDirectory.HasRank());
        //}
    }
}

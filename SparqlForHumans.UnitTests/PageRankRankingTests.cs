using System;
using System.Linq;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class PageRankRankingTests
    {
        [Fact]
        public void TestBuildIdRankDictionarySimpleIds()
        {
            const string filename = "Resources/PageRankBuildGraph.nt";

            var idRankDictionary = EntityPageRank.BuildPageRank(filename);

            Assert.Equal(1, Math.Round(idRankDictionary.Select(x=>x.Value).Sum()), 10);

            Assert.Equal(0.138, idRankDictionary[1].ToThreeDecimals());
            Assert.Equal(0.087, idRankDictionary[2].ToThreeDecimals());
            Assert.Equal(0.061, idRankDictionary[3].ToThreeDecimals());
            Assert.Equal(0.180, idRankDictionary[4].ToThreeDecimals());
            Assert.Equal(0.128, idRankDictionary[5].ToThreeDecimals());
            Assert.Equal(0.222, idRankDictionary[6].ToThreeDecimals());
            Assert.Equal(0.180, idRankDictionary[7].ToThreeDecimals());
        }

        [Fact]
        public void TestBuildIdRankDictionaryDifferentIds()
        {
            const string filename = "Resources/PageRankBuildGraphDifferentIds.nt";

            var idRankDictionary = EntityPageRank.BuildPageRank(filename);

            Assert.Equal(1, Math.Round(idRankDictionary.Select(x=>x.Value).Sum()), 10);

            Assert.Equal(0.138, idRankDictionary[332].ToThreeDecimals());
            Assert.Equal(0.087, idRankDictionary[8424].ToThreeDecimals());
            Assert.Equal(0.061, idRankDictionary[842].ToThreeDecimals());
            Assert.Equal(0.180, idRankDictionary[124].ToThreeDecimals());
            Assert.Equal(0.128, idRankDictionary[8].ToThreeDecimals());
            Assert.Equal(0.222, idRankDictionary[97].ToThreeDecimals());
            Assert.Equal(0.180, idRankDictionary[33].ToThreeDecimals());
        }

        [Fact]
        public void TestBuildDictionary()
        {
            const string filename = "Resources/PageRankBuildGraph.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var dictionary = EntityPageRank.BuildNodesDictionary(filename);
            Assert.Equal(entitiesCount, dictionary.Count);
            Assert.Equal(1, dictionary.Keys.ElementAt(0));
            Assert.Equal(2, dictionary.Keys.ElementAt(1));
            Assert.Equal(3, dictionary.Keys.ElementAt(2));
            Assert.Equal(4, dictionary.Keys.ElementAt(3));
            Assert.Equal(5, dictionary.Keys.ElementAt(4));
            Assert.Equal(6, dictionary.Keys.ElementAt(5));
            Assert.Equal(7, dictionary.Keys.ElementAt(6));
        }

        [Fact]
        public void TestBuildNodeArrayGraph()
        {
            const string filename = "Resources/PageRankBuildGraph.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var nodesGraph = EntityPageRank.BuildSimpleNodesGraph(filename);

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
            const string filename = "Resources/PageRankBuildGraph-Floats.nt";
            var nodesGraph = EntityPageRank.BuildSimpleNodesGraph(filename);

            var ranks = EntityPageRank.CalculateRanks(nodesGraph, 1);

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
            const string filename = "Resources/PageRankBuildGraph-Floats7.nt";
            var nodesGraph = EntityPageRank.BuildSimpleNodesGraph(filename);

            var ranks = EntityPageRank.CalculateRanks(nodesGraph, 7);

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
            const string filename = "Resources/PageRankBuildGraphDifferentIds-Floats.nt";
            var nodesGraph = EntityPageRank.BuildSimpleNodesGraph(filename);

            var ranks = EntityPageRank.CalculateRanks(nodesGraph, 7);

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
            const string filename = "Resources/PageRankBuildGraphMultipleProperties.nt";
            var nodesGraph = EntityPageRank.BuildSimpleNodesGraph(filename);

            var ranks = EntityPageRank.CalculateRanks(nodesGraph, 7);

            Assert.Equal(1, Math.Round(ranks.Sum()), 10);

            Assert.Equal(0.138, ranks[0].ToThreeDecimals());
            Assert.Equal(0.087, ranks[1].ToThreeDecimals());
            Assert.Equal(0.061, ranks[2].ToThreeDecimals());
            Assert.Equal(0.180, ranks[3].ToThreeDecimals());
            Assert.Equal(0.128, ranks[4].ToThreeDecimals());
            Assert.Equal(0.222, ranks[5].ToThreeDecimals());
            Assert.Equal(0.180, ranks[6].ToThreeDecimals());
        }
    }
}
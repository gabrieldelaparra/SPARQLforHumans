using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.Core.Services
{
    public static class IndexRanker
    {
        public static double ToThreeDecimals(this double input)
        {
            return Math.Truncate(input * 1000) / 1000;
        }

        private static double pageRankAlpha = 0.85d;

        //Read the file once
        //Order n
        public static Dictionary<string, int> BuildNodesDictionary(string triplesFilename)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupByEntities();

            var nodeCount = 0;
            var dictionary = new Dictionary<string, int>();

            foreach (var group in groups)
            {
                var subjectId = group.FirstOrDefault().GetTriple().Subject.GetId();
                dictionary.Add(subjectId, nodeCount);
                nodeCount++;
            }

            return dictionary;
        }

        //Read the file twice
        //Order n. Suponiendo que dictionary tiene orden 1.
        public static int[][] BuildSimpleNodesGraph(string triplesFilename)
        {
            var nodesDictionary = BuildNodesDictionary(triplesFilename);

            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupByEntities();

            var nodeCount = nodesDictionary.Count;
            var nodeArray = new int[nodeCount][];

            foreach (var group in groups)
            {
                var subjectId = group.FirstOrDefault().GetTriple().Subject.GetId();
                nodesDictionary.TryGetValue(subjectId, out int subjectIndex);

                var entityNode = new GraphNode(subjectId, nodeCount);
                var entityNodeConnections = new List<int>();

                foreach (var line in group)
                {
                    var (_, _, ntObject) = line.GetTripleAsTuple();

                    if (!ntObject.IsEntity())
                        continue;

                    var objectId = ntObject.GetId();
                    nodesDictionary.TryGetValue(objectId, out int objectIndex);

                    if (!entityNodeConnections.Contains(objectIndex))
                        entityNodeConnections.Add(objectIndex);
                }

                nodeArray[subjectIndex] = entityNodeConnections.ToArray();
            }

            return nodeArray;
        }

        public static IEnumerable<GraphNode> BuildNodesGraph(string triplesFilename)
        {
            var list = new List<GraphNode>();
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupByEntities();

            var nodeCount = 0;

            foreach (var group in groups)
            {
                var subjectId = group.FirstOrDefault().GetTriple().Subject.GetId();

                var entityNode = new GraphNode(subjectId, nodeCount);
                var entityNodeConnections = new List<string>();
                foreach (var line in group)
                {
                    var (_, _, ntObject) = line.GetTripleAsTuple();

                    if (!ntObject.IsEntity())
                        continue;

                    var objectId = ntObject.GetId();

                    if (!entityNodeConnections.Contains(objectId))
                        entityNodeConnections.Add(objectId);
                }

                entityNode.ConnectedNodes = entityNodeConnections.ToArray();

                list.Add(entityNode);
                nodeCount++;
            }

            //Assign initial rank to all nodes;
            var initialRank = 1d / nodeCount;
            foreach (var graphNode in list)
            {
                graphNode.Rank = initialRank;
            }

            return list;
        }

        public static void CalculateRanks(IEnumerable<GraphNode> graphNodes, int iterations)
        {
            var nodeCount = graphNodes.Count();

            for (var i = 0; i < iterations; i++)
            {
                IterateRank(graphNodes, nodeCount);
            }
        }

        //Order n
        public static double[] CalculateRanks(int[][] graphNodes, int iterations)
        {
            var nodesCount = graphNodes.Length;

            var oldRanks = CalculateInitialRanks(nodesCount);

            for (var i = 0; i < iterations; i++)
            {
                oldRanks = IterateGraph(graphNodes, oldRanks);
                Console.WriteLine("Iteration " + i + " finished!");
            }

            return oldRanks;
        }

        //Order n
        private static double[] CalculateInitialRanks(int nodesCount)
        {
            var oldRanks = new double[nodesCount];

            var initial = 1d / nodesCount;

            for (var i = 0; i < nodesCount; i++)
            {
                oldRanks[i] = initial;
            }

            return oldRanks;
        }

        private static double[] IterateGraph(int[][] graph, double[] oldRanks)
        {
            var noLinkRank = 0d;
            var nodesCount = graph.Length;
            var ranks = new double[nodesCount];

            for (var i = 0; i < nodesCount; i++)
            {
                if (graph[i].Length > 0)
                {
                    var share = oldRanks[i] * pageRankAlpha / graph[i].Length;
                    foreach (var j in graph[i])
                    {
                        ranks[j] += share;
                    }
                }
                else
                {
                    noLinkRank += oldRanks[i];
                }
            }

            var shareNoLink = noLinkRank * pageRankAlpha / nodesCount;
            var shareMinusD = (1d - pageRankAlpha) / nodesCount;
            var weakRank = shareNoLink + shareMinusD;

            for (var i = 0; i < nodesCount; i++)
            {
                ranks[i] += weakRank;
            }

            if (ranks.Sum() != 1)
                Console.WriteLine("error");

            //Array.Copy(ranks, 0, oldRanks, 0, nodesCount);
            return ranks;
        }

        public static void IterateRank(IEnumerable<GraphNode> graphNodes, int nodeCount)
        {
            var noLinkRank = 0d;
            //Change for float; Save space;
            var ranks = new double[nodeCount];

            foreach (var graphNode in graphNodes)
            {
                if (graphNode.ConnectedNodes.Any())
                {
                    //Change Count, pass it as a value.
                    //Check if Count is constant time. OTHERWISE CREATE A SIZE VAR WHATEVER;
                    var share = graphNode.Rank * pageRankAlpha / graphNode.ConnectedNodes.Length;
                    foreach (var connectedNode in graphNode.ConnectedNodes)
                    {
                        var destinationNode = graphNodes.FirstOrDefault(x => x.Id.Equals(connectedNode));
                        if (destinationNode != null)
                            ranks[destinationNode.Index] += share;
                    }
                }
                else
                {
                    noLinkRank += graphNode.Rank;
                }
            }
            var shareNoLink = (noLinkRank * pageRankAlpha) / nodeCount;
            var shareMinusD = (1d - pageRankAlpha) / nodeCount;
            var weakRank = shareNoLink + shareMinusD;

            foreach (var graphNode in graphNodes)
            {
                ranks[graphNode.Index] += weakRank;
                graphNode.Rank = ranks[graphNode.Index];
            }
        }
    }
}
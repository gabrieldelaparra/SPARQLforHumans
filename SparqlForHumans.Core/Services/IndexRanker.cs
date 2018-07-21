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

        private static float pageRankAlpha = 0.85f;

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

        private static double[] rankGraph(int[][] graph, int iterations)
        {
            var nodesCount = graph.Length;

            var oldRanks = new double[nodesCount];

            var initial = 1d / nodesCount;

            for (int i = 0; i < nodesCount; i++)
            {
                oldRanks[i] = initial;
            }

            double[] ranks = null;
            for (var i = 0; i < iterations; i++)
            {
                ranks = iterateGraph(graph, nodesCount, oldRanks);
                Console.WriteLine("Iteration " + i + " finished!");
            }

            return ranks;
        }

        private static double[] iterateGraph(int[][] graph, int nodes, double[] oldRanks)
        {
            var noLinkRank = 0d;
            var ranks = new double[nodes];

            for (var i = 0; i < nodes; i++)
            {
                if (graph[i] != null)
                {
                    var outGraph = graph[i];
                    var share = oldRanks[i] * pageRankAlpha / outGraph.Length;
                    foreach (var j in outGraph)
                    {
                        ranks[j] += share;
                    }
                }
                else
                {
                    noLinkRank += oldRanks[i];
                }
            }

            var shareNoLink = (noLinkRank * pageRankAlpha) / nodes;

            var shareMinusD = (1d - pageRankAlpha) / nodes;

            var weakRank = shareNoLink + shareMinusD;

            for (var i = 0; i < nodes; i++)
            {
                ranks[i] += weakRank;
            }

            Array.Copy(ranks, 0, oldRanks, 0, nodes);
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
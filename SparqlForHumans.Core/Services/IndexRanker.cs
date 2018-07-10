using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.Core.Services
{
    public static class IndexRanker
    {
        private static readonly NLog.Logger Logger = Utilities.Logger.Init();

        public static double ToThreeDecimals(this double input)
        {
            return Math.Truncate(input * 1000) / 1000;
        }

        private static double pageRankAlpha = 0.85d;
        public static int NotifyTicks { get; } = 1000;

        public static IEnumerable<GraphNode> BuildNodesGraph(string triplesFilename)
        {
            var list = new List<GraphNode>();
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupByEntities();

            var nodeCount = 0;

            foreach (var group in groups)
            {
                if (nodeCount % NotifyTicks == 0)
                    Logger.Info($"Group: {nodeCount:N0}");

                var subjectId = group.FirstOrDefault().GetTriple().Subject.GetId();
                var entityNode = new GraphNode(subjectId, nodeCount);

                foreach (var line in group)
                {
                    var (_, _, ntObject) = line.GetTripleAsTuple();

                    if (!ntObject.IsEntity())
                        continue;

                    var objectId = ntObject.GetId();

                    if (!entityNode.ConnectedNodes.Contains(objectId))
                        entityNode.ConnectedNodes.Add(objectId);
                }

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
                Logger.Info($"Iteration: {i}");
                IterateRank(graphNodes, nodeCount);
            }
        }

        public static void IterateRank(IEnumerable<GraphNode> graphNodes, int nodeCount)
        {
            var noLinkRank = 0d;
            var ranks = new double[nodeCount];

            foreach (var graphNode in graphNodes)
            {
                if (graphNode.ConnectedNodes.Any())
                {
                    var share = graphNode.Rank * pageRankAlpha / graphNode.ConnectedNodes.Count;
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
using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Core.Services
{
    public class GraphNode
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public List<string> ConnectedNodes { get; set; } = new List<string>();
        public double Rank { get; set; }

        public GraphNode(string id, int index)
        {
            Id = id;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Id} - {ConnectedNodes.Count}";
        }
    }

    public static class IndexRanker
    {
        public static double ToThreeDecimals(this double input)
        {
            return Math.Truncate(input * 1000) / 1000;
        }

        private static double pageRankAlpha = 0.85d;

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

        public static void CalculateRanks(IEnumerable<GraphNode> graphNodes, int iterations = 25)
        {
            for (var i = 0; i < iterations; i++)
            {
                CalculateRanks(graphNodes);
            }
        }

        public static void CalculateRanks(IEnumerable<GraphNode> graphNodes)
        {
            //TODO: Check if this takes too much time for a large graph;
            var nodeCount = graphNodes.Count();

            var noLinkRank = 0d;
            var ranks = new double[nodeCount];

            foreach (var graphNode in graphNodes)
            {
                if (graphNode.ConnectedNodes.Any())
                {
                    var share = graphNode.Rank * pageRankAlpha / graphNode.ConnectedNodes.Count;
                    foreach (var connectedNode in graphNode.ConnectedNodes)
                    {
                        //TODO: This might take too much time. That's why it seems to be easier to work with indexes.
                        var destinationNode = graphNodes.FirstOrDefault(x => x.Id.Equals(connectedNode));
                        if (destinationNode != null)
                            ranks[destinationNode.Index] += share;
                        else
                            Console.WriteLine("shiet");
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
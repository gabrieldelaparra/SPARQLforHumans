using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.Core.Services
{
    public static class EntityRanker
    {
        private static readonly NLog.Logger Logger = Utilities.Logger.Init();
        public static int NotifyTicks { get; } = 100000;

        private static double pageRankAlpha = 0.85d;


        /// <summary>
        /// Reads the file (Order n)
        /// Creates a Dictionary(string Q-entity, entityIndexInFile)
        /// Foreach group in the file, adds the (Q-id and the position in the file).
        /// </summary>
        /// <param name="triplesFilename"></param>
        /// <returns></returns>
        public static Dictionary<string, int> BuildNodesDictionary(string triplesFilename)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupBySubject();

            var nodeIndex = 0;
            var dictionary = new Dictionary<string, int>();

            foreach (var group in groups)
            {
                if (nodeIndex % NotifyTicks == 0)
                    Logger.Info($"Building Dictionary, Group: {nodeIndex:N0}");

               var subjectId = GetGroupSubjectId(group);
                dictionary.Add(subjectId, nodeIndex);
                nodeIndex++;
            }
            Logger.Info($"Building Dictionary, Group: {nodeIndex:N0}");

            return dictionary;
        }

        private static string GetGroupSubjectId(IEnumerable<string> group)
        {
            return group.FirstOrDefault().GetTriple().Subject.GetId();
        }

        /// <summary>
        /// Uses the <Q-EntityId, NodeIndex> to build an array with arrays.
        /// </summary>
        /// <param name="triplesFilename"></param>
        /// <returns></returns>
        public static int[][] BuildSimpleNodesGraph(string triplesFilename)
        {
            var nodesDictionary = BuildNodesDictionary(triplesFilename);
            return BuildSimpleNodesGraph(triplesFilename, nodesDictionary);
        }

        /// <summary>
        /// Reads the file (Order n)
        /// Foreach group of A-entities:
        ///     - Looks for all the properties that point to another B-entity.
        ///     - Uses the dictionary to lookup (Order 1?) the B-entity index in the file.
        ///     - Creates a [A-entityIndex, [B-entities-Indices] ]
        /// </summary>
        /// <param name="triplesFilename"></param>
        /// <param name="nodesDictionary"></param>
        /// <returns></returns>
        public static int[][] BuildSimpleNodesGraph(string triplesFilename, Dictionary<string, int> nodesDictionary)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupBySubject();

            var nodeCount = 0;
            var nodeArray = new int[nodesDictionary.Count][];

            foreach (var group in groups)
            {
                if (nodeCount % NotifyTicks == 0)
                    Logger.Info($"Building Graph, Group: {nodeCount:N0}");

                var subjectId = GetGroupSubjectId(group);
                nodesDictionary.TryGetValue(subjectId, out var subjectIndex);

                var entityNodeConnections = new List<int>();

                foreach (var line in group)
                {
                    var (_, _, ntObject) = line.GetTripleAsTuple();

                    //This takes, not only the properties, but direct/properties or other things that are not properties
                    //TODO: Use only properties (?)
                    if (!ntObject.IsEntity())
                        continue;

                    var objectId = ntObject.GetId();
                    nodesDictionary.TryGetValue(objectId, out var objectIndex);

                    if (!entityNodeConnections.Contains(objectIndex))
                        entityNodeConnections.Add(objectIndex);
                }

                nodeArray[subjectIndex] = entityNodeConnections.ToArray();
                nodeCount++;
            }
            Logger.Info($"Building Graph, Group: {nodeCount:N0}");
            return nodeArray;
        }

        //Order n
        public static double[] CalculateRanks(int[][] graphNodes, int iterations)
        {
            var nodesCount = graphNodes.Length;

            var oldRanks = CalculateInitialRanks(nodesCount);

            for (var i = 0; i < iterations; i++)
            {
                oldRanks = IterateGraph(graphNodes, oldRanks);
                Logger.Info($"Iteration {i} finished!");
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

            if (Math.Abs(ranks.Sum() - 1) <= 0.001)
                Logger.Info($"Sum Error: {ranks.Sum()} - 3decimals: {ranks.Sum().ToThreeDecimals()}");

            return ranks;
        }
    }
}
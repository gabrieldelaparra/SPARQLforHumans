using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public static class EntityPageRank
    {
        private const double PageRankAlpha = 0.85d;
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();
        public static int NotifyTicks { get; } = 100000;

        /// <summary>
        ///     Reads the triples file (Order n)
        ///     Creates a Dictionary(string Q-entity, entityIndexInFile)
        ///     Foreach group in the file, adds the (Q-id and the position in the file).
        /// </summary>
        /// <param name="triplesFilename"></param>
        /// <returns></returns>
        public static Dictionary<int, int> BuildNodesDictionary(string triplesFilename)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupBySubject();

            return BuildNodesDictionary(groups);
        }

        public static Dictionary<int, int> BuildNodesDictionary(IEnumerable<SubjectGroup> groups)
        {
            var nodeIndex = 0;
            var dictionary = new Dictionary<int, int>();

            foreach (var group in groups.Where(x => x.IsEntityQ())) {
                if (nodeIndex % NotifyTicks == 0) Logger.Info($"Building Dictionary, Group: {nodeIndex:N0}");

                dictionary.Add(group.IntId, nodeIndex);
                nodeIndex++;
            }

            Logger.Info($"Building Dictionary, Group: {nodeIndex:N0}");

            return dictionary;
        }

        /// <summary>
        ///     Read file x2
        /// </summary>
        /// <param name="triplesFilename"></param>
        /// <returns></returns>
        public static Dictionary<int, double> BuildPageRank(string triplesFilename)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupBySubject();

            return BuildPageRank(groups);
        }

        public static Dictionary<int, double> BuildPageRank(IEnumerable<SubjectGroup> subjectGroups)
        {
            Options.InternUris = false;
            var dictionary = new Dictionary<int, double>();

            Logger.Info("Building <EntityId, PageRankValue> Dictionary");

            //Read +1
            var nodesDictionary = BuildNodesDictionary(subjectGroups);

            //Read +1
            var nodesGraphArray = BuildSimpleNodesGraph(subjectGroups, nodesDictionary);

            var nodesGraphRanks = CalculateRanks(nodesGraphArray, 20);

            foreach (var node in nodesDictionary) {
                nodesDictionary.TryGetValue(node.Key, out var subjectIndex);
                var boost = nodesGraphRanks[subjectIndex];
                dictionary.Add(node.Key, boost);
            }

            return dictionary;
        }

        /// <summary>
        ///     Uses the <Q-EntityId, NodeIndex> to build an array with arrays.
        /// </summary>
        /// <param name="triplesFilename"></param>
        /// <returns></returns>
        public static int[][] BuildSimpleNodesGraph(string triplesFilename)
        {
            var nodesDictionary = BuildNodesDictionary(triplesFilename);
            return BuildSimpleNodesGraph(triplesFilename, nodesDictionary);
        }

        /// <summary>
        ///     Reads the file (Order n)
        ///     Foreach group of A-entities:
        ///     - Looks for all the properties that point to another B-entity.
        ///     - Uses the dictionary to lookup (Order 1?) the B-entity index in the file.
        ///     - Creates a [A-entityIndex, [B-entities-Indices] ]
        /// </summary>
        /// <param name="triplesFilename"></param>
        /// <param name="nodesDictionary"></param>
        /// <returns></returns>
        public static int[][] BuildSimpleNodesGraph(string triplesFilename, Dictionary<int, int> nodesDictionary)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupBySubject();

            return BuildSimpleNodesGraph(groups, nodesDictionary);
        }

        public static int[][] BuildSimpleNodesGraph(IEnumerable<SubjectGroup> groups,
                                                    Dictionary<int, int> nodesDictionary)
        {
            var nodeCount = 0;
            var nodeArray = new int[nodesDictionary.Count][];

            foreach (var group in groups) {
                if (nodeCount % NotifyTicks == 0) Logger.Info($"Building Graph, Group: {nodeCount:N0}");

                nodesDictionary.TryGetValue(group.IntId, out var subjectIndex);

                var entityNodeConnections = new HashSet<int>();

                foreach (var triple in group) {
                    if (triple == null) continue;

                    var (_, ntPredicate, ntObject) = triple.AsTuple();

                    if (!ntPredicate.IsProperty()) continue;
                    if (!ntObject.IsEntityQ()) continue;

                    var objectId = ntObject.GetIntId();
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

            for (var i = 0; i < iterations; i++) {
                oldRanks = IterateGraph(graphNodes, oldRanks);
                Logger.Info($"Iteration {i + 1} finished!");
            }

            return oldRanks;
        }

        //Order n
        private static double[] CalculateInitialRanks(int nodesCount)
        {
            var oldRanks = new double[nodesCount];

            var initial = 1d / nodesCount;

            for (var i = 0; i < nodesCount; i++) {
                oldRanks[i] = initial;
            }

            return oldRanks;
        }

        private static double[] IterateGraph(IReadOnlyList<IReadOnlyList<int>> graph, IReadOnlyList<double> oldRanks)
        {
            var noLinkRank = 0d;
            var nodesCount = graph.Count;
            var ranks = new double[nodesCount];

            for (var i = 0; i < nodesCount; i++) {
                if (graph[i].Count > 0) {
                    var share = oldRanks[i] * PageRankAlpha / graph[i].Count;
                    foreach (var j in graph[i]) {
                        ranks[j] += share;
                    }
                }
                else
                    noLinkRank += oldRanks[i];
            }

            var shareNoLink = noLinkRank * PageRankAlpha / nodesCount;
            var shareMinusD = (1d - PageRankAlpha) / nodesCount;
            var weakRank = shareNoLink + shareMinusD;

            for (var i = 0; i < nodesCount; i++) {
                ranks[i] += weakRank;
            }

            if (Math.Abs(ranks.Sum() - 1) > 0.001)
                Logger.Info($"Sum Error: {ranks.Sum()} - 3decimals: {ranks.Sum().ToThreeDecimals()}");

            return ranks;
        }
    }
}
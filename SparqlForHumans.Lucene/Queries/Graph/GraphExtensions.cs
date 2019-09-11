using System.Linq;
using System.Collections.Generic;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.Query;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class GraphExtensions
    {
        internal static bool HasInstanceOf(this string[] uris)
        {
            return uris.Any(IsInstanceOf);
        }
        internal static bool IsInstanceOf(this string uri)
        {
            return uri.EndsWith("P31");
        }
        internal static IEnumerable<Edge> GetInstanceOfEdges(this QueryNode node, QueryGraph graph)
        {
            return node.GetOutgoingEdges(graph)?.Where(x => x.IsInstanceOf);
        }
        internal static IEnumerable<string> GetInstanceOfValues(this QueryNode node, QueryGraph graph)
        {
            return node.GetInstanceOfEdges(graph)?.SelectMany(x => x.uris);
        }
        public static QueryNode GetSourceNode(this QueryEdge edge, QueryGraph graph)
        {
            return graph.Nodes.FirstOrDefault(x => x.Key.Equals(edge.sourceId)).Value;
        }
        public static QueryNode GetTargetNode(this QueryEdge edge, QueryGraph graph)
        {
            return graph.Nodes.FirstOrDefault(x => x.Key.Equals(edge.targetId)).Value;
        }
        internal static IEnumerable<QueryEdge> GetOutgoingEdges(this QueryNode node, QueryGraph graph)
        {
            return graph.Edges.Select(x=>x.Value).Where(x => x.sourceId.Equals(node.id));
        }

        internal static IEnumerable<QueryEdge> GetIncommingEdges(this QueryNode node, QueryGraph graph)
        {
            return graph.Edges.Select(x=>x.Value).Where(x => x.targetId.Equals(node.id));
        }

        internal static IEnumerable<QueryNode> GetOutgoingNodes(this QueryNode node, QueryGraph graph)
        {
            var edges = node.GetOutgoingEdges(graph);
            if (edges == null) return null;
            return edges.Select(x => x.GetTargetNode(graph));
        }

        //TODO: Change name:
        internal static void ParseQueryType(this QueryNode node, QueryGraph graph)
        {
            if (node.uris.Any())
                node.QueryType = QueryType.ConstantTypeDoNotQuery;
            else if (node.IsKnownType && node.IsDirectedToKnownType)
                node.QueryType = QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities;
            else if (node.IsKnownType)
                node.QueryType = QueryType.KnownSubjectTypeQueryInstanceEntities;
            else if (node.IsInferredTypeDomain && node.IsInferredTypeRange)
                node.QueryType = QueryType.InferredDomainAndRangeTypeEntities;
            else if (node.IsInferredTypeDomain && node.IsDirectedToKnownType)
                node.QueryType = QueryType.KnownPredicateAndObject;
            else if (node.IsInferredTypeDomain)
                node.QueryType = QueryType.InferredDomainTypeEntities;
            else if (node.IsInferredTypeRange)
                node.QueryType = QueryType.InferredRangeTypeEntities;
            else if (node.IsDirectedToKnownType)
                node.QueryType = QueryType.KnownObjectTypeNotUsed;
            else
                node.QueryType = QueryType.QueryTopEntities;
        }

        internal static void ParseQueryType(this QueryEdge edge, QueryGraph graph)
        {
            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);

            if (edge.uris.Any())
                edge.QueryType = QueryType.ConstantTypeDoNotQuery;
            else if (source.IsKnownType && target.IsKnownType)
                edge.QueryType = QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            else if (source.IsKnownType)
                edge.QueryType = QueryType.KnownSubjectTypeOnlyQueryDomainProperties;
            else if (target.IsKnownType)
                edge.QueryType = QueryType.KnownObjectTypeOnlyQueryRangeProperties;
            else if (source.IsInferredType && target.IsInferredType)
            {
                edge.QueryType = QueryType.InferredDomainAndRangeTypeProperties;
                edge.Domain = source.InferredTypes;
                edge.Range = target.InferredTypes;
            }
            else if (source.IsInferredType)
            {
                edge.QueryType = QueryType.InferredDomainTypeProperties;
                edge.Domain = source.InferredTypes;
            }
            else if (target.IsInferredType)
            {
                edge.QueryType = QueryType.InferredRangeTypeProperties;
                edge.Range = target.InferredTypes;
            }
            else
                edge.QueryType = QueryType.QueryTopProperties;
        }

        internal static void ExploreGraph(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            foreach (var edge in graph.Edges.Select(x=>x.Value))
            {
                if (edge.uris.HasInstanceOf())
                    edge.IsInstanceOf = true;
                else
                {
                    if (string.IsNullOrWhiteSpace(propertyIndexPath)) continue;
                    var properties = new BatchIdPropertyQuery(propertyIndexPath, edge.uris.Select(x => x.GetUriIdentifier())).Query();
                    edge.Domain = properties.SelectMany(x => x.Domain).Select(x => $"{Models.Wikidata.WikidataDump.EntityPrefix}{x}").ToList();
                    edge.Range = properties.SelectMany(x => x.Range).Select(x => $"{Models.Wikidata.WikidataDump.EntityPrefix}{x}").ToList();
                }
            }

            foreach (var node in graph.Nodes.Select(x=>x.Value))
            {
                if (node.GetInstanceOfValues(graph).Any())
                {
                    node.IsKnownType = true;
                    node.Types = node.GetOutgoingNodes(graph).SelectMany(x => x.uris).Distinct().ToList();
                }
                else if (node.uris.Any())
                {
                    node.IsKnownType = true;
                    node.Types = new BatchIdEntityQuery(entitiesIndexPath, node.uris.Distinct()).Query().SelectMany(x => x.InstanceOf).ToList();
                }
                else
                {
                    if (node.GetOutgoingEdges(graph).Any(x => x.Domain.Any()))
                    {
                        node.IsInferredTypeDomain = true;
                        node.InferredTypes = node.InferredTypes.Union(node.GetOutgoingEdges(graph).SelectMany(x => x.Domain)).ToList();
                    }
                    if (node.GetIncommingEdges(graph).Any(x => x.Range.Any()))
                    {
                        node.IsInferredTypeRange = true;
                        node.InferredTypes = node.InferredTypes.Union(node.GetIncommingEdges(graph).SelectMany(x => x.Range)).ToList();
                    }
                }
            }

            foreach (var node in graph.Nodes.Select(x=>x.Value))
            {
                if (node.GetOutgoingNodes(graph).Any(x => x.IsKnownType))
                    node.IsDirectedToKnownType = true;
            }

        }

        internal static void TraverseDepthFirstNode(this QueryGraph graph, int nodeId)
        {
            // Get the Node
            var node = graph.Nodes.FirstOrDefault(x => x.Key.Equals(nodeId)).Value;
            if (node.Traversed) return;

            // Check rules for this Node:
            node.ParseQueryType(graph);

            // Mark as Checked
            node.Traversed = true;

            // Recursion
            foreach (var edge in node.GetOutgoingEdges(graph).Where(x => !x.Traversed))
            {
                graph.TraverseDepthFirstEdge(edge.id);
            }
        }

        internal static void TraverseDepthFirstEdge(this QueryGraph graph, int edgeId)
        {
            // Get the Edge
            var edge = graph.Edges.FirstOrDefault(x => x.Key.Equals(edgeId)).Value;
            if (edge.Traversed) return;

            // Check rules for this Edge:
            edge.ParseQueryType(graph);

            // Mark as Checked
            edge.Traversed = true;

            // Recursion
            var node = edge.GetTargetNode(graph);
            if (!node.Traversed)
                graph.TraverseDepthFirstNode(node.id);
        }
    }


}

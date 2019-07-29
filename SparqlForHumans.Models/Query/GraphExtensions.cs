using System.Linq;
using System.Collections.Generic;
namespace SparqlForHumans.Models.Query
{
    public static class GraphExtensions
    {
        internal static bool HasInstanceOf(this string[] uris)
        {
            return uris.Any(u => u.EndsWith("P31"));
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
            return graph.Nodes.Find(x => x.id.Equals(edge.sourceId));
        }
        public static QueryNode GetTargetNode(this QueryEdge edge, QueryGraph graph)
        {
            return graph.Nodes.Find(x => x.id.Equals(edge.targetId));
        }
        internal static IEnumerable<QueryEdge> GetOutgoingEdges(this QueryNode node, QueryGraph graph)
        {
            return graph.Edges.Where(x => x.sourceId.Equals(node.id));
        }

        internal static IEnumerable<QueryNode> GetConnectedNodes(this QueryNode node, QueryGraph graph)
        {
            var edges = node.GetOutgoingEdges(graph);
            if (edges == null) return null;
            return edges.Select(x => x.GetTargetNode(graph));
        }

        //TODO: Change name:
        internal static void ParseQueryType(this QueryNode node, QueryGraph graph)
        {
            if(node.uris.Any())
                node.QueryType = QueryType.ConstantTypeDoNotQuery;
            else if (node.IsKnownType && node.IsConnectedToKnownType)
                node.QueryType = QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities;
            else if (node.IsKnownType)
                node.QueryType = QueryType.KnownSubjectTypeQueryInstanceEntities;
            else if (node.IsConnectedToKnownType)
                node.QueryType = QueryType.KnownObjectTypeNotUsed;
            else
                node.QueryType = QueryType.QueryTopEntities;
        }

        internal static void ParseQueryType(this QueryEdge edge, QueryGraph graph)
        {
            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);

            if(edge.uris.Any())
                edge.QueryType = QueryType.ConstantTypeDoNotQuery;
            else if (source.IsKnownType && target.IsKnownType)
                edge.QueryType = QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            else if (source.IsKnownType)
                edge.QueryType = QueryType.KnownSubjectTypeOnlyQueryDomainProperties;
            else if (target.IsKnownType)
                edge.QueryType = QueryType.KnownObjectTypeOnlyQueryRangeProperties;
            else
                edge.QueryType = QueryType.QueryTopProperties;
        }

        internal static void ExploreGraph(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges)
            {
                if (edge.uris.HasInstanceOf())
                    edge.IsInstanceOf = true;
            }

            foreach (var node in graph.Nodes)
            {
                if (node.GetInstanceOfValues(graph).Any())
                {
                    node.IsKnownType = true;
                    node.Types = node.GetConnectedNodes(graph).SelectMany(x=>x.uris).Distinct().ToList();
                }
            }

            foreach (var node in graph.Nodes)
            {
                if (node.GetConnectedNodes(graph).Any(x => x.IsKnownType))
                    node.IsConnectedToKnownType = true;
            }

        }

        internal static void TraverseDepthFirstNode(this QueryGraph graph, int nodeId)
        {
            // Get the Node
            var node = graph.Nodes.Find(x => x.id.Equals(nodeId));
            if (node.Traversed) return;

            // Check rules for this Node:
            node.ParseQueryType(graph);

            // Mark as Checked
            node.Traversed = true;

            // Recursion
            foreach (var edge in node.GetOutgoingEdges(graph).Where(x => !x.Traversed))
            {
                TraverseDepthFirstEdge(graph, edge.id);
            }
        }

        internal static void TraverseDepthFirstEdge(this QueryGraph graph, int edgeId)
        {
            // Get the Edge
            var edge = graph.Edges.Find(x => x.id.Equals(edgeId));
            if (edge.Traversed) return;

            // Check rules for this Edge:
            edge.ParseQueryType(graph);

            // Mark as Checked
            edge.Traversed = true;

            // Recursion
            var node = edge.GetTargetNode(graph);
            if (!node.Traversed)
                TraverseDepthFirstNode(graph, node.id);
        }
    }


}

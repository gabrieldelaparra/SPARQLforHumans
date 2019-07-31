using System.Linq;
using System.Collections.Generic;
namespace SparqlForHumans.Models.Query
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
            else if (node.IsKnownType && node.IsGoingToKnownType)
                node.QueryType = QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities;
            else if (node.IsKnownType)
                node.QueryType = QueryType.KnownSubjectTypeQueryInstanceEntities;
             //else if (node.IsInferredType)
             // node.QueryType = QueryType.InferredSubjectType;
            else if (node.IsGoingToKnownType)
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
            //else if (source.IsInferredType && target.IsInferredType)
            //  edge.QueryType = QueryType.InferredSubjectAndObjectPredicateType
            //else if (source.IsInferredType )
            //  edge.QueryType = QueryType.InferredSubjectPredicateType
            //else if (target.IsInferredType)
            //  edge.QueryType = QueryType.InferredObjectPredicateType
            else
                edge.QueryType = QueryType.QueryTopProperties;
        }

        internal static void ExploreGraph(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges)
            {
                if (edge.uris.HasInstanceOf())
                    edge.IsInstanceOf = true;
                else if (edge.uris.Any(x => !x.IsInstanceOf()))
                {
                    //var properties = new BatchIdPropertyQuery(propertyOutputPath, edge.uris.Any(x => !x.IsInstanceOf()).ToList()).Query();
                    //edge.Domain = properties.Select(x=>x.Domain).SelectMany(x=>$"{entityURI}{x}").ToList();
                    //edge.Range = properties.Select(x=>x.Range)..SelectMany(x=>$"{entityURI}{x}").ToList();
                }
            }

            foreach (var node in graph.Nodes)
            {
                if (node.GetInstanceOfValues(graph).Any())
                {
                    node.IsKnownType = true;
                    node.Types = node.GetOutgoingNodes(graph).SelectMany(x => x.uris).Distinct().ToList();
                }
                //else 
                //{
                //if (node.GetOutgoingEdges(graph)?.Domain.Any()){
                //  node.IsInferredType = true;
                //  node.InferredTypes.AddRange(node.GetOutgoingEdges(graph)?.Domain);
                //}
                //if (node.GetIncommingEdges(graph)?.Range.Any()){
                //  node.IsInferredType = true;
                //  node.InferredTypes.AddRange(node.GetIncommingEdges(graph)?.Range);
                //  }
                //}
            }

            foreach (var node in graph.Nodes)
            {
                if (node.GetOutgoingNodes(graph).Any(x => x.IsKnownType))
                    node.IsGoingToKnownType = true;
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

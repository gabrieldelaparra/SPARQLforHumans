using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class GraphTraversal
    {
        internal static void TraverseGraph(this QueryGraph graph)
        {
            //TODO: Need some comments here. Which cases are this. Or some tests.
            //For all Entities:
            //If the node has a GivenType, just use that type.
            //If the node is connected to a InstanceOf Node, get those Types.
            //
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if(node.IsGivenType)
                    node.Types = node.uris.ToList();
                else if (node.GetInstanceOfValues(graph).Any())
                {
                    node.IsKnownType = true;
                    node.Types = node.GetOutgoingNodes(graph).SelectMany(x => x.uris).Distinct().ToList();
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

            //TODO: Need some comments here. Which cases are this. Or some tests.
            foreach (var node in graph.Nodes.Select(x => x.Value))
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

        //TODO: Change name (?)
        internal static void ParseQueryType(this QueryNode node, QueryGraph graph)
        {
            //Given Type:
            if (node.uris.Any())
                node.QueryType = QueryType.ConstantTypeDoNotQuery;
            
            //Known Types, based on the outgoing P31 edges:
            else if (node.IsKnownType && node.IsDirectedToKnownType)
                node.QueryType = QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities;
            else if (node.IsKnownType)
                node.QueryType = QueryType.KnownSubjectTypeQueryInstanceEntities;
            
            //Inferred Types, based on destination P31:
            else if (node.IsInferredTypeDomain && node.IsInferredTypeRange)
                node.QueryType = QueryType.InferredDomainAndRangeTypeEntities;
            else if (node.IsInferredTypeDomain && node.IsDirectedToKnownType)
                node.QueryType = QueryType.KnownPredicateAndObjectNotUsed;
            else if (node.IsInferredTypeDomain)
                node.QueryType = QueryType.InferredDomainTypeEntities;
            else if (node.IsInferredTypeRange)
                node.QueryType = QueryType.InferredRangeTypeEntities;
            
            // TODO: Document what is this
            else if (node.IsDirectedToKnownType)
                node.QueryType = QueryType.KnownObjectTypeNotUsed;

            //No known case. No given, known or inferred types;
            else
                node.QueryType = QueryType.QueryTopEntities;
        }

        internal static void ParseQueryType(this QueryEdge edge, QueryGraph graph)
        {
            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);

            //Given type for the edge. Like P31 or others:
            if (edge.uris.Any())
                edge.QueryType = QueryType.ConstantTypeDoNotQuery;

            // When the parent node is known, then only the properties available for that node should be displayed (?)
            // TODO: Add cases for All 3 cases: Source&Destination; Source; Destination
            else if (source.IsGivenType && target.IsGivenType)
                edge.QueryType = QueryType.GivenSubjectUrisQueryGivenProperties;

            // TODO: I need some visual aid to describe these cases:
            else if (source.IsKnownType && target.IsKnownType)
                edge.QueryType = QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            else if (source.IsKnownType)
                edge.QueryType = QueryType.KnownSubjectTypeQueryDomainProperties;
            else if (target.IsKnownType)
                edge.QueryType = QueryType.KnownObjectTypeQueryRangeProperties;

            // Inferred Types:
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
            
            // Nothing found from our search cases:
            else
                edge.QueryType = QueryType.QueryTopProperties;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphTraversal
    {
        /// <summary>
        /// Given a graph, iterate on it:
        ///
        /// Run a first iteration on nodes, find if:
        /// Node.IsGivenType
        /// Node.IsInstanceOfType
        ///
        /// Run a second iteration on nodes, find if:
        /// Node.IsGoingToGivenType (OutgoingNodes Any IsGivenType)
        /// Node.IsComingFromGivenType (IncomingNodes Any IsGivenType)
        /// Node.IsGoingToInstanceOfType (OutgoingNodes Any IsInstanceOfType)
        /// Node.IsComingFromInstanceOfType (IncomingNodes Any IsInstanceOfType)
        /// </summary>
        internal static void CheckNodeTypes(this QueryGraph graph)
        {
            //First iteration, check self
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                //if (node.uris.Any()) node.IsGivenType = true;
                if (node.GetOutgoingEdges(graph).Any(x => x.IsInstanceOf)) node.IsInstanceOfType = true;
            }

            //Second iteration, check direct neighbors
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.GetOutgoingNodes(graph).Any(x => x.IsGivenType)) node.IsGoingToGivenType = true;
                if (node.GetIncomingNodes(graph).Any(x => x.IsGivenType)) node.IsComingFromGivenType = true;
                //Connects to a node that is InstanceOf.
                //TODO: I think that I do not need this:
                if (node.GetOutgoingNodes(graph).Any(x => x.IsInstanceOfType)) node.IsGoingToInstanceOfType = true;
                if (node.GetIncomingNodes(graph).Any(x => x.IsInstanceOfType)) node.IsComingFromInstanceOfType = true;
                //Has edges to other nodes, such as MotherOf, BornIn, etc. The type can be Inferred.
                if (node.GetOutgoingEdges(graph).Any(x => x.IsInferible())) node.IsInferredDomainType = true;
                if (node.GetIncomingEdges(graph).Any(x => x.IsInferible())) node.IsInferredRangeType = true;
            }
        }

        internal static void TraverseDepthFirstNode(this QueryGraph graph, int nodeId)
        {
            // Get the Node
            var node = graph.Nodes.FirstOrDefault(x => x.Key.Equals(nodeId)).Value;
            if (node.Traversed) return;

            // Check rules for this Node:
            node.QueryType = node.GetQueryType(graph);

            // Mark as Checked
            node.Traversed = true;

            // Recursion
            foreach (var edge in node.GetOutgoingEdges(graph).Where(x => !x.Traversed))
                graph.TraverseDepthFirstEdge(edge.id);
        }

        internal static void TraverseDepthFirstEdge(this QueryGraph graph, int edgeId)
        {
            // Get the Edge
            var edge = graph.Edges.FirstOrDefault(x => x.Key.Equals(edgeId)).Value;
            if (edge.Traversed) return;

            // Check rules for this Edge:
            edge.QueryType = edge.GetQueryType(graph);

            // Mark as Checked
            edge.Traversed = true;

            // Recursion
            var node = edge.GetTargetNode(graph);
            if (!node.Traversed)
                graph.TraverseDepthFirstNode(node.id);
        }

        private static QueryType GetQueryType(this QueryNode node, QueryGraph graph)
        {
            //Give Type
            if (node.IsGivenType) return QueryType.GivenEntityTypeNoQuery;
            
            if (node.IsInstanceOfType) return QueryType.SubjectIsInstanceOfTypeQueryEntities;

            if (node.IsGoingToGivenType) return QueryType.GivenObjectTypeQueryDirectly;
            if (node.IsComingFromGivenType) return QueryType.GivenSubjectTypeQueryDirectly;

            if (node.IsInferredDomainType && node.IsInferredRangeType) return QueryType.InferredDomainAndRangeTypeEntities;
            if (node.IsInferredDomainType) return QueryType.InferredDomainTypeEntities;
            if (node.IsInferredRangeType) return QueryType.InferredRangeTypeEntities;

            return QueryType.QueryTopEntities;
        }

        private static QueryType GetQueryType(this QueryEdge edge, QueryGraph graph)
        {
            //Given type for the edge. Like P31 or others:
            if (edge.IsGivenType) return QueryType.GivenPredicateTypeNoQuery;

            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);

            if (source.IsGivenType && target.IsGivenType) return QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties;
            if (source.IsGivenType) return QueryType.GivenSubjectTypeDirectQueryOutgoingProperties;
            if (target.IsGivenType) return QueryType.GivenObjectTypeDirectQueryIncomingProperties;

            if (source.IsInstanceOfType && target.IsInstanceOfType) return QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            if (source.IsInstanceOfType) return QueryType.KnownSubjectTypeQueryDomainProperties;
            if (target.IsInstanceOfType) return QueryType.KnownObjectTypeQueryRangeProperties;

            // Inferred Types:
            if (source.IsInferredType && target.IsInferredType) return QueryType.InferredDomainAndRangeTypeProperties;
            if (source.IsInferredType) return QueryType.InferredDomainTypeProperties;
            if (target.IsInferredType) return QueryType.InferredRangeTypeProperties;

            return QueryType.QueryTopProperties;
        }
    }
}

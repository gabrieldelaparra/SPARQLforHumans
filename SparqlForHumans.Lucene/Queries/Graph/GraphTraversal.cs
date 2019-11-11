using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class GraphTraversal
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
            //All combinations with Given type are not valid:
            //if (node.IsGivenType && node.IsInstanceOfType) return QueryType.Unknown;
            //if (node.IsGivenType && node.IsGoingToGivenType) return QueryType.Unknown;
            //if (node.IsGivenType && node.IsComingFromGivenType) return QueryType.Unknown;
            //if (node.IsGivenType && node.IsGoingToInstanceOfType) return QueryType.Unknown;
            //if (node.IsGivenType && node.IsComingFromInstanceOfType) return QueryType.Unknown;

            //if (node.IsInstanceOfType && node.IsGoingToInstanceOfType) return QueryType.Unknown;
            //if (node.IsInstanceOfType && node.IsComingFromInstanceOfType) return QueryType.Unknown;
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

            if (source.IsGivenType && target.IsGivenType) return QueryType.GivenSubjectAndObjectTypeQueryIntersectOutInProperties;
            if (source.IsGivenType) return QueryType.GivenSubjectTypeQueryOutgoingProperties;
            if (target.IsGivenType) return QueryType.GivenObjectTypeQueryIncomingProperties;

            if (source.IsInstanceOfType && target.IsInstanceOfType) return QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            if (source.IsInstanceOfType) return QueryType.KnownSubjectTypeQueryDomainProperties;
            if (target.IsInstanceOfType) return QueryType.KnownObjectTypeQueryRangeProperties;

            // Inferred Types:
            if (source.IsInferredType && target.IsInferredType) return QueryType.InferredDomainAndRangeTypeProperties;
            if (source.IsInferredType) return QueryType.InferredDomainTypeProperties;
            if (target.IsInferredType) return QueryType.InferredRangeTypeProperties;

            return QueryType.QueryTopProperties;
        }

        //TODO: Change name (?)
        //internal static void ParseQueryType(this QueryNode node, QueryGraph graph)
        //{
        //    node.QueryType = node.GetQueryType(graph);
        //    //Given Type:
        //    if (node.IsGivenType) node.QueryType = QueryType.GivenEntityTypeNoQuery;
        //    //if(node.IsGivenType && node.IsInstanceOfType) //Should not be valid

        //    //Known Types, based on the outgoing P31 edges:
        //    else if (node.IsInstanceOfType && node.IsGoingToInstanceOfType) node.QueryType = QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities;
        //    else if (node.IsInstanceOfType) node.QueryType = QueryType.KnownSubjectTypeQueryInstanceEntities;

        //    //Inferred Types, based on destination P31:
        //    else if (node.IsInferredDomainType && node.IsInferredRangeType)
        //        node.QueryType = QueryType.InferredDomainAndRangeTypeEntities;
        //    else if (node.IsInferredDomainType && node.IsGoingToInstanceOfType)
        //        node.QueryType = QueryType.KnownPredicateAndObjectNotUsed;
        //    else if (node.IsInferredDomainType)
        //        node.QueryType = QueryType.InferredDomainTypeEntities;
        //    else if (node.IsInferredRangeType)
        //        node.QueryType = QueryType.InferredRangeTypeEntities;

        //    // TODO: Document what is this
        //    else if (node.IsGoingToInstanceOfType)
        //        node.QueryType = QueryType.KnownObjectTypeNotUsed;

        //    //No known case. No given, known or inferred types;
        //    else
        //        node.QueryType = QueryType.QueryTopEntities;
        //}

        internal static void ParseQueryType(this QueryEdge edge, QueryGraph graph)
        {
            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);

            //Given type for the edge. Like P31 or others:
            if (edge.IsGivenType) edge.QueryType = QueryType.GivenPredicateTypeNoQuery;

            // When the parent node is known, then only the properties available for that node should be displayed (?)
            // TODO: Add cases for All 3 cases: Source&Destination; Source; Destination
            else if (source.IsGivenType && target.IsGivenType)
                edge.QueryType = QueryType.GivenSubjectTypeQueryOutgoingProperties;

            // TODO: I need some visual aid to describe these cases:
            else if (source.IsInstanceOfType && target.IsInstanceOfType)
                edge.QueryType = QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            else if (source.IsInstanceOfType)
                edge.QueryType = QueryType.KnownSubjectTypeQueryDomainProperties;
            else if (target.IsInstanceOfType)
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

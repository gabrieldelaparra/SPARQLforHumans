using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Models;

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
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                if (edge.uris.Any())
                    edge.IsGivenType = true;
            }
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.uris.Any())
                    node.IsConstant = true;
                if (node.GetOutgoingEdges(graph).Any(x => x.IsInstanceOf))
                    node.IsInstanceOf = true;
            }

            //Second iteration, check direct neighbors
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                //Connected to Given Types.
                //if (node.GetOutgoingNodes(graph).Any(x => x.IsConstant)) 
                //    node.IsGoingToGivenType = true;
                //if (node.GetIncomingNodes(graph).Any(x => x.IsConstant)) 
                //    node.IsComingFromGivenType = true;

                //Has edges to other nodes, such as MotherOf, BornIn, etc. The type can be Inferred.
                if (node.GetOutgoingEdges(graph).Any(x => x.IsInferible())) 
                    node.IsInferredDomainType = true;
                if (node.GetIncomingEdges(graph).Any(x => x.IsInferible())) 
                    node.IsInferredRangeType = true;
            }
        }

        //internal static void TraverseDepthFirstNode(this QueryGraph graph, int nodeId)
        //{
        //    // Get the Node
        //    var node = graph.Nodes.FirstOrDefault(x => x.Key.Equals(nodeId)).Value;
        //    if (node.Traversed) return;

        //    // Check rules for this Node:
        //    //node.QueryType = node.GetQueryType(graph);

        //    // Mark as Checked
        //    node.Traversed = true;

        //    // Recursion
        //    foreach (var edge in node.GetOutgoingEdges(graph).Where(x => !x.Traversed))
        //        graph.TraverseDepthFirstEdge(edge.id);
        //}

        //internal static void TraverseDepthFirstEdge(this QueryGraph graph, int edgeId)
        //{
        //    // Get the Edge
        //    var edge = graph.Edges.FirstOrDefault(x => x.Key.Equals(edgeId)).Value;
        //    if (edge.Traversed) return;

        //    // Check rules for this Edge:
        //    edge.QueryType = edge.GetQueryType(graph);

        //    // Mark as Checked
        //    edge.Traversed = true;

        //    // Recursion
        //    var node = edge.GetTargetNode(graph);
        //    if (!node.Traversed)
        //        graph.TraverseDepthFirstNode(node.id);
        //}

        //private static QueryType GetQueryType(this QueryNode node, QueryGraph graph)
        //{
        //    //Give Type
        //    if (node.IsGivenType) return QueryType.GivenEntityTypeNoQuery;

        //    if (node.IsInstanceOfType
        //        && !node.IsGoingToGivenType
        //        && !node.IsComingFromGivenType
        //        && !node.IsInferredDomainType
        //        && !node.IsInferredRangeType) return QueryType.SubjectIsInstanceOfTypeQueryEntities;

        //    //if (node.IsGoingToGivenType || node.IsComingFromGivenType) return QueryType.DirectQuery;
        //    if (node.IsGoingToGivenType) return QueryType.GivenObjectTypeQueryDirectlyEntities;
        //    if (node.IsComingFromGivenType) return QueryType.GivenSubjectTypeQueryDirectlyEntities;

        //    if (node.IsInferredDomainType && node.IsInferredRangeType) return QueryType.InferredDomainAndRangeTypeEntities;
        //    if (node.IsInferredDomainType) return QueryType.InferredDomainTypeEntities;
        //    if (node.IsInferredRangeType) return QueryType.InferredRangeTypeEntities;

        //    if (!node.IsGivenType
        //        && !node.IsInstanceOfType
        //        && !node.IsGoingToGivenType
        //        && !node.IsComingFromGivenType
        //        && !node.IsInferredDomainType
        //        && !node.IsInferredRangeType) return QueryType.QueryTopEntities;

        //    return QueryType.DirectQuery;
        //}


        //private static QueryType GetQueryType(this QueryEdge edge, QueryGraph graph)
        //{
        //    //Given type for the edge. Like P31 or others:
        //    //if (edge.IsGivenType) return QueryType.GivenPredicateTypeNoQuery;

        //    var source = edge.GetSourceNode(graph);
        //    var target = edge.GetTargetNode(graph);

        //    //if (source.IsGivenType && target.IsGivenType) return QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties;
        //    //if (source.IsGivenType) return QueryType.GivenSubjectTypeDirectQueryOutgoingProperties;
        //    //if (target.IsGivenType) return QueryType.GivenObjectTypeDirectQueryIncomingProperties;

        //    //if (source.IsInstanceOfType && target.IsInstanceOfType) return QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
        //    //if (source.IsInstanceOfType) return QueryType.KnownSubjectTypeQueryDomainProperties;
        //    //if (target.IsInstanceOfType) return QueryType.KnownObjectTypeQueryRangeProperties;

        //    // Inferred Types:
        //    //if (source.IsInferredType && target.IsInferredType) return QueryType.InferredDomainAndRangeTypeProperties;
        //    //if (source.IsInferredType) return QueryType.InferredDomainTypeProperties;
        //    //if (target.IsInferredType) return QueryType.InferredRangeTypeProperties;

        //    //if (source.IsDirectQuery() || target.IsDirectQuery()) return QueryType.DirectQuery;

        //    return QueryType.QueryTopProperties;
        //}
    }
}

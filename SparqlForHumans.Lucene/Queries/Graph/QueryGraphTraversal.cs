using System.Linq;
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
            foreach (var edge in graph.Edges.Select(x => x.Value)) {
                if (edge.uris.Any())
                    edge.IsConstant = true;
            }

            foreach (var node in graph.Nodes.Select(x => x.Value)) {
                if (node.uris.Any())
                    node.IsConstant = true;
                if (node.GetOutgoingEdges(graph).Any(x => x.IsInstanceOf))
                    node.IsInstanceOf = true;
            }

            //Second iteration, check direct neighbors
            foreach (var node in graph.Nodes.Select(x => x.Value)) {
                //Has edges to other nodes, such as MotherOf, BornIn, etc. The type can be Inferred.
                if (node.GetOutgoingEdges(graph).Any(x => x.IsInferible()))
                    node.IsInferredDomainType = true;
                if (node.GetIncomingEdges(graph).Any(x => x.IsInferible()))
                    node.IsInferredRangeType = true;
            }
        }
    }
}
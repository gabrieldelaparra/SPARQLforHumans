using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphExtensions
    {
        /// <summary>
        /// Given an Edge, checks if the Edge.uris has any value ending with "P31" (InstanceOf)
        /// </summary>
        public static bool HasInstanceOf(this QueryEdge edge)
        {
            return edge.uris.HasInstanceOf();
        }
        private static bool HasInstanceOf(this string[] uris)
        {
            return uris.Any(IsInstanceOf);
        }
        private static bool IsInstanceOf(this string uri)
        {
            return uri.EndsWith("P31");
        }

        public static bool IsInferible(this QueryEdge edge)
        {
            return edge.uris.Any() && !edge.HasInstanceOf();
        }

        public class Result
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }
        public static Dictionary<string, Result> ToDictionary(this IEnumerable<Property> subjects)
        {
            return subjects.ToDictionary(x => x.Id, y => new Result(){Id = $"http://www.wikidata.org/prop/direct/{y.Id}", Value = y.Label });
        }
        public static Dictionary<string, Result> ToDictionary(this IEnumerable<Entity> subjects)
        {
            return subjects.ToDictionary(x => x.Id, y => new Result(){Id = $"http://www.wikidata.org/entity/{y.Id}", Value = y.Label });
        }

        /// <summary>
        /// Given a Node, get all the outgoing edges (GetOutgoingEdges) that are InstanceOf.
        /// </summary>
        public static IEnumerable<QueryEdge> GetInstanceOfEdges(this QueryNode node, QueryGraph graph)
        {
            return node.GetOutgoingEdges(graph)?.Where(x => x.IsInstanceOf);
        }

        /// <summary>
        /// Given an Edge, get the Graph's Node, where Node.Id equals Edge.SourceId
        /// </summary>
        public static QueryNode GetSourceNode(this QueryEdge edge, QueryGraph graph)
        {
            return graph.Nodes.FirstOrDefault(x => x.Key.Equals(edge.sourceId)).Value;
        }

        /// <summary>
        /// Given an Edge, get the Graph's Node, where Node.Id equals Edge.TargetId
        /// </summary>
        public static QueryNode GetTargetNode(this QueryEdge edge, QueryGraph graph)
        {
            return graph.Nodes.FirstOrDefault(x => x.Key.Equals(edge.targetId)).Value;
        }

        /// <summary>
        /// Given a Node: Get the InstanceOfEdges. Get the TargetNodes. Get their Types;
        /// </summary>
        public static IEnumerable<string> GetInstanceOfValues(this QueryNode node, QueryGraph graph)
        {
            return node.GetInstanceOfEdges(graph)?.Select(x => x.GetTargetNode(graph)).SelectMany(x => x.uris);
        }

        /// <summary>
        /// Get all edges of the graph, where sourceId is equals to the node.id.
        /// </summary>
        public static IEnumerable<QueryEdge> GetOutgoingEdges(this QueryNode node, QueryGraph graph)
        {
            return graph.Edges.Select(x => x.Value).Where(x => x.sourceId.Equals(node.id));
        }

        /// <summary>
        /// Get all edges of the graph, where targetId is equals to the node.id.
        /// </summary>
        public static IEnumerable<QueryEdge> GetIncomingEdges(this QueryNode node, QueryGraph graph)
        {
            return graph.Edges.Select(x => x.Value).Where(x => x.targetId.Equals(node.id));
        }

        /// <summary>
        /// Given a node, get all nodes of the graph, where their edge.sourceId is equals to the node.id.
        /// </summary>
        public static IEnumerable<QueryNode> GetOutgoingNodes(this QueryNode node, QueryGraph graph)
        {
            return node.GetOutgoingEdges(graph)?.Select(x => x.GetTargetNode(graph));
        }

        /// <summary>
        /// Get all edges of the graph, where their edge.targetId is equals to the node.id.
        /// </summary>
        public static IEnumerable<QueryNode> GetIncomingNodes(this QueryNode node, QueryGraph graph)
        {
            return node.GetIncomingEdges(graph)?.Select(x => x.GetSourceNode(graph));
        }
    }
}

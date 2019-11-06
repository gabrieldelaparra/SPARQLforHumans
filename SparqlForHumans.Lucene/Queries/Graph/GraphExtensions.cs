using System.Linq;
using System.Collections.Generic;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.Query;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class GraphExtensions
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
            return node.GetInstanceOfEdges(graph)?.Select(x => x.GetTargetNode(graph)).SelectMany(x => x.Types);
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

        //TODO: Move out of here.
        internal static void ExploreGraph(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            //For all properties
            //TODO: (?) Should it be only for properties with a given type?
            //If the property, !InstanceOf, get the Domain and Range from the DB;
            //TODO: Why only !InstanceOf (?)
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                if (!edge.uris.Any()) continue;

                var properties = new BatchIdPropertyQuery(propertyIndexPath, edge.uris.Select(x => x.GetUriIdentifier())).Query();
                edge.Domain = properties.SelectMany(x => x.Domain).Select(x => $"{Models.Wikidata.WikidataDump.EntityPrefix}{x}").ToList();
                edge.Range = properties.SelectMany(x => x.Range).Select(x => $"{Models.Wikidata.WikidataDump.EntityPrefix}{x}").ToList();
            }

            //TODO: Need some comments here. Which cases are this. Or some tests.
            //TODO: Maybe more Unions are required.
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.IsGivenType)
                {
                    node.Types = node.uris.ToList();
                }
                else if (node.GetInstanceOfValues(graph).Any())
                {
                    node.Types = node.GetOutgoingNodes(graph).SelectMany(x => x.uris).Distinct().ToList();
                    node.Types = new BatchIdEntityQuery(entitiesIndexPath, node.uris.Distinct()).Query().SelectMany(x => x.InstanceOf).ToList();
                }
                else
                {
                    if (node.IsInferredDomainType)
                    {
                        node.InferredTypes = node.InferredTypes.Union(node.GetOutgoingEdges(graph).SelectMany(x => x.Domain)).ToList();
                    }
                    if (node.IsInferredRangeType)
                    {
                        node.InferredTypes = node.InferredTypes.Union(node.GetIncomingEdges(graph).SelectMany(x => x.Range)).ToList();
                    }
                }
            }

            ////TODO: Need some comments here. Which cases are this. Or some tests.
            //foreach (var node in graph.Nodes.Select(x => x.Value))
            //{
            //    if (node.GetOutgoingNodes(graph).Any(x => x.IsInstanceOfType))
            //        node.IsDirectedToKnownType = true;
            //}
        }
    }


}

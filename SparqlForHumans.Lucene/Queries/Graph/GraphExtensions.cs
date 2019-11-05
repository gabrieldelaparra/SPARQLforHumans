using System.Linq;
using System.Collections.Generic;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.Query;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class GraphExtensions
    {
        internal static bool HasInstanceOf(this QueryEdge edge)
        {
            return edge.uris.HasInstanceOf();
        }

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
        /// <summary>
        /// For a given node, get the InstanceOf edges, and get their types;
        /// TODO: I think that this is wrong, this will get the URIs of the edges, not of the connected node.
        /// TODO: Create some tests to expose this.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
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
            return graph.Edges.Select(x => x.Value).Where(x => x.sourceId.Equals(node.id));
        }

        internal static IEnumerable<QueryEdge> GetIncommingEdges(this QueryNode node, QueryGraph graph)
        {
            return graph.Edges.Select(x => x.Value).Where(x => x.targetId.Equals(node.id));
        }



        internal static IEnumerable<QueryNode> GetOutgoingNodes(this QueryNode node, QueryGraph graph)
        {
            var edges = node.GetOutgoingEdges(graph);
            if (edges == null) return null;
            return edges.Select(x => x.GetTargetNode(graph));
        }

        internal static void ExploreGraph(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            //For all properties
            //If the property, !InstanceOf, get the Domain and Range from the DB;
            //TODO: Why only !InstanceOf (?)
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                if (edge.IsInstanceOf) continue;

                if (string.IsNullOrWhiteSpace(propertyIndexPath)) continue;
                var properties = new BatchIdPropertyQuery(propertyIndexPath, edge.uris.Select(x => x.GetUriIdentifier())).Query();
                edge.Domain = properties.SelectMany(x => x.Domain).Select(x => $"{Models.Wikidata.WikidataDump.EntityPrefix}{x}").ToList();
                edge.Range = properties.SelectMany(x => x.Range).Select(x => $"{Models.Wikidata.WikidataDump.EntityPrefix}{x}").ToList();
            }

            //TODO: Need some comments here. Which cases are this. Or some tests.
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.GetInstanceOfValues(graph).Any())
                {
                    node.IsKnownType = true;
                    node.Types = node.GetOutgoingNodes(graph).SelectMany(x => x.uris).Distinct().ToList();
                }
                else if (node.IsGivenType)
                {
                    node.IsKnownType = true;
                    node.Types = node.uris.ToList();
                    //if (!string.IsNullOrWhiteSpace(entitiesIndexPath))
                    //    node.Types = new BatchIdEntityQuery(entitiesIndexPath, node.uris.Distinct()).Query().SelectMany(x => x.InstanceOf).ToList();
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
    }


}

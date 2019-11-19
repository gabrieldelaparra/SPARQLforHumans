using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphTypes
    {
        //TODO: TEST
        internal static void SetBaseNodeTypes(this QueryGraph graph)
        {
            //For all nodes:
            //If IsGivenType, get those types
            //If IsInstanceOfType (P31 to Type), Get those Types
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.IsGivenType)
                    node.Types = node.uris.ToList();
                else if (node.IsInstanceOfType)
                    node.Types = node.GetInstanceOfValues(graph).ToList();
            }
        }

        //TODO: TEST
        internal static void SetEdgeDomainRanges(this QueryGraph graph)
        {
            //For all properties, get the Domain and Range Types from the DB;
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                edge.SetEdgeDomainRanges(graph);
                //if (!edge.IsGivenType) continue;
                //var sourceNode = edge.GetSourceNode(graph);
                //var targetNode = edge.GetTargetNode(graph);
                //edge.Domain = sourceNode.IsGivenType ? sourceNode.Types : InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                //edge.Range = targetNode.IsGivenType ? targetNode.Types : InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
            }
        }

        //TODO: TEST
        internal static void SetEdgeDomainRanges(this QueryEdge edge, QueryGraph graph)
        {
            switch (edge.QueryType)
            {
                case QueryType.GivenPredicateTypeNoQuery:
                    var sourceNode = edge.GetSourceNode(graph);
                    var targetNode = edge.GetTargetNode(graph);
                    edge.Domain = sourceNode.IsGivenType ? sourceNode.Types : InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                    edge.Range = targetNode.IsGivenType ? targetNode.Types : InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                    break;
                case QueryType.QueryTopProperties:
                    edge.Domain = new MultiLabelTypeQuery(graph.EntitiesIndexPath, "*").Query().Select(x => x.Id).ToList();
                    edge.Range = edge.Domain.ToList();
                    break;
                case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                    edge.Domain = edge.GetSourceNode(graph).Types;
                    edge.Range = new List<string>();
                    break;
                case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                    edge.Domain = new List<string>();
                    edge.Range = edge.GetTargetNode(graph).Types;
                    break;
                case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                    edge.Domain = edge.GetSourceNode(graph).Types;
                    edge.Range = edge.GetTargetNode(graph).Types;
                    break;
                case QueryType.KnownSubjectTypeQueryDomainProperties:
                    edge.Domain = edge.GetSourceNode(graph).Types;
                    edge.Range = new List<string>();
                    break;
                case QueryType.KnownObjectTypeQueryRangeProperties:
                    edge.Domain = new List<string>();
                    edge.Range = edge.GetTargetNode(graph).Types;
                    break;
                case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                    edge.Domain = edge.GetSourceNode(graph).Types;
                    edge.Range = edge.GetTargetNode(graph).Types;
                    break;
                case QueryType.InferredDomainAndRangeTypeProperties:
                    edge.Domain = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                    edge.Range = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                    break;
                case QueryType.InferredDomainTypeProperties:
                    edge.Domain = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                    edge.Range = new List<string>();
                    break;
                case QueryType.InferredRangeTypeProperties:
                    edge.Domain = new List<string>();
                    edge.Range = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                    break;
                case QueryType.Unknown:
                default:
                    break;
            }
        }

        //TODO: TEST
        public static void SetTypesDomainsAndRanges(this QueryGraph graph)
        {
            InMemoryQueryEngine.Init(graph.EntitiesIndexPath, graph.PropertiesIndexPath);
            graph.SetBaseNodeTypes();
            graph.SetEdgeDomainRanges();

            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.IsInferredType)
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


        }
    }
}

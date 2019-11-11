using SparqlForHumans.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Lucene.Queries.Graph
{

    /// Case 1:
    /// ?var0
    /// ?var1
    /// ?var0 -> ?prop -> ?var1
    /// 
    /// Case 2: P31 and prop going from the same node
    /// ?var0 -> P31 -> Qxx
    /// ?var0 -> ?prop -> ?var1
    ///
    /// Case 3: P31 going from a different node.
    /// ?var1 -> ?prop -> ?var0
    ///                   ?var0 -> P31 -> Qxx
    /// Case 4: P31 going ot from both nodes
    /// ?var0 -> P31 -> Qxx
    ///                   ?var1 -> P31 -> Qyy
    /// ?var0 -> ?prop -> ?var1
    public static class GraphQueryResults
    {
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            graph.EntitiesIndexPath = entitiesIndexPath;
            graph.PropertiesIndexPath = propertyIndexPath;

            graph.SetTypesDomainsAndRanges(graph.EntitiesIndexPath, graph.PropertiesIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            graph.RunNodeQueries(graph.EntitiesIndexPath);
            graph.RunEdgeQueries(graph.PropertiesIndexPath);
        }

        public static void SetTypesDomainsAndRanges(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);

            //For all nodes:
            //If IsGivenType, get those types
            //If IsInstanceOfType (P31 to Type), Get those Types
            //For InferredTypes, Get those Types
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.IsGivenType)
                {
                    node.Types = node.uris.ToList();
                }
                else if (node.IsInstanceOfType)
                {
                    node.Types = node.GetInstanceOfValues(graph).ToList();
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

            //For all properties, get the Domain and Range Types from the DB;
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                if (edge.uris.Any())
                {
                    edge.Domain = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                    edge.Range = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                }
                else
                {
                    edge.Domain = edge.GetSourceNode(graph).Types;
                    edge.Range = edge.GetTargetNode(graph).Types;
                }
            }
        }

        private static void RunNodeQueries(this QueryGraph graph, string indexPath)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                switch (node.QueryType)
                {
                    case QueryType.SubjectIsInstanceOfTypeQueryEntities:
                    case QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities:
                        //This should be done with the Wikipedia Endpoint
                        node.Results = new BatchIdEntityInstanceQuery(indexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.QueryTopEntities:
                        //This should be done with the Wikipedia Endpoint
                        node.Results = new MultiLabelEntityQuery(indexPath, "*").Query();
                        //node.Results = new List<Entity>();
                        break;
                    case QueryType.InferredDomainAndRangeTypeEntities:
                    case QueryType.InferredDomainTypeEntities:
                    case QueryType.InferredRangeTypeEntities:
                        //This should be done with the Wikipedia Endpoint
                        node.Results = new BatchIdEntityInstanceQuery(indexPath, node.InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.GivenEntityTypeNoQuery:
                        node.Results = new BatchIdEntityQuery(indexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.KnownPredicateAndObjectNotUsed:
                    case QueryType.Unknown:
                    default:
                        break;
                }
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph, string indexPath)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                string targetUri = string.Empty;
                string sourceUri = string.Empty;
                //var domainProperties = new List<Property>(0);
                //var rangeProperties = new List<Property>(0);
                var domainPropertiesIds = new List<int>(0);
                var rangePropertiesIds = new List<int>(0);
                var propertiesIds = new List<string>(0);
                switch (edge.QueryType)
                {
                    case QueryType.QueryTopProperties:
                        edge.Results = new MultiLabelPropertyQuery(indexPath, "*").Query();
                        break;
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Intersect(rangePropertiesIds).Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(indexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        //domainProperties = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        //rangeProperties = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        //edge.Results = rangeProperties.Intersect(domainProperties, new PropertyComparer()).ToList();
                        break;
                    case QueryType.KnownSubjectTypeQueryDomainProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(indexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        //edge.Results = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.KnownObjectTypeQueryRangeProperties:
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = rangePropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(indexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        //edge.Results = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.InferredDomainAndRangeTypeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).InferredTypes.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).InferredTypes.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Intersect(rangePropertiesIds).Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(indexPath, propertiesIds).Query();
                        //domainProperties = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        //rangeProperties = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        //edge.Results = rangeProperties.Intersect(domainProperties, new PropertyComparer()).ToList();
                        break;
                    case QueryType.InferredDomainTypeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).InferredTypes.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(indexPath, propertiesIds).Query();
                        //edge.Results = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.InferredRangeTypeProperties:
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).InferredTypes.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = rangePropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(indexPath, propertiesIds).Query();
                        //edge.Results = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.Unknown:
                    default:
                        break;
                }
            }
        }
    }
}

using SparqlForHumans.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.Wikidata.Services;
using VDS.RDF.Query;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphResults
    {
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            graph.EntitiesIndexPath = entitiesIndexPath;
            graph.PropertiesIndexPath = propertyIndexPath;

            graph.SetBaseNodeTypes(graph.EntitiesIndexPath, graph.PropertiesIndexPath);
            graph.SetBaseEdgeDomainRanges(graph.EntitiesIndexPath, graph.PropertiesIndexPath);

            graph.SetTypesDomainsAndRanges(graph.EntitiesIndexPath, graph.PropertiesIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            graph.RunNodeQueries(graph.EntitiesIndexPath);
            graph.RunEdgeQueries(graph.PropertiesIndexPath);
        }

        //TODO: TEST
        public static void SetBaseNodeTypes(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
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
        public static void SetBaseEdgeDomainRanges(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);

            //For all properties, get the Domain and Range Types from the DB;
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                edge.Domain = edge.GetSourceNode(graph).Types;
                edge.Range = edge.GetTargetNode(graph).Types;

                if (!edge.Domain.Any() && edge.uris.Any())
                    edge.Domain = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                if (!edge.Range.Any() && edge.uris.Any())
                    edge.Range = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
            }
        }

        //TODO: TEST
        public static void SetTypesDomainsAndRanges(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            graph.SetBaseNodeTypes(graph.EntitiesIndexPath, graph.PropertiesIndexPath);
            graph.SetBaseEdgeDomainRanges(graph.EntitiesIndexPath, graph.PropertiesIndexPath);

            //For all nodes:
            //If IsGivenType, get those types
            //If IsInstanceOfType (P31 to Type), Get those Types
            //For InferredTypes, Get those Types
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

            ////For all properties, get the Domain and Range Types from the DB;
            //foreach (var edge in graph.Edges.Select(x => x.Value))
            //{
            //    if (edge.uris.Any())
            //    {
            //        edge.Domain = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
            //        edge.Range = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
            //    }
            //    else
            //    {
            //        edge.Domain = edge.GetSourceNode(graph).Types;
            //        edge.Range = edge.GetTargetNode(graph).Types;
            //    }
            //}
        }

        private static void RunNodeQueries(this QueryGraph graph, string indexPath)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                switch (node.QueryType)
                {
                    case QueryType.GivenSubjectTypeQueryDirectly:
                        node.Results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString()).Select(x=>x.ToEntity()).ToList();
                        break;
                    case QueryType.GivenObjectTypeQueryDirectly:
                        node.Results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString()).Select(x=>x.ToEntity()).ToList();
                        break;
                    case QueryType.SubjectIsInstanceOfTypeQueryEntities:
                        node.Results = new BatchIdEntityInstanceQuery(indexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.QueryTopEntities:
                        node.Results = new MultiLabelEntityQuery(indexPath, "*").Query();
                        break;
                    case QueryType.InferredDomainAndRangeTypeEntities:
                    case QueryType.InferredDomainTypeEntities:
                    case QueryType.InferredRangeTypeEntities:
                        node.Results = new BatchIdEntityInstanceQuery(indexPath, node.InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.GivenEntityTypeNoQuery:
                        node.Results = new BatchIdEntityInstanceQuery(indexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    default:
                        break;
                }
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph, string indexPath)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                List<int> domainPropertiesIds;
                List<int> rangePropertiesIds;
                List<string> propertiesIds;

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
                    default:
                        break;
                }
            }
        }
    }
}

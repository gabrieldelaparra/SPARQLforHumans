using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SparqlForHumans.Models;
using SparqlForHumans.Wikidata.Services;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphResults
    {
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath)
        {
            graph.SetIndexPaths(entitiesIndexPath, propertyIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            graph.SetTypesDomainsAndRanges();

            graph.RunNodeQueries();
            graph.RunEdgeQueries();
        }

        private static void RunNodeQueries(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                switch (node.QueryType)
                {
                    case QueryType.GivenSubjectTypeQueryDirectlyEntities:
                    case QueryType.GivenObjectTypeQueryDirectlyEntities:
                        var results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString())?.Select(x => x.ToEntity()).ToList();
                        node.Results = results ?? new List<Entity>();
                        break;
                    case QueryType.SubjectIsInstanceOfTypeQueryEntities:
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.QueryTopEntities:
                        node.Results = new MultiLabelEntityQuery(graph.EntitiesIndexPath, "*").Query();
                        break;
                    case QueryType.InferredDomainTypeEntities:
                        var domainProperties = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain);
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, domainProperties).Query();
                        break;
                    case QueryType.InferredRangeTypeEntities:
                        var rangeProperties = node.GetIncomingEdges(graph).SelectMany(x => x.Range);
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, rangeProperties).Query();
                        break;
                    case QueryType.InferredDomainAndRangeTypeEntities:
                        var domainProps = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain);
                        var rangeProps = node.GetIncomingEdges(graph).SelectMany(x => x.Range);
                        var properties = domainProps.Intersect(rangeProps);
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, properties).Query();
                        break;
                    case QueryType.GivenEntityTypeNoQuery:
                        node.Results = new List<Entity>();
                        break;
                    case QueryType.Unknown:
                    case QueryType.GivenPredicateTypeNoQuery:
                    case QueryType.QueryTopProperties:
                    case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                    case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                    case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                    case QueryType.KnownSubjectTypeQueryDomainProperties:
                    case QueryType.KnownObjectTypeQueryRangeProperties:
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                    case QueryType.InferredDomainAndRangeTypeProperties:
                    case QueryType.InferredDomainTypeProperties:
                    case QueryType.InferredRangeTypeProperties:
                    default:
                        break;
                }
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                List<int> domainPropertiesIds;
                List<int> rangePropertiesIds;
                List<string> propertiesIds;
                List<Entity> subjectNodes;
                List<Entity> objectNodes;

                switch (edge.QueryType)
                {
                    case QueryType.QueryTopProperties:
                        edge.Results = new MultiLabelPropertyQuery(graph.PropertiesIndexPath, "*").Query();
                        break;
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Intersect(rangePropertiesIds).Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        break;
                    case QueryType.KnownSubjectTypeQueryDomainProperties:
                        var domains = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types).ToList();
                        propertiesIds = domains.Select(x => $"{x.GetUriIdentifier()}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        break;
                    case QueryType.KnownObjectTypeQueryRangeProperties:
                        var ranges = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types).ToList();
                        propertiesIds = ranges.Select(x => $"{x.GetUriIdentifier()}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        break;
                    case QueryType.InferredDomainAndRangeTypeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Intersect(rangePropertiesIds).Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        break;
                    case QueryType.InferredDomainTypeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        break;
                    case QueryType.InferredRangeTypeProperties:
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = rangePropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        break;
                    case QueryType.GivenEntityTypeNoQuery:
                        edge.Results = new BatchIdPropertyDomainQuery(graph.PropertiesIndexPath, edge.uris.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                        objectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        propertiesIds = objectNodes.SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        break;
                    case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                        subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        propertiesIds = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        break;
                    case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                        subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        var subjectProperties = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                        objectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        var objectProperties = objectNodes.SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                        propertiesIds = subjectProperties.Intersect(objectProperties).ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        break;
                    case QueryType.GivenPredicateTypeNoQuery:
                        edge.Results = new List<Property>();
                        break;
                    case QueryType.Unknown:
                    case QueryType.QueryTopEntities:
                    case QueryType.GivenSubjectTypeQueryDirectlyEntities:
                    case QueryType.GivenObjectTypeQueryDirectlyEntities:
                    case QueryType.SubjectIsInstanceOfTypeQueryEntities:
                    case QueryType.InferredDomainTypeEntities:
                    case QueryType.InferredDomainAndRangeTypeEntities:
                    case QueryType.InferredRangeTypeEntities:
                    default:
                        break;
                }
            }
        }
    }
}

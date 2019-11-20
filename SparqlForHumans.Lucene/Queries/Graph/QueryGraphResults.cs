using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
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
                        node.Results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString()).Select(x => x.ToEntity()).ToList();
                        break;
                    case QueryType.GivenObjectTypeQueryDirectlyEntities:
                        //node.Results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString()).Select(x => x.ToEntity()).ToList();
                        break;
                    case QueryType.SubjectIsInstanceOfTypeQueryEntities:
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.QueryTopEntities:
                        node.Results = new MultiLabelEntityQuery(graph.EntitiesIndexPath, "*").Query();
                        break;
                    case QueryType.InferredDomainAndRangeTypeEntities:
                    case QueryType.InferredDomainTypeEntities:
                    case QueryType.InferredRangeTypeEntities:
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.GivenEntityTypeNoQuery:
                        //node.Results = new BatchIdEntityQuery(indexPath, node.Types.Select(x => x.GetUriIdentifier())).Query();
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
                        //domainProperties = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        //rangeProperties = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        //edge.Results = rangeProperties.Intersect(domainProperties, new PropertyComparer()).ToList();
                        break;
                    case QueryType.KnownSubjectTypeQueryDomainProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        //edge.Results = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.KnownObjectTypeQueryRangeProperties:
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = rangePropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query().OrderByDescending(x => x.Rank).ToList();
                        //edge.Results = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.InferredDomainAndRangeTypeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Intersect(rangePropertiesIds).Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        //domainPropertiesIds = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        //rangePropertiesIds = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        //edge.Results = rangeProperties.Intersect(domainProperties, new PropertyComparer()).ToList();
                        break;
                    case QueryType.InferredDomainTypeProperties:
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.GetSourceNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = domainPropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        //edge.Results = new BatchIdPropertyDomainQuery(indexPath, edge.GetSourceNode(graph).InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.InferredRangeTypeProperties:
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.GetTargetNode(graph).Types.Select(y => y.GetUriIdentifier().ToInt())).ToList();
                        propertiesIds = rangePropertiesIds.Distinct().Select(x => $"P{x}").ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        //edge.Results = new BatchIdPropertyRangeQuery(indexPath, edge.GetTargetNode(graph).InferredTypes.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.GivenEntityTypeNoQuery:
                        edge.Results = new BatchIdPropertyDomainQuery(graph.PropertiesIndexPath, edge.uris.Select(x => x.GetUriIdentifier())).Query();
                        break;
                    case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                        //edge.Results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString()).Select(x => x.ToProperty()).ToList();
                        edge.Results = new List<Property> { new Property { Id = "P31", Label = "instance of" } };
                        break;
                    case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                        var subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                        propertiesIds = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                        edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        //edge.Results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString()).Select(x => x.ToProperty()).ToList();
                        break;
                    case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                        edge.Results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString()).Select(x => x.ToProperty()).ToList();
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

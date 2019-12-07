using System;
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
        //private static Logger.Logger logger = Logger.Logger.Init();
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
                //Given Type, Do not Query
                if (node.IsGivenType)
                {
                    node.Results = new List<Entity>();
                    continue;
                }

                //Node that is not connected, return random results
                if (node.IsNotConnected(graph))
                {
                    var rnd = new Random();
                    var randomEntities = Enumerable.Repeat(1, 20).Select(_ => rnd.Next(999999)).Select(x => $"Q{x}");
                    node.Results = new BatchIdEntityQuery(graph.EntitiesIndexPath, randomEntities).Query();
                    continue;
                }

                //Just instance of, search only for that.
                if (!node.HasIncomingEdges(graph) && node.GetOutgoingEdges(graph).Count().Equals(1) &&
                    node.IsInstanceOfType)
                {
                    var instanceOfTypes = node.Types.Select(x => x.GetUriIdentifier()).ToList();
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, instanceOfTypes, 20).Query();
                    continue;
                }

                //The other complex queries. Try endpoint first, if timeout, try with the index.
                //If the user has a timeout, is because his query is still too broad.
                //Some suggestions will be proposed with the local index, until the query can be completed by the endpoint.
                var results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString())?.Select(x => x.ToEntity()).ToList();

                if (results == null)
                {
                    var intersectTypes = new List<string>();

                    //Outgoing edges candidates, take their domain
                    var domainTypes = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf).SelectMany(x => x.Domain).ToList();
                    intersectTypes = intersectTypes.IntersectIfAny(domainTypes).ToList();

                    //Incoming edges candidates, take their range.
                    var rangeTypes = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf).SelectMany(x => x.Range).ToList();
                    intersectTypes = intersectTypes.IntersectIfAny(rangeTypes).ToList();

                    //Combine domain & range, take a random sample and get those results:
                    intersectTypes = intersectTypes.TakeRandom(100).ToList();
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, intersectTypes, 20).Query(100);

                    //If the instance is of a specific type, intersect a random sample of the instances with the previous results filter out the valid results:
                    if (node.IsInstanceOfType)
                    {
                        var instanceOfTypes = node.Types.Select(x => x.GetUriIdentifier()).ToList();
                        var instanceOfResults =
                            new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, instanceOfTypes, 200).Query(20);

                        //Intersect (Not if any, we want only the results of that instance, even if there are none.):
                        node.Results = node.Results.Intersect(instanceOfResults).ToList();
                    }
                }
                else
                {
                    node.Results = results;
                }

                //switch (node.QueryType)
                //{
                //    case QueryType.GivenSubjectTypeQueryDirectlyEntities:
                //        results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString())?.Select(x => x.ToEntity()).ToList();
                //        if (results == null)
                //        {

                //        }
                //        else
                //        {
                //            node.Results = results;
                //        }
                //        break;
                //    case QueryType.GivenObjectTypeQueryDirectlyEntities:
                //        results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString())?.Select(x => x.ToEntity()).ToList();
                //        if (results == null)
                //        {
                //            domainTypes = node.GetOutgoingEdges(graph).Where(x=>!x.IsInstanceOf).SelectMany(x => x.Domain).ToList();
                //            rangeTypes = node.GetIncomingEdges(graph).Where(x=>!x.IsInstanceOf).SelectMany(x => x.Range).ToList();
                //            intersectTypes = new List<string>();
                //            intersectTypes = intersectTypes.IntersectIfAny(domainTypes).ToList();
                //            intersectTypes = intersectTypes.IntersectIfAny(rangeTypes).ToList();
                //            intersectTypes = intersectTypes.TakeRandom(200).ToList();
                //            node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, intersectTypes,200).Query(200);
                //            if (node.IsInstanceOfType)
                //            {
                //                instanceOfTypes = node.Types.Select(x => x.GetUriIdentifier()).ToList();
                //                node.Results = node.Results
                //                    .Intersect(new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath,
                //                        instanceOfTypes.Select(x => x.GetUriIdentifier()), 500).Query(500)).ToList();
                //            }
                //        }
                //        else
                //        {
                //            node.Results = results;
                //        }
                //        break;
                //    case QueryType.DirectQuery:
                //        results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString())?.Select(x => x.ToEntity()).ToList();
                //        if (results == null)
                //        {

                //        }
                //        else
                //        {
                //            node.Results = results;
                //        }
                //        break;
                //    case QueryType.SubjectIsInstanceOfTypeQueryEntities:
                //        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types.Select(x => x.GetUriIdentifier())).Query(100);
                //        break;
                //    case QueryType.QueryTopEntities:
                //        node.Results = new MultiLabelEntityQuery(graph.EntitiesIndexPath, "*").Query(100);
                //        break;
                //    case QueryType.InferredDomainTypeEntities:
                //        domainTypes = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain).ToList();
                //        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, domainTypes, 100).Query(100);
                //        break;
                //    case QueryType.InferredRangeTypeEntities:
                //        rangeTypes = node.GetIncomingEdges(graph).SelectMany(x => x.Range).ToList();
                //        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, rangeTypes, 100).Query(100);
                //        break;
                //    case QueryType.InferredDomainAndRangeTypeEntities:
                //        domainTypes = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain).ToList();
                //        rangeTypes = node.GetIncomingEdges(graph).SelectMany(x => x.Range).ToList();
                //        intersectTypes = new List<string>();
                //        intersectTypes = intersectTypes.IntersectIfAny(domainTypes).ToList();
                //        intersectTypes = intersectTypes.IntersectIfAny(rangeTypes).ToList();
                //        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, intersectTypes, 100).Query(100);
                //        break;
                //    case QueryType.GivenEntityTypeNoQuery:
                //        node.Results = new List<Entity>();
                //        break;

                //    case QueryType.Unknown:
                //    case QueryType.GivenPredicateTypeNoQuery:
                //    case QueryType.QueryTopProperties:
                //    case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                //    case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                //    case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                //    case QueryType.KnownSubjectTypeQueryDomainProperties:
                //    case QueryType.KnownObjectTypeQueryRangeProperties:
                //    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                //    case QueryType.InferredDomainAndRangeTypeProperties:
                //    case QueryType.InferredDomainTypeProperties:
                //    case QueryType.InferredRangeTypeProperties:
                //    default:
                //        break;
                //}
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                if (edge.IsGivenType)
                {
                    edge.Results = new List<Property>();
                    continue;
                }

                List<int> domainPropertiesIds;
                List<int> rangePropertiesIds;
                List<string> propertiesIds;
                List<Entity> subjectNodes;
                List<Entity> objectNodes;
                List<Property> results;

                switch (edge.QueryType)
                {
                    case QueryType.QueryTopProperties:
                        edge.Results = new MultiLabelPropertyQuery(graph.PropertiesIndexPath, "*").Query();
                        break;
                    case QueryType.DirectQuery:
                        results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString())?.Select(x => x.ToProperty()).ToList();
                        edge.Results = results ?? new List<Property>();
                        break;
                    case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                        results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString())?.Select(x => x.ToProperty()).ToList();
                        if (results == null)
                        {
                            subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                            propertiesIds = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        }
                        else
                        {
                            edge.Results = results;
                        }
                        break;
                    case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                        results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString())?.Select(x => x.ToProperty()).ToList();
                        if (results == null)
                        {
                            subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                            var subjectProperties = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                            objectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                            var objectProperties = objectNodes.SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                            propertiesIds = subjectProperties.Intersect(objectProperties).ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        }
                        else
                        {
                            edge.Results = results;
                        }
                        break;
                    case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                        results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString())?.Select(x => x.ToProperty()).ToList();
                        if (results == null)
                        {
                            objectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                            propertiesIds = objectNodes.SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                            edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                        }
                        else
                        {
                            edge.Results = results;
                        }
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

                    //case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                    //subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                    //propertiesIds = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                    //edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                    //break;
                    //case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                    //subjectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetSourceNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                    //var subjectProperties = subjectNodes.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                    //objectNodes = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.GetTargetNode(graph).Types.Select(x => x.GetUriIdentifier())).Query();
                    //var objectProperties = objectNodes.SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                    //propertiesIds = subjectProperties.Intersect(objectProperties).ToList();
                    //edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, propertiesIds).Query();
                    //break;
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

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
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types, 20).Query();
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
                        //Intersect (Not if any, we want only the results of that instance, even if there are none):
                        var instanceOfResults = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.Types, 200).Query(20);
                        node.Results = node.Results.Intersect(instanceOfResults).ToList();
                    }
                }
                else
                {
                    node.Results = results;
                }
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

                var results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString())?.Select(x => x.ToProperty()).ToList();

                if (results == null)
                {
                    var source = edge.GetSourceNode(graph);
                    var target = edge.GetTargetNode(graph);

                    var givenPropertiesIds = new List<string>();
                    var instanceOfPropertiesIds = new List<string>();
                    var domainPropertiesIds = new List<string>();
                    var rangePropertiesIds = new List<string>();

                    //They cannot be at the same time (given type and instance of type)
                    if (source.IsGivenType)
                        givenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, source.Types)
                            .Query().SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                    if (source.IsInstanceOfType)
                        instanceOfPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.Types).ToList();
                    if (source.IsInferredDomainType)
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.Types).ToList();
                    if (source.IsInferredRangeType)
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.Types).ToList();

                    var outgoingPropertyIds = givenPropertiesIds.IntersectIfAny(instanceOfPropertiesIds)
                        .IntersectIfAny(domainPropertiesIds).IntersectIfAny(rangePropertiesIds).ToList();

                    if (target.IsGivenType)
                        givenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, target.Types)
                            .Query().SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                    if (target.IsInstanceOfType)
                        instanceOfPropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.Types).ToList();
                    if (target.IsInferredDomainType)
                        domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.Types).ToList();
                    if (target.IsInferredRangeType)
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.Types).ToList();

                    var incomingPropertyIds = givenPropertiesIds.IntersectIfAny(instanceOfPropertiesIds)
                        .IntersectIfAny(domainPropertiesIds).IntersectIfAny(rangePropertiesIds).ToList();

                    //Get outgoing/incoming properties; Intersect; Query for those types:
                    var intersectPropertiesIds = outgoingPropertyIds
                        .IntersectIfAny(incomingPropertyIds).ToList();

                    if (!intersectPropertiesIds.Any())
                    {
                        edge.Results = new MultiLabelPropertyQuery(graph.PropertiesIndexPath, "*").Query();
                    }
                    else
                    {
                        results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, intersectPropertiesIds).Query();
                        edge.Results = results;
                    }
                }
                else
                {
                    edge.Results = results;
                }
            }
        }
    }
}

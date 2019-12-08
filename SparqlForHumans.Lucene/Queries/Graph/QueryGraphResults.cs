using System;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.Wikidata.Services;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphResults
    {
        //private static Logger.Logger logger = Logger.Logger.Init();
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath, bool runOnEndpoint = true)
        {
            graph.SetIndexPaths(entitiesIndexPath, propertyIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            graph.SetTypesDomainsAndRanges();

            graph.RunNodeQueries(runOnEndpoint);
            graph.RunEdgeQueries(runOnEndpoint);
        }

        private static void RunNodeQueries(this QueryGraph graph, bool runOnEndpoint = true)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                //Given Type, Do not Query
                if (node.IsGivenType)
                {
                    node.Results = new List<Entity>();
                    continue;
                }

                //CASE: AVOID ENDPOINT QUERY, IS KNOWN TO TIMEOUT
                //Node that is not connected, return random results
                if (!node.IsSomehowDefined(graph))
                {
                    var rnd = new Random();
                    var randomEntities = Enumerable.Repeat(1, 100).Select(_ => rnd.Next(99999)).Select(x => $"Q{x}");
                    node.Results = new BatchIdEntityQuery(graph.EntitiesIndexPath, randomEntities).Query();
                    //TODO: Temporary, for not getting empty results if there were none.
                    if (node.Results.Count < 20)
                    {
                        node.Results = node.Results.IntersectIfAny(new MultiLabelEntityQuery(graph.EntitiesIndexPath, "*").Query()).ToList();
                    }
                    continue;
                }

                //CASE: AVOID ENDPOINT QUERY, IS KNOWN TO TIMEOUT
                //Just instance of, search only for that.
                if (!node.HasIncomingEdges(graph) && node.GetOutgoingEdges(graph).Count().Equals(1) &&
                    node.IsInstanceOfType)
                {
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.InstanceOfBaseTypes, 20).Query();
                    continue;
                }

                //The other complex queries. Try endpoint first, if timeout, try with the index.
                //If the user has a timeout, is because his query is still too broad.
                //Some suggestions will be proposed with the local index, until the query can be completed by the endpoint.
                var results = GraphApiQueries.RunQuery(node.ToSparql(graph).ToString(), runOnEndpoint)?.Select(x => x.ToEntity()).ToList();

                if (results == null)
                {
                    var intersectTypes = new List<string>();

                    //Outgoing edges candidates, take their domain
                    var outgoingEdges = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf);
                    var domainTypes = new List<string>();
                    foreach (var outgoingEdge in outgoingEdges)
                        domainTypes = domainTypes.IntersectIfAny(outgoingEdge.DomainBaseTypes).ToList();

                    intersectTypes = intersectTypes.IntersectIfAny(domainTypes).ToList();

                    //Incoming edges candidates, take their range.
                    var incomingEdges = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf);
                    var rangeTypes = new List<string>();
                    foreach (var incomingEdge in incomingEdges)
                        rangeTypes = rangeTypes.IntersectIfAny(incomingEdge.RangeBaseTypes).ToList();

                    //var rangeTypes = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf).SelectMany(x => x.RangeBaseTypes).ToList();
                    intersectTypes = intersectTypes.IntersectIfAny(rangeTypes).ToList();

                    //Combine domain & range, take a random sample and get those results:
                    intersectTypes = intersectTypes.ToList();
                    //intersectTypes = intersectTypes.TakeRandom(100).ToList();
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, intersectTypes, 200).Query(1000);

                    //If the instance is of a specific type, intersect a random sample of the instances with the previous results filter out the valid results:
                    if (node.IsInstanceOfType)
                    {
                        //Intersect (Not if any, we want only the results of that instance, even if there are none):
                        var instanceOfResults = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.InstanceOfBaseTypes, 200).Query(20);
                        node.Results = node.Results.Intersect(instanceOfResults).ToList();
                    }

                    if (!intersectTypes.Any())
                    {
                        var rnd = new Random();
                        var randomEntities = Enumerable.Repeat(1, 100).Select(_ => rnd.Next(99999)).Select(x => $"Q{x}");
                        node.Results = new BatchIdEntityQuery(graph.EntitiesIndexPath, randomEntities).Query();
                        //TODO: Temporary, for not getting empty results if there were none.
                        if (node.Results.Count < 20)
                        {
                            node.Results = node.Results.IntersectIfAny(new MultiLabelEntityQuery(graph.EntitiesIndexPath, "*").Query()).ToList();
                        }
                    }
                }
                else
                {
                    node.Results = results;
                }
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph, bool runOnEndpoint = true)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                if (edge.IsGivenType)
                {
                    edge.Results = new List<Property>();
                    continue;
                }

                var results = GraphApiQueries.RunQuery(edge.ToSparql(graph).ToString(), runOnEndpoint)?.Select(x => x.ToProperty()).ToList();

                if (results == null)
                {
                    var source = edge.GetSourceNode(graph);
                    var target = edge.GetTargetNode(graph);

                    var givenPropertiesIds = new List<string>();
                    var instanceOfPropertiesIds = new List<string>();
                    var domainPropertiesIds = new List<string>();
                    var rangePropertiesIds = new List<string>();
                    var otherProperties = new List<string>();
                    var sourceResultsProperties = new List<string>();

                    //They cannot be at the same time (given type and instance of type)
                    if (source.IsGivenType)
                    {
                        givenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, source.GivenTypes).Query().SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                    }
                    else
                    {
                        //EXAMPLE of why both are needed:
                        //- Could have two properties: InstanceOf (InstanceOf), PositionHeld (Domain).
                        //- InstanceOf could be HUMAN: All human properties will return.
                        //- PositionHeld could be FICTIONAL CHARS. A different set of properties will return, than for HUMAN.
                        //- An intersection needs to be done.
                        if (source.IsInstanceOfType)
                            instanceOfPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.InstanceOfBaseTypes).ToList();
                        if (source.IsInferredDomainType)
                            domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(edge.DomainBaseTypes).ToList();
                        if (source.IsInferredRangeType)
                            rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.RangeBaseTypes).ToList();
                        if (source.IsGoingToGivenType)
                            sourceResultsProperties = source.Results.SelectMany(x => x.Properties).Distinct().Select(x => x.Id).ToList();
                    }
                    //TODO: Check if this is valid:
                    //if (source.IsGoingToGivenType)
                    //{
                    //    otherProperties = source.Results.SelectMany(x => x.Properties).Distinct().Select(x => x.Id).ToList();
                    //    //goingToGivenTypeProperties = new BatchIdEntityReversePropertiesQuery();
                    //}
                    //    goingOutProperties = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.Range).Query().SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                    //if(source.IsComingFromGivenType)
                    //    comingInProperties = new BatchIdEntityQuery(graph.EntitiesIndexPath, edge.Domain).Query().SelectMany(x => x.Properties).Select(x => x.Id).ToList();

                    //if (source.IsInstanceOfType) {
                    //    otherProperties = source.Results.SelectMany(x => x.Properties).Select(x => x.Id).ToList();
                    //}



                    //TODO: Check if this is valid:
                    //if (source.IsInferredRangeType)
                    //    rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.Range).ToList();

                    var outgoingPropertyIds = givenPropertiesIds
                        .IntersectIfAny(instanceOfPropertiesIds)
                        .IntersectIfAny(domainPropertiesIds)
                        .IntersectIfAny(sourceResultsProperties)
                        .IntersectIfAny(rangePropertiesIds)
                        .ToList();

                    if (target.IsGivenType)
                        givenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, target.GivenTypes)
                            .Query().SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();
                    if (target.IsInstanceOfType)
                        instanceOfPropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.InstanceOfBaseTypes).ToList();

                    //TODO: Check if this is valid:
                    //if (target.IsInferredDomainType)
                    //    domainPropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.Domain).ToList();
                    if (target.IsInferredRangeType)
                        rangePropertiesIds = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(edge.RangeBaseTypes).ToList();

                    var incomingPropertyIds = givenPropertiesIds
                        .IntersectIfAny(instanceOfPropertiesIds)
                        .IntersectIfAny(domainPropertiesIds)
                        .IntersectIfAny(rangePropertiesIds)
                        .ToList();

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

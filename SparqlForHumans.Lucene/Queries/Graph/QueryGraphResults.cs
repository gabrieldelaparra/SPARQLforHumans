using System;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparqlForHumans.Models;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Wikidata.Services;
using VDS.RDF.Query;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphResults
    {
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath, bool runOnEndpoint = true)
        {
            graph.SetIndexPaths(entitiesIndexPath, propertyIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            graph.SetTypesDomainsAndRanges();

            graph.ResetTraverse();
            graph.CheckAvoidQueries();

            //TODO: Run WikidataEndpointQueries
            var tasks = new List<Task<SparqlResultSet>>();
            if (runOnEndpoint)
                tasks = graph.RunWikidataEndpointQueries();

            graph.RunNodeQueries(runOnEndpoint);
            graph.RunEdgeQueries(runOnEndpoint);

            //TODO: Assign all results for those queries that ran.
            if (tasks.Any())
            {
                foreach (var task in tasks)
                {
                    task.Wait();
                    var resultsSet = task.Result;
                    if (resultsSet != null && resultsSet.Any()) {
                        var nodes = graph.Nodes.Select(x => x.Value);
                        var edges = graph.Edges.Select(x => x.Value);

                        var queryResultsGroup = resultsSet.Results.SelectMany(x => x).GroupBy(x=>x.Key);

                        foreach (var queryGroup in queryResultsGroup) {
                            var itemKey = $"?{queryGroup.Key}";
                            var itemResults = queryGroup.Select(x => x.Value).Select(x => x.GetId());
                            var node = nodes.FirstOrDefault(x => x.name.Equals(itemKey));
                            if(node != null)
                                node.Results = new BatchIdEntityQuery(graph.EntitiesIndexPath, itemResults).Query();
                            var edge = edges.FirstOrDefault(x => x.name.Equals(itemKey));
                            if(edge != null)
                                edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, itemResults).Query();
                        }
                    }
                }
            }
        }

        private static List<Task<SparqlResultSet>> RunWikidataEndpointQueries(this QueryGraph graph)
        {
            var tasks = new List<Task<SparqlResultSet>>();

            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.Traversed) continue;
                tasks.Add(GraphApiQueries.RunQueryAsync(node.ToSparql(graph).ToString()));
            }

            //foreach (var edge in graph.Edges.Select(x => x.Value))
            //{
            //    if (edge.Traversed) continue;
            //    tasks.Add(GraphApiQueries.RunQueryAsync(edge.ToSparql(graph).ToString()));
            //}

            return tasks.Where(x => x != null).ToList();
        }

        private static void RunNodeQueries(this QueryGraph graph, bool runOnEndpoint = true)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value).Where(x=>!x.AvoidQuery))
            {
                //The other complex queries. Try endpoint first, if timeout, try with the index.
                //If the user has a timeout, is because his query is still too broad.
                //Some suggestions will be proposed with the local index, until the query can be completed by the endpoint.
                if (node.IsInstanceOfType)
                {
                    //Intersect (Not if any, we want only the results of that instance, even if there are none):
                    var instanceOfResults = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.InstanceOfBaseTypes, 200).Query(20);
                    node.Results = instanceOfResults;
                    //TODO: Not sure if the previous run should consider this:
                    //node.Results = node.Results.Intersect(instanceOfResults).ToList();
                }
                else
                {
                    var intersectBaseTypes = new List<string>();

                    //Outgoing edges candidates, take their domain
                    var outgoingEdges = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf);
                    var baseDomainTypes = new List<string>();
                    foreach (var outgoingEdge in outgoingEdges)
                        baseDomainTypes = baseDomainTypes.IntersectIfAny(outgoingEdge.DomainBaseTypes).ToList();

                    intersectBaseTypes = intersectBaseTypes.IntersectIfAny(baseDomainTypes).ToList();

                    //Incoming edges candidates, take their range.
                    var incomingEdges = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf);
                    var baseRangeTypes = new List<string>();
                    foreach (var incomingEdge in incomingEdges)
                        baseRangeTypes = baseRangeTypes.IntersectIfAny(incomingEdge.RangeBaseTypes).ToList();

                    intersectBaseTypes = intersectBaseTypes.IntersectIfAny(baseRangeTypes).ToList();

                    if (intersectBaseTypes.Any())
                    {
                        //Combine domain & range, take a random sample and get those results:
                        intersectBaseTypes = intersectBaseTypes.TakeRandom(100000).ToList();
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, intersectBaseTypes, 200).Query(1000);
                    }
                    else
                    {
                        //If the instance is of a specific type, intersect a random sample of the instances with the previous results filter out the valid results:
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
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph, bool runOnEndpoint = true)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value).Where(x=>!x.AvoidQuery))
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
                    //if (source.IsGoingToGivenType)
                    //    sourceResultsProperties = source.Results.SelectMany(x => x.Properties).Distinct().Select(x => x.Id).ToList();
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
                    edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, intersectPropertiesIds).Query();
                }


            }
        }

        private static void CheckAvoidQueries(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {

                //Given Type, Do not Query
                if (node.IsGivenType)
                {
                    node.AvoidQuery = true;
                    node.Results = new List<Entity>();
                }

                //CASE: AVOID ENDPOINT QUERY, IS KNOWN TO TIMEOUT
                //Node that is not connected, return random results
                if (!node.IsSomehowDefined(graph))
                {
                    node.AvoidQuery = true;
                    var rnd = new Random();
                    var randomEntities = Enumerable.Repeat(1, 100).Select(_ => rnd.Next(99999)).Select(x => $"Q{x}");
                    node.Results = new BatchIdEntityQuery(graph.EntitiesIndexPath, randomEntities).Query();
                    //TODO: Temporary, for not getting empty results if there were none.
                    if (node.Results.Count < 20)
                    {
                        node.Results = node.Results.IntersectIfAny(new MultiLabelEntityQuery(graph.EntitiesIndexPath, "*").Query()).ToList();

                    }
                }

                //CASE: AVOID ENDPOINT QUERY, IS KNOWN TO TIMEOUT
                //Just instance of, search only for that.
                if (!node.HasIncomingEdges(graph) && node.GetOutgoingEdges(graph).Count().Equals(1) &&
                    node.IsInstanceOfType)
                {
                    node.AvoidQuery = true;
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.InstanceOfBaseTypes, 100).Query();
                }
            }

            foreach (var edge in graph.Edges.Select(x => x.Value))
            {

                if (edge.IsGivenType)
                {
                    edge.AvoidQuery = true;
                    edge.Results = new List<Property>();
                }
            }
        }
    }
}

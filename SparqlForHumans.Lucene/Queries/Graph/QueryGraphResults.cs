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
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertyIndexPath, bool runOnEndpoint = true, bool runNodeQueries = true)
        {
            graph.SetIndexPaths(entitiesIndexPath, propertyIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            //TODO: I think that I can move this somewhere in the future. (Performance improvement)
            graph.SetTypesDomainsAndRanges();

            graph.ResetTraverse();
            graph.CheckAvoidQueries();

            var tasks = runOnEndpoint ? graph.RunWikidataEndpointQueries(10000) : new List<Task<SparqlResultSet>>();

            if (runNodeQueries)
                graph.RunNodeQueries();

            graph.RunEdgeQueries();

            graph.AssignEndpointResults(tasks);
        }

        public static void AssignEndpointResults(this QueryGraph graph, List<Task<SparqlResultSet>> tasks)
        {
            if (!tasks.Any()) return;

            foreach (var task in tasks)
            {
                task.Wait();
                var resultsSet = task.Result;
                if (resultsSet == null) continue;

                graph.AssignEndpointResults(resultsSet);
            }
        }

        public static void AssignEndpointResults(this QueryGraph graph, SparqlResultSet resultsSet)
        {
            var nodes = graph.Nodes.Select(x => x.Value);
            var edges = graph.Edges.Select(x => x.Value);

            if (!resultsSet.IsEmpty)
            {
                foreach (var node in nodes.Where(x => x.Traversed))
                    node.Results = new List<Entity>();
                foreach (var edge in edges.Where(x => x.Traversed))
                    edge.Results = new List<Property>();
            }

            var queryResultsGroup = resultsSet.Results.SelectMany(x => x).GroupBy(x => x.Key);

            foreach (var queryGroup in queryResultsGroup)
            {
                var itemKey = $"{queryGroup.Key}";
                var itemResults = queryGroup.Select(x => x.Value).Select(x => x.GetId());
                var node = nodes.FirstOrDefault(x => x.name.Equals(itemKey));
                if (node != null)
                    node.Results = new BatchIdEntityQuery(graph.EntitiesIndexPath, itemResults).Query(10000);
                var edge = edges.FirstOrDefault(x => x.name.Equals(itemKey));
                if (edge != null)
                    edge.Results = new BatchIdPropertyQuery(graph.PropertiesIndexPath, itemResults).Query(10000);
            }
        }

        public static List<Task<SparqlResultSet>> RunWikidataEndpointQueries(this QueryGraph graph, int limit = 100)
        {
            var tasks = new List<Task<SparqlResultSet>>();

            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                if (node.Traversed) continue;
                var query = node.ToSparql(graph, limit).ToString().FixQuery();
                tasks.Add(GraphApiQueries.RunQueryAsync(query));
            }
            return tasks.Where(x => x != null).ToList();
        }

        private static void RunNodeQueries(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value).Where(x => !x.AvoidQuery))
            {
                //The other complex queries. Try endpoint first, if timeout, try with the index.
                //If the user has a timeout, is because his query is still too broad.
                //Some suggestions will be proposed with the local index, until the query can be completed by the endpoint.
                if (node.IsInstanceOfType)
                {
                    //Intersect (Not if any, we want only the results of that instance, even if there are none):
                    var instanceOfResults = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.InstanceOfTypes, 200).Query(20);
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
                        baseDomainTypes = baseDomainTypes.IntersectIfAny(outgoingEdge.DomainTypes).ToList();

                    intersectBaseTypes = intersectBaseTypes.IntersectIfAny(baseDomainTypes).ToList();

                    //Incoming edges candidates, take their range.
                    var incomingEdges = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf);
                    var baseRangeTypes = new List<string>();
                    foreach (var incomingEdge in incomingEdges)
                        baseRangeTypes = baseRangeTypes.IntersectIfAny(incomingEdge.RangeTypes).ToList();

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

        private static void RunEdgeQueries(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value).Where(x => !x.AvoidQuery))
            {
                var source = edge.GetSourceNode(graph);
                var target = edge.GetTargetNode(graph);

                var possibleProperties = new List<string>();

                if (source.IsGivenType || target.IsGivenType)
                {

                    var sourceGivenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, source.GivenTypes)
                        .Query().SelectMany(x => x.Properties).Select(x => x.Id).ToList();

                    var targetGivenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, target.GivenTypes)
                        .Query().SelectMany(x => x.ReverseProperties).Select(x => x.Id).ToList();

                    possibleProperties = possibleProperties.IntersectIfAny(sourceGivenPropertiesIds)
                        .IntersectIfAny(targetGivenPropertiesIds).ToList();
                }
                else
                {
                    if (source.IsInstanceOfType || target.IsInstanceOfType)
                    {
                        var instanceOfSourceProperties = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(source.GetInstanceOfValues(graph));
                        var instanceOfTargetProperties = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(target.GetInstanceOfValues(graph));
                        possibleProperties = possibleProperties.IntersectIfAny(instanceOfSourceProperties)
                            .IntersectIfAny(instanceOfTargetProperties).ToList();
                    }

                    var sourceGivenOutgoingEdges = source.GetOutgoingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();
                    var sourceGivenIncomingEdges = source.GetIncomingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();
                    var targetGivenOutgoingEdges = target.GetOutgoingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();
                    var targetGivenIncomingEdges = target.GetIncomingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();

                    foreach (var givenOutgoingEdge in sourceGivenOutgoingEdges)
                    {
                        foreach (var uri in givenOutgoingEdge.uris)
                        {
                            var sourceOutgoing = InMemoryQueryEngine.PropertyDomainOutgoingPropertiesQuery(uri);
                            possibleProperties = possibleProperties.IntersectIfAny(sourceOutgoing).ToList();
                        }
                    }

                    foreach (var givenIncomingEdge in sourceGivenIncomingEdges)
                    {
                        foreach (var uri in givenIncomingEdge.uris)
                        {
                            var sourceIncoming = InMemoryQueryEngine.PropertyRangeOutgoingPropertiesQuery(uri);
                            possibleProperties = possibleProperties.IntersectIfAny(sourceIncoming).ToList();
                        }
                    }

                    foreach (var givenOutgoingEdge in targetGivenOutgoingEdges)
                    {
                        foreach (var uri in givenOutgoingEdge.uris)
                        {
                            var targetOutgoing = InMemoryQueryEngine.PropertyDomainIncomingPropertiesQuery(uri);
                            possibleProperties = possibleProperties.IntersectIfAny(targetOutgoing).ToList();
                        }
                    }

                    foreach (var givenIncomingEdge in targetGivenIncomingEdges)
                    {
                        foreach (var uri in givenIncomingEdge.uris)
                        {
                            var targetIncoming = InMemoryQueryEngine.PropertyRangeIncomingPropertiesQuery(uri);
                            possibleProperties = possibleProperties.IntersectIfAny(targetIncoming).ToList();
                        }
                    }
                }
                edge.Results = !possibleProperties.Any()
                    ? new MultiLabelPropertyQuery(graph.PropertiesIndexPath, "*").Query()
                    : new BatchIdPropertyQuery(graph.PropertiesIndexPath, possibleProperties).Query(10000);

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
                    node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, node.InstanceOfTypes, 100).Query();
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

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
    public class QueryGraphResults
    {
        public void GetGraphQueryResults(QueryGraph graph, string entitiesIndexPath, string propertyIndexPath, bool runOnEndpoint = true, bool runNodeQueries = true)
        {
            graph.SetIndexPaths(entitiesIndexPath, propertyIndexPath);

            InMemoryQueryEngine.Init(entitiesIndexPath, propertyIndexPath);
            //TODO: I think that I can move this somewhere in the future. (Performance improvement)
            graph.SetTypesDomainsAndRanges();

            graph.ResetTraverse();
            CheckAvoidQueries(graph);

            var tasks = runOnEndpoint ? RunWikidataEndpointQueries(graph, 10000) : new List<Task<SparqlResultSet>>();

            if (runNodeQueries)
                RunNodeQueries(graph);

            RunEdgeQueries(graph);

            AssignEndpointResults(graph, tasks);
        }

        public void AssignEndpointResults(QueryGraph graph, List<Task<SparqlResultSet>> tasks)
        {
            if (!tasks.Any()) return;

            foreach (var task in tasks)
            {
                task.Wait();
                var resultsSet = task.Result;
                if (resultsSet == null) continue;

                AssignEndpointResults(graph, resultsSet);
            }
        }

        public void AssignEndpointResults(QueryGraph graph, SparqlResultSet resultsSet)
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

        public List<Task<SparqlResultSet>> RunWikidataEndpointQueries( QueryGraph graph, int limit = 100)
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

        private void RunNodeQueries(QueryGraph graph)
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
                    //Take domainTypes and intersect with rangeTypes.
                    var intersectTypes = new List<string>();

                    //Outgoing edges candidates, take their domain
                    var outgoingEdges = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf);
                    var domainTypes = new List<string>();
                    foreach (var outgoingEdge in outgoingEdges)
                        domainTypes = domainTypes.IntersectIfAny(outgoingEdge.DomainTypes).ToList();

                    intersectTypes = intersectTypes.IntersectIfAny(domainTypes).ToList();

                    //Incoming edges candidates, take their range.
                    var incomingEdges = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf);
                    var rangeTypes = new List<string>();
                    foreach (var incomingEdge in incomingEdges)
                        rangeTypes = rangeTypes.IntersectIfAny(incomingEdge.RangeTypes).ToList();

                    intersectTypes = intersectTypes.IntersectIfAny(rangeTypes).ToList();

                    if (intersectTypes.Any())
                    {
                        //Combine domain & range, take a random sample and get those results:
                        //TODO: Why sort them randomly? What is wrong with their current sorting?
                        intersectTypes = intersectTypes.TakeRandom(100000).ToList();
                        node.Results = new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, intersectTypes, 200).Query(1000);
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

        private void RunEdgeQueries(QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value).Where(x => !x.AvoidQuery))
            {
                var sourceNode = edge.GetSourceNode(graph);
                var targetNode = edge.GetTargetNode(graph);

                var possibleProperties = new List<string>();

                if (sourceNode.IsGivenType || targetNode.IsGivenType)
                {
                    var sourceGivenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, sourceNode.GivenTypes)
                        .Query().SelectMany(x => x.Properties).Select(x => x.Id);

                    var targetGivenPropertiesIds = new BatchIdEntityQuery(graph.EntitiesIndexPath, targetNode.GivenTypes)
                        .Query().SelectMany(x => x.ReverseProperties).Select(x => x.Id);

                    possibleProperties = possibleProperties.IntersectIfAny(sourceGivenPropertiesIds)
                        .IntersectIfAny(targetGivenPropertiesIds).ToList();
                }
                else
                {
                    if (sourceNode.IsInstanceOfType || targetNode.IsInstanceOfType)
                    {
                        //TODO: Why not use the source.InstanceOfTypes here:
                        var instanceOfSourceProperties = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(sourceNode.InstanceOfTypes);
                        var instanceOfTargetProperties = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(targetNode.InstanceOfTypes);
                        possibleProperties = possibleProperties.IntersectIfAny(instanceOfSourceProperties)
                            .IntersectIfAny(instanceOfTargetProperties).ToList();
                    }

                    var sourceGivenOutgoingEdges = sourceNode.GetOutgoingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();
                    var sourceGivenIncomingEdges = sourceNode.GetIncomingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();
                    var targetGivenOutgoingEdges = targetNode.GetOutgoingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();
                    var targetGivenIncomingEdges = targetNode.GetIncomingEdges(graph).Where(x => x.IsGivenType).Where(x => !x.IsInstanceOf).ToArray();

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

        private void CheckAvoidQueries(QueryGraph graph)
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
                if (!node.HasIncomingEdges(graph) 
                    && node.GetOutgoingEdges(graph).Count().Equals(1) 
                    && node.IsInstanceOfType)
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

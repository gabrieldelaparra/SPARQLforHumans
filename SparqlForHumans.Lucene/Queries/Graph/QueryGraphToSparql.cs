﻿using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphToSparql
    {
        private static TriplePatternPredicatePart ToSubject(this ITriplePatternBuilder builder, QueryNode node)
        {
            return node.IsGivenType ? builder.Subject(new Uri(node.uris.First())) : builder.Subject(node.name);
        }

        private static TriplePatternObjectPart ToPredicate(this TriplePatternPredicatePart subject, QueryEdge edge)
        {
            return edge.IsGivenType ? subject.PredicateUri(new Uri(edge.uris.First())) : subject.Predicate(edge.name);
        }

        private static ITriplePatternBuilder ToObject(this TriplePatternObjectPart predicate, QueryNode node)
        {
            return node.IsGivenType ? predicate.Object(new Uri(node.uris.First())) : predicate.Object(node.name);
        }

        public static SparqlQuery ToSparql(this QueryNode node, QueryGraph graph)
        {
            var variables = new List<string> {
                node.name,
                $"{node.name}Label",
            };

            var queryBuilder = QueryBuilder.Select(variables.ToArray()).Distinct();
            
            if(node.QueryType.Equals(QueryType.GivenSubjectTypeQueryDirectly))
            {
                var incomingEdges = node.GetIncomingEdges(graph);

                foreach (var incomingEdge in incomingEdges)
                {
                    var sourceNode = incomingEdge.GetSourceNode(graph);
                    queryBuilder.Where(x =>
                    {
                        x.ToSubject(sourceNode)
                            .ToPredicate(incomingEdge)
                            .ToObject(node);
                    });
                }
            }
            else if (node.QueryType.Equals(QueryType.GivenObjectTypeQueryDirectly))
            {
                var outgoingEdges = node.GetOutgoingEdges(graph);
                foreach (var incomingEdge in outgoingEdges)
                {
                    var targetNode = incomingEdge.GetTargetNode(graph);
                    queryBuilder.Where(x =>
                    {
                        x.ToSubject(node)
                            .ToPredicate(incomingEdge)
                            .ToObject(targetNode);
                    });
                }
            }

            queryBuilder.Service(new Uri("http://wikiba.se/ontology#label"),
                y => y.Where(x => x
                        .Subject(new Uri("http://www.bigdata.com/rdf#serviceParam"))
                        .PredicateUri(new Uri("http://wikiba.se/ontology#language"))
                        .ObjectLiteral("[AUTO_LANGUAGE],en")));
            
            queryBuilder.Limit(100);

            return queryBuilder.BuildQuery();
        }
    }
}

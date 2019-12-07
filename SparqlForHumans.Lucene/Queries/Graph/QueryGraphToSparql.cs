using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphToSparql
    {
        private static TriplePatternPredicatePart ToSubject(this ITriplePatternBuilder builder, QueryNode node)
        {
            return node.IsGivenType ? builder.Subject(new Uri(node.uris.First().ToEntityIri())) : builder.Subject(node.name);
        }

        private static TriplePatternObjectPart ToPredicate(this TriplePatternPredicatePart subject, QueryEdge edge)
        {
            return edge.IsGivenType ? subject.PredicateUri(new Uri(edge.uris.First().ToPropertyIri())) : subject.Predicate(edge.name);
        }

        private static ITriplePatternBuilder ToObject(this TriplePatternObjectPart predicate, QueryNode node)
        {
            return node.IsGivenType ? predicate.Object(new Uri(node.uris.First().ToEntityIri())) : predicate.Object(node.name);
        }

        public static Entity ToEntity(this SparqlResult result)
        {
            return new Entity
            {
                Id = result[0].GetUri().GetUriIdentifier(),
                Label = result[1].GetLiteralValue(),
            };
        }

        public static Property ToProperty(this SparqlResult result)
        {
            return new Property
            {
                Id = result[0].GetUri().GetUriIdentifier(),
                Label = result[1].GetLiteralValue(),
            };
        }

        public static void ResetTraverse(this QueryGraph graph)
        {
            foreach (var queryNode in graph.Nodes.Select(x => x.Value))
                queryNode.Traversed = false;
            foreach (var queryEdge in graph.Edges.Select(x => x.Value))
                queryEdge.Traversed = false;
        }

        public static SparqlQuery ToSparql(this QueryNode node, QueryGraph graph)
        {
            graph.ResetTraverse();

            var variables = new List<string> {
                node.name,
                $"{node.name}Label",
            };

            var queryBuilder = QueryBuilder.Select(variables.ToArray()).Distinct();

            node.TraverseNodeToSparql(queryBuilder, graph);

            queryBuilder.Service(new Uri("http://wikiba.se/ontology#label"),
                y => y.Where(x => x
                        .Subject(new Uri("http://www.bigdata.com/rdf#serviceParam"))
                        .PredicateUri(new Uri("http://wikiba.se/ontology#language"))
                        .ObjectLiteral("[AUTO_LANGUAGE],en")));

            var literal = new NodeFactory().CreateLiteralNode("entity/Q");
            var expr = new ContainsFunction(new StrFunction(new VariableTerm(node.name)), new ConstantTerm(literal));
            queryBuilder.Filter(expr);

            queryBuilder.Limit(100);

            return queryBuilder.BuildQuery();
        }

        public static SparqlQuery ToSparql(this QueryEdge edge, QueryGraph graph)
        {
            graph.ResetTraverse();

            var variables = new List<string> {
                "?propertyEntity",
                "?propertyEntityLabel",
            };

            var queryBuilder = QueryBuilder.Select(variables.ToArray()).Distinct();

            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);
            queryBuilder.Where(x =>
            {
                x.ToSubject(source)
                    .ToPredicate(edge)
                    .ToObject(target)
                    .Subject("?propertyEntity")
                    .PredicateUri(new Uri("http://wikiba.se/ontology#directClaim"))
                    .Object(edge.name)
                    ;
            });
            source.TraverseNodeToSparql(queryBuilder, graph);
            target.TraverseNodeToSparql(queryBuilder, graph);

            queryBuilder.Service(new Uri("http://wikiba.se/ontology#label"),
                y => y.Where(x => x
                    .Subject(new Uri("http://www.bigdata.com/rdf#serviceParam"))
                    .PredicateUri(new Uri("http://wikiba.se/ontology#language"))
                    .ObjectLiteral("[AUTO_LANGUAGE],en")));

            queryBuilder.Limit(100);

            return queryBuilder.BuildQuery();
        }

        public static IQueryBuilder TraverseNodeToSparql(this QueryNode node, IQueryBuilder queryBuilder, QueryGraph graph)
        {
            if (node.Traversed) return queryBuilder;
            node.Traversed = true;

            var incomingEdges = node.GetIncomingEdges(graph);
            foreach (var incomingEdge in incomingEdges)
            {
                var sourceNode = incomingEdge.GetSourceNode(graph);
                if (sourceNode.Traversed) continue;

                queryBuilder.Where(x =>
                {
                    x.ToSubject(sourceNode)
                        .ToPredicate(incomingEdge)
                        .ToObject(node)
                        .Subject(node.name)
                        .PredicateUri(new Uri("http://www.w3.org/2000/01/rdf-schema#label"))
                        .Object($"?{node.name}_discardNonEntities");
                });
                sourceNode.TraverseNodeToSparql(queryBuilder, graph);
            }

            var outgoingEdges = node.GetOutgoingEdges(graph);
            foreach (var incomingEdge in outgoingEdges)
            {
                var targetNode = incomingEdge.GetTargetNode(graph);
                if (targetNode.Traversed) continue;
                queryBuilder.Where(x =>
                {
                    x.ToSubject(node)
                        .ToPredicate(incomingEdge)
                        .ToObject(targetNode);
                });
                targetNode.TraverseNodeToSparql(queryBuilder, graph);
            }


            return queryBuilder;
        }
    }
}

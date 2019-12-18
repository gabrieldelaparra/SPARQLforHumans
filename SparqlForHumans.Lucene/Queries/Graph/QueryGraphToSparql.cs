using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using VDS.RDF.Query.Expressions.Primary;
using ContainsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ContainsFunction;

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

        public static IEnumerable<string> GetIds(this SparqlResultSet results)
        {
            return results?.Select(x => x[0].GetUri().GetUriIdentifier());
        }

        public static IEnumerable<string> GetQIds(this SparqlResultSet results)
        {
            return results.GetIds()?.Where(x => x.StartsWith(Constants.EntityPrefix));
        }

        public static IEnumerable<string> GetPIds(this SparqlResultSet results)
        {
            return results.GetIds()?.Where(x => x.StartsWith(Constants.PropertyPrefix));
        }

        public static void ResetTraverse(this QueryGraph graph)
        {
            foreach (var queryNode in graph.Nodes.Select(x => x.Value))
                queryNode.Traversed = false;
            foreach (var queryEdge in graph.Edges.Select(x => x.Value))
                queryEdge.Traversed = false;
        }
        public static SparqlQuery ToSparql(this QueryNode node, QueryGraph graph, string[] variables, int limit = 100)
        {
            var sparqlVariables = variables.Select(x => new SparqlVariable(x)).ToArray();
            var queryBuilder = QueryBuilder.Select(sparqlVariables).Distinct();

            node.TraverseNodeToSparql(queryBuilder, graph);

            if (limit > 0)
                queryBuilder.Limit(limit);

            return queryBuilder.BuildQuery();
        }


        public static SparqlQuery ToSparql(this QueryNode node, QueryGraph graph, int limit = 100)
        {
            var queryBuilder = QueryBuilder.SelectAll().Distinct();

            node.TraverseNodeToSparql(queryBuilder, graph);

            if (limit > 0)
                queryBuilder.Limit(limit);

            return queryBuilder.BuildQuery();
        }

        public static string FixQuery(this string sparqlQuery)
        {
            var queryLines = sparqlQuery.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            queryLines = queryLines.Distinct().ToArray();
            return string.Join(Environment.NewLine, queryLines);
        }

        public static SparqlQuery ToSparql(this QueryEdge edge, QueryGraph graph)
        {
            var queryBuilder = QueryBuilder.SelectAll().Distinct();

            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);

            source.TraverseNodeToSparql(queryBuilder, graph);
            target.TraverseNodeToSparql(queryBuilder, graph);

            queryBuilder.Limit(100);

            return queryBuilder.BuildQuery();
        }

        public static IQueryBuilder TraverseNodeToSparql(this QueryNode node, IQueryBuilder queryBuilder, QueryGraph graph)
        {
            var incomingEdges = node.GetIncomingEdges(graph);
            foreach (var incomingEdge in incomingEdges)
            {
                if (incomingEdge.Traversed) continue;
                var sourceNode = incomingEdge.GetSourceNode(graph);

                queryBuilder.Where(x =>
                {
                    x.ToSubject(sourceNode)
                        .ToPredicate(incomingEdge)
                        .ToObject(node);
                });

                if (!sourceNode.IsGivenType)
                {
                    var literal = new NodeFactory().CreateLiteralNode($"{Constants.EntityIRI}{Constants.EntityPrefix}");
                    var expr = new StartsWithFunction(new StrFunction(new VariableTerm(sourceNode.name)), new ConstantTerm(literal));
                    queryBuilder.Filter(expr);
                }

                if (!incomingEdge.IsGivenType)
                {
                    var literal = new NodeFactory().CreateLiteralNode($"{Constants.PropertyIRI}{Constants.PropertyPrefix}");
                    var expr = new StartsWithFunction(new StrFunction(new VariableTerm(incomingEdge.name)), new ConstantTerm(literal));
                    queryBuilder.Filter(expr);
                }

                if (!node.IsGivenType)
                {
                    var literal = new NodeFactory().CreateLiteralNode($"{Constants.EntityIRI}{Constants.EntityPrefix}");
                    var expr = new StartsWithFunction(new StrFunction(new VariableTerm(node.name)), new ConstantTerm(literal));
                    queryBuilder.Filter(expr);
                }

                incomingEdge.Traversed = true;
                sourceNode.TraverseNodeToSparql(queryBuilder, graph);
            }

            var outgoingEdges = node.GetOutgoingEdges(graph);
            foreach (var outgoingEdge in outgoingEdges)
            {
                if (outgoingEdge.Traversed) continue;
                var targetNode = outgoingEdge.GetTargetNode(graph);

                queryBuilder.Where(x =>
                {
                    x.ToSubject(node)
                        .ToPredicate(outgoingEdge)
                        .ToObject(targetNode);
                });

                if (!targetNode.IsGivenType)
                {
                    var literal = new NodeFactory().CreateLiteralNode($"{Constants.EntityIRI}{Constants.EntityPrefix}");
                    var expr = new StartsWithFunction(new StrFunction(new VariableTerm(targetNode.name)), new ConstantTerm(literal));
                    queryBuilder.Filter(expr);
                }

                if (!outgoingEdge.IsGivenType)
                {
                    var literal = new NodeFactory().CreateLiteralNode($"{Constants.PropertyIRI}{Constants.PropertyPrefix}");
                    var expr = new StartsWithFunction(new StrFunction(new VariableTerm(outgoingEdge.name)), new ConstantTerm(literal));
                    queryBuilder.Filter(expr);
                }

                if (!node.IsGivenType)
                {
                    var literal = new NodeFactory().CreateLiteralNode($"{Constants.EntityIRI}{Constants.EntityPrefix}");
                    var expr = new StartsWithFunction(new StrFunction(new VariableTerm(node.name)), new ConstantTerm(literal));
                    queryBuilder.Filter(expr);
                }

                outgoingEdge.Traversed = true;
                targetNode.TraverseNodeToSparql(queryBuilder, graph);
            }

            return queryBuilder;
        }
    }
}

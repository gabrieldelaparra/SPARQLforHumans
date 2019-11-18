using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphToSparql
    {
        public static TriplePatternPredicatePart ToSubject(this ITriplePatternBuilder builder, QueryNode node)
        {
            return node.IsGivenType ? builder.Subject(new Uri(node.uris.First())) : builder.Subject(node.name);
        }

        public static TriplePatternObjectPart ToPredicate(this TriplePatternPredicatePart subject, QueryEdge edge)
        {
            return edge.IsGivenType ? subject.PredicateUri(new Uri(edge.uris.First())) : subject.Predicate(edge.name);
        }

        public static ITriplePatternBuilder ToObject(this TriplePatternObjectPart predicate, QueryNode node)
        {
            return node.IsGivenType ? predicate.Object(new Uri(node.uris.First())) : predicate.Object(node.name);
        }

        public static string ApplyPrefix(this string uriString, (string, string)[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (uriString.Contains(prefix.Item2))
                    return uriString.Replace(prefix.Item2, $"{prefix.Item1}:");
            }

            return uriString;
        }

        public static (string, string)[] PrefixStrings = new[]{
            //("wdt","http://www.wikidata.org/prop/direct/"),
            //("wd", "http://www.wikidata.org/entity/"),
            ("rdfs", "http://www.w3.org/2000/01/rdf-schema#"),
            ("bd", "http://www.bigdata.com/rdf#"),
            ("wikibase", "http://wikiba.se/ontology#"),
            };

        public static SparqlQuery ToSparql(this QueryGraph graph)
        {
            var variables = new List<string>();
            var directNodes = graph.Nodes.Select(x => x.Value)
                .Where(x => x.QueryType.Equals(QueryType.GivenSubjectTypeQueryDirectly)
                            || x.QueryType.Equals(QueryType.GivenObjectTypeQueryDirectly))
                .ToList();

            foreach (var node in directNodes)
            {
                variables.Add(node.name);
            }

            var queryBuilder = QueryBuilder.Select(variables.ToArray()).Distinct();
            foreach (var node in directNodes.Where(n => n.QueryType.Equals(QueryType.GivenSubjectTypeQueryDirectly)))
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

            foreach (var node in directNodes.Where(n => n.QueryType.Equals(QueryType.GivenObjectTypeQueryDirectly)))
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

            return queryBuilder.BuildQuery();
        }
        public static SparqlQuery WithPrefix()
        {
            var prefixes = new NamespaceMapper(true);
            prefixes.AddNamespace("wdt", new Uri("http://www.wikidata.org/prop/direct/"));
            prefixes.AddNamespace("wd", new Uri("http://www.wikidata.org/entity/"));
            prefixes.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            prefixes.AddNamespace("bd", new Uri("http://www.bigdata.com/rdf#"));
            prefixes.AddNamespace("wikibase", new Uri("http://wikiba.se/ontology#"));

            //string cat = "item";
            var queryBuilder =
                QueryBuilder
                    .Select("item", "itemLabel")
                    .Distinct()
                    .Where(
                        triplePatternBuilder =>
                        {
                            triplePatternBuilder
                                .Subject("item")
                                .PredicateUri("wdt:P31")
                                .Object<IUriNode>("wd:Q146");
                        })
                    .Service(new Uri("http://wikiba.se/ontology#label"),
                        y =>
                            y.Where(x => x
                                .Subject<IUriNode>("bd:serviceParam")
                                .PredicateUri("wikibase:language")
                                .ObjectLiteral("[AUTO_LANGUAGE],en")))
                    .Limit(20);
            queryBuilder.Prefixes = prefixes;

            var query = queryBuilder.BuildQuery();
            Console.WriteLine(query.ToString());
            return query;
        }
    }
}

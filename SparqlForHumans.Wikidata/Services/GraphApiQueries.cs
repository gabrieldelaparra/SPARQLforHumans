using System;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

namespace SparqlForHumans.Wikidata.Services
{
    public static class GraphApiQueries
    {
        static SparqlQuery WithPrefix()
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

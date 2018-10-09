using System;
using System.Collections.Generic;
using System.Text;
using SparqlForHumans.Models;
using SparqlForHumans.Models.RDFQuery;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

namespace SparqlForHumans.RDF.Queries
{
    public static class QueryExtensions
    {
        public static SparqlQuery BuildQuery(this IQueryTriple triple)
        {
            var queryBuilder = VDS.RDF.Query.Builder.QueryBuilder
                .Select(triple.Object.Label)
                .Where(x => x
                    .Subject(new Uri(triple.Subject.Uri()))
                    .PredicateUri(new Uri(triple.Predicate.Uri()))
                    .Object(triple.Object.Label));

            return queryBuilder.BuildQuery();
        }

        public static SparqlResultSet RunWikidataQuery(this SparqlQuery query)
        {
            var endpoint = new SparqlRemoteEndpoint(new Uri("https://query.wikidata.org"), "https://query.wikidata.org");
            return query.RunQuery(endpoint);
        }


        public static SparqlResultSet RunQuery(this SparqlQuery query, SparqlRemoteEndpoint endpoint)
        {
            return endpoint.QueryWithResultSet(query.ToString());
        }

        public static void TestQuery()
        {
            //var queryBuilder = VDS.RDF.Query.Builder.QueryBuilder.Select()
            //var queryBuilder = VDS.RDF.Query.Builder.QueryBuilder
            //    .Select(keys.ToArray())
            //    .Where(a =>
            //        a.Subject(string.Format("<{0}>", uri)).PredicateUri("gwsw:hasAspect").Object("aspect")
            //            .Subject("aspect").PredicateUri("rdf:type").Object("type")
            //            .Subject("type").PredicateUri("rdfs:label").Object("label")
            //    )
            //    .Optional(opt => opt
            //        .Where(a =>
            //            a.Subject("aspect").PredicateUri("sn:hasValue").Object("value")
            //                .Subject("type").PredicateUri("sn:hasUnit").Object("unit")
            //        )
            //    );

        }
    }
}

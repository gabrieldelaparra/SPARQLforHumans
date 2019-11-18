using System;
using SparqlForHumans.Wikidata.Models;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

namespace SparqlForHumans.Wikidata.Services
{
    public static class GraphApiQueries
    {
        public static SparqlResultSet RunQuery(string sparqlQuery)
        {
            var endpoint = new CustomSparqlEndPoint(new Uri("https://query.wikidata.org/sparql"));
            return endpoint.QueryWithResultSet(sparqlQuery);
        }
    }
}

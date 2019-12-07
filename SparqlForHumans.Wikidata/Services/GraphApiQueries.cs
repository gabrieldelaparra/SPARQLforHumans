using System;
using System.Net;
using SparqlForHumans.Wikidata.Models;
using VDS.RDF.Query;

namespace SparqlForHumans.Wikidata.Services
{
    public static class GraphApiQueries
    {
        public static SparqlResultSet RunQuery(string sparqlQuery)
        {
            try {
                var endpoint = new CustomSparqlEndPoint(new Uri("https://query.wikidata.org/sparql"));
                return endpoint.QueryWithResultSet(sparqlQuery);
            }
            catch (Exception) {
                return null;
            }
        }
    }
}

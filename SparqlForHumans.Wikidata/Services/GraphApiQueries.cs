using System;
using System.Net;
using System.Threading.Tasks;
using SparqlForHumans.Wikidata.Models;
using VDS.RDF.Query;

namespace SparqlForHumans.Wikidata.Services
{
    public static class GraphApiQueries
    {
        public static int QueryTimeoutMs { get; set; } = 10000;
        public static SparqlResultSet RunQuery(string sparqlQuery, bool runOnEndpoint = true)
        {
            if (!runOnEndpoint)
                return null;

            try
            {
                var endpoint = new CustomSparqlEndPoint(new Uri("https://query.wikidata.org/sparql"));
                return endpoint.QueryWithResultSet(sparqlQuery);
            }
            catch (Exception e)
            {
                var logger = Logger.Logger.Init();
                if (e.InnerException is WebException webException && webException.Status == WebExceptionStatus.Timeout)
                    logger.Warn($"Timeout ({QueryTimeoutMs / 1000}s) on executing query:{Environment.NewLine}{sparqlQuery}");
                return null;
            }
        }

        public static Task<SparqlResultSet> RunQueryAsync(string sparqlQuery, bool runOnEndpoint = true)
        {
            if (!runOnEndpoint)
                return null;

            return Task.Run(() => RunQuery(sparqlQuery, runOnEndpoint));
        }
    }
}

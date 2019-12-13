using System;
using System.Net;
using SparqlForHumans.Wikidata.Services;
using VDS.RDF.Query;

namespace SparqlForHumans.Wikidata.Models
{
    public class CustomSparqlEndPoint : SparqlRemoteEndpoint
    {
        public CustomSparqlEndPoint(Uri endpointUri) : base(endpointUri) { }

        protected override void ApplyCustomRequestOptions(HttpWebRequest httpRequest)
        {
            httpRequest.Method = "GET";
            httpRequest.Accept = "application/sparql-results+json";
            httpRequest.UserAgent = ".Net Client";
            httpRequest.Timeout = GraphApiQueries.QueryTimeoutMs;
            base.ApplyCustomRequestOptions(httpRequest);
        }
    }
}
